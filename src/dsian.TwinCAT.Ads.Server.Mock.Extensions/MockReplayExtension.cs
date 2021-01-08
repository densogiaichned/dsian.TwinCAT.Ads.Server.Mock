using dsian.TwinCAT.Ads.Server.Mock;
using dsian.TwinCAT.AdsViewer.CapParser.Lib;
using dsian.TwinCAT.AdsViewer.CapParser.Lib.Cap;
using System.Collections.Generic;
using System.Linq;

namespace dsian.TwinCAT.Ads.Server.Mock.Extensions
{


    /// <summary>
    /// Extension methods for <see cref="Mock"/>
    /// </summary>
    public static class MockReplayExtension
    {

        /// <summary>
        /// Parses a *.cap file and regsiters all behaviors
        /// </summary>
        /// <param name="mockServer"></param>
        /// <param name="pathToCap"></param>
        /// <returns></returns>
        public static Mock RegisterReplay(this Mock mockServer, string pathToCap)
        {
            var rpe = new ReplayExtension(pathToCap);
            rpe.BehaviorList?.RegisterAllBehaviors(mockServer);
            return mockServer;
        }

        /// <summary>
        /// Registers all behaviors from a <see cref="NetMonFile"/>
        /// </summary>
        /// <param name=""></param>
        /// <param name="netMonFile">a parsed *.cap file from <see cref="NetMonFileFactory"/></param>
        /// <returns></returns>
        public static Mock RegisterReplay(this Mock mockServer, NetMonFile netMonFile)
        {
            var rpe = new ReplayExtension(netMonFile);
            rpe.BehaviorList?.RegisterAllBehaviors(mockServer);
            return mockServer;
        }


        private static void RegisterAllBehaviors(this IEnumerable<Behavior> behaviors, Mock mockServer)
        {
            if (behaviors is not null)
                behaviors.ToList().ForEach(b => mockServer.RegisterBehavior(b));
        }
    }
}
