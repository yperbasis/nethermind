using System.Threading.Tasks;
using Nethermind.Api;
using Nethermind.Core;
using Nethermind.Logging;
using Nethermind.Pipeline.Publishers;
using Nethermind.Serialization.Json;
using Nethermind.TxPool;

namespace Nethermind.Pipeline.Plugins.NewPendingTransactions
{
    public class NewPendingTransactionsPlugin : IPipelinePlugin
{
        public string Name => "NewPendingTransactionsPipelinePlugin";
        public string Description => "Pipeline plugin streaming pending txs from txpool";
        public string Author => "Nethermind";
        private INethermindApi _api;
        private IJsonSerializer _jsonSerializer;
        private ILogger _logger;
        private ITxPool _txPool;
        private NewPendingTransactionsPipelineElement<Transaction> _pipelineElement;
        private LogPublisher<Transaction, Transaction> _logPublisher;
        private ILogManager _logManager;
        private PipelineBuilder<Transaction, Transaction> _builder;

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
            
            if (_logger.IsInfo) _logger.Info("New Pending Transactions Pipeline plugin initialized");
            return Task.CompletedTask;
        }

        public Task InitNetworkProtocol()
        {
            _txPool = _api.TxPool;
            CreatePipelineElement();
            CreateLogPublisher();
            CreateBuilder();
            BuildPipeline();
            
            return Task.CompletedTask;
        }

        private void CreateLogPublisher()
        {
            _logPublisher = new LogPublisher<Transaction, Transaction>(_jsonSerializer, _logManager);
        }

        private void BuildPipeline()
        {
            IPipeline pipeline = _builder.Build();
        }

        private void CreateBuilder()
        {
            _builder = new PipelineBuilder<Transaction, Transaction>(_pipelineElement);
            _builder.AddElement(_logPublisher);
        }

        private void CreatePipelineElement()
        {
            _pipelineElement = new NewPendingTransactionsPipelineElement<Transaction>(_txPool);
        }

        public Task InitRpcModules()
        {
            return Task.CompletedTask;
        }
    }
}
