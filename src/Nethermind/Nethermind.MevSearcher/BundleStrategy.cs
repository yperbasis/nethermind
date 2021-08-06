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
using Nethermind.Abi;
using Nethermind.Blockchain;
using Nethermind.Blockchain.Tracing;
using Nethermind.Consensus;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Core.Extensions;
using Nethermind.Core.Specs;
using Nethermind.Crypto;
using Nethermind.Evm.Tracing;
using Nethermind.Evm.Tracing.GethStyle;
using Nethermind.Int256;
using Nethermind.Logging;
using Nethermind.MevSearcher.Data;
using Nethermind.State;

namespace Nethermind.MevSearcher
{
    public class BundleStrategy : IBundleStrategy
    {
        private readonly IStateProvider _stateProvider;
        private readonly ISigner _signer;
        private readonly ITracer _tracer;
        private readonly IBlockTree _blockTree;
        private readonly ISpecProvider _specProvider;
        private readonly IEthereumEcdsa _ecdsa;

        public BundleStrategy(
            IStateProvider stateProvider, 
            ISigner signer, 
            ITracer tracer, 
            IBlockTree blockTree, 
            ISpecProvider specProvider,
            IEthereumEcdsa ecdsa)
        {
            _stateProvider = stateProvider;
            _signer = signer;
            _tracer = tracer;
            _blockTree = blockTree;
            _specProvider = specProvider;
            _ecdsa = ecdsa;
        }

        private Address Address => _signer.Address;
        
        public bool ProcessTransaction(Transaction transaction, out MevBundle bundle)
        {
            // create strategy here, return true and create new MevBundle object here if you want to send a bundle
            // return false if you do not wish to send a bundle after processing the new transaction
            // it might be useful to create or use a block tracer that conforms to IBlockTracer, and _tracer.Trace on
            // the tracer using the BuildSimulationBlock method to get information about how the transaction would perform

            PrivateKey privateKey =
                new PrivateKey("ENTER_PRIV_KEY");
            
            if (!transaction.IsSigned || transaction.Data is null || transaction.SenderAddress == privateKey.Address)
            {
                bundle = null;
                return false;
            }
            
            IReadOnlyDictionary<string, AbiType> rlp = new Dictionary<string, AbiType>
            {
                {"_calldata", AbiType.DynamicBytes},
                {"_address", AbiType.Address},
                {"_value", AbiType.UInt256},
            };
            
            AbiSignature abiSignature = new AbiSignature("execute", AbiType.DynamicBytes, AbiType.Address, AbiType.UInt256);

            IAbiEncoder abiEncoder = new AbiEncoder();
            byte[] computedCallData = abiEncoder.Encode(
                AbiEncodingStyle.None,
                abiSignature,
                transaction.Data, transaction.To!, transaction.Value);
            
            Address contractAddress = new Address("0x5381337337367d54a999B36Eb2dA0ECcE1B6bfC8");
            
            Transaction newTx = new()
            {
                GasPrice = transaction.GasPrice,
                GasLimit = transaction.GasLimit + 40000,
                To = contractAddress,
                ChainId = 100,
                Nonce = _stateProvider.GetNonce(privateKey.Address),
                Value = 0,
                Data = computedCallData
            };

            Signer signer = new Signer(100, privateKey, LimboLogs.Instance);
            signer.Sign(newTx);
            newTx.Hash = transaction.CalculateHash();

            bundle = new MevBundle(_blockTree.Head!.Number + 1, new []{newTx});
            return true;
        }
        
        private Block BuildSimulationBlock(Transaction[] transactions)
        {
            BlockHeader parent = _blockTree.Head!.Header;
            
            BlockHeader header = new(
                parent.Hash ?? Keccak.OfAnEmptySequenceRlp, 
                Keccak.OfAnEmptySequenceRlp, 
                Address.Zero, 
                parent.Difficulty,  
                parent.Number + 1, 
                parent.GasLimit, 
                parent.Timestamp, 
                Bytes.Empty)
            {
                TotalDifficulty = parent.TotalDifficulty + parent.Difficulty
            };

            header.BaseFeePerGas = BaseFeeCalculator.Calculate(parent, _specProvider.GetSpec(header.Number));

            return new Block(header, transactions, Array.Empty<BlockHeader>());
        }
    }
}
