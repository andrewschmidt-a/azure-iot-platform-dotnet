using System;

namespace Mmm.Platform.IoT.Common.Services.Wrappers
{
    public class GuidKeyGenerator : IKeyGenerator
    {
        public string Generate()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
