using dsian.TwinCAT.Ads.Server.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SampleConsole;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTestSample
{
    [TestClass]
    public class UnitTestSample
    {
        private static ushort _port = (ushort)(1000 + Environment.Version.Major * 1000 + Environment.Version.Minor * 100 + Environment.Version.Build);

        [TestInitialize]
        public void TestInitialize()
        {
            _port += 1;
        }

        [TestMethod]
        public async Task Should_filter_even_and_odd_values()
        {
            // arrange
            string portName = "MyTestAdsServer";
            using (var mockServer = new Mock(_port, portName))   // ILogger optional
            {
                mockServer.RegisterBehavior(new ReadIndicationBehavior(1, 123, Enumerable.Range(1, 32).Select(i => (byte)i).ToArray()));
                Assert.IsNotNull(mockServer.ServerAddress);
                var myAdsCls = new MyAdsClass(mockServer.ServerAddress.Port);

                // act
                var even = await myAdsCls.GetValuesFilteredAsync(1, 123, (x) => x % 2 == 0);
                var odd = await myAdsCls.GetValuesFilteredAsync(1, 123, (x) => x % 2 != 0);

                // assert
                Assert.IsTrue(even.SequenceEqual(new byte[] { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32 }));
                Assert.IsTrue(odd.SequenceEqual(new byte[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31 }));
            }
        }
    }
}
