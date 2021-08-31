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

using System;
using FluentAssertions;
using FluentAssertions.Json;
using Nethermind.Blockchain.Contracts.Json;
using Nethermind.Consensus.AuRa.Contracts;
using Nethermind.Core;
using Nethermind.Int256;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Nethermind.Abi.Test.Json
{
    public class AbiDefinitionParserTests
    {
        [TestCase(typeof(BlockGasLimitContract))]
        [TestCase(typeof(RandomContract))]
        [TestCase(typeof(RewardContract))]
        [TestCase(typeof(ReportingValidatorContract))]
        [TestCase(typeof(ValidatorContract))]
        public void Can_load_contract(Type contractType)
        {
            var parser = new AbiDefinitionParser();
            var json = parser.LoadContract(contractType);
            var contract = parser.Parse(json);
            var serialized = parser.Serialize(contract);
            JToken.Parse(serialized).Should().ContainSubtree(json);
        }

        [Test]
        public void Can_load_custom_tuple()
        {
            var parser = new AbiDefinitionParser();
            var json = "[{\n      \"inputs\": [\n        {\n          \"components\": [\n            {\n              \"internalType\": \"address\",\n              \"name\": \"target\",\n              \"type\": \"address\"\n            },\n            {\n              \"internalType\": \"uint256\",\n              \"name\": \"nonce\",\n              \"type\": \"uint256\"\n            },\n            {\n              \"internalType\": \"bytes\",\n              \"name\": \"initCode\",\n              \"type\": \"bytes\"\n            },\n            {\n              \"internalType\": \"bytes\",\n              \"name\": \"callData\",\n              \"type\": \"bytes\"\n            },\n            {\n              \"internalType\": \"uint256\",\n              \"name\": \"callGas\",\n              \"type\": \"uint256\"\n            },\n            {\n              \"internalType\": \"uint256\",\n              \"name\": \"verificationGas\",\n              \"type\": \"uint256\"\n            },\n            {\n              \"internalType\": \"uint256\",\n              \"name\": \"maxFeePerGas\",\n              \"type\": \"uint256\"\n            },\n            {\n              \"internalType\": \"uint256\",\n              \"name\": \"maxPriorityFeePerGas\",\n              \"type\": \"uint256\"\n            },\n            {\n              \"internalType\": \"address\",\n              \"name\": \"paymaster\",\n              \"type\": \"address\"\n            },\n            {\n              \"internalType\": \"bytes\",\n              \"name\": \"paymasterData\",\n              \"type\": \"bytes\"\n            },\n            {\n              \"internalType\": \"bytes\",\n              \"name\": \"signature\",\n              \"type\": \"bytes\"\n            }\n          ],\n          \"internalType\": \"struct UserOperation\",\n          \"name\": \"userOp\",\n          \"type\": \"tuple\"\n        }\n      ],\n      \"name\": \"simulateWalletValidation\",\n      \"outputs\": [\n        {\n          \"internalType\": \"uint256\",\n          \"name\": \"gasUsedByPayForSelfOp\",\n          \"type\": \"uint256\"\n        }\n      ],\n      \"stateMutability\": \"nonpayable\",\n      \"type\": \"function\"\n    }]";
            parser.RegisterAbiTypeFactory(new AbiTuple<UserOperationAbi>());
            var contract = parser.Parse(json);
        }
        
        private struct UserOperationAbi
        {
            public Address Target { get; set; }
            public UInt256 Nonce { get; set; }
            public byte[] InitCode { get; set; }
            public byte[] CallData { get; set; }
            public UInt256 CallGas { get; set; }
            public UInt256 VerificationGas { get; set; }
            public UInt256 MaxFeePerGas { get; set; }
            public UInt256 MaxPriorityFeePerGas { get; set; }
            public Address Paymaster { get; set; }
            public byte[] PaymasterData { get; set; }
            public byte[] Signature { get; set; }
        }
    }
}
