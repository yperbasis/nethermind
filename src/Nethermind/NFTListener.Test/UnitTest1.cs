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
using Nethermind.State;
using System.Linq;

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
            // plugin = new ListenerPlugin();
            // api = Substitute.For<INethermindApi>();
            // builder = new BlockTreeBuilder();
            // api.StateProvider = Substitute.For<IStateProvider>();
            //
            // plugin.Init(api);
            // plugin.InitRpcModules();
        }

        [Test]
        public void checks_if_contract_is_erc721_correctly()
        {
            Assert.IsTrue(plugin.GetLastNftTransactions().ToArray().Length == 0);
            var blockHash = new Keccak("0xeb00a4db566f5eeef554eab2021d9d2239efd680be16607814269d577fa8248a");
            var transactionHash = new Keccak( "0xe9c2ae38fc17739277098ed804e0b7fb706d4dcb197f2f0024a1daafa5587b7f");
            var tokenAddress = new Address("0x1cd64c5af95c09d95e06798b2aff8eaa7d0173f4");
            var dataString = "0xe7f3ec380000000000000000000000000000000000000000000000000000000000000080000000000000000000000000c8541aae19c5069482239735ad64fac3dcc52ca2000000000000000000000000000000000000000000000000000000003b9ac9ff00000000000000000000000000000000000000000000000000000000000000010000000000000000000000000000000000000000000000000000000000000006534e465450440000000000000000000000000000000000000000000000000000";

            Block block = Build.A.Block.WithTransactions(Berlin.Instance, new Transaction { Hash = transactionHash, Data = Bytes.FromHexString(dataString), To =  tokenAddress }).TestObject;

            BlockProcessedEventArgs args = new BlockProcessedEventArgs(block, Array.Empty<TxReceipt>());

            var code = Bytes.FromHexString("");

            api.StateProvider.GetCode(tokenAddress).Returns(code);

            api.MainBlockProcessor.BlockProcessed += Raise.EventWith(new object(), args); 

            Assert.IsTrue(plugin.GetLastNftTransactions().ToArray().Length > 0);
        }

        [Test]
        public void test1()
        {
            var data =
                "23b872dd00000000000000000000000087dc88b44e999e0d1d7c0c78374162c92cdfd984000000000000000000000000a08b1126743e6a2957a5daa67ad84757065548a90000000000000000000000000000000000000000000000000000000000000001";
            var signature = data.Substring(0, 8);
            var tokenID = data.Substring(136, 64);
            Assert.AreEqual("23b872dd", signature);
            Assert.AreEqual("0000000000000000000000000000000000000000000000000000000000000001", tokenID);
        }
    }
}
