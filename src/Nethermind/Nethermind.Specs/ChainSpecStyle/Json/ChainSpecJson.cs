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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nethermind.Core;
using Nethermind.Int256;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nethermind.Specs.ChainSpecStyle.Json
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal class ChainSpecJson
    {
        public string Name { get; set; }
        public string DataDir { get; set; }
        public EngineJson Engine { get; set; }
        public ChainSpecParamsJson Params { get; set; }
        public ChainSpecGenesisJson Genesis { get; set; }
        public string[] Nodes { get; set; }
        public Dictionary<string, AllocationJson> Accounts { get; set; }

        internal class EthashEngineJson
        {
            public long? HomesteadTransition => Params?.HomesteadTransition;
            public long? DaoHardforkTransition => Params?.DaoHardforkTransition;
            public Address DaoHardforkBeneficiary => Params?.DaoHardforkBeneficiary;
            public Address[] DaoHardforkAccounts => Params?.DaoHardforkAccounts;
            public long? Eip100bTransition => Params?.Eip100bTransition;
            public long? FixedDifficulty => Params?.FixedDifficulty;
            public long? DifficultyBoundDivisor => Params?.DifficultyBoundDivisor;
            public long? DurationLimit => Params?.DurationLimit;
            public UInt256? MinimumDifficulty => Params?.MinimumDifficulty;
            public IDictionary<long, UInt256> BlockReward => Params?.BlockReward;
            public IDictionary<string, long> DifficultyBombDelays => Params?.DifficultyBombDelays;
            public EthashEngineParamsJson Params { get; set; }
        }

        internal class EthashEngineParamsJson
        {
            public UInt256? MinimumDifficulty { get; set; }
            public long? DifficultyBoundDivisor { get; set; }
            public long? DurationLimit { get; set; }
            public long HomesteadTransition { get; set; }
            public long? DaoHardforkTransition { get; set; }
            public Address DaoHardforkBeneficiary { get; set; }
            public Address[] DaoHardforkAccounts { get; set; }
            public long Eip100bTransition { get; set; }
            public long? FixedDifficulty { get; set; }
            public BlockRewardJson BlockReward { get; set; }
            public Dictionary<string, long> DifficultyBombDelays { get; set; }
        }

        internal class CliqueEngineJson
        {
            public ulong Period => Params.Period;
            public ulong Epoch => Params.Epoch;
            public UInt256? BlockReward => Params.BlockReward;
            public CliqueEngineParamsJson Params { get; set; }
        }

        internal class CliqueEngineParamsJson
        {
            public ulong Period { get; set; }
            public ulong Epoch { get; set; }
            public UInt256? BlockReward { get; set; }
        }

        internal class AuraEngineParamsJson
        {
            public StepDurationJson StepDuration { get; set; }
            public BlockRewardJson BlockReward { get; set; }
            public long MaximumUncleCountTransition { get; set; }
            public long? MaximumUncleCount { get; set; }
            public Address BlockRewardContractAddress { get; set; }
            public long? BlockRewardContractTransition { get; set; }
            public IDictionary<long, Address> BlockRewardContractTransitions { get; set; } = new Dictionary<long, Address>();
            public long ValidateScoreTransition { get; set; }
            public long ValidateStepTransition { get; set; }
            public AuRaValidatorJson Validators { get; set; }
            public IDictionary<long, Address> RandomnessContractAddress { get; set; } = new Dictionary<long, Address>();
            public IDictionary<long, Address> BlockGasLimitContractTransitions { get; set; } = new Dictionary<long, Address>();
            public long? TwoThirdsMajorityTransition { get; set; }
            public long? PosdaoTransition { get; set; }
            public IDictionary<long, IDictionary<Address, byte[]>> RewriteBytecode { get; set; } = new Dictionary<long, IDictionary<Address, byte[]>>();

            public class StepDurationJson : SortedDictionary<long, long> { }
        }

        public class BlockRewardJson : SortedDictionary<long, UInt256> { }

        internal class AuRaValidatorJson
        {
            public Address[] List { get; set; }
            public Address Contract { get; set; }
            public Address SafeContract { get; set; }
            public Dictionary<long, AuRaValidatorJson> Multi { get; set; }

            public AuRaParameters.ValidatorType GetValidatorType()
            {
                if (List != null)
                {
                    return AuRaParameters.ValidatorType.List;
                }
                else if (Contract != null)
                {
                    return AuRaParameters.ValidatorType.ReportingContract;
                }
                else if (SafeContract != null)
                {
                    return AuRaParameters.ValidatorType.Contract;
                }
                else if (Multi != null)
                {
                    return AuRaParameters.ValidatorType.Multi;
                }
                else
                {
                    throw new NotSupportedException("AuRa validator type not supported.");
                }
            }
        }

        internal class AuraEngineJson
        {
            public IDictionary<long, long> StepDuration => Params.StepDuration;

            public IDictionary<long, UInt256> BlockReward => Params.BlockReward;

            public long MaximumUncleCountTransition => Params.MaximumUncleCountTransition;

            public long? MaximumUncleCount => Params.MaximumUncleCount;

            public Address BlockRewardContractAddress => Params.BlockRewardContractAddress;

            public long? BlockRewardContractTransition => Params.BlockRewardContractTransition;

            public IDictionary<long, Address> BlockRewardContractTransitions => Params.BlockRewardContractTransitions;

            public long ValidateScoreTransition => Params.ValidateScoreTransition;

            public long ValidateStepTransition => Params.ValidateStepTransition;

            public long? PosdaoTransition => Params.PosdaoTransition;

            public long? TwoThirdsMajorityTransition => Params.TwoThirdsMajorityTransition;

            public AuRaValidatorJson Validator => Params.Validators;

            public IDictionary<long, Address> RandomnessContractAddress => Params.RandomnessContractAddress;

            public IDictionary<long, Address> BlockGasLimitContractTransitions => Params.BlockGasLimitContractTransitions;

            public IDictionary<long, IDictionary<Address, byte[]>> RewriteBytecode => Params.RewriteBytecode;

            public AuraEngineParamsJson Params { get; set; }
        }

        internal class NethDevJson
        {
        }

        internal class EngineJson
        {
            public EthashEngineJson Ethash { get; set; }
            public CliqueEngineJson Clique { get; set; }
            public AuraEngineJson AuthorityRound { get; set; }
            public NethDevJson NethDev { get; set; }

            [JsonExtensionData]
            public IDictionary<string, JToken> CustomEngineData { get; set; }
        }
    }
}
