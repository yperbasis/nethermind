﻿//  Copyright (c) 2021 Demerzel Solutions Limited
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

using Nethermind.Blockchain.Synchronization;
using Nethermind.Consensus;
using Nethermind.Core;
using Nethermind.Core.Test.Builders;
using Nethermind.Int256;
using Nethermind.Logging;
using Nethermind.Merge.Plugin.Synchronization;
using Nethermind.Synchronization;
using NSubstitute;
using NUnit.Framework;

namespace Nethermind.Merge.Plugin.Test;

public class MergeBetterPeerStrategyTests
{
    [TestCase(7, 2, 6, 4, -1)]
    [TestCase(7, 4, 6, 4, 0)]
    [TestCase(6, 4, 7, 2, 1)]
    [TestCase(3, 4, 6, 2, -1)]
    [TestCase(3, 2, 3, 4, 0)]
    [TestCase(6, 2, 3, 4, 1)]
    public void Compare_with_header_and_peer_return_expected_results(long totalDifficulty, long number, long peerTotalDifficulty, long peerNumber, int expectedResult)
    {
        ISyncPeer syncPeer = Substitute.For<ISyncPeer>();
        syncPeer.TotalDifficulty.Returns((UInt256)peerTotalDifficulty);
        syncPeer.HeadNumber.Returns(peerNumber);
        BlockHeader header = Build.A.BlockHeader.WithTotalDifficulty(totalDifficulty).WithNumber(number).TestObject;

        MergeBetterPeerStrategy betterPeerStrategy = CreateStrategy();

        Assert.AreEqual(expectedResult, betterPeerStrategy.Compare(header, syncPeer));
    }

    [TestCase(7, 2, 6, 4, -1)]
    [TestCase(7, 4, 6, 4, 0)]
    [TestCase(6, 4, 7, 2, 1)]
    [TestCase(3, 4, 6, 2, -1)]
    [TestCase(3, 2, 3, 4, 0)]
    [TestCase(6, 2, 3, 4, 1)]
    public void Compare_with_value_and_peer_return_expected_results(long totalDifficulty, long number, long peerTotalDifficulty, long peerNumber, int expectedResult)
    {
        ISyncPeer syncPeer = Substitute.For<ISyncPeer>();
        syncPeer.TotalDifficulty.Returns((UInt256)peerTotalDifficulty);
        syncPeer.HeadNumber.Returns(peerNumber);

        MergeBetterPeerStrategy betterPeerStrategy = CreateStrategy();
        Assert.AreEqual(expectedResult, betterPeerStrategy.Compare(((UInt256)totalDifficulty, number), syncPeer));
    }

    [TestCase(7, 2, 6, 4, -1)]
    [TestCase(7, 4, 6, 4, 0)]
    [TestCase(6, 4, 7, 2, 1)]
    [TestCase(3, 4, 6, 2, -1)]
    [TestCase(3, 2, 3, 4, 0)]
    [TestCase(6, 2, 3, 4, 1)]
    public void Compare_with_values_return_expected_results(long totalDifficulty, long number, long peerTotalDifficulty, long peerNumber, int expectedResult)
    {
        MergeBetterPeerStrategy betterPeerStrategy = CreateStrategy();
        Assert.AreEqual(expectedResult, betterPeerStrategy.Compare(((UInt256)totalDifficulty, number), ((UInt256)peerTotalDifficulty, peerNumber)));
    }

    [TestCase(6, 4, 7, 2, false)]
    [TestCase(6, 2, 7, 2, false)]
    [TestCase(7, 2, 7, 4, true)]
    [TestCase(3, 4, 5, 2, true)]
    [TestCase(3, 2, 3, 4, false)]
    [TestCase(4, 2, 3, 4, false)]
    public void IsBetterThanLocalChain_return_expected_results(long chainDifficulty, long bestFullBlock, long peerTotalDifficulty, long peerNumber, bool expectedResult)
    {
        ISyncPeer syncPeer = Substitute.For<ISyncPeer>();
        syncPeer.TotalDifficulty.Returns((UInt256)peerTotalDifficulty);
        syncPeer.HeadNumber.Returns(peerNumber);

        MergeBetterPeerStrategy betterPeerStrategy = CreateStrategy();
        Assert.AreEqual(expectedResult, betterPeerStrategy.IsBetterThanLocalChain(((UInt256)peerTotalDifficulty, peerNumber), ((UInt256)chainDifficulty, bestFullBlock)));
    }

    [TestCase(6, 4, 7, 2, false)]
    [TestCase(6, 2, 7, 2, false)]
    [TestCase(3, 4, 5, 2, true)]
    [TestCase(3, 2, 3, 4, true)]
    [TestCase(4, 2, 3, 4, false)]
    [TestCase(3, 4, 3, 2, false)]
    public void IsDesiredPeer_return_expected_results_pre_ttd(long chainDifficulty, long bestHeader, long peerTotalDifficulty, long peerNumber, bool expectedResult)
    {
        ISyncPeer syncPeer = Substitute.For<ISyncPeer>();
        syncPeer.TotalDifficulty.Returns((UInt256)peerTotalDifficulty);
        syncPeer.HeadNumber.Returns(peerNumber);

        MergeBetterPeerStrategy betterPeerStrategy = CreateStrategy();
        Assert.AreEqual(expectedResult, betterPeerStrategy.IsDesiredPeer(((UInt256)peerTotalDifficulty, peerNumber), ((UInt256)chainDifficulty, bestHeader)));
    }

    [TestCase(9, 7, 4, 7, 10, true)]
    [TestCase(9, 8, 2, 7, 7, false)]
    [TestCase(null, 9, 4, 5, 99, false)]
    [TestCase(3, 5,1,3,4, true)]
    public void IsDesiredPeer_return_expected_results_post_ttd(long? pivotNumber, long chainDifficulty, long bestHeader, long peerTotalDifficulty, long peerNumber, bool expectedResult)
    {
        ISyncPeer syncPeer = Substitute.For<ISyncPeer>();
        syncPeer.TotalDifficulty.Returns((UInt256)peerTotalDifficulty);
        syncPeer.HeadNumber.Returns(peerNumber);

        MergeBetterPeerStrategy betterPeerStrategy = CreateStrategy(pivotNumber);
        Assert.AreEqual(expectedResult, betterPeerStrategy.IsDesiredPeer(((UInt256)peerTotalDifficulty, peerNumber), ((UInt256)chainDifficulty, bestHeader)));
    }

    [TestCase(null, true)]
    [TestCase(4, true)]
    [TestCase(5, false)]
    [TestCase(6, false)]
    public void IsLowerThanTerminalTotalDifficulty_return_expected_results(long totalDifficulty, bool expectedResult)
    {
        MergeBetterPeerStrategy betterPeerStrategy = CreateStrategy();
        Assert.AreEqual(expectedResult, betterPeerStrategy.IsLowerThanTerminalTotalDifficulty((UInt256)totalDifficulty));
    }

    private MergeBetterPeerStrategy CreateStrategy(long? beaconPivotNum = null)
    {
        const long ttd = 5;
        IPoSSwitcher poSSwitcher = Substitute.For<IPoSSwitcher>();
        poSSwitcher.TerminalTotalDifficulty.Returns((UInt256)ttd);

        IBeaconPivot beaconPivot = Substitute.For<IBeaconPivot>();
        if (beaconPivotNum != null)
        {
            beaconPivot.BeaconPivotExists().Returns(true);
            beaconPivot.PivotNumber.Returns((long)beaconPivotNum);
        }

        TotalDifficultyBetterPeerStrategy preMergeBetterPeerStrategy = new(LimboLogs.Instance);
        MergeBetterPeerStrategy betterPeerStrategy = new(preMergeBetterPeerStrategy, poSSwitcher, beaconPivot, LimboLogs.Instance);
        return betterPeerStrategy;
    }
}
