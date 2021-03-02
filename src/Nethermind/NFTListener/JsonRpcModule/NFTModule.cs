using System.Collections.Generic;
using System.Linq;
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
           _plugin = plugin;
        }

        public ResultWrapper<NFTJsonRpcResult> nft_lastTransactions()
        {
            var result = _plugin.GetLastNftTransactions();

            var response = new NFTJsonRpcResult(result.transactions, result.blockNumber);

            if(response.Transactions.ToArray().Length > 0)
            {
                return ResultWrapper<NFTJsonRpcResult>.Success(response);
            }

            return ResultWrapper<NFTJsonRpcResult>.Fail($"No transactions to NFT were found in the latest block (Number: {response.Number})");
        }
    }

    public class NFTJsonRpcResult
    {
        public readonly IEnumerable<Keccak> Transactions;
        public readonly long Number;

        public NFTJsonRpcResult(IEnumerable<Keccak> transactions, long blockNumber)
        {
            Transactions = transactions; 
            Number = blockNumber;            
        }
    }
}