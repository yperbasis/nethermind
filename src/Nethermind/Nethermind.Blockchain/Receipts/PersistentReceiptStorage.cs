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
using System.Collections.Generic;
using System.Linq;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Core.Encoding;
using Nethermind.Core.Specs;
using Nethermind.Evm;
using Nethermind.Logging;
using Nethermind.Store;

namespace Nethermind.Blockchain.Receipts
{
    public class PersistentReceiptStorage : IReceiptStorage, IReceiptFinder
    {
        private readonly IDb _database;
        private readonly ISpecProvider _specProvider;
        private readonly ILogger _logger;

        public PersistentReceiptStorage(IDb receiptsDb, ISpecProvider specProvider, ILogManager logManager)
        {
            _logger = logManager?.GetClassLogger() ?? throw new ArgumentNullException(nameof(logManager));
            _database = receiptsDb ?? throw new ArgumentNullException(nameof(receiptsDb));
            _specProvider = specProvider ?? throw new ArgumentNullException(nameof(specProvider));

            byte[] lowestBytes = _database.Get(Keccak.Zero);
            LowestInsertedReceiptBlock = lowestBytes == null ? (long?) null : new RlpStream(lowestBytes).DecodeLong();
        }

        public TxReceipt Find(Keccak transactionHash)
        {
            var receiptData = _database.Get(transactionHash);
            if (receiptData != null)
            {
                var receipt = Rlp.Decode<TxReceipt>(new Rlp(receiptData), RlpBehaviors.Storage);
                receipt.TxHash = transactionHash;
                return receipt;
            }

            return null;
        }

        public void Insert(Block block, TxReceipt[] receipts)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));
            if (receipts == null) throw new ArgumentNullException(nameof(receipts));
            if (block.Transactions.Length != receipts.Length) throw new ArgumentException("Count mismatch between transactions and receipts.");
            
            InsertForBlock(block.Hash, block.Number, receipts);
            InsertForTransactions(block);
            UpdateLowestInsertedReceiptBlock(block.Number);
        }

        private void InsertForBlock(Keccak blockHash, long blockNumber, TxReceipt[] receipts)
        {
            var spec = _specProvider.GetSpec(blockNumber);
            RlpBehaviors behaviors = spec.IsEip658Enabled ? RlpBehaviors.Eip658Receipts : RlpBehaviors.None;
            _database.Set(blockHash, Rlp.Encode(receipts, behaviors).Bytes);
        }
        
        private void InsertForTransactions(Block block)
        {
            var transactions = block.Transactions;
            for (int i = 0; i < transactions.Length; i++)
            {
                var transaction = transactions[i];
                var receiptInfo = new TransactionInfo {BlockHash = block.Hash, TransactionIndex = i};
                _database.Set(transaction.Hash, Rlp.Encode(receiptInfo).Bytes);
            }
        }

        private void UpdateLowestInsertedReceiptBlock(in long blockNumber)
        {
            if (blockNumber > LowestInsertedReceiptBlock)
            {
                _logger.Error($"{blockNumber} > {LowestInsertedReceiptBlock}");
            }

            LowestInsertedReceiptBlock = blockNumber;
            _database.Set(Keccak.Zero, Rlp.Encode(LowestInsertedReceiptBlock.Value).Bytes);
        }

        public TxReceipt[] Get(Block block)
        {
            var receipts = Get(block.Hash);
            receipts?.RecoverData(block);
            return receipts;
        }

        public TxReceipt[] Get(Keccak blockHash)
        {
            var data = _database.Get(blockHash);
            return data == null ? null : Rlp.Decode<TxReceipt[]>(data);
        }

        public long? LowestInsertedReceiptBlock { get; private set; }
    }

    public class TransactionInfo
    {
        public Keccak BlockHash { get; set; }
        public int TransactionIndex { get; set; }
    }
    
    public static class ReceiptsRecovery
    {
        public static void RecoverData(this TxReceipt[] receipts, Block block)
        {
            if (block.Transactions.Length != receipts.Length) throw new ArgumentException("Count mismatch between transactions and receipts.");
            
            long gasUsedBefore = 0;
            for (int receiptIndex = 0; receiptIndex < block.Transactions.Length; receiptIndex++)
            {
                Transaction transaction = block.Transactions[receiptIndex];
                if (receipts.Length > receiptIndex)
                {
                    TxReceipt receipt = receipts[receiptIndex];
                    receipt.RecoverData(block, transaction, receiptIndex, gasUsedBefore);
                    gasUsedBefore = receipt.GasUsedTotal;
                }
            }
        }

        public static void RecoverData(this TxReceipt receipt, Block block, Transaction transaction, int transactionIndex)
        {
            receipt.RecoverData(block, transaction, transactionIndex, null);
        }
        
        private static void RecoverData(this TxReceipt receipt, Block block, Transaction transaction, int transactionIndex, long? gasUsedBefore)
        {
            receipt.BlockHash = block.Hash;
            receipt.BlockNumber = block.Number;
            receipt.TxHash = transaction.Hash;
            receipt.Index = transactionIndex;
            receipt.Sender = transaction.SenderAddress;
            receipt.Recipient = transaction.IsContractCreation ? null : transaction.To;
            receipt.ContractAddress = transaction.IsContractCreation ? transaction.To : null;
            if (gasUsedBefore.HasValue)
            {
                receipt.GasUsed = receipt.GasUsedTotal - gasUsedBefore.Value;
            }

            if (receipt.StatusCode != StatusCode.Success)
            {
                receipt.StatusCode = receipt.Logs.Length == 0 ? StatusCode.Failure : StatusCode.Success;
            }
        }
    }
}