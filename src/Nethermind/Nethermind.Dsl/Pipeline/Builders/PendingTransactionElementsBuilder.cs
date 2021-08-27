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
using Nethermind.Core.Extensions;
using Nethermind.Dsl.Pipeline.Data;
using Nethermind.Dsl.Pipeline.Sources;
using Nethermind.Int256;
using Nethermind.Logging;
using Nethermind.TxPool;

namespace Nethermind.Dsl.Pipeline.Builders
{
    public class PendingTransactionElementsBuilder
    {
        private readonly ITxPool _txPool;
        private readonly ILogger _logger;

        public PendingTransactionElementsBuilder(ITxPool txPool, ILogger logger)
        {
            _txPool = txPool ?? throw new ArgumentNullException(nameof(txPool));
            _logger = logger;
        }

        public PendingTransactionsSource<PendingTxData> GetSourceElement()
        {
            return new(_txPool, _logger);
        }
        
        public PipelineElement<PendingTxData, PendingTxData> GetConditionElement(string key, string operation, string value)
        {
            return operation switch
            {
                "IS" => new PipelineElement<PendingTxData, PendingTxData>(
                    condition: (t => t.GetType().GetProperty(key).GetValue(t).ToString()?.ToLowerInvariant() == value.ToLowerInvariant()),
                    transformData: (t => t)),
                "==" => new PipelineElement<PendingTxData, PendingTxData>(
                    condition: (t => t.GetType().GetProperty(key)?.GetValue(t)?.ToString()?.ToLowerInvariant() == value.ToLowerInvariant()),
                    transformData: (t => t)),
                "NOT" => new PipelineElement<PendingTxData, PendingTxData>(
                    condition: (t => t.GetType().GetProperty(key)?.GetValue(t)?.ToString()?.ToLowerInvariant() != value.ToLowerInvariant()),
                    transformData: (t => t)),
                "!=" => new PipelineElement<PendingTxData, PendingTxData>(
                    condition: (t => t.GetType().GetProperty(key)?.GetValue(t)?.ToString()?.ToLowerInvariant() != value.ToLowerInvariant()),
                    transformData: (t => t)),
                ">" => new PipelineElement<PendingTxData, PendingTxData>(
                    condition: (t => (UInt256) t.GetType().GetProperty(key)?.GetValue(t) > UInt256.Parse(value)),
                    transformData: (t => t)),
                "<" => new PipelineElement<PendingTxData, PendingTxData>(
                    condition: (t => (UInt256) t.GetType().GetProperty(key)?.GetValue(t) < UInt256.Parse(value)),
                    transformData: (t => t)),
                ">=" => new PipelineElement<PendingTxData, PendingTxData>(
                    condition: (t => (UInt256) t.GetType().GetProperty(key)?.GetValue(t) >= UInt256.Parse(value)),
                    transformData: (t => t)),
                "<=" => new PipelineElement<PendingTxData, PendingTxData>(
                    condition: (t => (UInt256) t.GetType().GetProperty(key)?.GetValue(t) <= UInt256.Parse(value)),
                    transformData: (t => t)),
                "CONTAINS" => new PipelineElement<PendingTxData, PendingTxData>(
                    condition: (t => CheckIfDataContains(t, value)),
                    transformData: (t => t)),
                _ => null
            };
        }
        
        private static bool CheckIfDataContains(Transaction tx, string value)
        {
            if (tx.Data == null) return false;

            return tx.Data.ToHexString().Contains(value);
        } 
    }
}