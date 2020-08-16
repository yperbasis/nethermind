//  Copyright (c) 2018 Demerzel Solutions Limited
//  This file is part of the Nethermind library.
// 
//  The Nethermind library is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  The Nethermind library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.
// 

using System.Collections.Generic;
using System.Linq;
using Nethermind.BeaconNode.Eth1Bridge.Bridge;
using Nethermind.Core2;
using Nethermind.Core2.Configuration;
using Nethermind.Core2.Containers;
using Nethermind.Core2.Crypto;
using Nethermind.Core2.Types;
using Nethermind.Merkleization;

namespace Nethermind.BeaconNode.Eth1Bridge
{
    public class DepositStore : IDepositStore
    {
        private readonly ICryptographyService _crypto;
        private readonly List<Deposit> _deposits = new List<Deposit>();

        // keep in the storage
        // split storage of deposit data and deposit where deposit is deposit data + proof
        public IReadOnlyList<Deposit> Deposits => _deposits;
        public Root Root => DepositTree.Root;

        private IMerkleList DepositTree { get; } = new MerkleTree();

        public DepositStore(ICryptographyService crypto, ChainConstants chainConstants)
        {
            _crypto = crypto;
        }
        
        public void Place(IEnumerable<DepositLog> depositData)
        {
            foreach (var log in depositData)
            {
                // assert index
                Place(log.DepositData);
                LargestBlock = log.BlockNumber;
            }
        }

        public ulong LargestBlock { get; private set; }

        private void Place(DepositData depositData)
        {
            Deposit deposit = depositData.ToDeposit(_crypto, DepositTree);
            _deposits.Add(deposit);
        }
    }

    public interface IDepositStore
    {
        IReadOnlyList<Deposit> Deposits { get; }
        Root Root { get; }
        void Place(IEnumerable<DepositLog> depositData);
        public void Place(params DepositLog[] depositData) => Place((IEnumerable<DepositLog>)depositData);
    }
}
