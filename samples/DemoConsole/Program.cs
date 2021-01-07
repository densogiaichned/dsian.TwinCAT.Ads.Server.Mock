using dsian.TwinCAT.Ads.Server.Mock;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads;

namespace DemoConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var serviceProvider = new ServiceCollection()
        .AddLogging(cfg => cfg.AddConsole())
        .Configure<LoggerFilterOptions>(cfg => cfg.MinLevel = LogLevel.Debug)
        .BuildServiceProvider())
            {
                var logger = serviceProvider.GetService<ILogger<Program>>();

                // setup mocking server
                ushort port = 0xffff;
                string portName = "MyTestPort";
                using (var mockServer = new Mock(port, portName,logger))
                {
                    var serverBuffer = new byte[65535];
                    mockServer.RegisterBehavior(new ReadIndicationBehavior(1, 123,  Enumerable.Range(1,32).Select(i => (byte)i).ToArray()))
                              .RegisterBehavior(new ReadIndicationBehavior(1, 1, Encoding.UTF8.GetBytes("acting as a ADS server")))
                              .RegisterBehavior(new ReadIndicationBehavior(0, 0, null, AdsErrorCode.DeviceAccessDenied))
                              .RegisterBehavior(new WriteIndicationBehavior(0, 0, 22));

                    Console.WriteLine("Server up and running");

                    // now the actual Ads Read/WriteRequests...

                    // create TwinCAT Ads client
                    using (var client = new AdsClient(logger))
                    {
                        // connect to our mocking server
                        client.Connect(port);
                        if (client.IsConnected)
                        {
                            // . . .
                            var buffer = new byte[65535];
                            var memory = new Memory<byte>(buffer);
                            // 1st behavior
                            var resRd = await client.ReadAsync(1, 123, memory[..32], CancellationToken.None);
                            // 2nd behavior
                            resRd = await client.ReadAsync(1, 1, memory, CancellationToken.None);
                            Console.WriteLine(Encoding.UTF8.GetString(memory.Slice(0,resRd.ReadBytes).Span));
                            // 3rd behavior
                            resRd = await client.ReadAsync(0, 0, memory, CancellationToken.None);
                            // 4th behavior
                            var resWr = await client.WriteAsync(0, 0, memory[..22], CancellationToken.None);
                        }
                    }
                }
            }

            Console.WriteLine("Press any key to close...");
            Console.ReadKey();
        }
    }
}
