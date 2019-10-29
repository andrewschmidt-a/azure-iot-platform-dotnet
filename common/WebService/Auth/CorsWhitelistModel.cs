// Copyright (c) Microsoft. All rights reserved.

namespace Mmm.Platform.IoT.Common.WebService.Auth
{
    public class CorsWhitelistModel
    {
        public string[] Origins { get; set; }
        public string[] Methods { get; set; }
        public string[] Headers { get; set; }
    }
}
