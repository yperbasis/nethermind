using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nethermind.Api;
using Nethermind.Api.Extensions;
using Nethermind.Blockchain.Processing;
using Nethermind.Core;
using Nethermind.Core.Extensions;
using Nethermind.JsonRpc.Modules;
using Nethermind.Logging;
using Nethermind.PubSub;
using Nethermind.Serialization.Json;
using Nethermind.State;
using NFTListener.Domain;
using NFTListener.JsonRpcModule;
using NFTListener.WebSocket;

namespace NFTListener
{
    public class ListenerPlugin : INethermindPlugin
    {
        private INethermindApi _api;
        private ILogger _logger;
        private IReadOnlyStateProvider _stateProvider;
        public string Name { get; private set; } = "NFTListener";
        public string Description { get; private set; } = "Listener plugin for new calls to ERC-721 tokens";
        public string Author { get; private set; } = "Nethermind Team";
        private readonly string[] _erc721Signatures = new string[] { "ddf252ad" };
        // private readonly string[] _erc721Signatures = new string[] { "ddf252ad","8c5be1e5","17307eab","70a08231","6352211e","b88d4fde","42842e0e","23b872dd","095ea7b3","a22cb465","081812fc","e985e9c5" };
        private IEnumerable<NFTTransaction> _lastFoundTransactions;
        private IList<IPublisher> _publishers;
        public IJsonSerializer _jsonSerializer;

        public void Dispose()
        {
        }

        public Task Init(INethermindApi nethermindApi)
        {
            _api = nethermindApi;
            _logger = nethermindApi.LogManager.GetClassLogger();
            if(_logger.IsInfo) _logger.Info("Initialization of ListenerPlugin");

            _lastFoundTransactions = new List<NFTTransaction>();

            if(_logger.IsInfo) _logger.Info("ListenerPlugin initialized");
            return Task.CompletedTask;
        }

        public Task InitNetworkProtocol()
        {
            return Task.CompletedTask;
        }

        public Task InitRpcModules()
        {
            //Doing it here because on .Init() MainBlockProcessor and StateProvider are both nulls - not initialized yet
            _api.MainBlockProcessor.BlockProcessed += OnBlockProcessed;
            _stateProvider = _api.StateProvider;
            _jsonSerializer = _api.EthereumJsonSerializer;
            
            if(_logger.IsInfo) _logger.Info("Initialization of NFT json rpc module");
            INFTModule nftModule = new NFTModule(_api.LogManager, this);

            _api.RpcModuleProvider.Register(new SingletonModulePool<INFTModule>(nftModule));

            if(_logger.IsInfo) _logger.Info("Initialized NFT json rpc module");

            InitWebSockets();

            return Task.CompletedTask;
        }

        public IEnumerable<NFTTransaction> GetLastNftTransactions()
        {
            return _lastFoundTransactions;
        }

        private void InitWebSockets()
        {
            var (getFromAPi, _) = _api.ForNetwork;

            NFTWebSocketsModule webSocketsModule = new(getFromAPi.EthereumJsonSerializer);
            getFromAPi.WebSocketsManager!.AddModule(webSocketsModule, true);
            getFromAPi.Publishers.Add(webSocketsModule);

            _publishers = getFromAPi.Publishers;
        }

        private void OnBlockProcessed(object sender, BlockProcessedEventArgs args)
        {
            Block block = args.Block;

            if (block.Transactions is null)
            {
                SendToWebSockets($"No transactions in block #{block.Number}");
                return;
            }

            SendToWebSockets($"Number of transactions in block #{block.Number} is {block.Transactions.Count()}");

            foreach (Transaction transaction in block.Transactions)
            {
                string signature;

                string dataString = transaction.Data.ToHexString();
                if (dataString.Length < 9)
                {
                    return;
                }

                try
                {
                    signature = dataString.Substring(0, 8);
                }
                catch (ArgumentOutOfRangeException)
                {
                    continue;
                }

                bool isERC721Signature = _erc721Signatures.Contains(signature);
                if (!isERC721Signature)
                {
                    continue;
                }

                string contractCode = GetContractCode(transaction.To);
                bool implementsERC721 = ImplementsERC721(contractCode);

                if (!implementsERC721)
                {
                    continue;
                }

                var NFTtransaction = new NFTTransaction(transaction.Hash, transaction.SenderAddress,
                    transaction.To);
                var serializedTransaction = _jsonSerializer.Serialize(NFTtransaction);

                _lastFoundTransactions.Append(NFTtransaction);
                SendToWebSockets(serializedTransaction);
            }
        }

        private string GetContractCode(Address address)
        {
            if(_stateProvider == null)
            {
                throw new Exception("State provider is null at ListenerPlugin");
            }
            return _stateProvider.GetCode(address).ToHexString();
        }

        private bool ImplementsERC721(string code)
        {
            return _erc721Signatures.All(signature => code.Contains(signature));
        }

        private void SendToWebSockets(string data)
        {
            foreach(var publisher in _publishers)
            {
                publisher.PublishAsync(data);
            }
        }
    }
}
