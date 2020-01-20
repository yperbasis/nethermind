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
using Nethermind.Core;
using Nethermind.Core.Crypto;

namespace Nethermind.Blockchain.Receipts
{
    public interface IReceiptFinder
    {
        TxReceipt Find(Keccak transactionHash);
        TxReceipt[] Get(Block block);
    }

    public class OldFormatReceiptFinder : IReceiptFinder
    {
        public TxReceipt Find(Keccak transactionHash)
        {
            throw new System.NotImplementedException();
        }

        public TxReceipt[] Get(Block block)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ReceiptFinderChain : IReceiptFinder
    {
        private readonly IReceiptFinder[] _finders;

        public ReceiptFinderChain(params IReceiptFinder[] finders)
        {
            _finders = finders;
        }
        
        public TxReceipt Find(Keccak transactionHash)
        {
            for (int i = 0; i < _finders.Length; i++)
            {
                var finder = _finders[i];
                var txReceipt = finder.Find(transactionHash);
                if (txReceipt != null)
                {
                    return txReceipt;
                }
            }

            return null;
        }

        public TxReceipt[] Get(Block block)
        {
            for (int i = 0; i < _finders.Length; i++)
            {
                var finder = _finders[i];
                var txReceipt = finder.Get(block);
                if (txReceipt != null)
                {
                    return txReceipt;
                }
            }

            return null;
        }
    }

    public interface IReceiptStorage : IReceiptFinder
    {
        void Insert(Block block, params TxReceipt[] receipts);
        long? LowestInsertedReceiptBlock { get; }
    }
}