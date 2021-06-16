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
using Nethermind.Int256;

namespace CryptoCompareStreams.Contracts
{
    public class UniswapV2Factory : BlockchainBridgeContract
    {
        public readonly Address ContractAddress;
        private readonly IBlockTree _blockTree;
        private readonly IConstantContract _contract;
        
        public UniswapV2Factory(Address contractAddress, AbiDefinition abi, IBlockchainBridge blockchainBridge, IBlockTree blockTree) : base(contractAddress, abi) 
        {
            _contract = GetConstant(blockchainBridge);
            ContractAddress = contractAddress;
            _blockTree = blockTree;
        }

        public Address allPairs(UInt256 index)
        {
            var header = _blockTree.Head?.Header;
            
            return header is null 
                ? null
                : _contract.Call<Address>(header, ContractAddress, nameof(allPairs), Address.Zero, index);
        }

        public UInt256 allPairsLength()
        {
            var header = _blockTree.Head?.Header;
            
            return header is null 
                ? UInt256.Zero 
                : _contract.Call<UInt256>(header, ContractAddress, nameof(allPairsLength), Address.Zero);
        }
    }
}