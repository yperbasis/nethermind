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
using Nethermind.Abi;
using Nethermind.Blockchain;
using Nethermind.Blockchain.Find;
using Nethermind.Core;
using Nethermind.Facade;

namespace CryptoCompareStreams.Contracts
{
    public class UniswapPair : BlockchainBridgeContract
    {
        public readonly Address ContractAddress;
        private readonly IConstantContract _contract;
        private readonly IBlockchainBridge _blockchainBridge;
        private readonly IBlockTree _blockTree;
        
        public UniswapPair(Address contractAddress, IBlockchainBridge blockchainBridge, IBlockTree blockTree) : base(contractAddress)
        {
            ContractAddress = contractAddress ?? throw new ArgumentNullException(nameof(contractAddress));
            _blockchainBridge = blockchainBridge ?? throw new ArgumentNullException(nameof(blockchainBridge));
            _contract = GetConstant(_blockchainBridge);
            _blockTree = blockTree ?? throw new ArgumentNullException(nameof(blockTree));
        }

        public Address token0()
        {
            var header = _blockTree.Head?.Header;

            return header is null
                ? null
                : _contract.Call<Address>(header, nameof(token0), Address.Zero);
        }
        
        public Address token1()
        {
            var header = _blockTree.Head?.Header;

            return header is null
                ? null
                : _contract.Call<Address>(header, nameof(token1), Address.Zero);
        }
    }
}