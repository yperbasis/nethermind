using System.Buffers.Binary;
using System.Collections.Generic;
using Nethermind.Core2;
using Nethermind.Core2.Containers;
using Nethermind.Core2.Crypto;
using Nethermind.Core2.Types;
using Nethermind.Merkleization;

namespace Nethermind.BeaconNode
{
    public class DepositTree
    {
        private readonly ICryptographyService _crypto;
        private readonly IList<Deposit> _deposits;
        private readonly IMerkleList _depositData = new MerkleTree();

        public IEnumerable<Deposit> Deposits => _deposits;
        public uint Count => (uint) _deposits.Count;
        public Root Root => _depositData.Root;

        public DepositTree(ICryptographyService crypto, IList<DepositData> deposits)
        {
            _crypto = crypto;
            _deposits = new List<Deposit>(deposits.Count);
            for (int i = 0; i < deposits.Count; i++)
            {
                
            }
        }
        
        private void Place(DepositData depositData)
        {
            Ref<DepositData> depositDataRef = depositData.OrRoot;
            Root leaf = _crypto.HashTreeRoot(depositDataRef);
            Bytes32 leafBytes = Bytes32.Wrap(leaf.Bytes);
            _depositData.Insert(leafBytes);
            
            var proof = _depositData.GetProof(_depositData.Count - 1);

            Deposit deposit = new Deposit(proof, depositDataRef);
            _deposits.Add(deposit);
        }
    }
}
