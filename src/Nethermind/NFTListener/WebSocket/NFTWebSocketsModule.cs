using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nethermind.JsonRpc;
using Nethermind.Serialization.Json;
using Nethermind.WebSockets;

namespace NFTListener.WebSocket
{
    public class NFTWebSocketsModule : IWebSocketsModule
    {
        private readonly ConcurrentDictionary<string, IWebSocketsClient> _clients =
            new ConcurrentDictionary<string, IWebSocketsClient>();

        private readonly JsonRpcProcessor _jsonRpcProcessor;
        private readonly JsonRpcService _jsonRpcService;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IJsonRpcLocalStats _jsonRpcLocalStats;

        public string Name { get; } = "nft";

        public NFTWebSocketsModule(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public IWebSocketsClient CreateClient(System.Net.WebSockets.WebSocket webSocket, string client)
        {
            var socketsClient = new NFTWebSocketsClient(
                new WebSocketsClient(webSocket, client, _jsonSerializer)
            );

            _clients.TryAdd(socketsClient.Id, socketsClient);
            return socketsClient;
        }

        public void RemoveClient(string clientId)
        {
            throw new System.NotImplementedException();
        }

        public Task SendAsync(WebSocketsMessage message)
        {
            return Task.CompletedTask;
        }

        public Task SendRawAsync(string rawMessage)
        {
            return Task.CompletedTask;
        }

        public bool TryInit(HttpRequest request)
        {
            throw new System.NotImplementedException();
        }
    }
}