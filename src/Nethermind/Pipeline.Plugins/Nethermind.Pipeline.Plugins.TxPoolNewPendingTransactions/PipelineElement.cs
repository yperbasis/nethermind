using System;
using Nethermind.Core;
using Nethermind.TxPool;

namespace Nethermind.Pipeline.Plugins.TxPoolNewPendingTransactions
{
    public class PipelineElement<TOut> : IPipelineElement<TOut> where TOut : Transaction
    {
        public Action<TOut> Emit { get; set; }

        public PipelineElement(ITxPool txPool)
        {
            txPool.NewPending += OnNewPending;
        }

        private void OnNewPending(object? sender, TxEventArgs args)
        {
            Emit((TOut)args.Transaction);
        }
    }
}
