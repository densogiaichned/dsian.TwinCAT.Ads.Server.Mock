using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TwinCAT.Ads;

namespace dsian.TwinCAT.Ads.Server.Mock.Tests
{
    [TestClass]
    public class MockTest
    {
        private Mock? _mock = default;
        private ILogger? _logger = default;

        private static int _nextPort  = Environment.Version.Major * 1000 + 200;
        private ushort _port;

        [TestInitialize]
        public void TestInitialize()
        {
            _port = (ushort) Interlocked.Increment(ref _nextPort);
            Console.WriteLine("Setting up Mock server");

            var serviceProvider = new ServiceCollection()
                .AddLogging(cfg => cfg.AddConsole())
                .Configure<LoggerFilterOptions>(cfg => cfg.MinLevel = LogLevel.Debug)
                .BuildServiceProvider();

            _logger = serviceProvider.GetService<ILogger<MockTest>>();

            _mock = new Mock(_port, "AdsServerMock", _logger);
            Assert.IsTrue(_mock.IsConnected);

        }

        [TestCleanup]
        public void TestCleanup()
        {
            _mock?.Disconnect();
            _mock?.Dispose();
        }

        [TestMethod]
        public async Task Should_read_32Bytes_from_Server()
        {
            // arrange
            var ig = 1u;
            var io = 123u;
            var buffer = new byte[256];
            Assert.IsNotNull(_mock);
            Assert.IsNotNull(_mock.ServerAddress);
            _mock.RegisterBehavior(new ReadIndicationBehavior(ig, io, Enumerable.Range(1, buffer.Length).Select(i => (byte)i).ToArray()));
            using var client = new AdsClient();
            client.Connect(_mock.ServerAddress.Port);

            // act
            var result = await client.ReadAsync(ig, io, buffer, CancellationToken.None);

            // assert
            Assert.IsTrue(result.Succeeded);
            Assert.HasCount(result.ReadBytes, buffer);
            Assert.IsTrue(buffer.SequenceEqual(Enumerable.Range(1, buffer.Length).Select(i => (byte)i).ToArray()));
        }

        [TestMethod]
        public async Task Should_write_32Bytes_to_Server()
        {
            // arrange
            var ig = 2u;
            var io = 456u;
            var buffer = new byte[32];
            Assert.IsNotNull(_mock);
            Assert.IsNotNull(_mock.ServerAddress);
            _mock.RegisterBehavior(new WriteIndicationBehavior(ig, io, buffer.Length));
            using var client = new AdsClient();
            client.Connect(_mock.ServerAddress.Port);

            // act
            var result = await client.WriteAsync(ig, io, buffer, CancellationToken.None);

            // assert
            Assert.IsTrue(result.Succeeded);
        }


        [TestMethod]
        public async Task Should_read_write_32Bytes_to_Server()
        {
            // arrange
            var ig = 2u;
            var io = 456u;
            var rdBuffer = new byte[32];
            var wrBuffer = new byte[32];
            Assert.IsNotNull(_mock);
            Assert.IsNotNull(_mock.ServerAddress);
            _mock.RegisterBehavior(new ReadWriteIndicationBehavior(ig, io, rdBuffer.Length, Enumerable.Range(1, rdBuffer.Length).Select(i => (byte)i).ToArray()));
            using var client = new AdsClient();
            client.Connect(_mock.ServerAddress.Port);

            // act
            var result = await client.ReadWriteAsync(ig, io, rdBuffer, wrBuffer, CancellationToken.None);

            // assert
            Assert.IsTrue(result.Succeeded);
            Assert.HasCount(result.ReadBytes, rdBuffer);
            Assert.IsTrue(rdBuffer.SequenceEqual(Enumerable.Range(1, rdBuffer.Length).Select(i => (byte)i).ToArray()));
        }
    }
}
