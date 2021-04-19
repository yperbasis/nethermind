using System;
using Nethermind.Core;
using Nethermind.TxPool;

namespace Nethermind.Pipeline.Plugins.TxPoolRemovedPendingTransactions
{
    public class PipelineElement<TOut> : IPipelineElement<TOut> where TOut : Transaction
    {
        public Action<TOut> Emit { get; set; }

        public PipelineElement(ITxPool txPool)
        {
            txPool.RemovedPending += OnRemovedPending;
        }

        private void OnRemovedPending(object? sender, TxEventArgs args)
        {
            Emit((TOut)args.Transaction);
        }
    }
}
