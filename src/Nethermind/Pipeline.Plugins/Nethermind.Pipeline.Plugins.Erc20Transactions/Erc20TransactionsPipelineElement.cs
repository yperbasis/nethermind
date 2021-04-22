using System;
using System.Collections.Generic;
using Nethermind.Abi;
using Nethermind.Blockchain.Processing;
using Nethermind.Core;
using Nethermind.Logging;
using Nethermind.Core.Extensions;
using Nethermind.Facade;
using Nethermind.Pipeline.Plugins.Erc20Transactions.Contracts;
using Nethermind.Pipeline.Plugins.Erc20Transactions.Models;
using Nethermind.State;

namespace Nethermind.Pipeline.Plugins.Erc20Transactions
{
    public class Erc20TransactionsPipelineElement<TOut> : IPipelineElement<TOut> where TOut : Erc20Transaction
    {
        private readonly IList<string> _erc20Signatures = new List<string>
        {
            "18160ddd", "70a08231", "a9059cbb", "dd62ed3e", "095ea7b3", "23b872dd"
        };
        private Erc20Metadata _erc20Metadata;
        private readonly IReadOnlyStateProvider _stateProvider;
        private readonly IAbiEncoder _abiEncoder;
        private readonly IBlockchainBridge _blockchainBridge;
        private readonly ILogger _logger;
        public Action<TOut> Emit { get; set; }

        public Erc20TransactionsPipelineElement(IBlockProcessor blockProcessor, IReadOnlyStateProvider stateProvider,
            IAbiEncoder abiEncoder, ILogger logger, IBlockchainBridge blockchainBridge)
        {
            _stateProvider = stateProvider;
            _abiEncoder = abiEncoder;
            _logger = logger;
            _blockchainBridge = blockchainBridge;
            blockProcessor.BlockProcessed += OnBlockProcessed;
        }

        private void OnBlockProcessed(object? sender, BlockProcessedEventArgs args)
        {
            Block block = args.Block;

            foreach (Transaction transaction in block.Transactions)
            {
                string signature;

                string dataString = transaction.Data.ToHexString();
                if (dataString.Length < 9)
                {
                    continue;
                }

                try
                {
                    signature = dataString.Substring(0, 8);
                }
                catch (ArgumentOutOfRangeException)
                {
                    continue;
                }

                bool isErc20Signature = _erc20Signatures.Contains(signature);
                if (!isErc20Signature)
                {
                    continue;
                }
                
                string contractCode = GetContractCode(transaction.To);
                bool implementsErc20 = ImplementsErc20(contractCode);

                if (!implementsErc20)
                {
                    continue;
                }

                string name;

                try
                {
                    _erc20Metadata = new Erc20Metadata(_abiEncoder, transaction.To, _blockchainBridge);
                    name = _erc20Metadata.Name(block.Header);
                }
                catch (AbiException exception)
                {
                    if (_logger.IsInfo) _logger.Info("Token name has not been fetched.");
                    continue;
                }

                var erc20Transaction = new Erc20Transaction(transaction.Hash, transaction.SenderAddress,
                    transaction.To, name);
                Emit((TOut)erc20Transaction);
            }
        }
        
        private string GetContractCode(Address address)
        {
            if (_stateProvider == null)
            {
                throw new Exception("State provider is null at Erc20 Pipeline Plugin");
            }

            try
            {
                return _stateProvider.GetCode(address)?.ToHexString();
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
        
        private bool ImplementsErc20(string code)
        {
            foreach (var siganture in _erc20Signatures)
            {
                if (!code.Contains(siganture))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
