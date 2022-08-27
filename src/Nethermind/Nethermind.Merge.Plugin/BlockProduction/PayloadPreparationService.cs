﻿//  Copyright (c) 2021 Demerzel Solutions Limited
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nethermind.Consensus;
using Nethermind.Consensus.Producers;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Core.Extensions;
using Nethermind.Core.Timers;
using Nethermind.Logging;
using Nethermind.Merge.Plugin.Handlers;
using Nethermind.Merge.Plugin.Handlers.V1;

namespace Nethermind.Merge.Plugin.BlockProduction
{
    /// <summary>
    /// A cache of pending payloads. A payload is created whenever a consensus client requests a payload creation in <see cref="ForkchoiceUpdatedV1Handler"/>.
    /// <seealso cref="https://github.com/ethereum/execution-apis/blob/main/src/engine/specification.md#engine_forkchoiceupdatedv1"/>
    /// Each payload is assigned a payloadId which can be used by the consensus client to retrieve payload later by calling a <see cref="GetPayloadV1Handler"/>.
    /// <seealso cref="https://github.com/ethereum/execution-apis/blob/main/src/engine/specification.md#engine_getpayloadv1"/>
    /// </summary>
    public class PayloadPreparationService : IPayloadPreparationService
    {
        private readonly PostMergeBlockProducer _blockProducer;
        private readonly IBlockImprovementContextFactory _blockImprovementContextFactory;
        private readonly ILogger _logger;
        private readonly List<string> _payloadsToRemove = new();

        // by default we will cleanup the old payload once per six slot. There is no need to fire it more often
        public const int SlotsPerOldPayloadCleanup = 6;
        public const int GetPayloadWaitForFullBlockMillisecondsDelay = 500;
        public const int DefaultImprovementDelayMs = 3000;
        public const int DefaultMinTimeForProductionMs = 500;

        /// <summary>
        /// Delay between block improvements
        /// </summary>
        private readonly TimeSpan _improvementDelay;

        /// <summary>
        /// Minimal time to try to improve block
        /// </summary>
        private readonly TimeSpan _minTimeForProduction;

        private readonly TimeSpan _cleanupOldPayloadDelay;
        private readonly TimeSpan _timePerSlot;

        // first ExecutionPayloadV1 is empty (without txs), second one is the ideal one
        private readonly ConcurrentDictionary<string, IBlockImprovementContext> _payloadStorage = new();

        public PayloadPreparationService(
            PostMergeBlockProducer blockProducer,
            IBlockImprovementContextFactory blockImprovementContextFactory,
            ITimerFactory timerFactory,
            ILogManager logManager,
            TimeSpan timePerSlot,
            int slotsPerOldPayloadCleanup = SlotsPerOldPayloadCleanup,
            int improvementDelay = DefaultImprovementDelayMs,
            int minTimeForProduction = DefaultMinTimeForProductionMs)
        {
            _blockProducer = blockProducer;
            _blockImprovementContextFactory = blockImprovementContextFactory;
            _timePerSlot = timePerSlot;
            TimeSpan timeout = timePerSlot;
            _cleanupOldPayloadDelay = 3 * timePerSlot; // 3 * slots time
            _improvementDelay = TimeSpan.FromMilliseconds(improvementDelay);
            _minTimeForProduction = TimeSpan.FromMilliseconds(minTimeForProduction);
            ITimer timer = timerFactory.CreateTimer(slotsPerOldPayloadCleanup * timeout);
            timer.Elapsed += CleanupOldPayloads;
            timer.Start();

            _logger = logManager.GetClassLogger();
        }

        public string StartPreparingPayload(BlockHeader parentHeader, PayloadAttributes payloadAttributes)
        {
            string payloadId = ComputeNextPayloadId(parentHeader, payloadAttributes);
            if (!_payloadStorage.ContainsKey(payloadId))
            {
                Block emptyBlock = ProduceEmptyBlock(payloadId, parentHeader, payloadAttributes);
                ImproveBlock(payloadId, parentHeader, payloadAttributes, emptyBlock, DateTime.Now);
            }
            else if (_logger.IsInfo) _logger.Info($"Payload with the same parameters has already started. PayloadId: {payloadId}");

            return payloadId;
        }

        private Block ProduceEmptyBlock(string payloadId, BlockHeader parentHeader, PayloadAttributes payloadAttributes)
        {
            if (_logger.IsTrace) _logger.Trace($"Preparing empty block from payload {payloadId} with parent {parentHeader}");
            Block emptyBlock = _blockProducer.PrepareEmptyBlock(parentHeader, payloadAttributes);
            if (_logger.IsTrace) _logger.Trace($"Prepared empty block from payload {payloadId} block: {emptyBlock}");
            return emptyBlock;
        }

        private void ImproveBlock(string payloadId, BlockHeader parentHeader, PayloadAttributes payloadAttributes, Block currentBestBlock, DateTime startDateTime)
        {
            IBlockImprovementContext? oldContext = null;

            _payloadStorage.AddOrUpdate(payloadId,
                id => CreateBlockImprovementContext(id, parentHeader, payloadAttributes, currentBestBlock, startDateTime),
                (id, currentContext) =>
                {
                    if (!currentContext.ImprovementTask.IsCompleted)
                    {
                        return currentContext;
                    }

                    oldContext = currentContext;
                    return CreateBlockImprovementContext(id, parentHeader, payloadAttributes, currentBestBlock, startDateTime);
                });

            oldContext?.Dispose();
        }

        private IBlockImprovementContext CreateBlockImprovementContext(string payloadId, BlockHeader parentHeader, PayloadAttributes payloadAttributes, Block currentBestBlock, DateTime startDateTime)
        {
            if (_logger.IsTrace) _logger.Trace($"Start improving block from payload {payloadId} with parent {parentHeader}");
            IBlockImprovementContext blockImprovementContext = _blockImprovementContextFactory.StartBlockImprovementContext(currentBestBlock, parentHeader, payloadAttributes, startDateTime);
            blockImprovementContext.ImprovementTask.ContinueWith(LogProductionResult);
            blockImprovementContext.ImprovementTask.ContinueWith(async _ =>
            {
                // if after delay we still have time to try producing the block in this slot
                if (DateTime.Now + _improvementDelay + _minTimeForProduction < startDateTime + _timePerSlot)
                {
                    await Task.Delay(_improvementDelay);
                    if (!blockImprovementContext.Disposed) // if GetPayload wasn't called for this item or it wasn't cleared
                    {
                        Block newBestBlock = blockImprovementContext.CurrentBestBlock ?? currentBestBlock;
                        ImproveBlock(payloadId, parentHeader, payloadAttributes, newBestBlock, startDateTime);
                    }
                }
            });

            return blockImprovementContext;
        }

        private void CleanupOldPayloads(object? sender, EventArgs e)
        {
            if (_logger.IsTrace) _logger.Trace("Started old payloads cleanup");
            foreach (KeyValuePair<string, IBlockImprovementContext> payload in _payloadStorage)
            {
                DateTime dateTime = DateTime.Now;
                if (payload.Value.StartDateTime + _cleanupOldPayloadDelay <= dateTime)
                {
                    if (_logger.IsDebug) _logger.Info($"A new payload to remove: {payload.Key}, Current time {dateTime:t}, Payload timestamp: {payload.Value.CurrentBestBlock?.Timestamp}");
                    _payloadsToRemove.Add(payload.Key);
                }
            }

            foreach (string payloadToRemove in _payloadsToRemove)
            {
                if (_payloadStorage.TryRemove(payloadToRemove, out IBlockImprovementContext? context))
                {
                    context.Dispose();
                    if (_logger.IsDebug) _logger.Info($"Cleaned up payload with id={payloadToRemove} as it was not requested");
                }
            }

            _payloadsToRemove.Clear();
            if (_logger.IsTrace) _logger.Trace($"Finished old payloads cleanup");
        }

        private Block? LogProductionResult(Task<Block?> t)
        {
            if (t.IsCompletedSuccessfully)
            {
                if (t.Result != null)
                {
                    BlockImproved?.Invoke(this, new BlockEventArgs(t.Result));
                    if (_logger.IsInfo) _logger.Info($"Improved post-merge block {t.Result.ToString(Block.Format.HashNumberDiffAndTx)}");
                }
                else
                {
                    if (_logger.IsInfo) _logger.Info("Failed to improve post-merge block");
                }
            }
            else if (t.IsFaulted)
            {
                if (_logger.IsError) _logger.Error("Post merge block improvement failed", t.Exception);
            }
            else if (t.IsCanceled)
            {
                if (_logger.IsInfo) _logger.Info($"Post-merge block improvement was canceled");
            }

            return t.Result;
        }

        public async ValueTask<Block?> GetPayload(string payloadId)
        {
            if (_payloadStorage.TryGetValue(payloadId, out IBlockImprovementContext? blockContext))
            {
                using (blockContext)
                {
                    if (!blockContext.ImprovementTask.IsCompleted && blockContext.CurrentBestBlock?.Transactions.Any() != true)
                    {
                        await Task.WhenAny(blockContext.ImprovementTask, Task.Delay(GetPayloadWaitForFullBlockMillisecondsDelay));
                    }

                    return blockContext.CurrentBestBlock;
                }
            }

            return null;
        }

        public event EventHandler<BlockEventArgs>? BlockImproved;

        private string ComputeNextPayloadId(BlockHeader parentHeader, PayloadAttributes payloadAttributes)
        {
            Span<byte> inputSpan = stackalloc byte[32 + 32 + 32 + 20];
            parentHeader.Hash!.Bytes.CopyTo(inputSpan.Slice(0, 32));
            payloadAttributes.Timestamp.ToBigEndian(inputSpan.Slice(32, 32));
            payloadAttributes.PrevRandao.Bytes.CopyTo(inputSpan.Slice(64, 32));
            payloadAttributes.SuggestedFeeRecipient.Bytes.CopyTo(inputSpan.Slice(96, 20));
            ValueKeccak inputHash = ValueKeccak.Compute(inputSpan);
            return inputHash.BytesAsSpan.Slice(0, 8).ToHexString(true);
        }
    }
}
