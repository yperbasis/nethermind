using System;
using BenchmarkDotNet.Attributes;
using Ethereum.Test.Base;

namespace Nethermind.EthereumTests.Benchmark
{
    public class JumpdestBenchmark : GeneralStateTestBase
    {
        // [Benchmark]
        // public void Gas10M_250M()
        // {
        //     FileTestsSource source = new(@"TestFiles\Jumpdest\10MGas_JUMPDEST_250M.json");
        //     var tests = source.LoadGeneralStateTests();
        //
        //     foreach (GeneralStateTest test in tests)
        //     {
        //         RunTest(test);
        //     }
        // }
        //
        // [Benchmark]
        // public void Gas30M_1320M()
        // {
        //     FileTestsSource source = new(@"TestFiles\Jumpdest\30MGas_JUMPDEST_1320M.json");
        //     var tests = source.LoadGeneralStateTests();
        //
        //     foreach (GeneralStateTest test in tests)
        //     {
        //         RunTest(test);
        //     }
        // }
    }
}
