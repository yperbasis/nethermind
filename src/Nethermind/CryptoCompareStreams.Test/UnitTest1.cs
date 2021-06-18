using Nethermind.Api;
using NSubstitute;
using NUnit.Framework;

namespace CryptoCompareStreams.Test
{
    public class Tests
    {
        private Plugin _plugin;
        private INethermindApi _api;
        
        [SetUp]
        public void Setup()
        {
            _plugin = new Plugin();
            _api = Substitute.For<INethermindApi>();
        }

        [Test]
        public void Test1()
        {
            _plugin.Init(_api);
            _plugin.InitRpcModules();
        }
    }
}