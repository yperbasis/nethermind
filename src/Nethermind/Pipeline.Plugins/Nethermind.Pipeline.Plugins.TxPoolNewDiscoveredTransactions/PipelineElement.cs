using System;
using Nethermind.Core;
using Nethermind.TxPool;

namespace Nethermind.Pipeline.Plugins.TxPoolNewDiscoveredTransactions
{
    public class PipelineElement<TOut> : IPipelineElement<TOut> where TOut : Transaction
    {
        public Action<TOut> Emit { get; set; }

        public PipelineElement(ITxPool txPool)
        {
            txPool.NewDiscovered += OnNewDiscovered;
        }

        private void OnNewDiscovered(object? sender, TxEventArgs args)
        {
            Emit((TOut)args.Transaction);
        }
    }
}
