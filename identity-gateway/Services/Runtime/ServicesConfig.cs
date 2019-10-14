// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;

namespace IdentityGateway.Services.Runtime
{
    public interface IServicesConfig
    {
        Dictionary<string, List<string>> UserPermissions { get; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public Dictionary<string, List<string>> UserPermissions { get; set; }
    }
}
