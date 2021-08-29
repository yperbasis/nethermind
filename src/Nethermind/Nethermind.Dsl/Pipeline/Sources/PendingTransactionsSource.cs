using System;
using Nethermind.Dsl.Pipeline.Data;
using Nethermind.Logging;
using Nethermind.Pipeline;
using Nethermind.TxPool;

#nullable enable
namespace Nethermind.Dsl.Pipeline.Sources
{
    public class PendingTransactionsSource<TOut> : IPipelineElement<TOut> where TOut : PendingTxData
    {
        private readonly ITxPool _txPool;
        private readonly ILogger _logger;

        public PendingTransactionsSource(ITxPool txPool, ILogger logger)
        {
            _txPool = txPool;
            _logger = logger;

            _txPool.NewPending += OnNewPending;
        }

        public Action<TOut>? Emit { private get; set; }

        private void OnNewPending(object? sender, TxEventArgs args)
        {
            var data = PendingTxData.FromTransaction(args.Transaction);
            
            if(_logger.IsInfo) _logger.Info(data.ToString());
            
            Emit?.Invoke((TOut) data);
        }
    }
}