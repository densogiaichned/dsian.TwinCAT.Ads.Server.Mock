using System;
using TwinCAT.Ads;

namespace dsian.TwinCAT.Ads.Server.Mock
{

    /// <summary>
    /// Behavior / response for a ADS Read Device State
    /// </summary>    
    public record ReadDeviceInfoIndicationBehavior( byte ResponseMajorVersion, byte ResponseMinorVersion, ushort VersionBuild, string ResponseDeviceName, AdsErrorCode ErrorCode = AdsErrorCode.Succeeded)
        : Behavior(0, 0, null, ErrorCode);


}
