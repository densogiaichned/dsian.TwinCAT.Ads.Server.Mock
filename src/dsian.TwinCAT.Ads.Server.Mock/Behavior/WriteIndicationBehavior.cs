using System;
using TwinCAT.Ads;

namespace dsian.TwinCAT.Ads.Server.Mock
{

    /// <summary>
    /// Behavior / response for a ADS WriteIndication
    /// </summary>    
    public record WriteIndicationBehavior(uint IndexGroup, uint IndexOffset, int ExpectedLength, AdsErrorCode ErrorCode = AdsErrorCode.Succeeded)
    : Behavior(IndexGroup, IndexOffset, null, ErrorCode);
}
