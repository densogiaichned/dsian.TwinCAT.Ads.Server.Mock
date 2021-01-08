using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.Server;
using TwinCAT.Ads.TcpRouter;

namespace dsian.TwinCAT.Ads.Server.Mock
{
    /// <summary>
    /// Simple implementation of <see cref="AdsServer"/> which can be used for mocking ADS read/write functions.
    /// </summary>
    public sealed class Mock : AdsServer
    {
        private ILogger? _Logger = null;
        private BehaviorManager? _behaviorManager = null;
        public BehaviorManager? BehaviorManager => _behaviorManager;
        private string _PortName = "dsian";
        private AmsTcpIpRouter? _router;

        public Mock(string portName) : base(portName)
        {

            Init(portName);
            Connect();
        }

        public Mock(ushort port, string portName) : base(port, portName)
        {
            Init(portName);
            Connect();
        }

        public Mock(string portName, ILogger? logger) : base(portName, logger)
        {
            Init(portName, logger);
            Connect();
        }

        public Mock(ushort port, string portName, ILogger? logger) : base(port, portName, logger)
        {
            Init(portName, logger);
            Connect();
        }

        public Mock(ushort port, string portName, bool useSingleNotificationHandler, ILogger? logger) : base(port, portName, useSingleNotificationHandler, logger)
        {
            Init(portName, logger);
            Connect();
        }

        public override bool Disconnect()
        {
            if (_router != null)
                _router.Stop();
            return base.Disconnect();
        }
        protected override void Dispose(bool disposing)
        {
            if (_router != null)
                _router.Stop();
            base.Dispose(disposing);
        }

        private async void Init(string portName, ILogger? logger = null)
        {
            _Logger = logger;
            _PortName = portName;
            _behaviorManager = new BehaviorManager(logger);
            
            try
            {
                _router = new AmsTcpIpRouter(AmsNetId.LocalHost);
                await _router.StartAsync(CancellationToken.None);
            }
            catch { }
        }
        private void Connect()
        {
            try
            {
                var res = base.ConnectServer();
                if (IsConnected)
                    _Logger?.LogDebug("Server \"{ServerName}\" with address \"{ServerAddress}\" connected.", _PortName, ServerAddress);
                else
                    _Logger?.LogWarning("Could not connect server \"{ServerName}\" with address \"{ServerAddress}\".", _PortName, ServerAddress);
            }
            catch (AdsException ex)
            {
                _Logger?.LogError(ex, "Could not register server \"{ServerName}\".", _PortName);
            }
        }

        /// <summary>
        /// Registers a behavior, which controls the response of a ADS read/write function.
        /// </summary>
        /// <param name="behavior"></param>
        /// <returns></returns>
        public Mock RegisterBehavior(Behavior behavior)
        {
            _behaviorManager?.RegisterBehavior(behavior);

            return this;
        }


        private ValueTuple<bool, int> LenghtIsOk(Memory<byte>? responseData, int readLength)
        {
            return responseData switch
            {
                null => (readLength == 0, 0),
                not null => ((readLength > 0), Math.Min(responseData.Value.Length, readLength))
            };
        }

        protected override Task<AdsErrorCode> ReadIndicationAsync(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, int readLength, CancellationToken cancel)
        {

            var behavior = _behaviorManager?.GetBehaviorOfType<ReadIndicationBehavior>(b => b.IndexGroup == indexGroup && b.IndexOffset == indexOffset);

            if (behavior is null)
                return base.ReadIndicationAsync(sender, invokeId, indexGroup, indexOffset, readLength, cancel);

            var (lenOk, len) = LenghtIsOk(behavior.ResponseData, readLength);
            if (lenOk)
                return ReadResponseAsync(sender, invokeId, behavior.ErrorCode, behavior.ResponseData.Slice(0, len), cancel);
            else
                return ReadResponseAsync(sender, invokeId, AdsErrorCode.DeviceInvalidParam, null, cancel);
        }

        protected override Task<AdsErrorCode> WriteIndicationAsync(AmsAddress target, uint invokeId, uint indexGroup, uint indexOffset, ReadOnlyMemory<byte> writeData, CancellationToken cancel)
        {
            var behavior = _behaviorManager?.GetBehaviorOfType<WriteIndicationBehavior>(b => b.IndexGroup == indexGroup && b.IndexOffset == indexOffset);


            if (behavior is null)
                return base.WriteIndicationAsync(target, invokeId, indexGroup, indexOffset, writeData, cancel);

            var errCode = behavior.ExpectedLength == writeData.Length ? behavior.ErrorCode : AdsErrorCode.DeviceInvalidSize;
            return WriteResponseAsync(target, invokeId, behavior.ErrorCode, cancel);
        }


        protected override Task<AdsErrorCode> ReadWriteIndicationAsync(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, int readLength, ReadOnlyMemory<byte> writeData, CancellationToken cancel)
        {
            var behavior = _behaviorManager?.GetBehaviorOfType<ReadWriteIndicationBehavior>(b => b.IndexGroup == indexGroup && b.IndexOffset == indexOffset);

            if (behavior is null)
                return base.ReadWriteIndicationAsync(sender, invokeId, indexGroup, indexOffset, readLength, writeData, cancel);

            var errCode = behavior.ExpectedLength == writeData.Length ? behavior.ErrorCode : AdsErrorCode.DeviceInvalidSize;
            var (lenOk, len) = LenghtIsOk(behavior.ResponseData, readLength);
            if (lenOk)
                return ReadWriteResponseAsync(sender, invokeId, errCode, behavior.ResponseData.Slice(0, len), cancel);
            else
                return ReadWriteResponseAsync(sender, invokeId, AdsErrorCode.DeviceInvalidParam, null, cancel);
        }

        protected override Task<AdsErrorCode> AddDeviceNotificationIndicationAsync(AmsAddress sender, uint invokeId, uint indexGroup, uint indexOffset, int dataLength, NotificationSettings settings, CancellationToken cancel)
        {
            var behavior = _behaviorManager?.GetBehaviorOfType<AddDeviceNotificationIndicationBehavior>(
                b => b.IndexGroup == indexGroup && b.IndexOffset == indexOffset && b.ExpectedLength == dataLength &&
                (b.ExpectedNotificationSettings != null ? b.ExpectedNotificationSettings == settings : true)    // optional
                );

            if (behavior is null)
                return base.AddDeviceNotificationIndicationAsync(sender, invokeId, indexGroup, indexOffset, dataLength, settings, cancel);

            return AddDeviceNotificationResponseAsync(sender, invokeId, behavior.ErrorCode, behavior.ResponseNotificationHandle, cancel);
        }

        protected override Task<AdsErrorCode> ReadDeviceStateIndicationAsync(AmsAddress sender, uint invokeId, CancellationToken cancel)
        {
            var behavior = _behaviorManager?.GetBehaviorOfType<ReadDeviceStateIndicationBehavior>()?
                            .Select(b => b)
                            .FirstOrDefault();

            if (behavior is null)
                return base.ReadDeviceStateIndicationAsync(sender, invokeId, cancel);

            return ReadDeviceStateResponseAsync(sender, invokeId, behavior.ErrorCode, behavior.ResponseAdsState, behavior.ResponseDeviceState, cancel);
        }


        protected override Task<AdsErrorCode> WriteControlIndicationAsync(AmsAddress sender, uint invokeId, AdsState adsState, ushort deviceState, ReadOnlyMemory<byte> data, CancellationToken cancel)
        {
            var behavior = _behaviorManager?.GetBehaviorOfType<WriteControlIndicationBehavior>(b => b.ExpectedAdsState == adsState && b.ExpectedDeviceState == deviceState && b.ExpectedLength == data.Length);

            if (behavior is null)
                return base.WriteControlIndicationAsync(sender, invokeId, adsState, deviceState, data, cancel);

            return WriteControlResponseAsync(sender, invokeId, behavior.ErrorCode, cancel);
        }

        protected override Task<AdsErrorCode> DeleteDeviceNotificationIndicationAsync(AmsAddress sender, uint invokeId, uint hNotification, CancellationToken cancel)
        {
            var behavior = _behaviorManager?.GetBehaviorOfType<DeleteDeviceNotificationIndicationBehavior>(b => b.RequestNotificationHandle == hNotification);

            if (behavior is null)
                return base.DeleteDeviceNotificationIndicationAsync(sender, invokeId, hNotification, cancel);

            return DeleteDeviceNotificationResponseAsync(sender, invokeId, behavior.ErrorCode, cancel);
        }


        protected override Task<AdsErrorCode> ReadDeviceInfoIndicationAsync(AmsAddress sender, uint invokeId, CancellationToken cancel)
        {
            var behavior = _behaviorManager?.GetBehaviorOfType<ReadDeviceInfoIndicationBehavior>()?
                .Select(b => b)
                .FirstOrDefault();

            if (behavior is null)
                return ReadDeviceInfoResponseAsync(sender, invokeId, AdsErrorCode.DeviceServiceNotSupported, _PortName, AdsVersion.Empty, cancel);

            return ReadDeviceInfoResponseAsync(sender, invokeId, behavior.ErrorCode, behavior.ResponseDeviceName, new AdsVersion(behavior.ResponseMajorVersion, behavior.ResponseMinorVersion, behavior.VersionBuild), cancel);
        }

    }
}
