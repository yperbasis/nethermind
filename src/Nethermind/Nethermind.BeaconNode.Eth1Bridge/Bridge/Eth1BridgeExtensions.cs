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
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Nethermind.Core2;
using Nethermind.Core2.Containers;
using Nethermind.Core2.Crypto;
using Nethermind.Core2.Types;

namespace Nethermind.BeaconNode.Eth1Bridge.Bridge
{
    public static class Eth1BridgeExtensions
    {
        // `Keccak("DepositEvent(bytes,bytes,bytes,bytes,bytes)")`
        private const string DepositEventTopic = "0x649bbc62d0e31342afea4e5cd82d4049e7e1ee912fc0889aa790803be39038c5";

        private static readonly string[] DepositEventTopics = {DepositEventTopic}; 
        
        public static Task<Log[]> GetDepositLogsAsync(this IEth1Bridge bridge, ulong fromBlock, ulong toBlock, string address) =>
            bridge.eth_getLogs(new Filter()
            {
                Address = address,
                FromBlock = Block.ToHexString(fromBlock),
                ToBlock = Block.ToHexString(toBlock),
                Topics = DepositEventTopics
            });

        public static async Task<IEnumerable<IGrouping<ulong, DepositLog>>> GetDepositLogsByBlockAsync(this IEth1Bridge bridge, ulong fromBlock, ulong toBlock, string address)
        {
            var logs = await bridge.GetDepositLogsAsync(fromBlock, toBlock, address);
            return logs
                .Where(l => !l.Removed)
                .Select(l => l.ToDepositLog())
                .OrderBy(l => l.Index)
                .GroupBy(l => l.BlockNumber)
                .OrderBy(g => g.Key);
        }

        public static async Task<Eth1Cache.BlockInfo> GetBlockInfoAsync(this IEth1Bridge bridge, ulong blockNumber, string address)
        {
            const int blockHashLength = 32;
            
            var blockTask = bridge.eth_getBlockByNumber(Block.ToHexString(blockNumber));
            var depositCountTask = bridge.GetDepositCountAsync(blockNumber, address);
            var depositRootTask = bridge.GetDepositRootAsync(blockNumber, address);

            var block = await blockTask;
            var bytes = Bytes.FromHexString(block.Hash);
            if (bytes.Length != blockHashLength)
            {
                throw new Eth1BridgeException($"Block hash was not {blockHashLength} bytes: {block.Hash}");
            }
            
            return new Eth1Cache.BlockInfo(
                block.Number,
                block.Timestamp,
                new Eth1Data(await depositRootTask, await depositCountTask, Bytes32.Wrap(bytes)));

        }

        public static Task<Block> GetBlockByNumberAsync(this IEth1Bridge bridge, ulong blockNumber) => 
            bridge.eth_getBlockByNumber(Block.ToHexString(blockNumber));

        public static async Task<ulong> GetDepositCountAsync(this IEth1Bridge bridge, ulong blockNumber, string address)
        {
            // `Keccak("get_deposit_count()")[0..4]`
            const string depositCountSignature = "0x621fd130";
            // Number of bytes in deposit contract deposit count response.
            const int depositCountResponseLength = 96;

            Transaction tx = new Transaction() {To = address, Data = depositCountSignature};
            var result = await bridge.eth_call(tx, Block.ToHexString(blockNumber));
            var bytes = Bytes.FromHexString(result);

            return bytes.Length == depositCountResponseLength 
                ? BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan().Slice(32 + 32, 8)) 
                : throw new Eth1BridgeException($"Deposit count response was not {depositCountResponseLength} bytes: {result}");
            
        }
        
        private static async Task<Root> GetDepositRootAsync(this IEth1Bridge bridge, ulong blockNumber, string address)
        {
            // `Keccak("get_deposit_root()")[0..4]`
            const string depositRootSignature = "0xc5f2892f";
            // Number of bytes in deposit contract deposit root (value only).
            const int depositRootLength = 32;

            Transaction tx = new Transaction() {To = address, Data = depositRootSignature};
            var result = await bridge.eth_call(tx, Block.ToHexString(blockNumber));
            var bytes = Bytes.FromHexString(result);
            
            return bytes.Length == depositRootLength
                ? Root.Wrap(bytes) 
                : throw new Eth1BridgeException($"Deposit root response was not {depositRootLength} bytes: {result}");
        }
            
        
    }
}
