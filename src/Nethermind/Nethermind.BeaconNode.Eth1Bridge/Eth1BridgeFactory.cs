using System;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Nethermind.BeaconNode.Eth1Bridge.JsonRpc;
using Nethermind.Core2.Configuration;
using StreamJsonRpc;

namespace Nethermind.BeaconNode.Eth1Bridge
{
    public interface IEth1BridgeFactory
    {
        Task<IEth1Bridge> CreateEth1BridgeAsync(Eth1BridgeConfiguration bridgeConfiguration, CancellationToken cancellationToken);
    }

    public class Eth1BridgeFactory : IEth1BridgeFactory
    {
        public async Task<IEth1Bridge> CreateEth1BridgeAsync(Eth1BridgeConfiguration bridgeConfiguration, CancellationToken cancellationToken)
        {
            var handler = await CreateMessageHandlerAsync(bridgeConfiguration, cancellationToken);
            var jsonRpc = new StreamJsonRpc.JsonRpc(handler);
            return jsonRpc.Attach<IEth1Bridge>();
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
