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
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Core.Extensions;
using Nethermind.Crypto;
using Nethermind.Serialization.Rlp;
using NUnit.Framework;

namespace Nethermind.Blockchain.Test
{
    [TestFixture]
    public class Carrier
    {
        [Test]
        public void Test()
        {
            byte[] mevPrefix = {0xF, 0x1, 0x4, 0x5, 0x6, 0x8, 0x0, 0x7, 0x5};
            Transaction mevTx = new();
            CryptoRandom cryptoRandom = new();
            PrivateKeyGenerator privateKeyGenerator = new(cryptoRandom);
            PrivateKey privateKey = privateKeyGenerator.Generate();
            PublicKey validatorPubKey = privateKey.PublicKey;
            TxDecoder txDecoder = new();
            byte[] mevRlp = txDecoder.Encode(mevTx).Bytes;
            byte[] encryptedMevRlp = Encrypt(validatorPubKey, mevRlp);

            Transaction carrier = new();
            carrier.Data = Bytes.Concat(mevPrefix, validatorPubKey.Bytes, encryptedMevRlp);
            byte[] carrierRlp = txDecoder.Encode(carrier).Bytes;
            Console.WriteLine(carrierRlp.ToHexString());
        }

        private static byte[] Encrypt(PublicKey publicKey, byte[] bytes)
        {
            return bytes;
        }
    }
}
