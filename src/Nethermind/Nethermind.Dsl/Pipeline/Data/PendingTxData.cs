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

using System.Text;
using Nethermind.Core;
using Nethermind.Core.Extensions;

namespace Nethermind.Dsl.Pipeline.Data
{
    public class PendingTxData : TxData
    {
        public Address From { get; set; } 
        
        public new static PendingTxData FromTransaction(Transaction tx)
        {
            return new()
            {
                Type = tx.Type,
                From = tx.SenderAddress,
                Hash = tx.Hash,
                SenderAddress = tx.SenderAddress,
                To = tx.To,
                GasPrice = tx.GasPrice,
                GasLimit = tx.GasLimit,
                Nonce = tx.Nonce,
                Value = tx.Value,
                Signature = tx.Signature,
                Timestamp = tx.Timestamp,
                Data = tx.Data
            };
        }
        
        public override string ToString()
        {
            StringBuilder builder = new();
            builder.Append("Found new transaction in the mempool");
            builder.AppendLine($"Hash:      {Hash}");
            builder.AppendLine($"From:      {From}");
            builder.AppendLine($"To:        {To}");
            if (IsEip1559)
            {
                builder.AppendLine($"MaxPriorityFeePerGas: {MaxPriorityFeePerGas}");
                builder.AppendLine($"MaxFeePerGas: {MaxFeePerGas}");
            }
            else
            {
                builder.AppendLine($"Gas Price: {GasPrice}");
            }

            builder.AppendLine($"Gas Limit: {GasLimit}");
            builder.AppendLine($"Nonce:     {Nonce}");
            builder.AppendLine($"Value:     {Value}");
            builder.AppendLine($"Data:      {(Data ?? new byte[0]).ToHexString()}");
            builder.AppendLine($"Signature: {(Signature?.Bytes ?? new byte[0]).ToHexString()}");
            builder.AppendLine($"V:         {Signature?.V}");
            builder.AppendLine($"ChainId:   {Signature?.ChainId}");
            builder.AppendLine($"Timestamp: {Timestamp}");


            return builder.ToString();
        }
    }
}