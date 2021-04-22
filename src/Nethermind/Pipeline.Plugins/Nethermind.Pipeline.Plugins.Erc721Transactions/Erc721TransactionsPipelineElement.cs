using System;
using System.Collections.Generic;
using Nethermind.Abi;
using Nethermind.Blockchain.Processing;
using Nethermind.Core;
using Nethermind.Logging;
using Nethermind.Core.Extensions;
using Nethermind.Facade;
using Nethermind.Int256;
using Nethermind.Pipeline.Plugins.Erc721Transactions.Contracts;
using Nethermind.Pipeline.Plugins.Erc721Transactions.Models;
using Nethermind.State;

namespace Nethermind.Pipeline.Plugins.Erc721Transactions
{
    public class Erc721TransactionsPipelineElement<TOut> : IPipelineElement<TOut> where TOut : Erc721Transaction
    {
        private readonly IList<string> _erc721Signatures = new List<string>
        {
            "ddf252ad",
            "b88d4fde",
            "42842e0e",
            "23b872dd",
            "40c10f19"
        };

        private const string SupportsInterfaceSignature = "01ffc9a7";
        private readonly IReadOnlyStateProvider _stateProvider;
        private readonly IAbiEncoder _abiEncoder;
        private readonly ILogger _logger;
        private readonly IBlockchainBridge _blockchainBridge;
        private Erc721Metadata _erc721Metadata;
        public Action<TOut> Emit { get; set; }

        public Erc721TransactionsPipelineElement(IBlockProcessor blockProcessor, IReadOnlyStateProvider stateProvider,
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
                UInt256 tokenID;

                string dataString = transaction.Data.ToHexString();
                if (dataString.Length < 9)
                {
                    continue;
                }

                try
                {
                    signature = dataString.Substring(0, 8);
                    string tokenIDString = dataString.Substring(136, 64);
                    tokenID = Bytes.ToUInt256(Bytes.FromHexString(tokenIDString));
                }
                catch (ArgumentOutOfRangeException)
                {
                    continue;
                }

                bool isErc721Signature = _erc721Signatures.Contains(signature);
                if (!isErc721Signature)
                {
                    continue;
                }

                string contractCode = GetContractCode(transaction.To);
                bool implementsErc721 = ImplementsErc721(contractCode);

                if (!implementsErc721)
                {
                    continue;
                }

                string name;
                string symbol;

                try
                {
                    _erc721Metadata = new Erc721Metadata(_abiEncoder, transaction.To, _blockchainBridge);
                    name = _erc721Metadata.Name(block.Header);
                    symbol = _erc721Metadata.Symbol(block.Header);
                }
                catch (AbiException exception)
                {
                    if (_logger.IsError) _logger.Error($"There was an error with getting token name. {transaction.Hash}");
                    continue;
                }

                var erc721Transaction = new Erc721Transaction(tokenID, transaction.Hash, transaction.SenderAddress,
                    transaction.To, name, symbol);
                Emit((TOut)erc721Transaction);
            }
        }

        private string GetContractCode(Address address)
        {
            if (_stateProvider == null)
            {
                throw new Exception("State provider is null at Erc721 Pipeline Plugin");
            }

            return _stateProvider.GetCode(address).ToHexString();
        }

        private bool ImplementsErc721(string code)
        {
            return code.Contains(SupportsInterfaceSignature);
        }
    }
}
