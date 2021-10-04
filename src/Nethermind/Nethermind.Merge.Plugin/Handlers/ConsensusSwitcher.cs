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
using System.Threading.Tasks;
using Nethermind.Blockchain;
using Nethermind.Consensus;
using Nethermind.Core;

namespace Nethermind.Merge.Plugin.Handlers
{
    public class ConsensusSwitcher
    {
        private readonly IBlockProducer _eth1BlockProducer;
        private readonly IBlockProducer _eth2BlockProducer;
        private readonly ITransitionProcessHandler _transitionProcessHandler;

        public ConsensusSwitcher(
            IBlockProducer eth1BlockProducer,
            IBlockProducer eth2BlockProducer,
            IBlockTree blockTree,
            ITransitionProcessHandler transitionProcessHandler)
        {
            _eth1BlockProducer = eth1BlockProducer;
            _eth2BlockProducer = eth2BlockProducer;
            _transitionProcessHandler = transitionProcessHandler;

            blockTree.NewHeadBlock += OnNewHeadBlock;
        }

        private void OnNewHeadBlock(object? sender, BlockEventArgs e)
        {
            if (e.Block.TotalDifficulty >= _transitionProcessHandler.TerminalTotalDifficulty)
            {
                _eth1BlockProducer.StopAsync();
                _eth2BlockProducer.Start();
            }    
        }
    }
}
