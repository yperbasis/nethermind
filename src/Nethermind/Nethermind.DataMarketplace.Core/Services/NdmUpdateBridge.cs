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
using System.Threading.Tasks;
using Nethermind.Blockchain.Find;
using Nethermind.Core;
using Nethermind.Facade;
using Nethermind.State;
using Nethermind.TxPool;

namespace Nethermind.DataMarketplace.Core.Services
{
    public class NdmUpdateBridge : INdmUpdateBridge
    {
        private readonly IBlockchainBridge _blockchainBridge;
        private readonly ITxSender _txSender;
        private readonly IBlockFinder _blockTree;
        private readonly IStateReader _stateReader;

        public NdmUpdateBridge(
            IBlockchainBridge blockchainBridge,
            IBlockFinder blockTree,
            IStateReader stateReader,
            ITxSender txSender)
        {
            _blockchainBridge = blockchainBridge ?? throw new ArgumentNullException(nameof(blockchainBridge));
            _txSender = txSender ?? throw new ArgumentNullException(nameof(txSender));
            _blockTree = blockTree ?? throw new ArgumentNullException(nameof(blockTree));
            _stateReader = stateReader ?? throw new ArgumentNullException(nameof(stateReader));
        }

        public Task IsSynchronized()
        {
            throw new NotImplementedException();
        }

        public Task GetBlockNumberAsync()
        {
            throw new NotImplementedException();
        }

        public Task GetBalanceAsync(Address address)
        {
            throw new NotImplementedException();
        }

        public Task GetNonceAsync(Address address)
        {
            throw new NotImplementedException();
        }
    }
}
