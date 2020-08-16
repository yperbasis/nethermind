//  Copyright (c) 2018 Demerzel Solutions Limited
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
using Microsoft.Extensions.Logging;
using Nethermind.Core2.Containers;
using Nethermind.Core2.Crypto;
using Nethermind.Core2.Types;

namespace Nethermind.BeaconNode.Eth1Bridge
{
    internal static class Log
    {
        // Event IDs: ABxx (based on Theory of Reply Codes)
        
        // Event ID Type:
        // 6bxx debug - general
        // 7bxx debug - test
        // 1bxx info - preliminary
        // 2bxx info - completion
        // 3bxx info - intermediate
        // 8bxx info - finalization
        // 4bxx warning
        // 5bxx error
        // 9bxx critical
        
        // Event ID Category:
        // a0xx core service, worker, configuration, peering
        // a1xx beacon chain, incl. state transition
        // a2xx fork choice
        // a3xx deposit contract, Eth1, genesis
        // a4xx honest validator, API
        // a5xx custody game
        // a6xx shard data chains
        // a9xx miscellaneous / other
        
        // 1bxx preliminary

        public static readonly Action<ILogger, string, string, int, Exception?> PeeringWorkerStarting =
            LoggerMessage.Define<string, string, int>(LogLevel.Information,
                new EventId(1350, nameof(PeeringWorkerStarting)),
                "Eth1 bridge {ProductTokenVersion} worker starting; {Environment} environment [{ThreadId}]");
        
        public static readonly Action<ILogger, int, Exception?> Eth1GenesisWaitingForDeposits =
            LoggerMessage.Define<int>(LogLevel.Information,
                new EventId(1351, nameof(Eth1GenesisWaitingForDeposits)),
                "Waiting for more Eth1 deposits before genesis. Current deposits {Deposits}");
        
        public static readonly Action<ILogger, ulong, ulong, Exception?> Eth1GenesisImportingBlocks =
            LoggerMessage.Define<ulong, ulong>(LogLevel.Information,
                new EventId(1352, nameof(Eth1GenesisWaitingForDeposits)),
                "Importing Eth1 blocks from {From} to {To} for genesis.");

        // 2bxx
        
        public static readonly Action<ILogger, Bytes32, ulong, uint, int, Exception?> Eth1GenesisSuccess =
            LoggerMessage.Define<Bytes32, ulong, uint, int>(LogLevel.Information,
                new EventId(2351, nameof(Eth1GenesisSuccess)),
                "Eth genesis succeeded with block hash {BlockHash}, genesis time {GenesisTime:n0}, and {DepositCount} deposits, at check {CheckGenesisCount}.");
        
        // 4bxx warning
        
        public static readonly Action<ILogger, ulong, ulong, ulong, Exception?> QuickStartEth1TimestampTooLow =
            LoggerMessage.Define<ulong, ulong, ulong>(LogLevel.Warning,
                new EventId(4390, nameof(QuickStartEth1TimestampTooLow)),
                "Quick start Eth1Timestamp {ConfiguredEth1Timestamp} to low for genesis {Genesis}; using {MinimumEth1Timestamp}.");
        public static readonly Action<ILogger, ulong, ulong, ulong, Exception?> QuickStartEth1TimestampTooHigh =
            LoggerMessage.Define<ulong, ulong, ulong>(LogLevel.Warning,
                new EventId(4391, nameof(QuickStartEth1TimestampTooHigh)),
                "Quick start Eth1Timestamp {ConfiguredEth1Timestamp} to high for genesis {Genesis}; using {MaximumEth1Timestamp}.");

        public static readonly Action<ILogger, ulong, ulong, Exception?> MockedQuickStart =
            LoggerMessage.Define<ulong, ulong>(LogLevel.Warning,
                new EventId(4900, nameof(MockedQuickStart)),
                "Mocked quick start with genesis time {GenesisTime:n0} and {ValidatorCount} validators.");
        
        public static readonly Action<ILogger, string, string, Exception?> Eth1Disconnected =
            LoggerMessage.Define<string, string>(LogLevel.Warning,
                new EventId(4392, nameof(Eth1Disconnected)),
                "Lost connection to Eth1 because {Reason}. {Description}.");

        // 5bxx error
        public static readonly Action<ILogger, Exception?> Eth1ConnectionFailure =
            LoggerMessage.Define(LogLevel.Error,
                new EventId(5352, nameof(Eth1ConnectionFailure)),
                "Failed to connect to Eth1.");
        
        public static readonly Action<ILogger, Exception?> Eth1CommunicationFailure =
            LoggerMessage.Define(LogLevel.Error,
                new EventId(5353, nameof(Eth1CommunicationFailure)),
                "Error communicating with Eth1.");

        // 8bxx finalization

        // 9bxx critical
        public static readonly Action<ILogger, int, Exception?> Eth1GenesisFailure =
            LoggerMessage.Define<int>(LogLevel.Error,
                new EventId(9351, nameof(Eth1GenesisFailure)),
                "Eth genesis failed after {CheckGenesisCount} checks.");
    }
}
