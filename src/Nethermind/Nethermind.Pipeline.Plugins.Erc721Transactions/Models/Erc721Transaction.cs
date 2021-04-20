using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Int256;

namespace Nethermind.Pipeline.Plugins.Erc721Transactions.Models
{
    public class Erc721Transaction
    {
        public readonly UInt256 TokenId;
        public readonly Keccak Hash;
        public readonly Address From;
        public readonly Address To;
        public readonly string Name;
        public readonly string Symbol;

        public Erc721Transaction(UInt256 tokenId, Keccak hash, Address from, Address to, string name = null,
            string symbol = null)
        {
            TokenId = tokenId;
            Hash = hash;
            From = from;
            To = to;
            Name = name;
            Symbol = symbol;
        }
    }
}
