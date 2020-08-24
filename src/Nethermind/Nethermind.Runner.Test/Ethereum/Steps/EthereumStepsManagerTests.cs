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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions.Execution;
using Nethermind.Config;
using Nethermind.Core.Crypto;
using Nethermind.Logging;
using Nethermind.Network;
using Nethermind.Network.Config;
using Nethermind.Runner.Ethereum;
using Nethermind.Runner.Ethereum.Context;
using Nethermind.Runner.Ethereum.Steps;
using Nethermind.Serialization.Json;
using NUnit.Framework;

namespace Nethermind.Runner.Test.Ethereum.Steps
{
    [TestFixture]
    public class EthereumStepsManagerTests
    {
        [Test]
        //Test source: https://github.com/ethereum/tests/blob/develop/BlockchainTests/GeneralStateTests/stMemoryTest/callDataCopyOffset.json
        //Log file: https://hivetests.ethdevops.io/viewer.html?file=results/nethermind_latest/client-e6ba769d.log
        public async Task Validate_genesis_from_hive_chainspec_correctly_1()
        {
            var configProvider = new ConfigProvider();
            Environment.SetEnvironmentVariable("NETHERMIND_HIVE_ENABLED", "true");

            var initConfig = configProvider.GetConfig<IInitConfig>();
            initConfig.DiagnosticMode = DiagnosticMode.MemDb;
            initConfig.HiveChainSpecPath = "chainspec/hive2.json";

            Assembly stepsAssembly = Assembly.GetAssembly(typeof(IStep));
            var serializer = new EthereumJsonSerializer();

            var factory = new EthereumRunnerContextFactory(configProvider, serializer, LimboLogs.Instance);
            factory.Context.EthereumJsonSerializer = new EthereumJsonSerializer();

            INetworkConfig networkConfig = configProvider.GetConfig<INetworkConfig>();
            var ipResolver = new IPResolver(networkConfig, LimboLogs.Instance);
            networkConfig.ExternalIp = ipResolver.ExternalIp.ToString();
            networkConfig.LocalIp = ipResolver.LocalIp.ToString(); 

            var stepsLoader = new EthereumStepsLoader(stepsAssembly);
            var stepsManager = new EthereumStepsManager(
                stepsLoader,
                factory.Context,
                LimboLogs.Instance);

            var source = new CancellationTokenSource();
            try
            {
                await stepsManager.InitializeAll(source.Token);
            }
            catch(Exception)
            {
            }

            Assert.AreEqual(new Keccak( "0x061298b1447e2e387dc75f913409d07e4b941aeafa0a7e292837a5cf0dcf1eb9"),factory.Context.BlockTree.Genesis.Hash);
        }

        [Test]
        //Test source: https://github.com/ethereum/tests/blob/develop/BlockchainTests/GeneralStateTests/stCreate2/create2callPrecompiles.json
        //Log file: https://hivetests.ethdevops.io/viewer.html?file=results/nethermind_latest/client-f2507dc6.log
        public async Task Validate_genesis_from_hive_chainspec_correctly_2()
        {
            var configProvider = new ConfigProvider();
            Environment.SetEnvironmentVariable("NETHERMIND_HIVE_ENABLED", "true");

            var initConfig = configProvider.GetConfig<IInitConfig>();
            initConfig.DiagnosticMode = DiagnosticMode.MemDb;
            initConfig.HiveChainSpecPath = "chainspec/hive3.json";

            Assembly stepsAssembly = Assembly.GetAssembly(typeof(IStep));
            var serializer = new EthereumJsonSerializer();

            var factory = new EthereumRunnerContextFactory(configProvider, serializer, LimboLogs.Instance);
            factory.Context.EthereumJsonSerializer = new EthereumJsonSerializer();

            INetworkConfig networkConfig = configProvider.GetConfig<INetworkConfig>();
            var ipResolver = new IPResolver(networkConfig, LimboLogs.Instance);
            networkConfig.ExternalIp = ipResolver.ExternalIp.ToString();
            networkConfig.LocalIp = ipResolver.LocalIp.ToString(); 

            var stepsLoader = new EthereumStepsLoader(stepsAssembly);
            var stepsManager = new EthereumStepsManager(
                stepsLoader,
                factory.Context,
                LimboLogs.Instance);

            var source = new CancellationTokenSource();
            try
            {
                await stepsManager.InitializeAll(source.Token);
            }
            catch(Exception)
            {
            }

            Assert.AreEqual(new Keccak( "0x4b266132baa182ed082ae4e4635d37c3dd2ed8f6e031ab43c55067440b16dc65"),factory.Context.BlockTree.Genesis.Hash);
        }

        [Test]
        public async Task When_no_assemblies_defined()
        {
            EthereumRunnerContext runnerContext = new EthereumRunnerContext(
                new ConfigProvider(),
                LimboLogs.Instance);

            IEthereumStepsLoader stepsLoader = new EthereumStepsLoader();
            EthereumStepsManager stepsManager = new EthereumStepsManager(
                stepsLoader,
                runnerContext,
                LimboLogs.Instance);

            CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            await stepsManager.InitializeAll(source.Token);
        }

        [Test]
        public async Task With_steps_from_here()
        {
            EthereumRunnerContext runnerContext = new EthereumRunnerContext(
                new ConfigProvider(),
                LimboLogs.Instance);

            IEthereumStepsLoader stepsLoader = new EthereumStepsLoader(GetType().Assembly);
            EthereumStepsManager stepsManager = new EthereumStepsManager(
                stepsLoader,
                runnerContext,
                LimboLogs.Instance);

            CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromSeconds(1));

            try
            {
                await stepsManager.InitializeAll(source.Token);
            }
            catch (Exception e)
            {
                if (!(e is OperationCanceledException))
                {
                    throw new AssertionFailedException($"Exception should be {nameof(OperationCanceledException)}");
                }
            }
        }

        [Test]
        public async Task With_steps_from_here_Clique()
        {
            EthereumRunnerContext runnerContext = new CliqueEthereumRunnerContext(
                new ConfigProvider(),
                LimboLogs.Instance);

            IEthereumStepsLoader stepsLoader = new EthereumStepsLoader(GetType().Assembly);
            EthereumStepsManager stepsManager = new EthereumStepsManager(
                stepsLoader,
                runnerContext,
                LimboLogs.Instance);

            CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromSeconds(1));

            try
            {
                await stepsManager.InitializeAll(source.Token);
            }
            catch (Exception e)
            {
                if (!(e is OperationCanceledException))
                {
                    throw new AssertionFailedException($"Exception should be {nameof(OperationCanceledException)}");
                }
            }
        }

        [Test]
        public async Task With_failing_steps()
        {
            EthereumRunnerContext runnerContext = new AuRaEthereumRunnerContext(
                new ConfigProvider(),
                LimboLogs.Instance);

            IEthereumStepsLoader stepsLoader = new EthereumStepsLoader(GetType().Assembly);
            EthereumStepsManager stepsManager = new EthereumStepsManager(
                stepsLoader,
                runnerContext,
                LimboLogs.Instance);

            CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromSeconds(1));

            try
            {
                await stepsManager.InitializeAll(source.Token);
            }
            catch (Exception e)
            {
                if (!(e is OperationCanceledException))
                {
                    throw new AssertionFailedException($"Exception should be {nameof(OperationCanceledException)}");
                }
            }
        }
    }

    public class StepLong : IStep
    {
        public async Task Execute(CancellationToken cancellationToken)
        {
            await Task.Delay(100000, cancellationToken);
        }

        public StepLong(EthereumRunnerContext runnerContext)
        {
        }
    }

    public class StepForever : IStep
    {
        public async Task Execute(CancellationToken cancellationToken)
        {
            await Task.Delay(100000);
        }

        public StepForever(EthereumRunnerContext runnerContext)
        {
        }
    }

    public class StepA : IStep
    {
        public Task Execute(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public StepA(EthereumRunnerContext runnerContext)
        {
        }
    }

    [RunnerStepDependencies(typeof(StepC))]
    public class StepB : IStep
    {
        public Task Execute(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public StepB(EthereumRunnerContext runnerContext)
        {
        }
    }

    public abstract class StepC : IStep
    {
        public virtual Task Execute(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public abstract class StepD : IStep
    {
        public virtual Task Execute(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Designed to fail
    /// </summary>
    public class StepCAuRa : StepC
    {
        public StepCAuRa(AuRaEthereumRunnerContext runnerContext)
        {
        }

        public override async Task Execute(CancellationToken cancellationToken)
        {
            await Task.Run(() => throw new Exception());
        }
    }

    public class StepCClique : StepC, IStep
    {
        public StepCClique(CliqueEthereumRunnerContext runnerContext)
        {
        }

        public override async Task Execute(CancellationToken cancellationToken)
        {
            await Task.Run(() => throw new Exception());
        }

        bool IStep.MustInitialize => false;
    }

    public class StepCStandard : StepC
    {
        public StepCStandard(EthereumRunnerContext runnerContext)
        {
        }
    }
}