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

using System;
using System.Threading;
using System.Threading.Tasks;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Int256;

namespace Nethermind.Consensus
{
    public class DiffDevSealEngine : ISealer, ISealValidator
    {
        private Random _random = new Random();

        private UInt256 difficulty;
        
        public DiffDevSealEngine(Address address = null)
        {
            Address = address ?? Address.Zero;
            difficulty = 1000;
        }
        
        public Task<Block> SealBlock(Block block, CancellationToken cancellationToken)
        {
            block.Header.MixHash = Keccak.Zero;
            block.Header.Difficulty = difficulty;
            int multiplier = _random.Next(100) * _random.Next(100);
            difficulty *= (UInt256) (10000m * (1 + 1 * (multiplier / 10000m))) / 10000;
            return Task.FromResult(block);
        }

        public bool CanSeal(long blockNumber, Keccak parentHash)
        {
            return true;
        }

        public Address Address { get; }

        public bool ValidateParams(BlockHeader parent, BlockHeader header)
        {
            return true;
        }

        public bool ValidateSeal(BlockHeader header, bool force)
        {
            return true;
        }
    }
}
