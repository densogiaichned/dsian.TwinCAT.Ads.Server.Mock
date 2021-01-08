
![build](https://github.com/densogiaichned/dsian.TwinCAT.Ads.Server.Mock/workflows/build/badge.svg?branch=main)
# dsian.TwinCAT.Ads.Server.Mock.Extensions

Extension methods for [dsian.TwinCAT.Ads.Server.Mock](https://github.com/densogiaichned/dsian.TwinCAT.Ads.Server.Mock), on [Nuget](https://www.nuget.org/packages/dsian.TwinCAT.Ads.Server.Mock/).

* **[Replay](#replay)**
  * [Example](#replayexample)
  * [TC3 ADS Monitor](#tc3adsmonitor)   
---

# Replay

Extension to register Behaviors from a recorded [TC3 ADS Monitor](https://www.beckhoff.com/en-en/products/automation/twincat/tfxxxx-twincat-3-functions/tf6xxx-tc3-connectivity/tf6010.html) file (*.cap, see [dsian.TwinCAT.AdsViewer.CapParser](https://github.com/densogiaichned/dsian.TwinCAT.AdsViewer.CapParser) or [Nuget](https://www.nuget.org/packages/dsian.TwinCAT.AdsViewer.CapParser.Lib/)).  
This can be used for more sophisticated ADS requests, like reading symbols from a server.  
With that tool, you can record a specific ADS communication and replay it for unit testing, see documentation [TF6010 | TwinCAT ADS Monitor](https://infosys.beckhoff.com/index.php?content=../content/1033/tc3_ads_diag_aid/36028797134849931.html).

```csharp
using (var serverMock = new Mock(12347, "AdsServerMock"))
{
    serverMock.RegisterReplay(@".\TestFiles\RecordedAdsCommunication.cap");
    // . . .
}
```
<a name="replayexample"/>

## Example

```csharp
using (var serverMock = new Mock(12347, "AdsServerMock"))
{
    serverMock.RegisterReplay(@".\TestFiles\RecordedAdsCommunication.cap");
    using (var client = new AdsClient())
    {
        // connect to our mocking server
        client.Connect(serverMock.ServerAddress.Port);
        if (client.IsConnected)
        {
            var symbolLoader = SymbolLoaderFactory.Create(client, new SymbolLoaderSettings(SymbolsLoadMode.Flat, TcAds.ValueAccess.ValueAccessMode.Default));
            var symbols = await symbolLoader.GetSymbolsAsync(CancellationToken.None);
            Assert.IsTrue(symbols.Succeeded);
        }
    }
}
```  
<a name="tc3adsmonitor"/>

## TC3 ADS Monitor:
Download: [TF6010 | TC3 ADS Monitor](https://www.beckhoff.com/en-en/products/automation/twincat/tfxxxx-twincat-3-functions/tf6xxx-tc3-connectivity/tf6010.html)  
Documentation: [infosys.beckhoff.com](https://infosys.beckhoff.com/index.php?content=../content/1033/tc3_ads_diag_aid/36028797134849931.html)  
Minimum requirement for TC3 ADS Monitor: [TC1000 | TC3 ADS](https://www.beckhoff.com/en-en/products/automation/twincat/tc1xxx-twincat-3-base/tc1000.html)
