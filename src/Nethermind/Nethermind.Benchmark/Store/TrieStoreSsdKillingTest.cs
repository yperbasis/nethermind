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
using System.Diagnostics;
using System.IO;
using BenchmarkDotNet.Attributes;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Core.Test.Builders;
using Nethermind.State;
using Nethermind.Db;
using Nethermind.Db.Rocks;
using Nethermind.Db.Rocks.Config;
using Nethermind.Logging;
using Nethermind.Trie.Pruning;

namespace Nethermind.Benchmarks.Store
{
    public class TrieStoreSsdKillingTest
    {
        private static readonly Account _empty = Build.An.Account.WithBalance(0).TestObject;
        private static readonly Account _account0 = Build.An.Account.WithBalance(1).TestObject;
        private static readonly Account _account1 = Build.An.Account.WithBalance(2).TestObject;
        private static readonly Account _account2 = Build.An.Account.WithBalance(3).TestObject;
        private static readonly Account _account3 = Build.An.Account.WithBalance(4).TestObject;

        private StateTree _tree;


        [Benchmark]
        public void Improved()
        {
            if (Directory.Exists(@"D:\nethermind_db\Code")) { Directory.Delete(@"D:\nethermind_db\Code", true); }

            Stopwatch s = new(); s.Start();
            DbProvider dbProvider = new(DbModeHint.Persisted);
            var db = new RocksDbFactory(DbConfig.Default,NullLogManager.Instance,"D:\\nethermind_db").CreateDb(new RocksDbSettings("Code", "Code"));
            dbProvider.RegisterDb("Code", db);
            var store = new TrieStore(dbProvider.GetDb<IDb>("Code"), NullLogManager.Instance);
            _tree = new StateTree(store, NullLogManager.Instance);
            var keccak = Keccak.OfAnEmptyString;

            Stopwatch a = new();
            for (int i = 0; i < 100; i++)
            {
                a.Restart();
                for (var n = 0; n < 30000; n++)
                {
                    keccak = Keccak.Compute(keccak.Bytes);
                    _tree.Set(keccak, _account0);
                }
                Console.WriteLine($"{a.ElapsedMilliseconds}");
                _tree.Commit(i);
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"TIMING: {s.ElapsedMilliseconds}");
            Console.ResetColor();
        }
    }
}
