using System.Threading.Tasks;
using Nethermind.Abi;
using Nethermind.Api;
using Nethermind.Blockchain.Processing;
using Nethermind.Facade;
using Nethermind.Logging;
using Nethermind.Pipeline.Plugins.Erc20Transactions.Models;
using Nethermind.Pipeline.Publishers;
using Nethermind.Serialization.Json;
using Nethermind.State;

namespace Nethermind.Pipeline.Plugins.Erc20Transactions
{
    public class Erc20TransactionsPlugin : IPipelinePlugin
    {
        public string Name => "Erc20TransactionsPipelinePlugin";
        public string Description => "Pipeline plugin streaming Erc20 txs from block processor";
        public string Author => "Nethermind";
        private INethermindApi _api;
        private IJsonSerializer _jsonSerializer;
        private IBlockProcessor _blockProcessor;
        private Erc20TransactionsPipelineElement<Erc20Transaction> _pipelineElement;
        private LogPublisher<Erc20Transaction, Erc20Transaction> _logPublisher;
        private WebSocketsPublisher<Erc20Transaction, Erc20Transaction> _webSocketsPublisher;
        private ILogManager _logManager;
        private PipelineBuilder<Erc20Transaction, Erc20Transaction> _builder;
        private IReadOnlyStateProvider? _stateProvider;
        private IAbiEncoder _abiEncoder;
        private ILogger _logger;
        private IBlockchainBridge _blockchainBridge;

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public Task Init(INethermindApi nethermindApi)
        {
            _api = nethermindApi;
            _logger = _api.LogManager.GetClassLogger();
            _logManager = _api.LogManager;
            _jsonSerializer = _api.EthereumJsonSerializer;
            
            if (_logger.IsInfo) _logger.Info("Erc20 Transactions Pipeline plugin initialized");
            return Task.CompletedTask;
        }

        public Task InitNetworkProtocol()
        {
            return Task.CompletedTask;
        }

        private void CreatePublishers()
        {
            _logPublisher = new LogPublisher<Erc20Transaction, Erc20Transaction>(_jsonSerializer, _logManager);
            _webSocketsPublisher = new WebSocketsPublisher<Erc20Transaction, Erc20Transaction>("erc20", _jsonSerializer, _logger);
            _api.WebSocketsManager.AddModule(_webSocketsPublisher);
        }

        private void BuildPipeline()
        {
            IPipeline pipeline = _builder.Build();
        }

        private void CreateBuilder()
        {
            _builder = new PipelineBuilder<Erc20Transaction, Erc20Transaction>(_pipelineElement);
            _builder.AddElement(_logPublisher);
            _builder.AddElement(_webSocketsPublisher);
        }

        private void CreatePipelineElement()
        {
            _pipelineElement = new Erc20TransactionsPipelineElement<Erc20Transaction>(_blockProcessor, _stateProvider,
                _abiEncoder, _logger, _blockchainBridge);
        }

        public Task InitRpcModules()
        {
            _blockProcessor = _api.MainBlockProcessor;
            _stateProvider = _api.ChainHeadStateProvider;
            _abiEncoder = _api.AbiEncoder;
            _blockchainBridge = _api.CreateBlockchainBridge();
            CreatePipelineElement();
            CreatePublishers();
            CreateBuilder();
            BuildPipeline();
            return Task.CompletedTask;
        }
    }
}
