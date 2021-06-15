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
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nethermind.Serialization.Json;
using Nethermind.WebSockets;

namespace CryptoCompareStreams
{
    public class WebSocketsStreamer : IWebSocketsModule
    {
        public string Name { get; } = "uniswap";
        private ConcurrentDictionary<string, IWebSocketsClient> _clients = new();
        public bool TryInit(HttpRequest request)
        {
            return true;
        }

        public IWebSocketsClient CreateClient(WebSocket webSocket, string client)
        {
            var newClient = new WebSocketsClient(webSocket, client, new EthereumJsonSerializer());
            _clients.TryAdd(newClient.Id, newClient);

            return newClient;
        }

        public async Task SendRawAsync(string rawMessage)
        {
            await Task.WhenAll(_clients.Values.Select(v => v.SendRawAsync(rawMessage)));
        }

        public async Task SendAsync(WebSocketsMessage message)
        {
            await Task.WhenAll(_clients.Values.Select(v => v.SendAsync(message)));
        }

        public void RemoveClient(string clientId)
        {
            _clients.TryRemove(clientId, out var webSocketsClient);
        }
    }
}