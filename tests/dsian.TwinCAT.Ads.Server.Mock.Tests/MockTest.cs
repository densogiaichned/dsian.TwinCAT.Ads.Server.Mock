using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads;

namespace dsian.TwinCAT.Ads.Server.Mock.Tests
{
    [TestClass]
    public class MockTest
    {
        private static Mock _Mock = default;
        private static ILogger _Logger = default;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            System.Console.WriteLine("Setting up Mock server");

            var serviceProvider = new ServiceCollection()
                .AddLogging(cfg => cfg.AddConsole())
                .Configure<LoggerFilterOptions>(cfg => cfg.MinLevel = LogLevel.Debug)
                .BuildServiceProvider();

            _Logger = serviceProvider.GetService<ILogger<MockTest>>();

            _Mock = new Mock(12345, "AdsServerMock", _Logger);
            Assert.IsTrue(_Mock.IsConnected);

        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            if (_Mock is not null)
            {
                _Mock.Disconnect();
            }
        }

        [TestMethod]
        public async Task Should_read_32Bytes_from_Server()
        {
            // arrange
            var ig = 1u;
            var io = 123u;
            var buffer = new byte[256];
            _Mock.RegisterBehavior(new ReadIndicationBehavior(ig, io, Enumerable.Range(1, buffer.Length).Select(i => (byte)i).ToArray()));
            using (var client = new AdsClient())
            {
                client.Connect(_Mock.ServerAddress.Port);

                // act
                var result = await client.ReadAsync(ig, io, buffer, CancellationToken.None);

                // assert
                Assert.IsTrue(result.Succeeded);
                Assert.AreEqual(result.ReadBytes, buffer.Length);
            }

        }
    }
}
