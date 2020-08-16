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
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethermind.BeaconNode.Eth1Bridge.Bridge;
using Nethermind.BeaconNode.Eth1Bridge.JsonRpc;
using Nethermind.Core2;
using Nethermind.Core2.Configuration;
using Nethermind.Core2.Containers;
using Nethermind.Core2.Types;
using Nethermind.Logging.Microsoft;
using Nito.Collections;

namespace Nethermind.BeaconNode.Eth1Bridge
{
    public class Eth1GenesisProvider : IEth1GenesisProvider
    {
        private const int DepositDataHeadroom = 1024;
        
        private readonly ILogger<Eth1GenesisProvider> _logger;
        private readonly IEth1BridgeProvider _bridgeProvider;
        private readonly IOptionsMonitor<Eth1BridgeConfiguration> _configurationOptions;
        private readonly IOptionsMonitor<MiscellaneousParameters> _miscellaneousParameterOptions;
        private readonly HonestValidatorConstants _honestValidatorConstants;
        private readonly IDepositStore _depositStore;

        public Eth1GenesisProvider(ILogger<Eth1GenesisProvider> logger,
            IEth1BridgeProvider bridgeProvider, 
            IOptionsMonitor<Eth1BridgeConfiguration> configurationOptions,
            IOptionsMonitor<MiscellaneousParameters> miscellaneousParameterOptions,
            HonestValidatorConstants honestValidatorConstants,
            IDepositStore depositStore)
        {
            _logger = logger;
            _bridgeProvider = bridgeProvider;
            _configurationOptions = configurationOptions;
            _miscellaneousParameterOptions = miscellaneousParameterOptions;
            _honestValidatorConstants = honestValidatorConstants;
            _depositStore = depositStore;
        }

        public async IAsyncEnumerable<Eth1GenesisData> GetEth1GenesisCandidatesDataAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // start looking from deposits from next block after deposit contract was deployed
            ulong currentBlock = _configurationOptions.CurrentValue.DepositContractDeployBlock + 1;
            
            // assume current head of chain is 0
            ulong currentHead = 0;
            
            // current follow block = eth1 currentHead - 'ETH1_FOLLOW_DISTANCE'
            ulong currentFollow = 0;
            
            // this will get canceled when:
            // 1. Genesis was found
            // 2. Worker is cancelled (app is closed)
            while (!cancellationToken.IsCancellationRequested)
            {
                Eth1GenesisData? genesisCandidate = null;
                try
                {
                    // if we are close to head - lets update head, if not we can skip call
                    if (currentFollow < currentBlock + _configurationOptions.CurrentValue.MaxLogsBatch)
                    {
                        currentHead = await _bridgeProvider.CallAsync(b => b.eth_blockNumber(), cancellationToken);
                        currentFollow = GetCurrentFollow(currentHead);
                    }

                    // We have nothing to check
                    // We could be before deployment of deposit contract on first iteration or we just processed up to head
                    if (currentFollow < currentBlock)
                    {
                        await Task.Delay(IEth1Bridge.CheckDelayMs, cancellationToken);
                        if (_logger.IsInfo()) Log.Eth1GenesisWaitingForDeposits(_logger, _depositStore.Deposits.Count, null);
                    }
                    else // We are behind head and contract was deployed lets download some logs and construct genesis candidates
                    {
                        ulong batchLength = Math.Min(currentFollow - currentBlock, _configurationOptions.CurrentValue.MaxLogsBatch - 1);
                        ulong toBlock = currentBlock + batchLength;

                        if (_logger.IsInfo()) Log.Eth1GenesisImportingBlocks(_logger, currentBlock, toBlock, null);

                        var contractAddress = _configurationOptions.CurrentValue.DepositContractAddress;
                        IEnumerable<IGrouping<ulong, DepositLog>>? logsByBlocks = await _bridgeProvider.CallAsync(
                            b => b.GetDepositLogsByBlockAsync(currentBlock, toBlock, contractAddress)
                            , cancellationToken);

                        if (logsByBlocks != null)
                        {
                            foreach (var logs in logsByBlocks)
                            {
                                currentBlock = logs.Key;
                                Block block = await _bridgeProvider.CallAsync(b => b.GetBlockByNumberAsync(logs.Key), cancellationToken);
                                _depositStore.Place(logs);
                                if (_logger.IsDebug()) LogDebug.Eth1GenesisCandidate(_logger, block.Number, block.Timestamp, _depositStore.Deposits.Count, null);
                                genesisCandidate = new Eth1GenesisData(new Bytes32(block.Hash), block.Timestamp, _depositStore.Deposits, _depositStore.Root);
                            }
                        }
                    }
                }
                catch (Eth1BridgeException e)
                {
                    if (_logger.IsError()) Log.Eth1CommunicationFailure(_logger, e);
                    await Task.Delay(IEth1Bridge.FailDelayMs, cancellationToken);
                    continue;
                }

                if (genesisCandidate != null)
                {
                    yield return genesisCandidate;
                }
            }
        }

        private ulong GetCurrentFollow(in ulong currentHead) => Math.Max(0, currentHead - _honestValidatorConstants.Eth1FollowDistance);
    }
}
