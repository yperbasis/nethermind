using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethermind.Core.Crypto;
using Nethermind.State.Snap;

namespace Nethermind.State.Snap
{
    public class AccountsToRefreshRequest
    {
        /// <summary>
        /// Root hash of the account trie to serve
        /// </summary>
        public Keccak RootHash { get; set; }

        public AccountWithStorageStartingHash[] Paths { get; set; }
    }

    public class AccountWithStorageStartingHash
    {
        public PathWithAccount PathAndAccount { get; set; }
        public Keccak StorageStartingHash { get; set; }
    }
}
