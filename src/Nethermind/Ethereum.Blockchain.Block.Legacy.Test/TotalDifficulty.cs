using System.Collections.Generic;
using System.Threading.Tasks;
using Ethereum.Test.Base;
using NUnit.Framework;

namespace Ethereum.Blockchain.Block.Legacy.Test
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class TotalDifficultyTests : BlockchainTestBase
    {
        [TestCaseSource(nameof(LoadTests))]
        public async Task Test(BlockchainTest test)
        {
            await RunTest(test);
        }
        public static IEnumerable<BlockchainTest> LoadTests()
        {
            var loader = new TestsSourceLoader(new LoadLegacyBlockchainTestsStrategy(), "bcTotalDifficultyTest");
            return (IEnumerable<BlockchainTest>)loader.LoadTests();
        }
    }
}
