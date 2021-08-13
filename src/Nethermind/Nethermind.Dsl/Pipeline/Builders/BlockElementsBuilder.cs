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

using Nethermind.Blockchain.Processing;
using Nethermind.Dsl.Pipeline.Data;
using Nethermind.Dsl.Pipeline.Sources;
using Nethermind.Int256;

namespace Nethermind.Dsl.Pipeline.Builders
{
    public class BlockElementsBuilder
    {
        private readonly IBlockProcessor _blockProcessor;

        public BlockElementsBuilder(IBlockProcessor blockProcessor)
        {
            _blockProcessor = blockProcessor;
        }

        public BlocksSource<BlockData> GetSourceElement()
        {
            return new(_blockProcessor);
        }
        
        public PipelineElement<BlockData, BlockData> GetConditionElement(string key, string operation, string value)
        {
            return operation switch
            {
                "IS" => new PipelineElement<BlockData, BlockData>(
                    condition: (b => b.GetType().GetProperty(key)?.GetValue(b)?.ToString()?.ToLowerInvariant() == value.ToLowerInvariant()),
                    transformData: (b => b)),
                "==" => new PipelineElement<BlockData, BlockData>(
                    condition: (b => b.GetType().GetProperty(key)?.GetValue(b)?.ToString()?.ToLowerInvariant() == value.ToLowerInvariant()),
                    transformData: (b => b)),
                "!=" => new PipelineElement<BlockData, BlockData>(
                    condition: (b => b.GetType().GetProperty(key)?.GetValue(b)?.ToString()?.ToLowerInvariant() != value.ToLowerInvariant()),
                    transformData: (b => b)),
                "NOT" => new PipelineElement<BlockData, BlockData>(
                    condition: (b => b.GetType().GetProperty(key)?.GetValue(b)?.ToString()?.ToLowerInvariant() != value.ToLowerInvariant()),
                    transformData: (b => b)),
                ">" => new PipelineElement<BlockData, BlockData>(
                    condition: (b => (UInt256) b.GetType().GetProperty(key)?.GetValue(b) > UInt256.Parse(value)),
                    transformData: (b => b)),
                "<" => new PipelineElement<BlockData, BlockData>(
                    condition: (b => (UInt256) b.GetType().GetProperty(key)?.GetValue(b) < UInt256.Parse(value)),
                    transformData: (b => b)),
                ">=" => new PipelineElement<BlockData, BlockData>(
                    condition: (b => (UInt256) b.GetType().GetProperty(key)?.GetValue(b) >= UInt256.Parse(value)),
                    transformData: (b => b)),
                "<=" => new PipelineElement<BlockData, BlockData>(
                    condition: (b => (UInt256) b.GetType().GetProperty(key)?.GetValue(b) <= UInt256.Parse(value)),
                    transformData: (b => b)),
                _ => null
            };
        }
    }
}