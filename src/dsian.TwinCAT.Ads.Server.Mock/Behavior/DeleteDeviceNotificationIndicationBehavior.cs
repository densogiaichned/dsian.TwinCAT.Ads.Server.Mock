using System;
using TwinCAT.Ads;

namespace dsian.TwinCAT.Ads.Server.Mock
{

    /// <summary>
    /// Behavior / response for a ADS DeleteDeviceNotificationIndication
    /// </summary>    
    public record DeleteDeviceNotificationIndicationBehavior(uint RequestNotificationHandle, AdsErrorCode ErrorCode = AdsErrorCode.NoError)
        : Behavior(0, 0, null, ErrorCode);


}
