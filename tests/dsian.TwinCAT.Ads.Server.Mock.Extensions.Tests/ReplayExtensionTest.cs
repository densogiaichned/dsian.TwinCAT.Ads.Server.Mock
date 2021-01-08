using dsian.TwinCAT.AdsViewer.CapParser.Lib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace dsian.TwinCAT.Ads.Server.Mock.Extensions.Tests
{
    [TestClass]
    public class ReplayExtensionTest
    {

        [TestMethod]
        public void Should_throw_ArgumentNullException_parameter_netMonFile()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                var rpe = new ReplayExtension(netMonFile: null);
            });
        }

        [TestMethod]
        public void Should_throw_ArgumentNullException_parameter_path()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                var rpe = new ReplayExtension("");
            });
        }

        [TestMethod]
        public void Should_not_throw_on_invalid_file_path()
        {
            var rpe = new ReplayExtension("invalidPath.cap");
            Assert.IsNull(rpe.BehaviorList);
        }

        [TestMethod]
        public async Task Should_add_one_ReadIndicationBehavior_to_BehaviorList_from_NetMonFile()
        {
            var capFilePath = @".\TestFiles\ReadRequestPort10k.cap";
            var (ok, nmf) = await NetMonFileFactory.TryParseNetMonFileAsync(capFilePath, CancellationToken.None,null);
            Assert.IsTrue(ok);
            var rpe = new ReplayExtension(nmf);
            Assert.IsNotNull(rpe.BehaviorList);
            Assert.AreEqual(rpe.BehaviorList.Count(), 1);
            Assert.IsInstanceOfType(rpe.BehaviorList.First(), typeof(ReadIndicationBehavior));
        }

        [TestMethod]
        public void Should_add_one_ReadIndicationBehavior_to_BehaviorList_from_testfile()
        {
            var rpe = new ReplayExtension(@".\TestFiles\ReadRequestPort10k.cap");

            Assert.IsNotNull(rpe.BehaviorList);
            Assert.AreEqual(rpe.BehaviorList.Count(), 1);
            Assert.IsInstanceOfType(rpe.BehaviorList.First(), typeof(ReadIndicationBehavior));
        }

        [TestMethod]
        public void Should_add_one_ReadDeviceInfoIndicationBehavior_to_BehaviorList_from_testfile()
        {
            var rpe = new ReplayExtension(@".\TestFiles\ReadDeviceInfo.cap");

            Assert.IsNotNull(rpe.BehaviorList);
            Assert.AreEqual(rpe.BehaviorList.Count(), 1);
            Assert.IsInstanceOfType(rpe.BehaviorList.First(), typeof(ReadDeviceInfoIndicationBehavior));
        }

    }
}
