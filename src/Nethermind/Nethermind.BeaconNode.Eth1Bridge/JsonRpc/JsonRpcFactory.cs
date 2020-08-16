using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Nethermind.Core2.Configuration;
using StreamJsonRpc;

namespace Nethermind.BeaconNode.Eth1Bridge.JsonRpc
{
    public class JsonRpcFactory : IJsonRpcFactory
    {
        public async Task<StreamJsonRpc.JsonRpc> CreateJsonRpcAsync(Eth1BridgeConfiguration bridgeConfiguration, CancellationToken cancellationToken)
        {
            var handler = await CreateMessageHandlerAsync(bridgeConfiguration, cancellationToken);
            return new StreamJsonRpc.JsonRpc(handler);
        }

        private static async Task<IJsonRpcMessageHandler> CreateMessageHandlerAsync(Eth1BridgeConfiguration bridgeConfiguration, CancellationToken cancellationToken)
        {
            const string uriSchemeWs = "ws";
            const string uriSchemeWss = "wss";

            string scheme = bridgeConfiguration.EndPoint.Scheme;
            if (scheme == Uri.UriSchemeHttp || scheme == Uri.UriSchemeHttps)
            {
                return new HttpClientMessageHandler(new HttpClient(), bridgeConfiguration.EndPoint);
            }
            else if (scheme == uriSchemeWs || scheme == uriSchemeWss)
            {
                var webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(bridgeConfiguration.EndPoint, cancellationToken);
                return new WebSocketMessageHandler(webSocket);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(bridgeConfiguration.EndPoint.Scheme));
            }
        }
    }
}
