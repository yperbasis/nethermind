using System;
using Nethermind.Blockchain.Processing;
using Nethermind.Core;
using Nethermind.Dsl.Pipeline.Data;
using Nethermind.Pipeline;

#nullable enable
namespace Nethermind.Dsl.Pipeline.Sources
{
    public class ProcessedTransactionsSource<TOut> : IPipelineElement<TOut> where TOut : TxData
    {
        public Action<TOut>? Emit { private get; set; }
        private IBlockProcessor _blockProcessor;

        public ProcessedTransactionsSource(IBlockProcessor blockProcessor)
        {
            _blockProcessor = blockProcessor;
            try
            {
                _blockProcessor.TransactionProcessed += OnProcesedTransaction;
            }
            catch (Exception e)
            {
            }
        }

        private void OnProcesedTransaction(object? sender, TxProcessedEventArgs args)
        {
            Emit?.Invoke((TOut) args.Transaction);
        }
    }
}