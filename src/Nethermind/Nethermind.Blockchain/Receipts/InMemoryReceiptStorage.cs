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
using System.Collections.Concurrent;
using System.Collections.Generic;
using Nethermind.Core;
using Nethermind.Core.Crypto;

namespace Nethermind.Blockchain.Receipts
{
    public class InMemoryReceiptStorage : IReceiptStorage
    {
        private readonly ConcurrentDictionary<Keccak, TxReceipt[]> _receipts =
            new ConcurrentDictionary<Keccak, TxReceipt[]>();
        
        private readonly ConcurrentDictionary<Keccak, TxReceipt> _transactions =
            new ConcurrentDictionary<Keccak, TxReceipt>();

        public TxReceipt Find(Keccak transactionHash)
        {
            _transactions.TryGetValue(transactionHash, out var transaction);
            return transaction;
        }

        public void Insert(Block block, TxReceipt[] receipts)
        {
            _receipts[block.Hash] = receipts;
            for (var index = 0; index < block.Transactions.Length; index++)
            {
                var transaction = block.Transactions[index];
                _transactions[transaction.Hash] = receipts[index];
            }

            LowestInsertedReceiptBlock = Math.Min(LowestInsertedReceiptBlock ?? long.MaxValue, block.Number);
        }

        public TxReceipt[] Get(Block block) => Get(block.Hash);

        public TxReceipt[] Get(Keccak blockHash)
        {
            _receipts.TryGetValue(blockHash, out var receipts);
            return receipts;
        }

        public long? LowestInsertedReceiptBlock { get; private set; }
        
        public int Count => _receipts.Count;
    }
}