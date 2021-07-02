//  Copyright (c) 2021 Demerzel Solutions Limited
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
using Nethermind.Blockchain;
using Nethermind.Stats;
using Nethermind.Stats.Model;

namespace Nethermind.Synchronization.Peers.AllocationStrategies
{
    internal class SupportedClientTypesStrategy : IPeerAllocationStrategy
    {
        private readonly TotalDiffStrategy _strategy;
        private readonly ISet<NodeClientType> _supportedClientTypes;

        public SupportedClientTypesStrategy(TotalDiffStrategy strategy, ISet<NodeClientType> supportedClientTypes)
        {
            _strategy = strategy;
            _supportedClientTypes = supportedClientTypes;
        }

        public bool CanBeReplaced => _strategy.CanBeReplaced;
        
        public PeerInfo? Allocate(PeerInfo? currentPeer, IEnumerable<PeerInfo> peers, INodeStatsManager nodeStatsManager, IBlockTree blockTree) => 
            _strategy.Allocate(currentPeer, peers.Where(p => _supportedClientTypes.Contains(p.PeerClientType)), nodeStatsManager, blockTree);
    }
}