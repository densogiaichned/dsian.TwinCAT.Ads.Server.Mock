using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using TwinCAT.Ads;

namespace dsian.TwinCAT.Ads.Server.Mock.Tests
{
    [TestClass]
    public class BehaviorManagerTest
    {

        [TestMethod]
        public void Should_register_ReadIndicationBehavior()
        {
            var bm = new BehaviorManager(null);
            bm.RegisterBehavior(new ReadIndicationBehavior(1, 1, null, AdsErrorCode.NoError));
            Assert.HasCount(1, bm.BehaviorDictionary.Keys);
        }

        [TestMethod]
        public void Should_register_all_behaviors()
        {
            var bm = new BehaviorManager(null);
            bm.RegisterBehavior(new ReadIndicationBehavior(1, 1, null, AdsErrorCode.NoError));
            bm.RegisterBehavior(new WriteIndicationBehavior(1, 1, 0, AdsErrorCode.NoError));
            bm.RegisterBehavior(new AddDeviceNotificationIndicationBehavior(1, 1, 1, null, 1, AdsErrorCode.NoError));
            bm.RegisterBehavior(new DeleteDeviceNotificationIndicationBehavior(1, AdsErrorCode.NoError));
            bm.RegisterBehavior(new ReadDeviceInfoIndicationBehavior(1, 1, 1, "test", AdsErrorCode.NoError));
            bm.RegisterBehavior(new ReadDeviceStateIndicationBehavior(AdsState.Config, 1, AdsErrorCode.NoError));
            bm.RegisterBehavior(new ReadWriteIndicationBehavior(1, 1, 1, null, AdsErrorCode.NoError));
            bm.RegisterBehavior(new WriteControlIndicationBehavior(AdsState.Config, 1, 1, AdsErrorCode.NoError));

            Assert.HasCount(8, bm.BehaviorDictionary.Keys);
        }

        [TestMethod]
        public void Should_register_and_retrieve_3_WriteIndicationBehavior()

        {
            var bm = new BehaviorManager(null);
            bm.RegisterBehavior(new WriteIndicationBehavior(1, 1, 0, AdsErrorCode.NoError));
            bm.RegisterBehavior(new WriteIndicationBehavior(1, 2, 0, AdsErrorCode.NoError));
            bm.RegisterBehavior(new WriteIndicationBehavior(1, 3, 0, AdsErrorCode.NoError));
            bm.RegisterBehavior(new ReadIndicationBehavior(1, 1, new byte[1], AdsErrorCode.NoError));

            var res = bm.GetBehaviorOfType<WriteIndicationBehavior>();
            Assert.IsNotNull(res);
            Assert.AreEqual(3, res.Count());
        }

        [TestMethod]
        public void Should_register_and_retrieve_a_ReadIndicationBehavior_Ig1_I01_Len123()

        {
            var bm = new BehaviorManager(null);
            bm.RegisterBehavior(new ReadIndicationBehavior(1, 1, new byte[123], AdsErrorCode.NoError));
            bm.RegisterBehavior(new ReadIndicationBehavior(1, 1, null, AdsErrorCode.NoError));
            var res = bm.GetBehaviorOfType<ReadIndicationBehavior>(b => b.IndexGroup == 1 && b.IndexOffset == 1 && b.ResponseData.Length == 123);
            Assert.IsNotNull(res);
            Assert.IsInstanceOfType(res, typeof(ReadIndicationBehavior));
            Assert.AreEqual(123, res.ResponseData.Length);
        }
    }
}
