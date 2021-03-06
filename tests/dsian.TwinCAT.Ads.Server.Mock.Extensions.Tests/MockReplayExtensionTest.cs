﻿using Microsoft.Extensions.DependencyInjection;
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
using TcAds = TwinCAT.Ads;  // due to poor namespace naming, there are collisions with Beckhoff TwinCAT.Ads...

namespace dsian.TwinCAT.Ads.Server.Mock.Extensions.Tests
{
    [TestClass]
    public class MockReplayExtensionTest
    {

        [TestMethod]
        public void Should_register_one_ReadIndicationBehavior_from_testfile()
        {
            using (var serverMock = new Mock(12345, "AdsServerMock"))
            {
                serverMock.RegisterReplay(@".\TestFiles\ReadRequestPort10k.cap");

                Assert.IsTrue(serverMock.BehaviorManager.BehaviorDictionary.Keys.Count== 1);
                var res = serverMock.BehaviorManager.GetBehaviorOfType<ReadIndicationBehavior>();
                Assert.IsNotNull(res);
                Assert.AreEqual(res.Count(), 1);
            }
        }

        [TestMethod]
        public void Should_register_three_Behaviors_from_testfile()
        {
            using (var serverMock = new Mock(12346, "AdsServerMock"))
            {
                serverMock.RegisterReplay(@".\TestFiles\ReadSymbolsPort851.cap");
                Assert.IsTrue(serverMock.BehaviorManager.BehaviorDictionary.Keys.Count == 3);
                var res = serverMock.BehaviorManager.GetBehaviorOfType<Behavior>();
            }
        }

        [TestMethod]
        public async Task Should_read_symbols_from_server()
        {
            using (var serverMock = new Mock(12347, "AdsServerMock"))
            {
                serverMock.RegisterReplay(@".\TestFiles\ReadSymbolsPort851.cap");
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
        }
    }
}
