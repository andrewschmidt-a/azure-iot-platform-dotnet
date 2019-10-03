// Copyright (c) Microsoft. All rights reserved.

namespace MMM.Azure.IoTSolutions.TenantManager.WebService
{
    class CorsWhitelistModel
    {
        public string[] Origins { get; set; }
        public string[] Methods { get; set; }
        public string[] Headers { get; set; }
    }
}