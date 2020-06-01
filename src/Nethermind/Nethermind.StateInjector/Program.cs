using System;
using System.IO;
using Nethermind.Core.Crypto;
using Nethermind.Core.Extensions;
using Nethermind.Db.Rocks;
using Nethermind.Db.Rocks.Config;
using Nethermind.Logging;

namespace Nethermind.StateInjector
{
    public static class Program
    {
        private static ConsoleColor defaultColor;

        private static int Main(string[] args)
        {
            defaultColor = Console.ForegroundColor;
            ILogManager logger = new OneLoggerLogManager(new ConsoleAsyncLogger(LogLevel.Trace));

            if (args.Length != 3)
            {
                WriteInRed("Need three arguments - [DB path] [key in the hex format] and [value in hex format]");
                WriteInRed("e.g. /home/user/nethermind_db/mainnet/ 0xab12...67da 0x001200aa0012...d0b0");
                return -1;
            }

            string stateDbPath = args[0];
            string keyHex = args[1];
            string valueHex = args[2];
            string basePath = stateDbPath;
            
            DbOnTheRocks dbOnTheRocks;
            try
            {
                string dbDir = Path.Combine(stateDbPath, "state");
                if (!Directory.Exists(dbDir))
                {
                    WriteInRed($"{dbDir} directory does not exist");
                    return -1;
                }
                
                Console.WriteLine($"Loading State DB from {stateDbPath}");
                dbOnTheRocks = new StateRocksDb(basePath, new DbConfig(), logger);
                WriteInGreen("OK");
            }
            catch (Exception e)
            {
                WriteInRed(e.ToString());
                return -1;
            }

            Console.WriteLine($"Planning to add a state node {keyHex}->{valueHex}");
            Console.WriteLine($"Verifying that key == hash(value)...");

            byte[] keyBytes;
            byte[] valueBytes;
            try
            {
                keyBytes = Bytes.FromHexString(keyHex);
                valueBytes = Bytes.FromHexString(valueHex);

                Keccak key = new Keccak(keyBytes);
                if (key == Keccak.Compute(valueBytes))
                {
                    WriteInGreen("OK");
                }
                else
                {
                    WriteInRed("Key failed validation - key is not the hash of the value.");
                    return -1;
                }
            }
            catch (Exception e)
            {
                WriteInRed(e.ToString());
                return -1;
            }
            
            try
            {
                Console.WriteLine($"Adding key->value pair to the database:");
                dbOnTheRocks[keyBytes] = valueBytes;
                WriteInGreen("OK");
            }
            catch (Exception e)
            {
                WriteInRed(e.ToString());
                return -1;
            }
            
            try
            {
                Console.WriteLine($"Verifying the key->value pair is in the DB:");
                dbOnTheRocks.Dispose();
                dbOnTheRocks = new StateRocksDb(basePath, new DbConfig(), logger);
                var valueRecovered = dbOnTheRocks[keyBytes];
                dbOnTheRocks.Dispose();
                WriteInGreen($"OK - managed to load value {valueRecovered.ToHexString()}");
            }
            catch (Exception e)
            {
                WriteInRed(e.ToString());
                return -1;
            }

            return 0;
        }

        private static void WriteInGreen(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(text);
            Console.ForegroundColor = defaultColor;
        }

        private static void WriteInRed(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ForegroundColor = defaultColor;
        }
    }
}