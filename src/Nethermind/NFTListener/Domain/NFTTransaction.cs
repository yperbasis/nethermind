using Nethermind.Core;
using Nethermind.Core.Crypto;

namespace NFTListener.Domain
{
    public class NFTTransaction
    {
        public readonly Keccak Transaction;
        public readonly Address From; 
        public readonly Address To;

        public NFTTransaction(Keccak transaction, Address from, Address to)
        {
            Transaction = transaction;
            From = from;
            To = to;
        }
    }
}