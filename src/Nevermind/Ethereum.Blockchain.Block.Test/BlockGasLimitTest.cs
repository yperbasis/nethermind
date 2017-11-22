﻿using Ethereum.Blockchain.Test;
using NUnit.Framework;

namespace Ethereum.Blockchain.Block.Test
{
    [TestFixture]
    public class BlockGasLimitTest : BlockchainTestBase
    {
        [TestCaseSource(nameof(LoadTests), new object[] { "bcBlockGasLimitTest" })]
        public void Test(BlockchainTest test)
        {
            RunTest(test);
        }
    }
}