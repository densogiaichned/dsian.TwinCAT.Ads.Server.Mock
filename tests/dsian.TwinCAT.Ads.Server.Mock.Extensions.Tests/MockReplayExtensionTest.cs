using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.ValueAccess;

namespace dsian.TwinCAT.Ads.Server.Mock.Extensions.Tests
{
    [TestClass]
    public class MockReplayExtensionTest
    {
        [TestMethod]
        public void Should_register_one_ReadIndicationBehavior_from_testfile()
        {
            using (var serverMock = new Mock(12300, "AdsServerMock"))
            {
                serverMock.RegisterReplay(@"./TestFiles/ReadRequestPort10k.cap");
                Assert.IsNotNull(serverMock.BehaviorManager);
                Assert.IsTrue(serverMock.BehaviorManager.BehaviorDictionary.Keys.Count == 1);
                var res = serverMock.BehaviorManager.GetBehaviorOfType<ReadIndicationBehavior>();
                Assert.IsNotNull(res);
                Assert.AreEqual(res.Count(), 1);
            }
        }

        [TestMethod]
        public void Should_register_three_Behaviors_from_testfile()
        {
            using (var serverMock = new Mock(12301, "AdsServerMock"))
            {
                serverMock.RegisterReplay(@"./TestFiles/ReadSymbolsPort851.cap");
                Assert.IsNotNull(serverMock.BehaviorManager);
                Assert.IsTrue(serverMock.BehaviorManager.BehaviorDictionary.Keys.Count == 3);
                var res = serverMock.BehaviorManager.GetBehaviorOfType<Behavior>();
            }
        }

        [TestMethod]
        public async Task Should_read_symbols_from_server()
        {
            using (var serverMock = new Mock(12302, "AdsServerMock"))
            {
                serverMock.RegisterReplay(@"./TestFiles/ReadSymbolsPort851.cap");
                using (var client = new AdsClient())
                {
                    // connect to our mocking server
                    Assert.IsNotNull(serverMock.ServerAddress);
                    client.Connect(serverMock.ServerAddress.Port);
                    if (client.IsConnected)
                    {
                        var symbolLoader = SymbolLoaderFactory.Create(client, new SymbolLoaderSettings(SymbolsLoadMode.Flat, ValueAccessMode.SymbolicByHandle));
                        var symbols = await symbolLoader.GetSymbolsAsync(CancellationToken.None);
                        Assert.IsTrue(symbols.Succeeded);
                    }
                }
            }
        }

        [TestMethod]
        public async Task WriteControl_should_succeed()
        {
            using var serverMock = new Mock(12303, "AdsServerMock");
            serverMock.RegisterReplay(@"./TestFiles/ReadStateWriteControl.cap");
            using var client = new AdsClient();

            Assert.IsNotNull(serverMock.ServerAddress);
            client.Connect(serverMock.ServerAddress.Port);
            Assert.IsTrue(client.IsConnected);
            var actual = await client.WriteControlAsync(AdsState.Stop, 0, CancellationToken.None);
            Assert.IsTrue(actual.Succeeded);
            Assert.AreEqual(actual.ErrorCode, AdsErrorCode.NoError);
        }
    }
}
