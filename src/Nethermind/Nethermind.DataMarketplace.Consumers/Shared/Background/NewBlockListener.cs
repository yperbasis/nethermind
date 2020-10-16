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
using System.Threading;
using System.Timers;
using Nethermind.Core.Crypto;
using Nethermind.DataMarketplace.Core.Services;
using Polly;
using Timer = System.Timers.Timer;

namespace Nethermind.DataMarketplace.Consumers.Shared.Background
{
    public class NewBlockListener : INewBlockListener
    {
        public event Action<long, Keccak>? BlockArrived;
        private readonly INdmBlockchainBridge _ndmBlockchainBridge;
        private long _currentBlockNumber;
        
        public NewBlockListener(INdmBlockchainBridge ndmBlockchainBridge)
        {
            _ndmBlockchainBridge = ndmBlockchainBridge;
            Timer timer = new Timer(1000);
            timer.Elapsed += TimerOnElapsed;
            timer.Start();
        }
        
        private async void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            var block = await Policy.Handle<Exception>()
                .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(1))
                .ExecuteAsync(async () => await _ndmBlockchainBridge.GetLatestBlockAsync());
            
            if (Interlocked.CompareExchange(ref _currentBlockNumber, block.Number, _currentBlockNumber) ==
                _currentBlockNumber)
            {
                return;
            }

            if (_currentBlockNumber == 0)
            {
                return;
            }
            
            BlockArrived?.Invoke(block.Number, block.Hash);
        }
    }
}
