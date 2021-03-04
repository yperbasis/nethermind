using Nethermind.Core;
using Nethermind.Core.Crypto;

namespace NFTListener.Domain
{
    public class NFTTransaction
    {
        public readonly Keccak Hash;
        public readonly Address From; 
        public readonly Address To;

        public NFTTransaction(Keccak hash, Address from, Address to)
        {
            //NFT ID missing 
            Hash = hash;
            From = from;
            To = to;
        }
    }
}