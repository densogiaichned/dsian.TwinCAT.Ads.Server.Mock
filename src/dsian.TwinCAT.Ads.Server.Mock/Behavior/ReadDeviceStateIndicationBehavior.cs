using System;
using TwinCAT.Ads;

namespace dsian.TwinCAT.Ads.Server.Mock
{

    /// <summary>
    /// Behavior / response for a ADS Read Device State
    /// </summary>    
    public record ReadDeviceStateIndicationBehavior(AdsState ResponseAdsState, ushort ResponseDeviceState, AdsErrorCode ErrorCode = AdsErrorCode.Succeeded)
        : Behavior(0, 0, null, ErrorCode);


}
