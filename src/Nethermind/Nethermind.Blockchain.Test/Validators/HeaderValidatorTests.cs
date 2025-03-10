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
using System.Numerics;
using Nethermind.Consensus;
using Nethermind.Consensus.Ethash;
using Nethermind.Consensus.Validators;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Core.Specs;
using Nethermind.Core.Test;
using Nethermind.Core.Test.Builders;
using Nethermind.Crypto;
using Nethermind.Db;
using Nethermind.Db.Blooms;
using Nethermind.Int256;
using Nethermind.Logging;
using Nethermind.Merge.Plugin;
using Nethermind.Specs;
using Nethermind.Specs.Forks;
using Nethermind.Specs.Test;
using Nethermind.State.Repositories;
using NSubstitute;
using NUnit.Framework;

namespace Nethermind.Blockchain.Test.Validators
{
    [TestFixture]
    public class HeaderValidatorTests
    {
        private IHeaderValidator _validator;
        private ISealValidator _ethash;
        private TestLogger _testLogger;
        private Block _parentBlock;
        private Block _block;
        private IBlockTree _blockTree;
        private ISpecProvider _specProvider;

        [SetUp]
        public void Setup()
        {
            EthashDifficultyCalculator calculator = new(new SingleReleaseSpecProvider(Frontier.Instance, ChainId.Mainnet));
            _ethash = new EthashSealValidator(LimboLogs.Instance, calculator, new CryptoRandom(), new Ethash(LimboLogs.Instance), Timestamper.Default);
            _testLogger = new TestLogger();
            MemDb blockInfoDb = new();
            _blockTree = new BlockTree(new MemDb(), new MemDb(), blockInfoDb, new ChainLevelInfoRepository(blockInfoDb), FrontierSpecProvider.Instance, Substitute.For<IBloomStorage>(), LimboLogs.Instance);
            _specProvider = new SingleReleaseSpecProvider(Byzantium.Instance, 3);

            _validator = new HeaderValidator(_blockTree, _ethash, _specProvider, new OneLoggerLogManager(_testLogger));
            _parentBlock = Build.A.Block.WithDifficulty(1).TestObject;
            _block = Build.A.Block.WithParent(_parentBlock)
                .WithDifficulty(131072)
                .WithMixHash(new Keccak("0xd7db5fdd332d3a65d6ac9c4c530929369905734d3ef7a91e373e81d0f010b8e8"))
                .WithNonce(0).TestObject;

            _blockTree.SuggestBlock(_parentBlock);
            _blockTree.SuggestBlock(_block);
        }

        [Test]
        public void Valid_when_valid()
        {
            _block.Header.SealEngineType = SealEngineType.None;
            bool result = _validator.Validate(_block.Header);
            if (!result)
            {
                foreach (string error in _testLogger.LogList)
                {
                    Console.WriteLine(error);
                }
            }

            Assert.True(result);
        }

        [Test]
        public void When_gas_limit_too_high()
        {
            _block.Header.GasLimit = _parentBlock.Header.GasLimit + (long)BigInteger.Divide(_parentBlock.Header.GasLimit, 1024);
            _block.Header.SealEngineType = SealEngineType.None;
            _block.Header.Hash = _block.CalculateHash();

            bool result = _validator.Validate(_block.Header);
            Assert.False(result);
        }

        [Test]
        public void When_gas_limit_just_correct_high()
        {
            _block.Header.GasLimit = _parentBlock.Header.GasLimit + (long)BigInteger.Divide(_parentBlock.Header.GasLimit, 1024) - 1;
            _block.Header.SealEngineType = SealEngineType.None;
            _block.Header.Hash = _block.CalculateHash();

            bool result = _validator.Validate(_block.Header);
            Assert.True(result);
        }

        [Test]
        public void When_gas_limit_just_correct_low()
        {
            _block.Header.GasLimit = _parentBlock.Header.GasLimit - (long)BigInteger.Divide(_parentBlock.Header.GasLimit, 1024) + 1;
            _block.Header.SealEngineType = SealEngineType.None;
            _block.Header.Hash = _block.CalculateHash();

            bool result = _validator.Validate(_block.Header);
            Assert.True(result);
        }

        [Test]
        public void When_gas_limit_is_just_too_low()
        {
            _block.Header.GasLimit = _parentBlock.Header.GasLimit - (long)BigInteger.Divide(_parentBlock.Header.GasLimit, 1024);
            _block.Header.SealEngineType = SealEngineType.None;
            _block.Header.Hash = _block.CalculateHash();

            bool result = _validator.Validate(_block.Header);
            Assert.False(result);
        }

        [Test]
        public void When_gas_used_above_gas_limit()
        {
            _block.Header.GasUsed = _parentBlock.Header.GasLimit + 1;
            _block.Header.SealEngineType = SealEngineType.None;
            _block.Header.Hash = _block.CalculateHash();

            bool result = _validator.Validate(_block.Header);
            Assert.False(result);
        }

        [Test]
        public void When_no_parent_invalid()
        {
            _block.Header.ParentHash = Keccak.Zero;
            _block.Header.SealEngineType = SealEngineType.None;
            _block.Header.Hash = _block.CalculateHash();
            _block.Header.MaybeParent = null;

            bool result = _validator.Validate(_block.Header);
            Assert.False(result);
        }

        [Test]
        public void When_timestamp_same_as_parent()
        {
            _block.Header.Timestamp = _parentBlock.Header.Timestamp;
            _block.Header.SealEngineType = SealEngineType.None;
            _block.Header.Hash = _block.CalculateHash();

            bool result = _validator.Validate(_block.Header);
            Assert.False(result);
        }

        [Test]
        public void When_extra_data_too_long()
        {
            _block.Header.ExtraData = new byte[33];
            _block.Header.SealEngineType = SealEngineType.None;
            _block.Header.Hash = _block.CalculateHash();

            bool result = _validator.Validate(_block.Header);
            Assert.False(result);
        }

        [Test]
        public void When_incorrect_difficulty_then_invalid()
        {
            _block.Header.Difficulty = 1;
            _block.Header.SealEngineType = SealEngineType.None;
            _block.Header.Hash = _block.CalculateHash();

            bool result = _validator.Validate(_block.Header);
            Assert.False(result);
        }

        [Test]
        public void When_incorrect_number_then_invalid()
        {
            _block.Header.Number += 1;
            _block.Header.SealEngineType = SealEngineType.None;
            _block.Header.Hash = _block.CalculateHash();

            bool result = _validator.Validate(_block.Header);
            Assert.False(result);
        }

        [TestCase(10000000, 4, 20000000, true)]
        [TestCase(10000000, 4, 20019530, true)]
        [TestCase(10000000, 4, 20019531, false)]
        [TestCase(10000000, 4, 19980470, true)]
        [TestCase(10000000, 4, 19980469, false)]
        [TestCase(20000000, 5, 20000000, true)]
        [TestCase(20000000, 5, 20019530, true)]
        [TestCase(20000000, 5, 20019531, false)]
        [TestCase(20000000, 5, 19980470, true)]
        [TestCase(20000000, 5, 19980469, false)]
        [TestCase(40000000, 5, 40039061, true)]
        [TestCase(40000000, 5, 40039062, false)]
        [TestCase(40000000, 5, 39960939, true)]
        [TestCase(40000000, 5, 39960938, false)]
        public void When_gaslimit_is_on_london_fork(long parentGasLimit, long blockNumber, long gasLimit, bool expectedResult)
        {
            OverridableReleaseSpec spec = new(London.Instance)
            {
                Eip1559TransitionBlock = 5
            };
            TestSpecProvider specProvider = new(spec);
            _validator = new HeaderValidator(_blockTree, _ethash, specProvider, new OneLoggerLogManager(_testLogger));
            _parentBlock = Build.A.Block.WithDifficulty(1)
                            .WithGasLimit(parentGasLimit)
                            .WithNumber(blockNumber)
                            .TestObject;
            _block = Build.A.Block.WithParent(_parentBlock)
                .WithDifficulty(131072)
                .WithMixHash(new Keccak("0xd7db5fdd332d3a65d6ac9c4c530929369905734d3ef7a91e373e81d0f010b8e8"))
                .WithGasLimit(gasLimit)
                .WithNumber(_parentBlock.Number + 1)
                .WithBaseFeePerGas(BaseFeeCalculator.Calculate(_parentBlock.Header, specProvider.GetSpec(_parentBlock.Number + 1)))
                .WithNonce(0).TestObject;
            _block.Header.SealEngineType = SealEngineType.None;
            _block.Header.Hash = _block.CalculateHash();

            bool result = _validator.Validate(_block.Header, _parentBlock.Header);
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void When_gas_limit_is_long_max_value()
        {
            _validator = new HeaderValidator(_blockTree, _ethash, _specProvider, new OneLoggerLogManager(_testLogger));
            _parentBlock = Build.A.Block.WithDifficulty(1)
                .WithGasLimit(long.MaxValue)
                .WithNumber(5)
                .TestObject;
            _block = Build.A.Block.WithParent(_parentBlock)
                .WithDifficulty(131072)
                .WithMixHash(new Keccak("0xd7db5fdd332d3a65d6ac9c4c530929369905734d3ef7a91e373e81d0f010b8e8"))
                .WithGasLimit(long.MaxValue)
                .WithNumber(_parentBlock.Number + 1)
                .WithNonce(0).TestObject;
            _block.Header.SealEngineType = SealEngineType.None;
            _block.Header.Hash = _block.CalculateHash();

            bool result = _validator.Validate(_block.Header, _parentBlock.Header);

            Assert.True(result);
        }

        [Test]
        public void When_block_number_is_negative()
        {
            _block.Header.Number = -1;
            _block.Header.Hash = _block.CalculateHash();

            bool result = _validator.Validate(_block.Header);
            Assert.False(result);
        }

        [Test]
        public void When_gas_used_is_negative()
        {
            _block.Header.GasUsed = -1;
            _block.Header.Hash = _block.CalculateHash();

            bool result = _validator.Validate(_block.Header);
            Assert.False(result);
        }

        [TestCase(0)]
        [TestCase(null)]
        public void When_total_difficulty_0_or_null_we_should_skip_total_difficulty_validation(long? totalDifficulty)
        {
            _block.Header.Difficulty = 1;
            _block.Header.TotalDifficulty = totalDifficulty == null ? null : (UInt256)totalDifficulty;
            _block.Header.SealEngineType = SealEngineType.None;
            _block.Header.Hash = _block.CalculateHash();

            HeaderValidator validator = new HeaderValidator(_blockTree, Always.Valid, _specProvider, new OneLoggerLogManager(_testLogger));
            bool result = validator.Validate(_block.Header);
            Assert.True(result);
        }

        [Test]
        public void When_gas_limit_is_negative()
        {
            _block.Header.GasLimit = -1;
            _block.Header.Hash = _block.CalculateHash();

            bool result = _validator.Validate(_block.Header);
            Assert.False(result);
        }
    }
}
