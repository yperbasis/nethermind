using System;
using System.Net;

namespace Nethermind.Core2.Configuration
{
    public class Eth1BridgeConfiguration
    {
        public Uri EndPoint { get; set; } = new Uri("ws://localhost:8545");
        public ulong ChainId { get; set; } = 1;
        public ulong NetworkId { get; set; } = 1;
        public string DepositContractAddress { get; set; } = string.Empty;
        public ulong DepositContractDeployBlock { get; set; } = 10_000_000;
        public ulong MaxLogsBatch { get; set; } = 1000;
    }
}
