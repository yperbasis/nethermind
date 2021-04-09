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
using System.Threading.Tasks;
using Nethermind.Core;

namespace Nethermind.Pipeline.Pipelines
{
    public class Pipeline<T> : IPipeline<T> where T : class
    {
        private static readonly HashSet<Type> _validPartTypes = new()
        {
            typeof(Transaction), typeof(Block), typeof(LogEntry)
        };
        private readonly List<IPipelineElement<T>> _parts = new();
        
        public async Task<PipelineOutput<T>> ProcessAsync(T input)
        {
            PipelineOutput<T> output = null;
            foreach (var part in Elements)
            {
                output = await part.ProcessAsync(input);
                input = output.Data;
            }

            return output;
        }

        public IReadOnlyList<IPipelineElement<T>> Elements => _parts;

        public void AddElement(IPipelineElement<T> part)
        {
            var partType = part.GetType();
            var genericType = partType.GetGenericTypeDefinition();
            if (!_validPartTypes.Contains(genericType))
            {
                throw new InvalidOperationException($"Invalid pipeline part type: {genericType}");
            }
            
            _parts.Add(part);
        } 

        public void RemoveElement(IPipelineElement<T> part) => _parts.Remove(part);
    }

    public class PipelinePartXyz : PipelineElement<Transaction>
    {
        public override async Task<PipelineOutput<Transaction>> ProcessAsync(Transaction input)
        {
            return new PipelineOutput<Transaction> {Data = input};
        }
    }
    
    public class PipelineData
    {
        public Transaction Transaction { get; set; }
        public Block Block { get; set; }
        public LogEntry LogEntry { get; set; }
    }
    
    public class PipelineOutput
    {
        public PipelineData Data { get; set; }
        public bool IsValid { get; set; }
    }

    public interface IPipelineElement
    {
        Task<PipelineOutput> ProcessAsync(PipelineData input);
    }

    public interface IPipeline
    {
        Task<PipelineOutput> ProcessAsync(PipelineData input);
        IReadOnlyList<IPipelineElement> Element { get; }
        void AddPart(IPipelineElement element);
        void RemovePart(IPipelineElement element);
    }

    public class Pipeline : IPipeline
    {
        private readonly List<IPipelineElement> _elements = new();
        
        public async Task<PipelineOutput> ProcessAsync(PipelineData input)
        {
            PipelineOutput output = null;
            foreach (var element in _elements)
            {
                output = await element.ProcessAsync(input);
                if (!output.IsValid)
                {
                    break;
                }
                // TIn = TOut (poprzedni element);
                input = output.Data;
            }

            return output;
        }

        public IReadOnlyList<IPipelineElement> Element => _elements;

        public void AddPart(IPipelineElement element) => _elements.Add(element);

        public void RemovePart(IPipelineElement element) => _elements.Remove(element);
    }
}
