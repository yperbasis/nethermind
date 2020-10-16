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
using Nethermind.Core.Crypto;
using Nethermind.DataMarketplace.Consumers.Deposits.Domain;
using Nethermind.DataMarketplace.Consumers.Deposits.Queries;
using Nethermind.DataMarketplace.Consumers.Deposits.Repositories;
using Nethermind.DataMarketplace.Consumers.Notifiers;
using Nethermind.DataMarketplace.Consumers.Shared.Background;
using Nethermind.DataMarketplace.Core.Domain;
using Nethermind.DataMarketplace.Core.Services;
using Nethermind.Facade.Proxy;
using Nethermind.Logging;
using Polly;
using Timer = System.Timers.Timer;

namespace Nethermind.DataMarketplace.Consumers.Shared.Services
{
    public class ConsumerServicesBackgroundProcessor : IConsumerServicesBackgroundProcessor, IDisposable
    {
        private readonly IDepositDetailsRepository _depositRepository;
        private readonly IConsumerNotifier _consumerNotifier;
        private readonly INewBlockListener _newBlockListener;
        private readonly bool _useDepositTimer;
        private readonly IEthJsonRpcClientProxy? _ethJsonRpcClientProxy;
        private readonly IEthPriceService _ethPriceService;
        private readonly IBackgroundDepositService _backgroundDepositService;
        private readonly IBackgroundRefundService _backgroundRefundService;
        private readonly IGasPriceService _gasPriceService;
        private readonly ILogger _logger;

        private Timer? _depositTimer;
        private uint _depositTimerPeriod;
        private long _currentBlockTimestamp;
        private long _currentBlockNumber;

        public ConsumerServicesBackgroundProcessor(
            IEthPriceService ethPriceService,
            IBackgroundDepositService backgroundDepositService,
            IBackgroundRefundService backgroundRefundService,
            IGasPriceService gasPriceService,
            IDepositDetailsRepository depositRepository,
            IConsumerNotifier consumerNotifier,
            ILogManager logManager,
            INewBlockListener newBlockListener,
            bool useDepositTimer = false,
            IEthJsonRpcClientProxy? ethJsonRpcClientProxy = null,
            uint depositTimer = 10000)
        {
            _ethPriceService = ethPriceService;
            _backgroundDepositService = backgroundDepositService;
            _backgroundRefundService = backgroundRefundService;
            _gasPriceService = gasPriceService;
            _depositRepository = depositRepository;
            _consumerNotifier = consumerNotifier;
            _newBlockListener = newBlockListener;
            _useDepositTimer = useDepositTimer;
            _ethJsonRpcClientProxy = ethJsonRpcClientProxy;
            _logger = logManager.GetClassLogger();
            _ethPriceService.UpdateAsync();
            _gasPriceService.UpdateAsync();
            _depositTimerPeriod = depositTimer;
        }

        public void Init()
        {
            if (_useDepositTimer)
            {
                if (_depositTimer == null)
                {
                    if (_ethJsonRpcClientProxy == null)
                    {
                        if (_logger.IsError)
                            _logger.Error("Cannot find any configured ETH proxy to run deposit timer.");
                        return;
                    }

                    _depositTimer = new Timer(_depositTimerPeriod);
                    // _depositTimer.Elapsed += DepositTimerOnElapsed;
                    _depositTimer.Start();
                }

                if (_logger.IsInfo) _logger.Info("Initialized NDM consumer services background processor.");
            }
            else
            {
                _newBlockListener.BlockArrived += OnBlockArrived;
            }
        }

        private async void OnBlockArrived(long blockNumber, Keccak blockHash)
        {
            await _consumerNotifier.SendBlockProcessedAsync(blockNumber);
            PagedResult<DepositDetails> depositsToConfirm = await _depositRepository.BrowseAsync(new GetDeposits());
            
            await _backgroundDepositService.TryConfirmDepositsAsync(depositsToConfirm.Items, blockHash);
            PagedResult<DepositDetails> depositsToRefund = await _depositRepository.BrowseAsync(new GetDeposits
            {
                EligibleToRefund = true, CurrentBlockTimestamp = _currentBlockTimestamp, Results = int.MaxValue
            });

            await _backgroundRefundService.TryClaimRefundsAsync(depositsToRefund.Items);
            await _ethPriceService.UpdateAsync();
            await _consumerNotifier.SendEthUsdPriceAsync(_ethPriceService.UsdPrice, _ethPriceService.UpdatedAt);
            await _gasPriceService.UpdateAsync();

            if (_gasPriceService.Types != null)
            {
                await _consumerNotifier.SendGasPriceAsync(_gasPriceService.Types);
            }
        }

        public void Dispose()
        {
            _depositTimer?.Dispose();
        }
    }
}
