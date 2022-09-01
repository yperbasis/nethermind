//  Copyright (c) 2021 Demerzel Solutions Limited
//  This file is part of the Nethermind library.
//
//  The Nethermind library is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  The Nethermind library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nethermind.Api;
using Nethermind.Blockchain;
using Nethermind.Blockchain.Synchronization;
using Nethermind.Consensus;
using Nethermind.Consensus.Processing;
using Nethermind.Consensus.Validators;
using Nethermind.Core;
using Nethermind.Core.Caching;
using Nethermind.Core.Crypto;
using Nethermind.Core.Specs;
using Nethermind.Crypto;
using Nethermind.Int256;
using Nethermind.JsonRpc;
using Nethermind.Logging;
using Nethermind.Merge.Plugin.Data.V1;
using Nethermind.Merge.Plugin.InvalidChainTracker;
using Nethermind.Merge.Plugin.Synchronization;
using Nethermind.Synchronization;

namespace Nethermind.Merge.Plugin.Handlers.V1
{
    /// <summary>
    /// Verifies the payload according to the execution environment rule set (EIP-3675) and returns the <see cref="PayloadStatusV1"/> of the verification and the hash of the last valid block.
    ///
    /// <seealso cref="http://github.com/ethereum/execution-apis/blob/main/src/engine/specification.md#engine_newpayloadv1"/>
    /// </summary>
    public class NewPayloadV1Handler : IAsyncHandler<ExecutionPayloadV1, PayloadStatusV1>
    {
        private readonly IBlockValidator _blockValidator;
        private readonly IBlockTree _blockTree;
        private readonly ISyncConfig _syncConfig;
        private readonly IPoSSwitcher _poSSwitcher;
        private readonly IBeaconSyncStrategy _beaconSyncStrategy;
        private readonly IBeaconPivot _beaconPivot;
        private readonly IBlockCacheService _blockCacheService;
        private readonly IBlockProcessingQueue _processingQueue;
        private readonly IMergeSyncController _mergeSyncController;
        private readonly ISpecProvider _specProvider;
        private readonly IInvalidChainTracker _invalidChainTracker;
        private readonly ILogger _logger;
        private readonly LruCache<Keccak, bool>? _latestBlocks;
        private readonly ProcessingOptions _defaultProcessingOptions;
        private readonly TimeSpan _timeout;

        public NewPayloadV1Handler(
            IBlockValidator blockValidator,
            IBlockTree blockTree,
            IInitConfig initConfig,
            ISyncConfig syncConfig,
            IPoSSwitcher poSSwitcher,
            IBeaconSyncStrategy beaconSyncStrategy,
            IBeaconPivot beaconPivot,
            IBlockCacheService blockCacheService,
            IBlockProcessingQueue processingQueue,
            IInvalidChainTracker invalidChainTracker,
            IMergeSyncController mergeSyncController,
            ISpecProvider specProvider,
            ILogManager logManager,
            TimeSpan? timeout = null,
            int cacheSize = 50)
        {
            _blockValidator = blockValidator ?? throw new ArgumentNullException(nameof(blockValidator));
            _blockTree = blockTree;
            _syncConfig = syncConfig;
            _poSSwitcher = poSSwitcher;
            _beaconSyncStrategy = beaconSyncStrategy;
            _beaconPivot = beaconPivot;
            _blockCacheService = blockCacheService;
            _processingQueue = processingQueue;
            _invalidChainTracker = invalidChainTracker;
            _mergeSyncController = mergeSyncController;
            _specProvider = specProvider;
            _logger = logManager.GetClassLogger();
            _defaultProcessingOptions = initConfig.StoreReceipts ? ProcessingOptions.EthereumMerge | ProcessingOptions.StoreReceipts : ProcessingOptions.EthereumMerge;
            _timeout = timeout ?? TimeSpan.FromSeconds(7);
            if (cacheSize > 0)
                _latestBlocks = new LruCache<Keccak, bool>(cacheSize, 0, "LatestBlocks");
        }

        public async Task<ResultWrapper<PayloadStatusV1>> HandleAsync(ExecutionPayloadV1 request)
        {
            string requestStr = $"a new payload: {request}";
            if (_logger.IsInfo) { _logger.Info($"Received {requestStr}"); }

            if (!request.TryGetBlock(out Block? block, _poSSwitcher.FinalTotalDifficulty))
            {
                if (_logger.IsWarn) _logger.Warn($"Invalid block. Result of {requestStr}.");
                return NewPayloadV1Result.Invalid(null, $"Block {request} could not be parsed as a block");
            }

            if (!HeaderValidator.ValidateHash(block!.Header))
            {
                if (_logger.IsWarn) _logger.Warn($"InvalidBlockHash. Result of {requestStr}.");
                return NewPayloadV1Result.InvalidBlockHash;
            }

            Keccak blockHash = block.Hash!;
            _invalidChainTracker.SetChildParent(blockHash, request.ParentHash);
            if (_invalidChainTracker.IsOnKnownInvalidChain(blockHash, out Keccak? lastValidHash))
            {
                if (_logger.IsInfo) _logger.Info($"Invalid - block {request} is known to be a part of an invalid chain.");
                return NewPayloadV1Result.Invalid(lastValidHash, $"Block {request} is known to be a part of an invalid chain.");
            }

            if (block.Header.Number <= _syncConfig.PivotNumberParsed)
            {
                if (_logger.IsInfo) _logger.Info($"Pre-pivot block, ignored and returned Syncing. Result of {requestStr}.");
                return NewPayloadV1Result.Syncing;
            }

            block.Header.TotalDifficulty = _poSSwitcher.FinalTotalDifficulty;

            BlockHeader? parentHeader = _blockTree.FindHeader(request.ParentHash, BlockTreeLookupOptions.DoNotCreateLevelIfMissing);
            if (parentHeader is null)
            {
                // possible that headers sync finished before this was called, so blocks in cache weren't inserted
                if (!_beaconSyncStrategy.IsBeaconSyncFinished(parentHeader))
                {
                    bool inserted = TryInsertDanglingBlock(block);
                    if (_logger.IsInfo) _logger.Info(inserted ? $"BeaconSync not finished - block {block} inserted" : $"BeaconSync not finished - block {block} added to cache.");
                    return NewPayloadV1Result.Syncing;
                }

                if (_logger.IsInfo) _logger.Info($"Insert block into cache without parent {block}");
                _blockCacheService.BlockCache.TryAdd(blockHash, block);
                return NewPayloadV1Result.Syncing;
            }

            // we need to check if the head is greater than block.Number. In fast sync we could return Valid to CL without this if
            if (_blockTree.IsOnMainChainBehindOrEqualHead(block))
            {
                if (_logger.IsInfo) _logger.Info($"Valid... A new payload ignored. Block {block.ToString(Block.Format.FullHashAndNumber)} found in main chain.");
                return NewPayloadV1Result.Valid(block.Hash);
            }

            if (!ShouldProcessBlock(block, parentHeader, out ProcessingOptions processingOptions)) // we shouldn't process block
            {
                if (!_blockValidator.ValidateSuggestedBlock(block))
                {
                    if (_logger.IsInfo) _logger.Info($"Rejecting invalid block received during the sync, block: {block}");
                    return NewPayloadV1Result.Invalid(null);
                }

                BlockTreeInsertHeaderOptions insertHeaderOptions = BlockTreeInsertHeaderOptions.BeaconBlockInsert;

                if (block.Number <= Math.Max(_blockTree.BestKnownNumber, _blockTree.BestKnownBeaconNumber) && _blockTree.FindBlock(block.GetOrCalculateHash(), BlockTreeLookupOptions.TotalDifficultyNotNeeded) != null)
                {
                    if (_logger.IsInfo) _logger.Info($"Syncing... Parent wasn't processed. Block already known in blockTree {block}.");
                    return NewPayloadV1Result.Syncing;
                }

                if (_beaconPivot.ProcessDestination != null && _beaconPivot.ProcessDestination.Hash == block.ParentHash)
                {
                    insertHeaderOptions |= BlockTreeInsertHeaderOptions.MoveToBeaconMainChain; // we're extending our beacon canonical chain
                    _beaconPivot.ProcessDestination = block.Header;
                }

                _beaconPivot.EnsurePivot(block.Header, true);
                _blockTree.Insert(block, BlockTreeInsertBlockOptions.SaveHeader | BlockTreeInsertBlockOptions.SkipCanAcceptNewBlocks, insertHeaderOptions);

                if (_logger.IsInfo) _logger.Info($"Syncing... Parent wasn't processed. Inserting block {block}.");
                return NewPayloadV1Result.Syncing;
            }

            if ((block.TotalDifficulty ?? 0) != 0 && (_poSSwitcher.MisconfiguredTerminalTotalDifficulty() || _poSSwitcher.BlockBeforeTerminalTotalDifficulty(parentHeader)))
            {
                if (_logger.IsWarn) _logger.Warn($"Invalid terminal block. Nethermind TTD {_poSSwitcher.TerminalTotalDifficulty}, Parent TD: {parentHeader.TotalDifficulty}. Request: {requestStr}.");

                // {status: INVALID, latestValidHash: 0x0000000000000000000000000000000000000000000000000000000000000000, validationError: errorMessage | null} if terminal block conditions are not satisfied
                return NewPayloadV1Result.Invalid(Keccak.Zero);
            }

            // Otherwise, we can just process this block and we don't need to do BeaconSync anymore.
            _mergeSyncController.StopSyncing();

            // Try to execute block
            (ValidationResult result, string? message) = await ValidateBlockAndProcess(block, parentHeader, processingOptions);

            if (result == ValidationResult.Invalid)
            {
                if (_logger.IsInfo) _logger.Info($"Invalid block found. Validation message: {message}. Result of {requestStr}.");
                _invalidChainTracker.OnInvalidBlock(blockHash, request.ParentHash);
                return ResultWrapper<PayloadStatusV1>.Success(BuildInvalidPayloadStatusV1(request, message));
            }

            if (result == ValidationResult.AlreadyKnown) // this could happen only when we processed a parent, repeated the same block and we're processing this block via sync
            {
                if (_blockTree.IsMainChain(block.GetOrCalculateHash())) // if the block is on main chain it means that we've already finished processing it so we can return VALID
                {
                    if (_logger.IsInfo) _logger.Info($"Valid - already known processed block {requestStr}");
                    return ResultWrapper<PayloadStatusV1>.Success(new PayloadStatusV1 { Status = PayloadStatus.Valid, LatestValidHash = request.BlockHash });
                }

                if (_logger.IsInfo) _logger.Info($"Syncing - already known not processed block {requestStr}.");
                return NewPayloadV1Result.Syncing;
            }

            if (result == ValidationResult.Syncing)
            {
                if (_logger.IsInfo) _logger.Info($"Processing queue wasn't empty added to queue {requestStr}.");
                return NewPayloadV1Result.Syncing;
            }

            if (_logger.IsInfo) _logger.Info($"Valid. Result of {requestStr}.");
            return NewPayloadV1Result.Valid(request.BlockHash);
        }

        /// <summary>
        /// Decides if we should process the block or try syncing to it. It also returns what options to process the block with.
        /// </summary>
        /// <param name="block">Block</param>
        /// <param name="parent">Parent header</param>
        /// <param name="processingOptions">Options that should be used for processing</param>
        /// <returns>Options which should be used for block processing. Null if we shouldn't process the block.</returns>
        /// <remarks>
        /// We decide to process blocks in two situations:
        /// 1. The block parent was already processed. Then we process with ProcessingOptions.EthereumMerge with potentially also StoringReceipts.
        ///    This contains ProcessingOptions.IgnoreParentNotOnMainChain flag in order not to collect whole branch for processing, but only process this block directly on parent.
        ///    As parent was processed the state to process on should also be available.
        ///
        /// 2. If the parent wasn't processed, but it was a PoW block (terminal block) and we are not syncing PoW chain and are in the deep past.
        ///    In this case we remove ~ProcessingOptions.IgnoreParentNotOnMainChain flag in order to collect whole branch for processing.
        ///    If we didn't support this edge case then we couldn't process this block and would have to return Syncing, which is not desired during transition.
        ///
        /// Scenario 2 proved to be quite common on testnets which produced multiple transition blocks.
        /// </remarks>
        private bool ShouldProcessBlock(Block block, BlockHeader parent, out ProcessingOptions processingOptions)
        {
            processingOptions = _defaultProcessingOptions;

            BlockInfo? parentBlockInfo = _blockTree.GetInfo(parent.Number, parent.GetOrCalculateHash()).Info;
            bool parentProcessed = parentBlockInfo is { WasProcessed: true };

            // During the transition we can have a case of NP built over a transition block that wasn't processed.
            // We want to force process the whole branch then, but not longer than few blocks.
            // But we don't want this to trigger when we are in beacon sync.
            // The last condition: !parentBlockInfo.IsBeaconInfo will be true for terminal blocks.
            // Checking _posSwitcher.IsTerminal might not be the best, because we're loading parentHeader with DoNotCalculateTotalDifficulty option
            bool weHaveOnlyFewBlocksToProcess = (_blockTree.Head?.Number ?? 0) + 8 >= block.Number;
            bool parentIsPoWBlock = parent.Difficulty != UInt256.Zero;
            bool processTerminalBlock = !_poSSwitcher.TransitionFinished // we haven't finished transition
                                        && weHaveOnlyFewBlocksToProcess // we won't try to process too much blocks (if we are behind the transition block and still processing blocks)
                                        && parentBlockInfo is { IsBeaconInfo: false } // we are not in beacon sync
                                        && parentIsPoWBlock; // parent was PoW block -> so it was a transition block

            if (!parentProcessed && processTerminalBlock) // so if parent wasn't processed
            {
                if (_logger.IsInfo) _logger.Info($"Forced processing block {block}, block TD: {block.TotalDifficulty}, parent: {parent}, parent TD: {parent.TotalDifficulty}");

                // if parent wasn't processed and we want to force processing terminal block then we need to allow to process whole branch, not just one block
                // in all other cases when parent is processed ProcessingOptions.IgnoreParentNotOnMainChain allows us to process just this block ignoring that its not on Head
                // this option is part of ProcessingOptions.EthereumMerge option
                processingOptions &= ~ProcessingOptions.IgnoreParentNotOnMainChain;
            }

            return parentProcessed || processTerminalBlock;
        }

        private async Task<(ValidationResult, string? Message)> ValidateBlockAndProcess(Block block, BlockHeader parent, ProcessingOptions processingOptions)
        {
            ValidationResult ToValid(bool valid) => valid ? ValidationResult.Valid : ValidationResult.Invalid;
            string? validationMessage = null;

            // If duplicate, reuse results
            if (_latestBlocks is not null && _latestBlocks.TryGet(block.Hash!, out bool isValid))
            {
                if (!isValid && _logger.IsWarn)
                {
                    validationMessage = $"Invalid block found in latestBlock cache.";
                    if (_logger.IsWarn) _logger.Warn(validationMessage);
                }

                return (ValidationResult.AlreadyKnown | ToValid(isValid), validationMessage);
            }

            // Validate
            bool validAndProcessed = ValidateWithBlockValidator(block, parent);
            ValidationResult? result = ValidationResult.Syncing;

            if (!validAndProcessed)
            {
                return (ValidationResult.Invalid, string.Empty);
            }

            TaskCompletionSource<ValidationResult> blockProcessedTaskCompletionSource = new();
            Task<ValidationResult> blockProcessed = blockProcessedTaskCompletionSource.Task;

            void GetProcessingQueueOnBlockRemoved(object? o, BlockHashEventArgs e)
            {
                if (e.BlockHash == block.Hash)
                {
                    _processingQueue.BlockRemoved -= GetProcessingQueueOnBlockRemoved;

                    const string blockProcessingThrewException = "Block processing threw exception.";

                    if (e.ProcessingResult == ProcessingResult.Exception)
                    {
                        blockProcessedTaskCompletionSource.SetException(new BlockchainException(blockProcessingThrewException, e.Exception));
                        return;
                    }

                    ValidationResult validationResult = e.ProcessingResult switch
                    {
                        ProcessingResult.Success => ValidationResult.Valid,
                        ProcessingResult.QueueException => ValidationResult.Syncing,
                        ProcessingResult.MissingBlock => ValidationResult.Syncing,
                        ProcessingResult.ProcessingError => ValidationResult.Invalid,
                        _ => ValidationResult.Syncing
                    };

                    validationMessage = e.ProcessingResult switch
                    {
                        ProcessingResult.QueueException => "Block cannot be added to processing queue.",
                        ProcessingResult.MissingBlock => "Block wasn't found in tree.",
                        ProcessingResult.Exception => blockProcessingThrewException,
                        ProcessingResult.ProcessingError => "Block processing failed.",
                        _ => null
                    };

                    blockProcessedTaskCompletionSource.TrySetResult(validationResult);
                }
            }

            _processingQueue.BlockRemoved += GetProcessingQueueOnBlockRemoved;
            try
            {
                Task timeoutTask = Task.Delay(_timeout);
                AddBlockResult addResult = await _blockTree.SuggestBlockAsync(block, BlockTreeSuggestOptions.ForceDontSetAsMain)
                    .AsTask().TimeoutOn(timeoutTask);

                result = addResult switch
                {
                    AddBlockResult.InvalidBlock => ValidationResult.Invalid,
                    AddBlockResult.AlreadyKnown => ValidationResult.AlreadyKnown,
                    _ => null
                };

                validationMessage = addResult switch
                {
                    AddBlockResult.InvalidBlock => "Block couldn't be added to the tree.",
                    AddBlockResult.AlreadyKnown => "Block was already known in the tree.",
                    _ => null
                };

                // the right condition in the OR feels hacky, if the block is already known
                // then we should either it was processed with valid or invalid result or it hasn't
                // been processed. the problem is that right now BlockTree.WasProcessed will only be
                // true if it was processed with a valid result, so if we check this we would be unable
                // to distinguish the block not being processed yet from the block being invalid during processing.
                // one possible way to fix this is by adding a Contains method to the processing queue and if the block wasn't
                // processed then we check the processing queue to see whether it was invalid during processing or if it's still pending in the queue.
                if (!result.HasValue || (result & ValidationResult.AlreadyKnown) != 0)
                {
                    _processingQueue.Enqueue(block, processingOptions);

                    result = await blockProcessed.TimeoutOn(timeoutTask);
                }

                if ((result & ValidationResult.Valid) == 0 && (result & ValidationResult.Syncing) == 0)
                {
                    if (_logger.IsWarn) _logger.Warn($"Block {block.ToString(Block.Format.FullHashAndNumber)} cannot be processed and wont be accepted to the tree.");
                }
            }
            catch (TimeoutException)
            {
                if (_logger.IsDebug) _logger.Debug($"Block {block.ToString(Block.Format.FullHashAndNumber)} timed out when processing. Assume Syncing.");
                result = ValidationResult.Syncing;
            }
            finally
            {
                _processingQueue.BlockRemoved -= GetProcessingQueueOnBlockRemoved;
            }

            _latestBlocks?.Set(block.Hash!, validAndProcessed);
            return (result.Value, validationMessage);
        }

        private bool ValidateWithBlockValidator(Block block, BlockHeader parent)
        {
            block.Header.TotalDifficulty ??= parent.TotalDifficulty + block.Difficulty;
            block.Header.IsPostMerge = true; // I think we don't need to set it again here.
            bool isValid = _blockValidator.ValidateSuggestedBlock(block);
            if (!isValid && _logger.IsWarn) _logger.Warn($"Block validator rejected the block {block.ToString(Block.Format.FullHashAndNumber)}.");
            return isValid;
        }

        private PayloadStatusV1 BuildInvalidPayloadStatusV1(ExecutionPayloadV1 request, string? validationMessage) =>
            new()
            {
                Status = PayloadStatus.Invalid,
                ValidationError = validationMessage,
                LatestValidHash = _invalidChainTracker.IsOnKnownInvalidChain(request.BlockHash!, out Keccak? lastValidHash)
                    ? lastValidHash
                    : request.ParentHash
            };

        /// Pop blocks from cache up to ancestor on the beacon chain. Which is then inserted into the block tree
        /// which I assume will switch the canonical chain.
        /// Return false if no ancestor that is part of beacon chain found.
        private bool TryInsertDanglingBlock(Block block)
        {
            BlockTreeInsertHeaderOptions insertHeaderOptions = BlockTreeInsertHeaderOptions.BeaconBlockInsert | BlockTreeInsertHeaderOptions.MoveToBeaconMainChain;

            if (!_blockTree.IsKnownBeaconBlock(block.Number, block.Hash ?? block.CalculateHash()))
            {
                // last block inserted is parent of current block, part of the same chain
                Block? current = block;
                Stack<Block> stack = new();
                while (current != null)
                {
                    stack.Push(current);
                    Keccak currentHash = current.Hash!;
                    if (currentHash == _beaconPivot.PivotHash || _blockTree.IsKnownBeaconBlock(current.Number, currentHash))
                    {
                        break;
                    }

                    _blockCacheService.BlockCache.TryGetValue(current.ParentHash!, out Block? parentBlock);
                    current = parentBlock;
                }

                if (current == null)
                {
                    // block not part of beacon pivot chain, save in cache
                    _blockCacheService.BlockCache.TryAdd(block.Hash!, block);
                    return false;
                }

                while (stack.TryPop(out Block? child))
                {
                    _blockTree.Insert(child, BlockTreeInsertBlockOptions.SaveHeader, insertHeaderOptions);
                }

                _beaconPivot.ProcessDestination = block.Header;
            }

            return true;
        }

        [Flags]
        private enum ValidationResult
        {
            Invalid = 0,
            Valid = 1,
            AlreadyKnown = 2,
            Syncing = 4
        }
    }
}
