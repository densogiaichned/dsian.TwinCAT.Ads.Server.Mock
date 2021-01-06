using System;
using TwinCAT.Ads;

namespace dsian.TwinCAT.Ads.Server.Mock
{

    /// <summary>
    /// Behavior / response for a ADS ReadWriteIndication
    /// </summary>    
    public record ReadWriteIndicationBehavior(uint IndexGroup, uint IndexOffset,int ExpectedLength, Memory<byte> ResponseData, AdsErrorCode ErrorCode = AdsErrorCode.Succeeded)
        : Behavior(IndexGroup, IndexOffset, ResponseData, ErrorCode);


}