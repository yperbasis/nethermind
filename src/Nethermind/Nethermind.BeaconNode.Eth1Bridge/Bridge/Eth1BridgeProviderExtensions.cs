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

namespace Nethermind.BeaconNode.Eth1Bridge.Bridge
{
    public static class Eth1BridgeProviderExtensions
    {
        public static async Task<T> CallAsync<T>(this IEth1BridgeProvider bridgeProvider, Func<IEth1Bridge, Task<T>> action, CancellationToken cancellationToken)
        {
            try
            {
                var bridge = await bridgeProvider.GetEth1BridgeAsync(cancellationToken);
                return await action(bridge);
            }
            catch (Exception e)
            {
                throw new Eth1BridgeException("Error communicating with Eth1.", e);
            }
        }
    }
}
