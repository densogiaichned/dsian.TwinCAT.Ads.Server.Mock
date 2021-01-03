using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwinCAT.Ads;

namespace dsian.TwinCAT.Ads.Server.Mock
{

    /// <summary>
    /// Base behavior, internal use only
    /// </summary>
    public abstract record Behavior(uint IndexGroup, uint IndexOffset, Memory<byte> ResponseData, AdsErrorCode ErrorCode = AdsErrorCode.Succeeded);

}
