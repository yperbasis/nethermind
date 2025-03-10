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

using System;
using System.Threading.Tasks;
using Nethermind.Blockchain;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Evm.Tracing;

namespace Nethermind.Consensus.Processing
{
    public interface IBlockchainProcessor : IDisposable
    {
        ITracerBag Tracers { get; }

        void Start();

        Task StopAsync(bool processRemainingBlocks = false);

        Block? Process(Block block, ProcessingOptions options, IBlockTracer tracer);

        bool IsProcessingBlocks(ulong? maxProcessingInterval);

        event EventHandler<InvalidBlockEventArgs> InvalidBlock;

        public class InvalidBlockEventArgs : EventArgs
        {
            public Block InvalidBlock { get; init; }
        }
    }
}
