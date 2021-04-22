using System;
using Nethermind.Core;
using Nethermind.TxPool;

namespace Nethermind.Pipeline.Plugins.RemovedPendingTransactions
{
    public class RemovedPendingTransactionsPipelineElement<TOut> : IPipelineElement<TOut> where TOut : Transaction
    {
        public Action<TOut> Emit { get; set; }

        public RemovedPendingTransactionsPipelineElement(ITxPool txPool)
        {
            txPool.RemovedPending += OnRemovedPending;
        }

        private void OnRemovedPending(object? sender, TxEventArgs args)
        {
            Emit((TOut)args.Transaction);
        }
    }
}
