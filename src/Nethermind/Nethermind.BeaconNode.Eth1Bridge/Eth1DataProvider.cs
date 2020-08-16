//  Copyright (c) 2018 Demerzel Solutions Limited
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
using System.Linq;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethermind.BeaconNode.Eth1Bridge.Bridge;
using Nethermind.Core2;
using Nethermind.Core2.Configuration;
using Nethermind.Core2.Containers;
using Nethermind.Core2.Types;
using Nethermind.HashLib;
using Nethermind.Logging.Microsoft;
using Nito.Collections;

namespace Nethermind.BeaconNode.Eth1Bridge
{
    public class Eth1DataProvider : IEth1DataProvider, IEth1ChainFollower
    {
        private const ulong CacheOversize = 110;
        private const ulong FutureCacheOversize = (CacheOversize - 100) / 2 + 100;
        
        private readonly ILogger<Eth1DataProvider> _logger;
        private readonly IEth1BridgeProvider _bridgeProvider;
        private readonly IOptionsMonitor<Eth1BridgeConfiguration> _configurationOptions;
        private readonly IOptionsMonitor<MiscellaneousParameters> _miscellaneousParameterOptions;
        private readonly HonestValidatorConstants _constants;
        private readonly Eth1Cache _cache;
        private Task _currentBlockBatch;

        public Eth1DataProvider(ILogger<Eth1DataProvider> logger,
            IEth1BridgeProvider bridgeProvider, 
            IOptionsMonitor<Eth1BridgeConfiguration> configurationOptions,
            IOptionsMonitor<MiscellaneousParameters> miscellaneousParameterOptions,
            HonestValidatorConstants constants)
        {
            _logger = logger;
            _bridgeProvider = bridgeProvider;
            _configurationOptions = configurationOptions;
            _miscellaneousParameterOptions = miscellaneousParameterOptions;
            _constants = constants;
            

            // we are caching a little bit more blocks as SecondsPerEth1Block is estimation
            _cache = new Eth1Cache(constants.Eth1FollowDistance * CacheOversize / 100ul);
        }
        
        public IAsyncEnumerable<Eth1Data> GetEth1DataDescendingAsync(ulong maximumTimestampInclusive, ulong minimumTimestampInclusive, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<Deposit> GetDepositsAsync(Bytes32 eth1BlockHash, ulong startIndex, ulong maximum, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        
        public async Task FollowChainAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    ulong cachedBlockNumber = _cache.Newest?.BlockNumber ?? 0;
                    ulong headBlockNumber = await _bridgeProvider.CallAsync(b => b.eth_blockNumber(), cancellationToken);
                    ulong followBlockNumber = GetCurrentFollow(headBlockNumber);

                    if (followBlockNumber > cachedBlockNumber)
                    {
                        ulong blocksToGet = followBlockNumber - cachedBlockNumber;

                        // we are so behind that its more than our cache, so we need to drop it
                        if (blocksToGet >= _cache.Capacity)
                        {
                            _cache.Clear();
                        }

                        blocksToGet = Math.Min(blocksToGet, _cache.Capacity);

                        _currentBlockBatch = GetBlocksAsync(followBlockNumber - blocksToGet, followBlockNumber, cancellationToken);
                        await _currentBlockBatch;
                        await Task.Delay(IEth1Bridge.CheckDelayMs, cancellationToken);
                    }
                }
                catch (Eth1BridgeException e)
                {
                    if (_logger.IsError()) Log.Eth1CommunicationFailure(_logger, e);
                    await Task.Delay(IEth1Bridge.FailDelayMs, cancellationToken);
                }
            }
        }

        private async Task GetBlocksAsync(ulong from, ulong to, CancellationToken cancellationToken)
        {
            string contractAddress = _configurationOptions.CurrentValue.DepositContractAddress;
            var depositLogsByBlock = await _bridgeProvider.CallAsync(
                b => b.GetDepositLogsByBlockAsync(from, to, contractAddress), cancellationToken);
            
            using var enumerator = depositLogsByBlock.GetEnumerator();
            bool hasValue = enumerator.MoveNext();
            
            for (ulong blockNumber = from; blockNumber < to; blockNumber++)
            {
                var block = await _bridgeProvider.CallAsync(b => b.GetBlockInfoAsync(blockNumber, contractAddress), cancellationToken);

                IReadOnlyCollection<Deposit> deposits;
                
                if (hasValue && enumerator.Current.Key == blockNumber)
                {
                    // deposits = enumerator.Current.Select(d => new Deposit(d.));
                    hasValue = enumerator.MoveNext();
                }
                // else
                {
                    deposits = Array.Empty<Deposit>();
                }
                
                _cache.AddBlock(block, deposits);
            }
        }

        private ulong GetCurrentFollow(in ulong currentHead) => Math.Max(0, currentHead - _constants.Eth1FollowDistance * 100ul / FutureCacheOversize);
    }

    public interface IEth1ChainFollower
    {
        Task FollowChainAsync(CancellationToken cancellationToken);
    }

    public class Eth1Cache
    {
        private Deque<BlockInfo> Blocks { get; }
        private Deque<DepositInfo> Deposits { get; }
        public BlockInfo? Newest => Blocks.Count == 0 ? null : Blocks[^1];
        public ulong Capacity { get; }

        public Eth1Cache(ulong capacity)
        {
            Capacity = capacity;
            Blocks = new Deque<BlockInfo>((int) capacity);
            Deposits = new Deque<DepositInfo>((int) capacity);
        }

        public void AddBlock(BlockInfo block, IReadOnlyCollection<Deposit> deposits)
        {
            // make room, we shouldn't need oldest block anymore
            if (Blocks.Count == Blocks.Capacity)
            {
                RemoveOldestBlock();
            }
            
            // eth1 reorg happened - we need to drop blocks
            while (Newest?.BlockNumber >= block.BlockNumber)
            {
                RemoveNewestBlock();
            }

            if (Newest != null && Newest.BlockNumber + 1 != block.BlockNumber)
            {
                throw new ArgumentException("Blocks are not consecutive.");
            }

            Blocks.AddToFront(block);
            ulong index = block.Eth1Data.DepositCount - (ulong) deposits.Count;
            foreach (Deposit deposit in deposits)
            {
                Deposits.AddToFront(new DepositInfo(block, index++, deposit));
            }
        }

        public void Clear()
        {
            Blocks.Clear();
            Deposits.Clear();
        }

        private void RemoveOldestBlock()
        {
            BlockInfo oldest = Blocks.RemoveFromBack();

            while (Deposits.Count > 0)
            {
                if (Deposits[0].Block == oldest)
                {
                    Deposits.RemoveFromBack();
                }
                else
                {
                    break;
                }
            }
        }

        private void RemoveNewestBlock()
        {
            BlockInfo newest = Blocks.RemoveFromFront();

            for (int i = Deposits.Count; i >= 0; i--)
            {
                if (Deposits[i].Block == newest)
                {
                    Deposits.RemoveFromFront();
                }
                else
                {
                    break;
                }
            }
        }

        private class DepositInfo
        {
            public BlockInfo Block { get; }
            public ulong Index { get; }
            public Deposit Deposit { get; }

            public DepositInfo(BlockInfo block, ulong index, Deposit deposit)
            {
                Block = block;
                Index = index;
                Deposit = deposit;
            }
        }

        public class BlockInfo
        {
            public ulong BlockNumber { get; }
            public ulong Timestamp { get; }
            public Eth1Data Eth1Data { get; }

            public BlockInfo(ulong blockNumber, ulong timestamp, Eth1Data eth1Data)
            {
                BlockNumber = blockNumber;
                Timestamp = timestamp;
                Eth1Data = eth1Data;
            }
        }
    }
}
