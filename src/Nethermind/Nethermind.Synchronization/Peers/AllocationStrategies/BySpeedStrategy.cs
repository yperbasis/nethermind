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

using System;
using System.Collections.Generic;
using Nethermind.Blockchain;
using Nethermind.Stats;

namespace Nethermind.Synchronization.Peers.AllocationStrategies
{
    public class BySpeedStrategy : IPeerAllocationStrategy
    {
        private readonly TransferSpeedType _speedType;
        private readonly bool _priority;
        private readonly decimal _minDiffPercentageForSpeedSwitch;
        private readonly int _minDiffForSpeedSwitch;

        public BySpeedStrategy(
            TransferSpeedType speedType,
            bool priority,
            decimal minDiffPercentageForSpeedSwitch = 0.0m,
            int minDiffForSpeedSwitch = 0
        )
        {
            _speedType = speedType;
            _priority = priority;
            _minDiffPercentageForSpeedSwitch = minDiffPercentageForSpeedSwitch;
            _minDiffForSpeedSwitch = minDiffForSpeedSwitch;
        }

        public bool CanBeReplaced => false;

        public PeerInfo? Allocate(PeerInfo? currentPeer, IEnumerable<PeerInfo> peers, INodeStatsManager nodeStatsManager, IBlockTree blockTree)
        {
            long nullSpeed = _priority ? -1 : long.MaxValue;
            long peerCount = 0;
            long currentSpeed = currentPeer == null ? nullSpeed : nodeStatsManager.GetOrAdd(currentPeer.SyncPeer.Node).GetAverageTransferSpeed(_speedType) ?? nullSpeed;
            (PeerInfo? Info, long TransferSpeed) bestPeer = (currentPeer, currentSpeed);

            foreach (PeerInfo info in peers)
            {
                peerCount++;
                (this as IPeerAllocationStrategy).CheckAsyncState(info);

                long averageTransferSpeed = nodeStatsManager.GetOrAdd(info.SyncPeer.Node).GetAverageTransferSpeed(_speedType) ?? 0;
                if (_priority ? averageTransferSpeed > bestPeer.TransferSpeed : averageTransferSpeed < bestPeer.TransferSpeed)
                {
                    bestPeer = (info, averageTransferSpeed);
                }
            }

            if (peerCount == 0)
            {
                return currentPeer;
            }

            decimal speedRatio = bestPeer.TransferSpeed / (decimal)Math.Max(1L, currentSpeed);
            if (speedRatio > 1m + _minDiffPercentageForSpeedSwitch
                && bestPeer.TransferSpeed - currentSpeed > _minDiffForSpeedSwitch)
            {
                return bestPeer.Info;
            }

            return currentPeer ?? bestPeer.Info;
        }
    }
}
