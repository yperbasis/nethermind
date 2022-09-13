using System.Runtime;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Running;
using FASTER.core;
using Nethermind.Core;
using Nethermind.Db;
using Nethermind.Db.Rocks;
using RocksDbSharp;
public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<DbBenchmarker>();
    }

    [NativeMemoryProfiler]
    [MemoryDiagnoser]
    public class DbBenchmarker
    {
        private IDb _rocks;
        private IDb _faster = new FasterDbTest();

        [GlobalSetup]
        public void Setup()
        {
        }

        [Benchmark]
        public void Rocks()
        {
            TestDb(_rocks);
        }

        [Benchmark]
        public void Faster()
        {
            TestDb(_faster);
        }

        private static void TestDb(IDb _db)
        {
            byte[] key = GetByteArray();
            byte[] value = GetByteArray(1024);
            _db[key] = value;
            _ = _db[key];
            _db[key] = null;
        }

        private static byte[] GetByteArray(int size = 32)
        {
            Random rnd = new();
            byte[] b = new byte[size];
            rnd.NextBytes(b);
            return b;
        }

    }

    

}
