
using dsian.TwinCAT.AdsViewer.CapParser.Lib;
using dsian.TwinCAT.AdsViewer.CapParser.Lib.Cap;
using dsian.TwinCAT.AdsViewer.CapParser.Lib.Cap.AdsCommands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using TcAds = TwinCAT.Ads;  // due to poor namespace naming, there are collisions with Beckhoff TwinCAT.Ads...

namespace dsian.TwinCAT.Ads.Server.Mock.Extensions
{
    public class ReplayExtension : IMockExtension
    {
        private readonly AmsNetId _amsNetId;
        private readonly int _port;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="amsNetId">server AmsNetId</param>
        /// <param name="port">Server port</param>
        public ReplayExtension(string path, AmsNetId amsNetId, int port)
        {
            ParseNetMonFile(path);
            _amsNetId = amsNetId;
            _port = port;
        }
        public ReplayExtension(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            ParseNetMonFile(path);

        }
        public ReplayExtension(NetMonFile netMonFile)
        {
            if (netMonFile is null)
                throw new ArgumentNullException(nameof(netMonFile));
            BehaviorList = ConvertToBehaviorList(netMonFile);
        }

        public IEnumerable<Behavior>? BehaviorList { get; private set; } = default;

        private void ParseNetMonFile(string path)
        {
            // we have to sync here
            var (ok, packets) = NetMonFileFactory.TryParseNetMonFileAsync(new FileInfo(path), CancellationToken.None, null).GetAwaiter().GetResult();
            if (ok)
            {
                BehaviorList = ConvertToBehaviorList(packets);
            }
        }

        private IEnumerable<Behavior> ConvertToBehaviorList(NetMonFile? nmf)
        {

            var behaviorList = new List<Behavior>();

            var pairOfReqResList = GetRequestResponsePair(nmf);

            foreach (var pair in pairOfReqResList)
            {
                switch (pair.request.Data.AmsHeader.AmsCommandId)
                {
                    case dsian.TwinCAT.AdsViewer.CapParser.Lib.TcAds.AmsCommandId.ADS_Read:
                        behaviorList.Add(GetReadIndicationBehavior(pair)); break;
                    case dsian.TwinCAT.AdsViewer.CapParser.Lib.TcAds.AmsCommandId.ADS_Write:
                        behaviorList.Add(GetWriteIndicationBehavior(pair)); break;
                    case dsian.TwinCAT.AdsViewer.CapParser.Lib.TcAds.AmsCommandId.ADS_Read_Write:
                        behaviorList.Add(GetReadWriteIndicationBehavior(pair)); break;
                    case dsian.TwinCAT.AdsViewer.CapParser.Lib.TcAds.AmsCommandId.ADS_Add_Device_Notification:
                        behaviorList.Add(GetAddDeviceNotificationIndicationBehavior(pair)); break;
                    case dsian.TwinCAT.AdsViewer.CapParser.Lib.TcAds.AmsCommandId.ADS_Read_State:
                        behaviorList.Add(GetReadDeviceStateNotificationIndicationBehavior(pair)); break;
                    case dsian.TwinCAT.AdsViewer.CapParser.Lib.TcAds.AmsCommandId.ADS_Write_Control:
                        behaviorList.Add(GetWriteControlIndicationBehavior(pair)); break;
                    case dsian.TwinCAT.AdsViewer.CapParser.Lib.TcAds.AmsCommandId.ADS_Delete_Device_Notification:
                        behaviorList.Add(GetDeleteDeviceNotificationIndicationBehavior(pair)); break;
                    case dsian.TwinCAT.AdsViewer.CapParser.Lib.TcAds.AmsCommandId.ADS_Read_Device_Info:
                        behaviorList.Add(GetReadDeviceInfoIndicationBehavior(pair)); break;
                    case dsian.TwinCAT.AdsViewer.CapParser.Lib.TcAds.AmsCommandId.ADS_Device_Notification:  // request only ...
                    default:
                        // not supported yet....
                        break;
                }
            }
            return behaviorList;
        }



        private Behavior GetReadWriteIndicationBehavior(ValueTuple<FramePacket, FramePacket> pair)

        {
            var (request, response) = pair;
            var adsRequest = (AdsReadWriteRequest)request.Data.AdsCommand;
            var adsResponse = (AdsReadWriteResponse)response.Data.AdsCommand;
            return new ReadWriteIndicationBehavior(
                    adsRequest.IndexGroup,
                    adsRequest.IndexOffset,
                    (int)adsRequest.WriteLength,
                    adsResponse.Data.ToArray(),
                    (TcAds.AdsErrorCode)response.Data.AmsHeader.Error_Code
            );
        }

        private ReadIndicationBehavior GetReadIndicationBehavior(ValueTuple<FramePacket, FramePacket> pair)
        {
            var (request, response) = pair;
            var adsRequest = (AdsReadRequest)request.Data.AdsCommand;
            var adsResponse = (AdsReadResponse)response.Data.AdsCommand;
            return new ReadIndicationBehavior(
                    adsRequest.IndexGroup,
                    adsRequest.IndexOffset,
                    adsResponse.Data.ToArray(),
                    (TcAds.AdsErrorCode)adsResponse.Result
            );
        }
        private WriteIndicationBehavior GetWriteIndicationBehavior(ValueTuple<FramePacket, FramePacket> pair)
        {
            var (request, response) = pair;
            var adsRequest = (AdsWriteRequest)request.Data.AdsCommand;
            var adsResponse = (AdsWriteResponse)response.Data.AdsCommand;
            return new WriteIndicationBehavior(
                    adsRequest.IndexGroup,
                    adsRequest.IndexOffset,
                    (int)adsRequest.Length,
                    (TcAds.AdsErrorCode)adsResponse.Result
            );
        }

        private AddDeviceNotificationIndicationBehavior GetAddDeviceNotificationIndicationBehavior(ValueTuple<FramePacket, FramePacket> pair)
        {
            var (request, response) = pair;
            var adsRequest = (AdsAddDeviceNotificationRequest)request.Data.AdsCommand;
            var adsResponse = (AdsAddDeviceNotificationResponse)response.Data.AdsCommand;
            return new AddDeviceNotificationIndicationBehavior(
                    adsRequest.IndexGroup,
                    adsRequest.IndexOffset,
                    (int)adsRequest.Length,
                    new((TcAds.AdsTransMode)adsRequest.TransmissonMode, adsRequest.CycleTime, adsRequest.MaxDelay),
                    adsResponse.NotificationHandle,
                    (TcAds.AdsErrorCode)adsResponse.Result
                );
        }
        private Behavior GetDeleteDeviceNotificationIndicationBehavior((FramePacket request, FramePacket response) pair)
        {
            var (request, response) = pair;
            var adsRequest = (AdsDeleteDeviceNotificationRequest)request.Data.AdsCommand;
            var adsResponse = (AdsDeleteDeviceNotificationResponse)response.Data.AdsCommand;
            return new DeleteDeviceNotificationIndicationBehavior(
                adsRequest.NotificationHandle,
                (TcAds.AdsErrorCode)adsResponse.Result
                );
        }

        private Behavior GetReadDeviceStateNotificationIndicationBehavior((FramePacket request, FramePacket response) pair)
        {
            var (request, response) = pair;
            var adsRequest = (AdsReadStateRequest)request.Data.AdsCommand;
            var adsResponse = (AdsReadStateResponse)response.Data.AdsCommand;
            return new ReadDeviceStateIndicationBehavior(
                    (TcAds.AdsState)adsResponse.ADS_State,
                    adsResponse.Device_State,
                    (TcAds.AdsErrorCode)adsResponse.Result
                );
        }
        private Behavior GetWriteControlIndicationBehavior((FramePacket request, FramePacket response) pair)
        {
            var (request, response) = pair;
            var adsRequest = (AdsWriteControlRequest)request.Data.AdsCommand;
            var adsResponse = (AdsWriteControlResponse)response.Data.AdsCommand;
            return new WriteControlIndicationBehavior(
                    (TcAds.AdsState)adsRequest.ADS_State,
                    adsRequest.Device_State,
                    (int)adsRequest.Length,
                    (TcAds.AdsErrorCode)adsResponse.Result
                );
        }

        private Behavior GetReadDeviceInfoIndicationBehavior((FramePacket request, FramePacket response) pair)
        {
            var (request, response) = pair;
            var adsRequest = (AdsReadDeviceInfoRequest)request.Data.AdsCommand;
            var adsResponse = (AdsReadDeviceInfoResponse)response.Data.AdsCommand;
            return new ReadDeviceInfoIndicationBehavior(
                 adsResponse.Major_Version,
                 adsResponse.Minor_Version,
                 adsResponse.Version_Build,
                 adsResponse.Device_Name,
                 (TcAds.AdsErrorCode)adsResponse.Result
                );
        }

        /// <summary>
        /// Gets a list of matching Request-Response pairs from all packets.
        /// </summary>
        /// <param name="nmf"></param>
        /// <returns></returns>
        private IEnumerable<(FramePacket request, FramePacket response)> GetRequestResponsePair(NetMonFile? nmf)
        {

            var requests = from p in nmf?.FramePackets
                           where p.Data.AmsHeader.IsValid && p.Data.AmsHeader.IsRequest
                           select p;
            var responses = from p in nmf?.FramePackets
                            where p.Data.AmsHeader.IsValid && p.Data.AmsHeader.IsResponse
                            select p;

            var pairOfReqRes = from req in requests
                               from res in responses
                               where req.Data.AmsHeader.AMSNetId_Source == res.Data.AmsHeader.AMSNetId_Target
                               && req.Data.AmsHeader.AMSNetId_Target == res.Data.AmsHeader.AMSNetId_Source
                               && req.Data.AmsHeader.AMSPort_Source == res.Data.AmsHeader.AMSPort_Target
                               && req.Data.AmsHeader.AMSPort_Target == res.Data.AmsHeader.AMSPort_Source
                               && req.Data.AmsHeader.AmsCommandId == res.Data.AmsHeader.AmsCommandId
                               && req.Data.AmsHeader.Invoke_Id == res.Data.AmsHeader.Invoke_Id
                               select new ValueTuple<FramePacket, FramePacket>(req, res);
            return pairOfReqRes;
        }
    }
}
