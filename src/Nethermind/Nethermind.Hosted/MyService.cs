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
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Nethermind.Config;

namespace Nethermind.Hosted
{
    public class MyService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IAsyncExperiment _asyncExperiment;
        private readonly IExperiment _experiment;

        public MyService(
            IServiceProvider serviceProvider,
            IHostApplicationLifetime appLifetime,
            IConfigProvider configProvider,
            IAsyncExperiment asyncExperiment,
            IExperiment experiment)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _appLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));
            _asyncExperiment = asyncExperiment;
            _experiment = experiment;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _appLifetime.ApplicationStopped.Register(OnStopped);
            await Task.Delay(200000000, stoppingToken);
        }

        public void OnStopped()
        {
            for (int i = 0; i < 2; i++)
            {
                Console.WriteLine($"Closing {i}");
                Task.Delay(1000).GetAwaiter().GetResult();
            }

            // Log.Information("Window will close automatically in 20 seconds.");

            Console.WriteLine("Closed");
        }
    }
}