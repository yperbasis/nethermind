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

using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using System.Linq;
using Nethermind.Benchmarks.Store;

namespace Nethermind.Benchmark.Runner
{
    // public class DashboardConfig : ManualConfig
    // {
    //     public DashboardConfig(params Job[] jobs)
    //     {
    //         foreach (Job job in jobs)
    //         {
    //             AddJob(job);
    //         }
    //
    //         AddColumnProvider(BenchmarkDotNet.Columns.DefaultColumnProviders.Statistics);
    //         AddColumnProvider(BenchmarkDotNet.Columns.DefaultColumnProviders.Params);
    //         AddLogger(BenchmarkDotNet.Loggers.ConsoleLogger.Default);
    //         AddExporter(BenchmarkDotNet.Exporters.Json.JsonExporter.FullCompressed);
    //         AddDiagnoser(BenchmarkDotNet.Diagnosers.MemoryDiagnoser.Default);
    //         WithSummaryStyle(SummaryStyle.Default.WithMaxParameterColumnWidth(100));
    //     }
    // }

    public static class Program
    {
        public static void Main(string[] args)
        {
            new TrieStoreSsdKillingTest().Improved();
        }
    }
}
