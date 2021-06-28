using System;
using System.IO;
using Nethermind.Core;
using Nethermind.Db;
using Nethermind.Db.Rocks;
using Nethermind.Db.Rocks.Config;
using Nethermind.Logging;
using Nethermind.Serialization.Rlp;

namespace Nethermind.Tools.GasHistorian
{
    static class ProgramHeaders
    {
        private static readonly ChainLevelDecoder _chainLevelDecoder = new();
        private static readonly HeaderDecoder _decoder = new();

        static void Main(string[] args)
        {
            string baseDir = args[0];
            Console.WriteLine($"Scanning blocks in {baseDir}");

            RocksDbSettings chainDbSettings = new("blockInfos", "blockInfos");
            DbOnTheRocks chainDb = new(
                baseDir,
                chainDbSettings,
                DbConfig.Default,
                LimboLogs.Instance);

            RocksDbSettings headerDbSettings = new("headers", "headers");
            DbOnTheRocks headersDb = new(
                baseDir,
                headerDbSettings,
                DbConfig.Default,
                LimboLogs.Instance);

            using FileStream fs = File.OpenWrite("output.csv");
            using StreamWriter sw = new(fs);
            sw.WriteLine($"Block Nr,Difficulty,Timestamp");
            for (int i = 0; i < 10489461; i++)
            {
                if (i % 10000 == 0)
                {
                    Console.WriteLine($"Scanning block {i}");
                }

                ChainLevelInfo? chainLevelInfo = chainDb.Get(i, _chainLevelDecoder);

                BlockInfo? mainChainBlock = chainLevelInfo?.MainChainBlock;
                if (mainChainBlock is not null)
                {
                    BlockHeader? header = headersDb.Get(mainChainBlock.BlockHash, _decoder);

                    if (header is not null)
                    {
                        sw.WriteLine($"{header.Number},{header.Difficulty},{header.Timestamp}");
                    }
                }
            }
        }
    }
}
