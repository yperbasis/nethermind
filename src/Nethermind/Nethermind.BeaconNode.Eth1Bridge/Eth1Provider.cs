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
using System.Threading;
using System.Threading.Tasks;
using Nethermind.Core2;
using Nethermind.Core2.Configuration;
using Nethermind.Core2.Containers;
using Nethermind.Core2.Types;
using Nito.Collections;

namespace Nethermind.BeaconNode.Eth1Bridge
{
    public class Eth1Provider : IEth1GenesisProvider, IEth1DataProvider, IDisposable
    {
        private readonly IEth1BridgeFactory _bridgeFactory;
        private readonly HonestValidatorConstants _constants;
        private readonly Eth1BridgeConfiguration _configuration;
        private IEth1Bridge? _bridge;
        
        public Eth1Provider(IEth1BridgeFactory bridgeFactory, HonestValidatorConstants constants, Eth1BridgeConfiguration configuration)
        {
            _bridgeFactory = bridgeFactory ?? throw new ArgumentNullException(nameof(bridgeFactory));
            _constants = constants ?? throw new ArgumentNullException(nameof(constants));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));;
        }

        private async Task<IEth1Bridge> GetEth1BridgeAsync(CancellationToken cancellationToken) => 
            _bridge ??= await _bridgeFactory.CreateEth1BridgeAsync(_configuration, cancellationToken);

        public async Task<Eth1GenesisData> GetEth1GenesisCandidateDataAsync(CancellationToken cancellationToken)
        {
            var bridge = await GetEth1BridgeAsync(cancellationToken);
            throw new NotImplementedException();
        }

        public async IAsyncEnumerable<Eth1Data> GetEth1DataDescendingAsync(ulong maximumTimestampInclusive, ulong minimumTimestampInclusive, CancellationToken cancellationToken)
        {
            var bridge = await GetEth1BridgeAsync(cancellationToken);
            yield break;
        }

        public async IAsyncEnumerable<Deposit> GetDepositsAsync(Bytes32 eth1BlockHash, ulong startIndex, ulong maximum, CancellationToken cancellationToken)
        {
            var bridge = await GetEth1BridgeAsync(cancellationToken);
            yield break;
        }

        public void Dispose()
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            (_bridge as IDisposable)?.Dispose();
        }

        public IAsyncEnumerable<Eth1GenesisData> GetEth1GenesisCandidatesDataAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    public class Eth1Cache
    {
        public Deque<Eth1Data> Eth1Data { get; }
    }
}
