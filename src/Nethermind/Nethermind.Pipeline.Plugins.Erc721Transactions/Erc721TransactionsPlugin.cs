using System.Threading.Tasks;
using Nethermind.Abi;
using Nethermind.Api;
using Nethermind.Blockchain.Processing;
using Nethermind.Facade;
using Nethermind.Logging;
using Nethermind.Pipeline.Plugins.Erc721Transactions.Models;
using Nethermind.Pipeline.Publishers;
using Nethermind.Serialization.Json;
using Nethermind.State;

namespace Nethermind.Pipeline.Plugins.Erc721Transactions
{
    public class Erc721TransactionsPlugin : IPipelinePlugin
{
        public string Name => "Erc721TransactionsPipelinePlugin";
        public string Description => "Pipeline plugin streaming Erc721 txs from block processor";
        public string Author => "Nethermind";
        private INethermindApi _api;
        private IJsonSerializer _jsonSerializer;
        private IBlockProcessor _blockProcessor;
        private Erc721TransactionsPipelineElement<Erc721Transaction> _pipelineElement;
        private LogPublisher<Erc721Transaction, Erc721Transaction> _logPublisher;
        private ILogManager _logManager;
        private PipelineBuilder<Erc721Transaction, Erc721Transaction> _builder;
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
            
            if (_logger.IsInfo) _logger.Info("Erc721 Transactions Pipeline plugin initialized");
            return Task.CompletedTask;
        }

        public Task InitNetworkProtocol()
        {
            return Task.CompletedTask;
        }

        private void CreateLogPublisher()
        {
            _logPublisher = new LogPublisher<Erc721Transaction, Erc721Transaction>(_jsonSerializer, _logManager);
        }

        private void BuildPipeline()
        {
            IPipeline pipeline = _builder.Build();
        }

        private void CreateBuilder()
        {
            _builder = new PipelineBuilder<Erc721Transaction, Erc721Transaction>(_pipelineElement);
            _builder.AddElement(_logPublisher);
        }

        private void CreatePipelineElement()
        {
            _pipelineElement = new Erc721TransactionsPipelineElement<Erc721Transaction>(_blockProcessor, _stateProvider,
                _abiEncoder, _logger, _blockchainBridge);
        }

        public Task InitRpcModules()
        {
            _blockProcessor = _api.MainBlockProcessor;
            _stateProvider = _api.ChainHeadStateProvider;
            _abiEncoder = _api.AbiEncoder;
            _blockchainBridge = _api.CreateBlockchainBridge();
            CreatePipelineElement();
            CreateLogPublisher();
            CreateBuilder();
            BuildPipeline();
            return Task.CompletedTask;
        }
    }
}
