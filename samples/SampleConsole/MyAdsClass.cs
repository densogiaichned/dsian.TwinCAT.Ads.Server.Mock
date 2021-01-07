using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SampleConsole
{
    public class MyAdsClass
    {
        public MyAdsClass(int port)
        {
            Port = port;
        }
        public int Port { get; }

        private byte[] _buffer = new byte[32];

        public async Task<byte[]> GetValuesFilteredAsync(uint ig, uint io, Func<byte, bool> filterFunc)
        {
            using (var client = new TwinCAT.Ads.AdsClient())
            {
                client.Connect(Port);
                var result = await client.ReadAsync(ig, io, _buffer, CancellationToken.None);
                if (result.Succeeded)
                    return _buffer.Where(x => filterFunc(x)).Select(x => x).ToArray();
            }
            return new byte[0];
        }
    }
}
