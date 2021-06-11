//  Copyright (c) 2021 Demerzel Solutions Limited
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
// 

using System;
using System.Threading.Tasks;
using Nethermind.Core;
using Nethermind.Core.Crypto;

namespace Nethermind.TxPool
{
    public class TxPoolSender : ITxSender
    {
        private readonly ITxPool _txPool;
        private readonly ITxSealer[] _sealers;

        public TxPoolSender(ITxPool txPool, params ITxSealer[] sealers)
        {
            _txPool = txPool ?? throw new ArgumentNullException(nameof(txPool));
            _sealers = sealers ?? throw new ArgumentNullException(nameof(sealers));
            if (sealers.Length == 0) throw new ArgumentException("Sealers can not be empty.", nameof(sealers));
        }

        public ValueTask<Keccak> SendTransaction(Transaction tx, TxHandlingOptions txHandlingOptions)
        {
            // TODO: this is very not intuitive - can we fix it...?
            // maybe move nonce reservation to sender itself before sealing
            // sealers should behave like composite and not like chain of commands
            if (tx.GasPrice > tx.FeeCap)
            {
                throw new Exception(String.Format(
                    "The gas price of the transaction ({0}) was greater than the FeeCap ({1})."
                    , tx.GasPrice, tx.FeeCap));
            }
            foreach (var sealer in _sealers)
            {
                sealer.Seal(tx, txHandlingOptions);
                
                AddTxResult result = _txPool.AddTransaction(tx, txHandlingOptions);
                
                //create some way to use AddTxResult?

                if (result != AddTxResult.OwnNonceAlreadyUsed || (txHandlingOptions & TxHandlingOptions.ManagedNonce) != TxHandlingOptions.ManagedNonce)
                {
                    if (result != AddTxResult.Added)
                    {
                        throw new Exception(String.Format("The result is set to {0}", result.ToString()));
                    }
                    break;
                }
            }

            return new ValueTask<Keccak>(tx.Hash);
        }
    }
}
