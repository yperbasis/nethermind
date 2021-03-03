using System.Collections.Generic;
using System.Linq;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.JsonRpc;
using Nethermind.JsonRpc.Modules;
using Nethermind.Logging;
using NFTListener.Domain;

namespace NFTListener.JsonRpcModule
{
    public class NFTModule : INFTModule
    {
        private readonly ILogger _logger;
        private readonly ListenerPlugin _plugin;

        public NFTModule(ILogManager logManager, ListenerPlugin plugin)
        {
           _logger = logManager.GetClassLogger(); 
           _plugin = plugin;
        }

        public ResultWrapper<IEnumerable<NFTTransaction>> nft_lastTransactions()
        {
            var transactions = _plugin.GetLastNftTransactions();

            if(transactions.ToArray().Length > 0)
            {
                return ResultWrapper<IEnumerable<NFTTransaction>>.Success(transactions);
            }

            return ResultWrapper<IEnumerable<NFTTransaction>>.Fail($"No transactions to NFT were found in the latest block");
        }
    }
}