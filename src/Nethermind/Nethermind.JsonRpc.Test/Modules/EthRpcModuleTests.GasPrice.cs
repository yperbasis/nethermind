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

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Nethermind.Blockchain;
using Nethermind.Blockchain.Find;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Core.Specs;
using Nethermind.Core.Test.Builders;
using Nethermind.Facade;
using Nethermind.Int256;
using Nethermind.JsonRpc.Modules.Eth;
using Nethermind.Logging;
using Nethermind.State;
using Nethermind.TxPool;
using Nethermind.Wallet;
using NSubstitute;
using NUnit.Framework;
using static Nethermind.JsonRpc.Test.Modules.TestBlockConstructor;

namespace Nethermind.JsonRpc.Test.Modules
{
    public partial class EthRpcModuleTests
    {
        [Test]
        public void Eth_gasPrice_WhenHeadBlockIsNull_ThrowsException()
        {
            IBlockFinder blockFinder = Substitute.For<IBlockFinder>();
            blockFinder.FindHeadBlock().Returns((Block) null);
            EthRpcModule testEthRpcModule = GetTestEthRpcModule(blockFinder: blockFinder);
            
            Action gasPriceCall = () => testEthRpcModule.eth_gasPrice();
            
            gasPriceCall.Should().Throw<Exception>().WithMessage("Head Block was not found.");
        }

        [Test]
        public void Eth_gasPrice_ForBlockTreeWithBlocks_CreatesMatchingBlockDict()
        {
            Block testBlockA = GetTestBlockA();
            Block testBlockB = GetTestBlockB();
            IBlockFinder blockFinder = Substitute.For<IBlockFinder>();
            EthRpcModule testEthRpcModule = GetTestEthRpcModule(blockFinder:blockFinder);
            blockFinder.FindBlock(0).Returns(testBlockA);
            blockFinder.FindBlock(1).Returns(testBlockB);
            blockFinder.FindHeadBlock().Returns(testBlockB);
            Dictionary<long, Block> expected = new Dictionary<long, Block>
            {
                {0, testBlockA},
                {1, testBlockB}
            };
            
            testEthRpcModule.eth_gasPrice();
            
            testEthRpcModule.BlockNumberToBlockDictionary.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Eth_gasPrice_GivenValidHeadBlock_CallsGasPriceEstimateFromGasPriceOracle()
        {
            IBlockFinder blockFinder = Substitute.For<IBlockFinder>();
            IGasPriceOracle gasPriceOracle = Substitute.For<IGasPriceOracle>();
            EthRpcModule testEthRpcModule = GetTestEthRpcModule(blockFinder, gasPriceOracle);
            Block testBlock = Build.A.Block.Genesis.TestObject;
            blockFinder.FindHeadBlock().Returns(testBlock);
            blockFinder.FindBlock(Arg.Is<long>(a => a == 0)).Returns(testBlock);

            testEthRpcModule.eth_gasPrice();
            
            gasPriceOracle.Received(1).GasPriceEstimate(Arg.Any<Block>(), Arg.Any<Dictionary<long, Block>>());
        }

        [Test]
        public void Eth_gasPrice_BlocksAvailableLessThanBlocksToCheck_ShouldBeSuccessful()
        {
            Block[] blocks = GetThreeTestBlocks();

            BlockTreeSetup blockTreeSetup = new BlockTreeSetup(blocks: blocks, blockLimit: 4);
            ResultWrapper<UInt256?> resultWrapper = blockTreeSetup.EthRpcModule.eth_gasPrice();
            
            resultWrapper.Result.Should().Be(Result.Success); 
        }

        [Test]
        public void Eth_gasPrice_NumTxInMinBlocksGreaterThanBlockLimit_GetTxFromBlockLimitBlocks()
        {
            Block[] blocks = GetThreeTestBlocks();
            BlockTreeSetup blockTreeSetup = new BlockTreeSetup(blocks: blocks, blockLimit: 2);
            
            blockTreeSetup.EthRpcModule.eth_gasPrice();
            
            List<UInt256> expected = new List<UInt256>{3, 4, 5, 6};
            blockTreeSetup.GasPriceOracle.TxGasPriceList.Should().Equal(expected);
        }

        private static Block[] GetThreeTestBlocks()
        {
            Block firstBlock = Build.A.Block.WithNumber(0).WithParentHash(Keccak.Zero).WithTransactions(
                Build.A.Transaction.WithGasPrice(1).SignedAndResolved(TestItem.PrivateKeyA).WithNonce(0).TestObject,
                Build.A.Transaction.WithGasPrice(2).SignedAndResolved(TestItem.PrivateKeyB).WithNonce(0).TestObject
            ).TestObject;

            Block secondBlock = Build.A.Block.WithNumber(1).WithParentHash(firstBlock.Hash).WithTransactions(
                Build.A.Transaction.WithGasPrice(3).SignedAndResolved(TestItem.PrivateKeyC).WithNonce(0).TestObject,
                Build.A.Transaction.WithGasPrice(4).SignedAndResolved(TestItem.PrivateKeyD).WithNonce(0).TestObject
            ).TestObject;

            Block thirdBlock = Build.A.Block.WithNumber(2).WithParentHash(secondBlock.Hash).WithTransactions(
                Build.A.Transaction.WithGasPrice(5).SignedAndResolved(TestItem.PrivateKeyA).WithNonce(1).TestObject,
                Build.A.Transaction.WithGasPrice(6).SignedAndResolved(TestItem.PrivateKeyB).WithNonce(1).TestObject
            ).TestObject;
           
            return new[]{firstBlock, secondBlock, thirdBlock};
        }

        private EthRpcModule GetTestEthRpcModule(IBlockFinder blockFinder = null, IGasPriceOracle gasPriceOracle = null)
        {
            return new EthRpcModule
            (
                Substitute.For<IJsonRpcConfig>(),
                Substitute.For<IBlockchainBridge>(),
                blockFinder ?? Substitute.For<IBlockFinder>(),
                Substitute.For<IStateReader>(),
                Substitute.For<ITxPool>(),
                Substitute.For<ITxSender>(),
                Substitute.For<IWallet>(),
                Substitute.For<ILogManager>(),
                Substitute.For<ISpecProvider>(),
                gasPriceOracle ?? Substitute.For<IGasPriceOracle>()
            );
        }

        public class BlockTreeSetup
        {
            public Block[] Blocks { get; private set; }
            private BlockTree BlockTree { get; set; }
            public EthRpcModule EthRpcModule { get; }
            public IGasPriceOracle GasPriceOracle { get; private set; }

            public BlockTreeSetup(
                Block[] blocks = null,
                bool addBlocks = false,
                IGasPriceOracle gasPriceOracle = null, 
                int? blockLimit = null,
                IReleaseSpec releaseSpec = null,
                UInt256? ignoreUnder = null,
                UInt256? baseFee = null,
                ITxInsertionManager txInsertionManager = null,
                IHeadBlockChangeManager headBlockChangeManager = null)
            {
                GetBlocks(blocks, addBlocks);

                InitializeAndAddToBlockTree();

                GasPriceOracle = gasPriceOracle ?? GetGasPriceOracle(releaseSpec, ignoreUnder, blockLimit, baseFee, txInsertionManager, headBlockChangeManager);

                EthRpcModule = new EthRpcModuleTests().GetTestEthRpcModule(BlockTree, GasPriceOracle);
            }

            private void InitializeAndAddToBlockTree()
            {
                BlockTree = BuildABlockTreeWithGenesisBlock(Blocks[0]);
                foreach (Block block in Blocks)
                {
                    BlockTreeBuilder.AddBlock(BlockTree, block);
                }
            }

            private void GetBlocks(Block[] blocks, bool shouldAddToBlocks)
            {
                if (NoBlocksGiven(blocks) || shouldAddToBlocks)
                {
                    Blocks = GetBlockArray();
                    if (shouldAddToBlocks)
                    {
                        AddExtraBlocksToArray(blocks);
                    }
                }
                else
                {
                    Blocks = blocks;
                }
            }
            
            private static bool NoBlocksGiven(Block[] blocks)
            {
                return blocks == null;
            }

            private BlockTree BuildABlockTreeWithGenesisBlock(Block genesisBlock)
            {
                return Build.A.BlockTree(genesisBlock).TestObject;
            }

            private Block[] GetBlockArray()
            {
                Block firstBlock = Build.A.Block.WithNumber(0).WithParentHash(Keccak.Zero).WithTransactions(
                        Build.A.Transaction.WithGasPrice(1).SignedAndResolved(TestItem.PrivateKeyA).WithNonce(0)
                            .TestObject,
                        Build.A.Transaction.WithGasPrice(2).SignedAndResolved(TestItem.PrivateKeyB).WithNonce(0)
                            .TestObject)
                    .TestObject;
                Block secondBlock = Build.A.Block.WithNumber(2).WithParentHash(firstBlock.Hash).WithTransactions(
                        Build.A.Transaction.WithGasPrice(3).SignedAndResolved(TestItem.PrivateKeyC).WithNonce(0)
                            .TestObject)
                    .TestObject;
                Block thirdBlock = Build.A.Block.WithNumber(3).WithParentHash(secondBlock.Hash).WithTransactions(
                        Build.A.Transaction.WithGasPrice(5).SignedAndResolved(TestItem.PrivateKeyD).WithNonce(0)
                            .TestObject)
                    .TestObject;
                Block fourthBlock = Build.A.Block.WithNumber(4).WithParentHash(thirdBlock.Hash).WithTransactions(
                        Build.A.Transaction.WithGasPrice(4).SignedAndResolved(TestItem.PrivateKeyA).WithNonce(1)
                            .TestObject)
                    .TestObject;
                Block fifthBlock = Build.A.Block.WithNumber(5).WithParentHash(fourthBlock.Hash).WithTransactions(
                        Build.A.Transaction.WithGasPrice(6).SignedAndResolved(TestItem.PrivateKeyB).WithNonce(1)
                            .TestObject)
                    .TestObject;
                return new[] {firstBlock, secondBlock, thirdBlock, fourthBlock, fifthBlock};
            }

            private void AddExtraBlocksToArray(Block[] blocks)
            {
                List<Block> listBlocks = Blocks.ToList();
                foreach (Block block in blocks)
                {
                    listBlocks.Add(block);
                }

                Blocks = listBlocks.ToArray();
            }
            
            private IGasPriceOracle GetGasPriceOracle(
                IReleaseSpec releaseSpec, 
                UInt256? ignoreUnder,
                int? blockLimit, 
                UInt256? baseFee,
                ITxInsertionManager txInsertionManager,
                IHeadBlockChangeManager headBlockChangeManager)
            {
                GasPriceOracle gasPriceOracle = new GasPriceOracle(releaseSpec, ignoreUnder, blockLimit, baseFee, 
                    txInsertionManager, headBlockChangeManager);
                return gasPriceOracle;
            }
        }
    }
}