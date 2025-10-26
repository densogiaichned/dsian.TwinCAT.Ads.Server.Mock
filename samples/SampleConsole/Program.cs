using dsian.TwinCAT.Ads.Server.Mock;
using dsian.TwinCAT.Ads.Server.Mock.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.ValueAccess;

namespace SampleConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var serviceProvider = new ServiceCollection()
        .AddLogging(cfg => cfg.AddConsole())
        .Configure<LoggerFilterOptions>(cfg => cfg.MinLevel = LogLevel.Information)
        .BuildServiceProvider())
            {
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<Program>();

                // setup mocking server
                ushort port = 12345;
                string portName = "MyTestAdsServer";
                using var mockServer = new Mock(port, portName, loggerFactory);
                var serverBuffer = new byte[65535];
                mockServer.RegisterBehavior(new ReadIndicationBehavior(IndexGroup: 1, IndexOffset: 123, Enumerable.Range(1, 32).Select(i => (byte)i).ToArray()))
                          .RegisterBehavior(new ReadIndicationBehavior(1, 1, Encoding.UTF8.GetBytes("acting as a ADS server")))
                          .RegisterBehavior(new ReadIndicationBehavior(0, 0, null, AdsErrorCode.DeviceAccessDenied))
                          .RegisterBehavior(new WriteIndicationBehavior(0, 0, 22));

                Console.WriteLine("Server up and running");

                // now the actual Ads Read/WriteRequests...

                // create TwinCAT Ads client
                using (var client = new AdsClient(loggerFactory))
                {
                    // connect to our mocking server
                    Assert.IsNotNull(mockServer.ServerAddress);
                    client.Connect(mockServer.ServerAddress.Port);
                    if (client.IsConnected)
                    {
                        // . . .
                        var readBuffer = new byte[65535];
                        var readMemory = new Memory<byte>(readBuffer);
                        var writeBuffer = new byte[65535];
                        var writeMemory = new Memory<byte>(writeBuffer);

                        // 1st behavior
                        var resRd = await client.ReadAsync(1, 123, readMemory[..32], CancellationToken.None);
                        // 2nd behavior
                        resRd = await client.ReadAsync(1, 1, readMemory, CancellationToken.None);
                        Console.WriteLine(Encoding.UTF8.GetString(readMemory.Slice(0, resRd.ReadBytes).Span));
                        // 3rd behavior
                        resRd = await client.ReadAsync(0, 0, readMemory, CancellationToken.None);
                        // 4th behavior
                        var resWr = await client.WriteAsync(0, 0, writeMemory[..22], CancellationToken.None);
                    }
                }


                var myAdsCls = new MyAdsClass(mockServer.ServerAddress.Port);
                var even = await myAdsCls.GetValuesFilteredAsync(1, 123, (x) => x % 2 == 0);
                Assert.IsTrue(even.SequenceEqual(new byte[] { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32 }));
                var odd = await myAdsCls.GetValuesFilteredAsync(1, 123, (x) => x % 2 != 0);
                Assert.IsTrue(odd.SequenceEqual(new byte[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31 }));

                Console.WriteLine();
                Console.WriteLine(" -- dsian.TwinCAT.Ads.Server.Mock.Extensions -- ");
                // dsian.TwinCAT.Ads.Server.Mock.Extensions                   
                // Extension to register Behaviors from a recorded TwinCAT Ads Viewer file (*.cap).
                // This can be used for more sophisticated ADS requests, like reading symbols from a server.
                mockServer.RegisterReplay(@"./SampleFiles/ReadSymbolsPort851.cap");

                // create TwinCAT Ads client
                using (var client = new AdsClient(loggerFactory))
                {
                    // connect to our mocking server
                    client.Connect(mockServer.ServerAddress.Port);
                    if (client.IsConnected)
                    {
                        var symbolLoader = SymbolLoaderFactory.Create(client, new SymbolLoaderSettings(SymbolsLoadMode.Flat, ValueAccessMode.SymbolicByHandle));
                        var symbols = await symbolLoader.GetSymbolsAsync(CancellationToken.None);
                        Assert.IsNotNull(symbols.Symbols);
                        foreach (var symbol in symbols.Symbols)
                            Console.WriteLine(symbol.InstancePath);
                    }
                }
            }

            Console.WriteLine("Press any key to close...");
            Console.ReadKey();
        }
    }
}

