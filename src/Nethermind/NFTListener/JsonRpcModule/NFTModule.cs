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

        public ResultWrapper<IEnumerable<string>> nft_lastTransactions()
        {
            var transactions = _plugin.GetLastNftTransactions();
            var serializedTransactions = GetSerializedTransactions(transactions); 

            if(transactions.ToArray().Length > 0)
            {
                return ResultWrapper<IEnumerable<string>>.Success(serializedTransactions);
            }

            return ResultWrapper<IEnumerable<string>>.Fail($"No transactions to NFT were found in the latest block");
        }

        private IEnumerable<string> GetSerializedTransactions(IEnumerable<NFTTransaction> transactions)
        {
            foreach(var t in transactions)
            {
                yield return _plugin._jsonSerializer.Serialize(t);
            }
        }
    }
}