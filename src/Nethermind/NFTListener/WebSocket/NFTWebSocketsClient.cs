using System;
using System.Threading.Tasks;
using Nethermind.JsonRpc;
using Nethermind.Serialization.Json;
using Nethermind.WebSockets;

namespace NFTListener.WebSocket
{
    public class NFTWebSocketsClient : IWebSocketsClient
    {
        private readonly IWebSocketsClient _client;
        public string Id => _client.Id;
        public string Client { get; }

        public NFTWebSocketsClient(
            IWebSocketsClient client
            )
        {
            _client = client;
        }
        public Task ReceiveAsync(Memory<byte> data)
        {
            return Task.CompletedTask;
        }

        public Task SendAsync(WebSocketsMessage message)
        {
            return _client.SendAsync(message);
        }

        public Task SendRawAsync(string data)
        {
            return _client.SendRawAsync(data);
        }
    }
}