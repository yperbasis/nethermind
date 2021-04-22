using Nethermind.Core;
using Nethermind.Core.Crypto;

namespace Nethermind.Pipeline.Plugins.Erc20Transactions.Models
{
    public class Erc20Transaction
    {
        public readonly Keccak Hash;
        public readonly Address From;
        public readonly Address To;
        public readonly string Name;

        public Erc20Transaction(Keccak hash, Address from, Address to, string name = null)
        {
            Hash = hash;
            From = from;
            To = to;
            Name = name;
        }
    }
}
