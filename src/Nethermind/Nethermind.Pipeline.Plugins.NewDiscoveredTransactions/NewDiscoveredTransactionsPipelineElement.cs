using System;
using Nethermind.Core;
using Nethermind.TxPool;

namespace Nethermind.Pipeline.Plugins.NewDiscoveredTransactions
{
    public class NewDiscoveredTransactionsPipelineElement<TOut> : IPipelineElement<TOut> where TOut : Transaction
    {
        public Action<TOut> Emit { get; set; }

        public NewDiscoveredTransactionsPipelineElement(ITxPool txPool)
        {
            txPool.NewDiscovered += OnNewDiscovered;
        }

        private void OnNewDiscovered(object? sender, TxEventArgs args)
        {
            Emit((TOut)args.Transaction);
        }
    }
}
