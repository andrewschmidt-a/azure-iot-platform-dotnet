// Copyright (c) Microsoft. All rights reserved.

namespace MMM.Azure.IoTSolutions.TenantManager.Services.Http
{
    public class HttpRequestOptions
    {
        public bool EnsureSuccess { get; set; } = false;

        public bool AllowInsecureSSLServer { get; set; } = false;

        public int Timeout { get; set; } = 30000;
    }
}
