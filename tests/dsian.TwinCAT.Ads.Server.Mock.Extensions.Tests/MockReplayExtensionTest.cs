using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.ValueAccess;

namespace dsian.TwinCAT.Ads.Server.Mock.Extensions.Tests
{
    [TestClass]
    public class MockReplayExtensionTest
    {
        private static ushort _port = (ushort)(Environment.Version.Major * 1000);

        [TestInitialize]
        public void TestInitialize()
        {
            _port += 1;
        }

        [TestMethod]
        public void Should_register_one_ReadIndicationBehavior_from_testfile()
        {
            using (var serverMock = new Mock(_port, "AdsServerMock"))
            {
                serverMock.RegisterReplay(@"./TestFiles/ReadRequestPort10k.cap");
                Assert.IsNotNull(serverMock.BehaviorManager);
                Assert.HasCount(1, serverMock.BehaviorManager.BehaviorDictionary.Keys);
                var res = serverMock.BehaviorManager.GetBehaviorOfType<ReadIndicationBehavior>();
                Assert.IsNotNull(res);
                Assert.AreEqual(1, res.Count());
            }
        }

        [TestMethod]
        public void Should_register_three_Behaviors_from_testfile()
        {
            using (var serverMock = new Mock(_port, "AdsServerMock"))
            {
                serverMock.RegisterReplay(@"./TestFiles/ReadSymbolsPort851.cap");
                Assert.IsNotNull(serverMock.BehaviorManager);
                Assert.HasCount(3, serverMock.BehaviorManager.BehaviorDictionary.Keys);
                var res = serverMock.BehaviorManager.GetBehaviorOfType<Behavior>();
            }
        }

        [TestMethod]
        public async Task Should_read_symbols_from_server()
        {
            using (var serverMock = new Mock(_port, "AdsServerMock"))
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
            using var serverMock = new Mock(_port, "AdsServerMock");
            serverMock.RegisterReplay(@"./TestFiles/ReadStateWriteControl.cap");
            using var client = new AdsClient();

            Assert.IsNotNull(serverMock.ServerAddress);
            client.Connect(serverMock.ServerAddress.Port);
            Assert.IsTrue(client.IsConnected);
            var actual = await client.WriteControlAsync(AdsState.Stop, 0, CancellationToken.None);
            Assert.IsTrue(actual.Succeeded);
            Assert.AreEqual(AdsErrorCode.NoError, actual.ErrorCode);
        }
    }
}

