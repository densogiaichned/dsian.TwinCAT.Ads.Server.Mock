using System;
using TwinCAT.Ads;

namespace dsian.TwinCAT.Ads.Server.Mock
{

    /// <summary>
    /// Behavior / response for a ADS AddDeviceNotificationIndication
    /// </summary>    
    public record AddDeviceNotificationIndicationBehavior(uint IndexGroup, uint IndexOffset, int ExpectedLength, NotificationSettings? ExpectedNotificationSettings, uint ResponseNotificationHandle, AdsErrorCode ErrorCode = AdsErrorCode.NoError)
        : Behavior(IndexGroup, IndexOffset, null, ErrorCode);


}
