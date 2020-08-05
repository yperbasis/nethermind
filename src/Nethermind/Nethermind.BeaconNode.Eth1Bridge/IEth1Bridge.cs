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

using System.Threading.Tasks;

namespace Nethermind.BeaconNode.Eth1Bridge
{
    public interface IEth1Bridge
    {
        Task<ulong> eth_chainId();
        Task<ulong> eth_blockNumber();
        Task<Block> eth_getBlockByNumber(string block, bool includeTransaction = false);
        Task<Eth1Log[]> eth_getLogs(Filter filter);
        Task<string> eth_call(Transaction transaction, string block);
    }

    public struct Eth1Log
    {
        public bool Removed { get; set; }
        public ulong LogIndex { get; set; }
        public ulong TransactionIndex { get; set; }
        public byte[] TransactionHash { get; set; }
        public byte[] BlockHash { get; set; }
        public ulong BlockNumber { get; set; }
        public byte[] Address { get; set; }
        public byte[] Data { get; set; }
        public byte[][] Topics { get; set; }
    }
    
    public struct Filter
    {
        public string FromBlock { get; set; }
        public string ToBlock { get; set; }
        public byte[] Address { get; set; }
        public byte[][] Topics { get; set; }
    }

    public struct Transaction
    {
        public byte[] To { get; set; }
        public byte[] Data { get; set; }
    }

    public struct Block
    {
        public static string Latest = "latest";
        public static string ToHexString(ulong blockNumber) => blockNumber.ToString("X");

        public ulong Number { get; set; }
        public byte[] Hash { get; set; }
        public byte[] ParentHash { get; set; }
        public ulong Timestamp { get; set; }
    }
}
