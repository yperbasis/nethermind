using System;
using Nethermind.Core;
using Nethermind.Dsl.Pipeline.Data;
using Nethermind.Pipeline;
using Nethermind.TxPool;

#nullable enable
namespace Nethermind.Dsl.Pipeline.Sources
{
    public class PendingTransactionsSource<TOut> : IPipelineElement<TOut> where TOut : PendingTxData
    {
        private readonly ITxPool _txPool;

        public PendingTransactionsSource(ITxPool txPool)
        {
            _txPool = txPool;
            try
            {
                _txPool.NewPending += OnNewPending;
            }
            catch (Exception e)
            {
            }
        }

        public Action<TOut>? Emit { private get; set; }

        private void OnNewPending(object? sender, TxEventArgs args)
        {
            var data = PendingTxData.FromTransaction(args.Transaction);
            Emit?.Invoke((TOut) data);
        }
    }
}