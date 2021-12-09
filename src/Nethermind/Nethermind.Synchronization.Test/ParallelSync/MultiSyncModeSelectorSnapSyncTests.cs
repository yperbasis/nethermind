using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethermind.Synchronization.ParallelSync;
using NUnit.Framework;

namespace Nethermind.Synchronization.Test.ParallelSync
{
    [Parallelizable(ParallelScope.All)]
    [TestFixture]
    public class MultiSyncModeSelectorSnapSyncTests : MultiSyncModeSelectorTestsBase
    {

        [Test]
        public void Simple_snap_sync()
        {
            Scenario.GoesLikeThis()
                .IfThisNodeHasNeverSyncedBefore()
                .AndGoodPeersAreKnown()
                .WhenSnapSyncWithoutFastBlocksIsConfigured()
                .TheSyncModeShouldBe(SyncMode.FastSync);
        }

        [Test]
        public void Simple_snap_sync_with_fast_blocks()
        {
            // note that before we download at least one header we cannot start fast sync
            Scenario.GoesLikeThis()
                .IfThisNodeHasNeverSyncedBefore()
                .AndGoodPeersAreKnown()
                .WhenSnapSyncWithFastBlocksIsConfigured()
                .TheSyncModeShouldBe(SyncMode.FastHeaders);
        }

        [Test]
        public void Finished_fast_sync_but_not_snap_sync()
        {
            Scenario.GoesLikeThis()
                .IfThisNodeJustFinishedFastBlocksAndFastSync()
                .AndGoodPeersAreKnown()
                .WhenSnapSyncWithFastBlocksIsConfigured()
                .TheSyncModeShouldBe(SyncMode.SnapSync);
        }

        [Test]
        public void Finished_fast_sync_but_not_snap_sync_and_fast_blocks_in_progress()
        {
            Scenario.GoesLikeThis()
                .ThisNodeFinishedFastSyncButNotFastBlocks()
                .AndGoodPeersAreKnown()
                .WhenSnapSyncWithFastBlocksIsConfigured()
                .TheSyncModeShouldBe(SyncMode.SnapSync | SyncMode.FastHeaders);
        }

        [Test]
        public void Finished_snap_node_but_not_fast_blocks()
        {
            Scenario.GoesLikeThis()
                .ThisNodeFinishedFastSyncButNotFastBlocks()
                .WhenSnapSyncWithFastBlocksIsConfigured()
                .AndGoodPeersAreKnown()
                .TheSyncModeShouldBe(SyncMode.SnapSync | SyncMode.FastHeaders);
        }
    }
}
