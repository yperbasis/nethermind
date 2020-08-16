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
// 

using System;
using Nethermind.Core2;
using Nethermind.Core2.Containers;
using Nethermind.Core2.Crypto;
using Nethermind.Core2.Types;

namespace Nethermind.BeaconNode.Eth1Bridge.Bridge
{
    public static class LogExtensions
    {
        /// The following constants define the layout of bytes in the deposit contract `DepositEvent`. The
        /// event bytes are formatted according to the Ethereum ABI.
        const int PublicKeyStart = 192;
        const int PublicKeyLength = Ssz.Ssz.BlsPublicKeyLength;
        const int WithdrawalCredentialsStart = PublicKeyStart + 64 + 32;
        const int WithdrawalCredentialsLength = Ssz.Ssz.Bytes32Length;
        const int AmountStart = WithdrawalCredentialsStart + 32 + 32;
        const int AmountLength = Ssz.Ssz.GweiLength;
        const int SignatureStart = AmountStart + 32 + 32;
        const int SignatureLength = Ssz.Ssz.BlsSignatureLength;
        const int IndexStart = SignatureStart + 96 + 32;
        const int IndexLength = Ssz.Ssz.ValidatorIndexLength;
        
        public static DepositLog ToDepositLog(this Log log)
        {
            var data = log.Data;
            
            if (data.Length < IndexStart + IndexLength)
            {
                throw new InvalidOperationException("Insufficient bytes in log data.");
            }

            var publicKey = data.AsSpan(PublicKeyStart, PublicKeyLength);
            var withdrawalCredentials = data.AsSpan(WithdrawalCredentialsStart, WithdrawalCredentialsLength);
            var amount = data.AsSpan(AmountStart, AmountLength);
            var signature = data.AsSpan(SignatureStart, SignatureLength);
            var index = data.AsSpan(IndexStart, IndexLength);

            return new DepositLog(
                new DepositData(
                    Ssz.Ssz.DecodeBlsPublicKey(publicKey),
                    Ssz.Ssz.DecodeBytes32(withdrawalCredentials),
                    Ssz.Ssz.DecodeGwei(amount),
                    Ssz.Ssz.DecodeBlsSignature(signature)),
                log.BlockNumber,
                Ssz.Ssz.DecodeULong(index),
                true // TODO: Validate signature?
            );
        }
    }
}
