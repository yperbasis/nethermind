using System;
using Nethermind.Pipeline;

namespace Nethermind.Pipeline.Plugins.Erc20Transactions.Test
{
    public class Erc20TransactionsPluginTests
    {
        private PipelinePluginConfig _mergeConfig = null!;
        private NethermindApi _context = null!;
        private MergePlugin _plugin = null!;

        [SetUp]
        public void Setup()
        {
            _mergeConfig = new MergeConfig() {Enabled = true, BlockAuthorAccount = TestItem.AddressA.ToString()};
            _context = Build.ContextWithMocks();
            _context.ConfigProvider.GetConfig<IMergeConfig>().Returns(_mergeConfig);
            _context.ConfigProvider.GetConfig<ISyncConfig>().Returns(new SyncConfig());
            _context.MemDbFactory = new MemDbFactory();
            _plugin = new MergePlugin();
        }
        
        [TestCase(true)]
        [TestCase(false)]
        public void Init_merge_plugin_does_not_throw_exception(bool enabled)
        {
            _mergeConfig.Enabled = enabled;
            Assert.DoesNotThrowAsync(async () => await _plugin.Init(_context));
            Assert.DoesNotThrowAsync(async () => await _plugin.InitNetworkProtocol());
            Assert.DoesNotThrowAsync(async () => await _plugin.InitBlockProducer());
            Assert.DoesNotThrowAsync(async () => await _plugin.InitRpcModules());
            Assert.DoesNotThrowAsync(async () => await _plugin.DisposeAsync());
        }
    }
}
