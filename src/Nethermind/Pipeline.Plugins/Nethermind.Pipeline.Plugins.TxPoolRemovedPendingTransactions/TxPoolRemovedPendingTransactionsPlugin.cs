//  Copyright (c) 2021 Demerzel Solutions Limited
//  This file is part of the Nethermind library.
// 
//  The Nethermind library is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  The Nethermind library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.
// 

using System.Threading.Tasks;
using Nethermind.Api;
using Nethermind.Api.Extensions;
using Nethermind.Core;
using Nethermind.Logging;
using Nethermind.Pipeline.Publishers;
using Nethermind.Serialization.Json;
using Nethermind.TxPool;

namespace Nethermind.Pipeline.Plugins.TxPoolRemovedPendingTransactions
{
    public class TxPoolNewPendingTransactionsPlugin : INethermindPlugin
    {
        public string Name => "TxPool RemovedPending Transactions Pipeline Plugin";
        public string Description => "Pipeline plugin streaming removed pending txs from txpool";
        public string Author => "Nethermind";
        private INethermindApi _api;
        private IJsonSerializer _jsonSerializer;
        private ILogger _logger;
        private ITxPool _txPool;
        private PipelineElement<Transaction> _pipelineElement;
        private WebSocketsPublisher<Transaction, Transaction> _wsPublisher;
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
            
            if (_logger.IsInfo) _logger.Info("Pipeline plugin initialized");
            return Task.CompletedTask;
        }

        public Task InitNetworkProtocol()
        {
            _txPool = _api.TxPool;
            CreatePipelineElement();
            CreateLogPublisher();
            // CreateWsPipelineElement();
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

        private void CreateWsPipelineElement()
        {
            _wsPublisher = new WebSocketsPublisher<Transaction, Transaction>("pipeline", _jsonSerializer);
            _api.WebSocketsManager.AddModule(_wsPublisher);
        }

        private void CreateBuilder()
        {
            _builder = new PipelineBuilder<Transaction, Transaction>(_pipelineElement);
            _builder.AddElement(_logPublisher);
            // _builder.AddElement(_wsPublisher);
        }

        private void CreatePipelineElement()
        {
            _pipelineElement = new PipelineElement<Transaction>(_txPool);
        }

        public Task InitRpcModules()
        {
            return Task.CompletedTask;
        }
    }
}
