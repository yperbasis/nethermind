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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Nethermind.Analytics;
using Nethermind.Api;
using Nethermind.Blockchain.Synchronization;
using Nethermind.Config;
using Nethermind.Config.Test;
using Nethermind.Core;
using Nethermind.EthStats;
using Nethermind.Grpc;
using Nethermind.JsonRpc;
using Nethermind.Monitoring.Config;
using Nethermind.Network.Config;
using Nethermind.Db.Blooms;
using Nethermind.Db.Rocks.Config;
using Nethermind.Init;
using Nethermind.Logging;
using Nethermind.Merge.Plugin;
using Nethermind.TxPool;
using NUnit.Framework;
using Nethermind.Specs.ChainSpecStyle;
using Nethermind.Serialization.Json;

namespace Nethermind.Runner.Test
{
    [Parallelizable(ParallelScope.All)]
    [TestFixture]
    public class ChainspecFilesTests
    {

        private IJsonSerializer _jsonSerializer = new EthereumJsonSerializer();
        private IChainSpecLoader _loader;
        public ILogger _logger;
        public ChainspecFilesTests()
        {
            _loader = new ChainSpecLoader(_jsonSerializer);
            _logger = NSubstitute.Substitute.For<ILogger>();
        }

        [TestCase("foundation", 1UL)]
        [TestCase("chainspec/foundation", 1UL)]
        [TestCase("chainspec/foundation.json", 1UL)]
        public void different_formats_to_chainspecPath(string chainspecPath, ulong chainId)
        {
            var chainspec = _loader.LoadEmbeddedOrFromFile(chainspecPath, _logger);
            Assert.AreEqual(chainspec.ChainId, chainId);
        }

        [TestCase("testspec.json", 5UL)]
        public void Chainspec_from_file(string chainspecPath, ulong chainId)
        {
            var chainspec = _loader.LoadEmbeddedOrFromFile(chainspecPath, _logger);
            Assert.AreEqual(chainspec.ChainId, chainId);
        }

        [TestCase("chainspec/custom_chainspec_that_does_not_exist.json")]
        public void ChainspecNotFound(string chainspecPath)
        {
            Assert.Catch(new TestDelegate(() =>
            {
                _loader.LoadEmbeddedOrFromFile(chainspecPath, _logger);
            }));
        }

    }
}
