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
using System.Collections.Generic;
using System.Text;
using Nethermind.Core;

namespace Nethermind.Dsl.Pipeline.Data
{
    public class BlockData : Block
    {
        public BlockData(BlockHeader blockHeader, BlockBody body) : base(blockHeader, body)
        {
        }

        public BlockData(BlockHeader blockHeader, IEnumerable<Transaction> transactions, IEnumerable<BlockHeader> ommers) : base(blockHeader, transactions, ommers)
        {
        }

        public BlockData(BlockHeader blockHeader) : base(blockHeader)
        {
        }

        public static BlockData FromBlock(Block block)
        {
            return new(block.Header, block.Body);
        }

        public override string ToString()
        {
            StringBuilder builder = new();
            builder.AppendLine("Found new block on chain");
            builder.AppendLine($"Block {Number}");
            builder.AppendLine("  Header:");
            builder.Append($"{Header.ToString("    ")}");

            builder.AppendLine("  Ommers:");
            foreach (BlockHeader ommer in Body.Ommers ?? Array.Empty<BlockHeader>())
            {
                builder.Append($"{ommer.ToString("    ")}");
            }

            builder.AppendLine("  Transactions:");
            foreach (Transaction tx in Body?.Transactions ?? Array.Empty<Transaction>())
            {
                builder.Append($"{tx.Hash}");
            }

            return builder.ToString();
        }
    }
}