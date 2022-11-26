
![build](https://github.com/densogiaichned/dsian.TwinCAT.Ads.Server.Mock/workflows/build/badge.svg?branch=main)
[![NuGet version (dsian.TwinCAT.AdsViewer.CapParser.Lib)](https://img.shields.io/nuget/v/dsian.TwinCAT.Ads.Server.Mock.svg?style=flat-square)](https://www.nuget.org/packages/dsian.TwinCAT.Ads.Server.Mock/)
# dsian.TwinCAT.Ads.Server.Mock

Mocking a TwinCAT Ads Server, for unit testing code with ADS read/write requests.  
With this Nuget package it is possible to test your code even if it is relying on `TwinCAT.Ads.dll` calls and dependencies on that library (e.g. `TwinCAT.Ads.TypeSystem.SymbolFactory`).  
You don't need to setup a PLC or TwinCAT runtime to test your code, hence no special requirements on a build server.

* [How-to](#how-to)
* [Example](#example)
* [Extensions](#extensions)

---

## How-to
1. **Setup mocking server**  
    ```csharp
    ushort port = 12345;
    string portName = "MyTestAdsServer";
    using (var mockServer = new Mock(port, portName))   // ILogger optional
    {
        // ...
    }
    ```
2. **Register Behaviors**
   ```csharp
    mockServer.RegisterBehavior(new ReadIndicationBehavior(1, 123,  Enumerable.Range(1,32).Select(i => (byte)i).ToArray()))
            .RegisterBehavior(new ReadIndicationBehavior(1, 1, Encoding.UTF8.GetBytes("acting as a ADS server")))
            .RegisterBehavior(new ReadIndicationBehavior(0, 0, null, AdsErrorCode.DeviceAccessDenied))
            .RegisterBehavior(new WriteIndicationBehavior(0, 0, 22));
    ```
    * **Behaviors**  
    Behaviros like `ReadIndicationBehavior` are used as enpdpoint for a specific ADS call, e.g.  
        ```csharp
        new ReadIndicationBehavior(1, 123,  Enumerable.Range(1,32).Select(i => (byte)i).ToArray())
        ```
        A `ReadAsync` ADS call is made to `IndexGroup=1`, `IndexOffset=123` with `Length=32`, in this case the server will return 32 bytes (0x01..0x20) to the client.

3. **Test**  
    For example, the class we want to test:
    ```csharp
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    namespace Sample
    {
        public class MyAdsClass
        {
            public MyAdsClass(int port)
            {
                Port = port;
            }
            public int Port { get; }

            private byte[] _buffer = new byte[32];

            public async Task<byte[]> GetValuesFilteredAsync(uint ig, uint io, Func<byte, bool> filterFunc)
            {
                using (var client = new TwinCAT.Ads.AdsClient())
                {
                    client.Connect(Port);
                    var result = await client.ReadAsync(ig, io, _buffer, CancellationToken.None);
                    if (result.Succeeded)
                        return _buffer.Where(x => filterFunc(x)).Select(x => x).ToArray();
                }
                return new byte[0];
            }
        }
    }
    ```
    Unit test:
    ```csharp
    [TestMethod]
    public async Task Should_filter_even_and_odd_values()
    {
        // arrange
        ushort port = 12345;
        string portName = "MyTestAdsServer";
        using (var mockServer = new Mock(port, portName))   // ILogger optional
        {
            mockServer.RegisterBehavior(new ReadIndicationBehavior(1, 123, Enumerable.Range(1, 32).Select(i => (byte)i).ToArray()));
            var myAdsCls = new MyAdsClass(mockServer.ServerAddress.Port);

            // act
            var even = await myAdsCls.GetValuesFilteredAsync(1, 123, (x) => x % 2 == 0);
            var odd = await myAdsCls.GetValuesFilteredAsync(1, 123, (x) => x % 2 != 0);

            // assert
            Assert.IsTrue(even.SequenceEqual(new byte[] { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32 }));
            Assert.IsTrue(odd.SequenceEqual(new byte[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31 }));
        }
    }
    ```
## Example  
```csharp
// setup mocking server
ushort port = 12345;
string portName = "MyTestAdsServer";
using (var mockServer = new Mock(port, portName))
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
            var readBuffer = new byte[65535];
            var readMemory = new Memory<byte>(readBuffer);
            var writeBuffer = new byte[65535];
            var writeMemory = new Memory<byte>(writeBuffer);

            // 1st behavior
            var resRd = await client.ReadAsync(1, 123, readMemory[..32], CancellationToken.None);
            // 2nd behavior
            resRd = await client.ReadAsync(1, 1, readMemory, CancellationToken.None);
            Console.WriteLine(Encoding.UTF8.GetString(readMemory.Slice(0,resRd.ReadBytes).Span));
            // 3rd behavior
            resRd = await client.ReadAsync(0, 0, readMemory, CancellationToken.None);
            // 4th behavior
            var resWr = await client.WriteAsync(0, 0, writeMemory[..22], CancellationToken.None);
        }
    }
}
```
See `SampleConsole` and `UnitTestSample` for functional examples.

## Extensions
These extension methods are defined in project [dsian.TwinCAT.Ads.Server.Mock.Extensions](https://github.com/densogiaichned/dsian.TwinCAT.Ads.Server.Mock/tree/main/src/dsian.TwinCAT.Ads.Server.Mock.Extensions):
* [Replay](README.Extensions.md#replay)
