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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethermind.BeaconNode.Eth1Bridge.JsonRpc;
using Nethermind.Core2.Configuration;
using Nethermind.Logging.Microsoft;
using StreamJsonRpc;

namespace Nethermind.BeaconNode.Eth1Bridge.Bridge
{
    public class Eth1BridgeProvider : IEth1BridgeProvider
    {
        private const int ConnectionFailDelayMs = 5000;
        private readonly ILogger<Eth1BridgeProvider> _logger;
        private readonly IJsonRpcFactory _jsonRpcFactory;
        private readonly IOptionsMonitor<Eth1BridgeConfiguration> _configuration;
        private StreamJsonRpc.JsonRpc? _jsonRpc;
        private IEth1Bridge? _eth1Bridge;

        public Eth1BridgeProvider(ILogger<Eth1BridgeProvider> logger, 
            IJsonRpcFactory jsonRpcFactory, 
            IOptionsMonitor<Eth1BridgeConfiguration> configuration)
        {
            _logger = logger;
            _jsonRpcFactory = jsonRpcFactory;
            _configuration = configuration;
        }
        
        public async Task<IEth1Bridge> GetEth1BridgeAsync(CancellationToken cancellationToken)
        {
            while (_eth1Bridge == null)
            {
                try
                {
                    _jsonRpc = await _jsonRpcFactory.CreateJsonRpcAsync(_configuration.CurrentValue, cancellationToken);
                    _eth1Bridge = _jsonRpc.Attach<IEth1Bridge>();
                    await ValidateChainIdAsync(_eth1Bridge, _configuration.CurrentValue.ChainId);
                    await ValidateNetworkIdAsync(_eth1Bridge, _configuration.CurrentValue.NetworkId);
                    _jsonRpc.Disconnected += OnDisconnected;
                }
                catch (Exception e)
                {
                    if (_logger.IsError()) Eth1Bridge.Log.Eth1ConnectionFailure(_logger, e);
                    await Task.Delay(ConnectionFailDelayMs, cancellationToken);
                }
            }

            return _eth1Bridge;
        }

        private async Task ValidateNetworkIdAsync(IEth1Bridge eth1Bridge, ulong expectedNetworkId)
        {
            var networkId = await eth1Bridge.net_version();
            if (networkId != expectedNetworkId)
            {
                Cleanup();
                throw new InvalidOperationException($"Trying to connect to incorrect Eth1 network. Expected network {expectedNetworkId}, but found {networkId}.");
            }
        }

        private async Task ValidateChainIdAsync(IEth1Bridge eth1Bridge, ulong expectedChainId)
        {
            var chainId = await eth1Bridge.eth_chainId();
            if (chainId != expectedChainId)
            {
                Cleanup();
                throw new InvalidOperationException($"Trying to connect to incorrect Eth1 chain. Expected chain {expectedChainId}, but found {chainId}.");
            }
        }

        private void OnDisconnected(object? sender, JsonRpcDisconnectedEventArgs e)
        {
            Cleanup();
            if (_logger.IsWarn()) Eth1Bridge.Log.Eth1Disconnected(_logger, e.Reason.ToString(), e.Description, e.Exception);
        }
        
        private void Cleanup()
        {
            var jsonRpc = _jsonRpc;
            _jsonRpc = null;
            _eth1Bridge = null;
            if (jsonRpc != null)
            {
                jsonRpc.Disconnected -= OnDisconnected;
                jsonRpc.Dispose();
            }
        }

        public void Dispose()
        {
            Cleanup();
        }
    }
}
