using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nethermind.Api;
using Nethermind.Api.Extensions;
using Nethermind.Blockchain.Processing;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Core.Extensions;
using Nethermind.JsonRpc.Modules;
using Nethermind.Logging;
using NFTListener.JsonRpcModule;

namespace NFTListener
{
    public class ListenerPlugin : INethermindPlugin
    {
        private INethermindApi _api;
        private ILogger _logger;
        public string Name { get; private set; } = "NFTListener";

        public string Description { get; private set; } = "Listener plugin for new calls to ERC-721 tokens";

        public string Author { get; private set; } = "Nethermind Team";
        private readonly string[] erc721Signatures = new string[] { "ddf252ad","8c5be1e5","17307eab","70a08231","6352211e","b88d4fde","42842e0e","23b872dd","095ea7b3","a22cb465","081812fc","e985e9c5", "a9059cbb" };
        private List<Keccak> LastFoundTransactions;
        private long LastBlockNumber;

        public void Dispose()
        {
        }

        public Task Init(INethermindApi nethermindApi)
        {
            _api = nethermindApi;
            _logger = nethermindApi.LogManager.GetClassLogger();
            if(_logger.IsInfo) _logger.Info("Initialization of ListenerPlugin");

            LastFoundTransactions = new List<Keccak>();

            if(_logger.IsInfo) _logger.Info("ListenerPlugin initialized");
            return Task.CompletedTask;
        }

        public Task InitNetworkProtocol()
        {
            return Task.CompletedTask;
        }

        public Task InitRpcModules()
        {
            _api.MainBlockProcessor.BlockProcessed += OnBlockProcessed;
            if(_logger.IsInfo) _logger.Info("Initialization of NFT json rpc module");
            INFTModule nftModule = new NFTModule(_api.LogManager, this);

            _api.RpcModuleProvider.Register(new SingletonModulePool<INFTModule>(nftModule));

            if(_logger.IsInfo) _logger.Info("Initialized NFT json rpc module");
            return Task.CompletedTask;
        }

        public (IEnumerable<Keccak> transactions, long blockNumber) GetLastNftTransactions()
        {
            return (LastFoundTransactions, LastBlockNumber);
        }

        private void OnBlockProcessed(object sender, BlockProcessedEventArgs args)
        {
            Block block = args.Block;
            LastBlockNumber = block.Number;
            
            foreach(Transaction transaction in block.Transactions)
            {
                string signature;

                string dataString = transaction.Data.ToHexString();
                if(dataString.Length < 9)
                {
                    return;
                }

                try
                {
                    signature = dataString.Substring(0, 8);
                }
                catch(ArgumentOutOfRangeException)
                {
                    return; 
                }

                if(erc721Signatures.Contains(signature))
                {
                    LastFoundTransactions.Add(transaction.Hash); 
                }
            }
        }
    }
}