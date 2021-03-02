using System.Collections.Generic;
using Nethermind.Core.Crypto;
using Nethermind.JsonRpc;
using Nethermind.JsonRpc.Modules;
using Nethermind.Logging;

namespace NFTListener.JsonRpcModule
{
    public class NFTModule : INFTModule
    {
        private readonly ILogger _logger;
        private readonly ListenerPlugin _plugin;

        public NFTModule(ILogManager logManager, ListenerPlugin plugin)
        {
           _logger = logManager.GetClassLogger(); 
        }

        public ResultWrapper<IEnumerable<Keccak>> nft_lastTransactions()
        {
            var transactionHashes = _plugin.LastFoundTransactions.ToArray();

            if(transactionHashes.Length > 0)
            {
                return ResultWrapper<IEnumerable<Keccak>>.Success(transactionHashes);
            }

            return ResultWrapper<IEnumerable<Keccak>>.Fail("No transactions to NFT were found in the latest block");
        }
    }
}