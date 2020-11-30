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
using Nethermind.Consensus.Transactions;
using Nethermind.Core;
using Nethermind.Core.Extensions;
using Nethermind.Crypto;
using Nethermind.Int256;
using Nethermind.State;

namespace Nethermind.Runner.Ethereum.Steps
{
    internal class TestTxSource : ITxSource
    {
        private readonly PrivateKey _key;
        private readonly IStateProvider _stateProvider;
        private readonly IEthereumEcdsa _ecdsa;
        private readonly CryptoRandom _rand = new CryptoRandom();
        private UInt256 _nonce;

        public TestTxSource(IStateProvider stateProvider, IEthereumEcdsa ethereumEcdsa)
        {
            _key = new PrivateKey(Bytes.FromHexString("82d30cef9aa4ad6af51b6cc00940e778c4143de1667f358ae6e503c8036f3144"));
            _stateProvider = stateProvider;
            _ecdsa = ethereumEcdsa;
            _nonce = _stateProvider.GetNonce(_key.Address);

        }
        public IEnumerable<Transaction> GetTransactions(BlockHeader parent, long gasLimit)
        {
            var txs = new List<Transaction>();

            var ratio = _rand.NextInt(100) / 100.0;
            var maxNoOfBlocks = ratio * gasLimit / 21000;

            for (var i = 0; i < maxNoOfBlocks; ++i)
            {
                var transaction = new Transaction();
                transaction.Value = 1.Wei();
                transaction.GasLimit = 21000;
                transaction.Nonce = _nonce;
                transaction.To = new Address("707Fc13C0eB628c074f7ff514Ae21ACaeE0ec072");
                transaction.FeeCap = 10.GWei();
                transaction.GasPrice = 1.GWei();

                _ecdsa.Sign(_key, transaction);

                transaction.Hash = transaction.CalculateHash();
                transaction.SenderAddress = _ecdsa.RecoverAddress(transaction, true);

                txs.Add(transaction);

                _nonce++;
            }

            return txs;
        }


    }
}
