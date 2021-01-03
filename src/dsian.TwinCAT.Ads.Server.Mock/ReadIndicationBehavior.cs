using System;
using TwinCAT.Ads;

namespace dsian.TwinCAT.Ads.Server.Mock
{

    /// <summary>
    /// Behavior / response for a ADS ReadIndication
    /// </summary>    
    public record ReadIndicationBehavior(uint IndexGroup, uint IndexOffset, Memory<byte> ResponseData, AdsErrorCode ErrorCode = AdsErrorCode.Succeeded)
        : Behavior(IndexGroup, IndexOffset, ResponseData, ErrorCode);


}
