using System;
using Nethermind.Core;
using Nethermind.TxPool;

namespace Nethermind.Pipeline.Plugins.NewPendingTransactions
{
    public class NewPendingTransactionsPipelineElement<TOut> : IPipelineElement<TOut> where TOut : Transaction
    {
        public Action<TOut> Emit { get; set; }

        public NewPendingTransactionsPipelineElement(ITxPool txPool)
        {
            txPool.NewPending += OnNewPending;
        }

        private void OnNewPending(object? sender, TxEventArgs args)
        {
            Emit((TOut)args.Transaction);
        }
    }
}
