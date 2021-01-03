using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads;
using TwinCAT.Ads.Server;

namespace dsian.TwinCAT.Ads.Server.Mock
{
    /// <summary>
    /// Simple implementation of <see cref="AdsServer"/> which can be used for mocking ADS read/write functions.
    /// </summary>
    public class Mock : AdsServer
    {

        private List<Behavior> _BehaviorList = new List<Behavior>();

        public Mock(string portName) : base(portName)
        {
            Connect();
        }

        public Mock(ushort port, string portName) : base(port, portName)
        {
            Connect();
        }

        public Mock(string portName, ILogger? logger) : base(portName, logger)
        {
            Connect();
        }

        public Mock(ushort port, string portName, ILogger? logger) : base(port, portName, logger)
        {
            Connect();
        }

        public Mock(ushort port, string portName, bool useSingleNotificationHandler, ILogger? logger) : base(port, portName, useSingleNotificationHandler, logger)
        {
            Connect();
        }

        private void Connect()
        {
            var res = base.ConnectServer();
        }


        /// <summary>
        /// Registers a behavior, which controls the response of a ADS read/write function.
        /// </summary>
        /// <param name="behavior"></param>
        /// <returns></returns>
        public Mock RegisterBehavior(Behavior behavior)
        {
            _BehaviorList.Add(behavior);
            return this;
        }


        private ValueTuple<bool, int> LenghtIsOk(Memory<byte>? responseData, int readLength)
        {
            return responseData switch
            {
                null => (readLength == 0,0),
                not null => ((readLength > 0), Math.Min(responseData.Value.Length,readLength))
            };
        }

        protected override Task<AdsErrorCode> ReadIndicationAsync(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, int readLength, CancellationToken cancel)
        {


            var behavior = (from b in _BehaviorList
                            where b is ReadIndicationBehavior && (b.IndexGroup == indexGroup && b.IndexOffset == indexOffset)
                            select b).FirstOrDefault() as ReadIndicationBehavior;

            if (behavior is not null)
            {
                var (lenOk, len) = LenghtIsOk(behavior.ResponseData, readLength);
                if (lenOk)
                    return ReadResponseAsync(sender, invokeId, behavior.ErrorCode, behavior.ResponseData.Slice(0,len), cancel);
                else
                    return ReadResponseAsync(sender, invokeId, AdsErrorCode.DeviceInvalidParam, null, cancel);
            }
            else
                return ReadResponseAsync(sender, invokeId, AdsErrorCode.DeviceServiceNotSupported, null, cancel);
        }

        protected override Task<AdsErrorCode> WriteIndicationAsync(AmsAddress target, uint invokeId, uint indexGroup, uint indexOffset, ReadOnlyMemory<byte> writeData, CancellationToken cancel)
        {


            var behavior = (from b in _BehaviorList
                            where b is WriteIndicationBehavior && (b.IndexGroup == indexGroup && b.IndexOffset == indexOffset)
                            select b).FirstOrDefault() as WriteIndicationBehavior;

            if (behavior is not null)
            {
                var errCode = behavior.ExpectedLength == writeData.Length ? behavior.ErrorCode : AdsErrorCode.DeviceInvalidSize;
                return WriteResponseAsync(target, invokeId, behavior.ErrorCode, cancel);
            }
            else
                return WriteResponseAsync(target, invokeId, AdsErrorCode.DeviceServiceNotSupported, cancel);
        }

    }
}
