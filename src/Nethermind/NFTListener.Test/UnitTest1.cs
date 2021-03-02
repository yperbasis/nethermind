using Nethermind.Api;
using Nethermind.Blockchain.Processing;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using NSubstitute;
using NUnit.Framework;
using Nethermind.Core.Extensions;
using Nethermind.Core.Test.Builders;
using Nethermind.Blockchain;
using System;
using Nethermind.Core.Test.Blockchain;
using Nethermind.Specs.Forks;

namespace NFTListener.Test
{
    public class Tests
    {
        ListenerPlugin plugin;
        INethermindApi api;
        BlockTreeBuilder builder;

        [SetUp]
        public void Setup()
        {
            plugin = new ListenerPlugin();
            api = Substitute.For<INethermindApi>();
            builder = new BlockTreeBuilder();

            plugin.Init(api);
        }

        [Test]
        public void Test1()
        {
            var blockHash = new Keccak("0xeb00a4db566f5eeef554eab2021d9d2239efd680be16607814269d577fa8248a");
            var transactionHash = new Keccak( "0x63e938f313e542d404e646974577ca847f14de0ca0e1aea1eccd6184399c0905");
            var dataString = "0x23b872dd000000000000000000000000e052113bd7d7700d623414a0a4585bcae754e9d5000000000000000000000000cd06f1a6254061e816b6871a2e2e69a412c4b2a800000000000000000000000000000000000000000000000000000002dd234275";
            Block block = Build.A.Block.WithTransactions(Berlin.Instance, new Transaction { Hash = transactionHash, Data = Bytes.FromHexString(dataString) }).TestObject;

            BlockProcessedEventArgs args = new BlockProcessedEventArgs(block, Array.Empty<TxReceipt>());

            api.MainBlockProcessor.BlockProcessed += Raise.EventWith(new object(), args);
        }
    }
}