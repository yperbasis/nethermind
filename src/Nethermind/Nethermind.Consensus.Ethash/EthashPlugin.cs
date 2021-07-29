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

using System.Threading.Tasks;
using Nethermind.Api;
using Nethermind.Api.Extensions;
using Nethermind.Blockchain.Rewards;
using Nethermind.Consensus.Transactions;
using Nethermind.Core;

namespace Nethermind.Consensus.Ethash
{
    public class EthashPlugin : IConsensusPlugin
    {
        private INethermindApi _api;
        private IMiningConfig _miningConfig;
        private IDifficultyCalculator _difficultyCalculator;

        public ValueTask DisposeAsync() { return ValueTask.CompletedTask; }

        public string Name => "Ethash";

        public string Description => "Ethash Consensus";

        public string Author => "Nethermind"; 
        
        public Task Init(INethermindApi nethermindApi)
        {
            _api = nethermindApi;
            if (_api!.SealEngineType != Nethermind.Core.SealEngineType.Ethash)
            {
                return Task.CompletedTask;
            }
            
            var (getFromApi, setInApi) = _api.ForInit;
            setInApi.RewardCalculatorSource = new RewardCalculator(getFromApi.SpecProvider);
            
            _difficultyCalculator = new EthashDifficultyCalculator(getFromApi.SpecProvider, getFromApi.ChainSpec.Ethash.MinimumDifficulty);
            Ethash ethash = new(getFromApi.LogManager);

            _miningConfig = getFromApi.Config<IMiningConfig>();
            setInApi.Sealer = _miningConfig.Enabled
                ? new EthashSealer(ethash, getFromApi.EngineSigner, getFromApi.LogManager)
                : NullSealEngine.Instance;
            setInApi.SealValidator = new EthashSealValidator(
                getFromApi.LogManager, _difficultyCalculator, getFromApi.CryptoRandom, ethash);

            return Task.CompletedTask;
        }
        
        public Task<IBlockProducer> InitBlockProducer(IBlockProductionTrigger? blockProductionTrigger = null, ITxSource? additionalTxSource = null)
        {
            BlockProducerEnv producerEnv = _api.BlockProducerEnvFactory.Create(txSource);
            IBlockProducer minedBlockProducer = new MinedBlockProducer(
                producerEnv.TxSource, 
                producerEnv.ChainProcessor, 
                _api.Sealer, 
                _api.BlockTree,
                _api.BlockProcessingQueue, 
                producerEnv.ReadOnlyStateProvider,
                new TargetAdjustedGasLimitCalculator(_api.SpecProvider, _miningConfig),
                _api.Timestamper,
                _api.SpecProvider,
                _api.LogManager,
                _difficultyCalculator);
            _api.BlockProducer = minedBlockProducer;
            return Task.FromResult(minedBlockProducer);
        }

        public Task InitNetworkProtocol()
        {
            return Task.CompletedTask;
        }

        public Task InitRpcModules()
        {
            return Task.CompletedTask;
        }
        
        public string SealEngineType => Nethermind.Core.SealEngineType.Ethash;

        public IBlockProductionTrigger DefaultBlockProductionTrigger => _nethermindApi.ManualBlockProductionTrigger;
    }
}
