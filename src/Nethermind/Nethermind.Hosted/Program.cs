using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Nethermind.Hosted
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<MyService>();
                    services.AddNethermindJson();
                    services.AddNethermindConfigProvider();
                    services.AddNethermindConfigSources("a", new Dictionary<string, string>());
                    services.AddExperiments();
                    services.Configure<HostOptions>(
                        opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(15));
                })
                .UseConsoleLifetime()
                .Build();

            Task shutdown = host.WaitForShutdownAsync();
            
            await host.RunAsync().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Console.WriteLine("Faulted");
                    Console.WriteLine(t.Exception);
                    Console.ReadLine();
                }

                if (t.IsCanceled)
                {
                    Console.WriteLine("Canceled");
                }

                if (t.IsCompleted)
                {
                    Console.WriteLine("Complete");
                }
            }); // use RunAsync() if you have access to async Main()

            await shutdown;
            
            Console.WriteLine("Awaited");
        }
    }
}