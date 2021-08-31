using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nethermind.Logging;
using Nethermind.Serialization.Json;
using Nethermind.Sockets;

namespace Nethermind.Pipeline.Publishers
{
    public class WebSocketsPublisher : IPublisher, IWebSocketsModule
    {
        private readonly ConcurrentDictionary<string, ISocketsClient> _clients = new();
        private readonly ConcurrentDictionary<string, ISocketHandler> _clientsHandlers = new();
        private readonly IJsonSerializer _jsonSerializer;
        public string Name { get; }
        private readonly ILogger _logger;
        private bool _isEnabled;

        public WebSocketsPublisher(string name, IJsonSerializer jsonSerializer, ILogger logger)
        {
            Name = name;
            _jsonSerializer = jsonSerializer;
            _logger = logger;
            Start();
        }

        public ISocketsClient CreateClient(WebSocket webSocket, string client)
        {
            var handler = new WebSocketHandler(webSocket, _logger);
            var newClient = new SocketClient(client, handler, _jsonSerializer);
            _clients.TryAdd(newClient.Id, newClient);
            _clientsHandlers.TryAdd(newClient.Id, handler);

            if(_logger.IsInfo) _logger.Info($"Creating new WS client for {client}");

            return newClient;
        }

        public void RemoveClient(string clientId)
        {
            _clients.TryRemove(clientId, out var socketsClient);
            _clientsHandlers.TryRemove(clientId, out var handler);
        }

        public async Task SendAsync(SocketsMessage message)
        {
            await Task.WhenAll(_clients.Values.Select(v => v.SendAsync(message)));
        }

        public bool TryInit(HttpRequest request)
        {
            return true; 
        }
        
        public async void SubscribeToData<T>(T data)
        {
            try
            {
                var message = _jsonSerializer.Serialize(data);
                if (!_isEnabled) return;
                if(_logger.IsInfo) _logger.Info($"Sending data to websockets ... ");
                await Task.WhenAll(_clientsHandlers.Values.Select(v => v.SendRawAsync(message)));
            }
            catch (Exception ex)
            {
                if(_logger.IsInfo) _logger.Info($"Exception during sending data with websockets, inner exception: {ex.InnerException}");
            };
        }

        public void Stop()
        {
            _isEnabled = false;
        }

        public void Start()
        {
            _isEnabled = true;
        }
    }
}
