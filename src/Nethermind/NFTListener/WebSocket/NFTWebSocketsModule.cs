using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nethermind.JsonRpc;
using Nethermind.PubSub;
using Nethermind.Serialization.Json;
using Nethermind.WebSockets;

namespace NFTListener.WebSocket
{
    public class NFTWebSocketsModule : IWebSocketsModule, IPublisher
    {
        private readonly ConcurrentDictionary<string, IWebSocketsClient> _clients = new();
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
            var socketsClient = new WebSocketsClient(webSocket, client, _jsonSerializer);
            _clients.TryAdd(socketsClient.Id, socketsClient);

            return socketsClient;
        }

        public void RemoveClient(string id) => _clients.TryRemove(id, out _);

        public async Task SendRawAsync(string data)
        {
            await Task.WhenAll(_clients.Values.Select(v => v.SendRawAsync(data)));
        } 
        
        public async Task SendAsync(WebSocketsMessage message)
        {
            await Task.WhenAll(_clients.Values.Select(v => v.SendAsync(message)));
        }

        public bool TryInit(HttpRequest request)
        {
            return true;
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public async Task PublishAsync<T>(T data) where T : class
        {
            await SendAsync(new WebSocketsMessage("NFT", null, data));
        }
    }
}
