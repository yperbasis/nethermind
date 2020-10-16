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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nethermind.Core;
using Nethermind.DataMarketplace.Consumers.Deposits.Domain;
using Nethermind.DataMarketplace.Consumers.Notifiers;
using Nethermind.DataMarketplace.Consumers.Refunds;
using Nethermind.DataMarketplace.Consumers.Shared.Services.Models;
using Nethermind.Logging;

namespace Nethermind.DataMarketplace.Consumers.Shared.Background.Services
{
    public class BackgroundRefundService : IBackgroundRefundService
    {
        private readonly IAccountService _accountService;
        private readonly ILogger _logger;
        private readonly IRefundClaimant _refundClaimant;
        private readonly IConsumerNotifier _consumerNotifier;

        public BackgroundRefundService(IAccountService accountService, IConsumerNotifier consumerNotifier,
            ILogManager logManager, IRefundClaimant refundClaimant)
        {
            _accountService = accountService;
            _refundClaimant = refundClaimant;
            _consumerNotifier = consumerNotifier;
            _logger = logManager.GetClassLogger();
        }

        public async Task TryClaimRefundsAsync(IReadOnlyList<DepositDetails> deposits)
        {
            if (!deposits.Any())
            {
                if (_logger.IsInfo) _logger.Info("No claimable refunds have been found.");
                return;
            }

            if (_logger.IsInfo) _logger.Info($"Found {deposits.Count} claimable refunds.");

            foreach (DepositDetails deposit in deposits)
            {
                Address refundTo = _accountService.GetAddress();
                RefundClaimStatus earlyRefundClaimStatus = await _refundClaimant.TryClaimEarlyRefundAsync(deposit, refundTo);
                if (earlyRefundClaimStatus.IsConfirmed)
                {
                    await _consumerNotifier.SendClaimedEarlyRefundAsync(deposit.Id, deposit.DataAsset.Name,
                        earlyRefundClaimStatus.TransactionHash!);
                }

                RefundClaimStatus refundClaimStatus = await _refundClaimant.TryClaimRefundAsync(deposit, refundTo);
                if (refundClaimStatus.IsConfirmed)
                {
                    await _consumerNotifier.SendClaimedRefundAsync(deposit.Id, deposit.DataAsset.Name,
                        refundClaimStatus.TransactionHash!);
                }
            }
        }
    }
}
