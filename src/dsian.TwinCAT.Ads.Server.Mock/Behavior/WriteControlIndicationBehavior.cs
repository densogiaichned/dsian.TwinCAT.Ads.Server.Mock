using System;
using TwinCAT.Ads;

namespace dsian.TwinCAT.Ads.Server.Mock
{

    /// <summary>
    /// Behavior / response for a ADS WriteControlIndication
    /// </summary>    
    public record WriteControlIndicationBehavior(AdsState ExpectedAdsState, UInt16 ExpectedDeviceState, int ExpectedLength, AdsErrorCode ErrorCode = AdsErrorCode.NoError)
    : Behavior(0, 0, null, ErrorCode);
}
