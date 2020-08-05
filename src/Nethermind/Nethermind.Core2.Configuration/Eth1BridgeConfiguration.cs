using System;
using System.Net;

namespace Nethermind.Core2.Configuration
{
    public class Eth1BridgeConfiguration
    {
        public Uri EndPoint { get; set; }

        public string DepositContractAddress { get; set; }
        
        public ulong DepositContractDeployBlock { get; set; }
        
    }
}
