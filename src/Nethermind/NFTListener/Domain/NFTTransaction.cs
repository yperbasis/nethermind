using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Int256;

namespace NFTListener.Domain
{
    public class NFTTransaction
    {
        public readonly UInt256 TokenId;
        public readonly Keccak Hash;
        public readonly Address From; 
        public readonly Address To;

        public NFTTransaction(UInt256 tokenId, Keccak hash, Address from, Address to)
        {
            TokenId = tokenId;
            Hash = hash;
            From = from;
            To = to;
        }
    }
}