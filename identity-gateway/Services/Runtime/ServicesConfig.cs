// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;

namespace IdentityGateway.Services.Runtime
{
    public interface IServicesConfig
    {
        Dictionary<string, List<string>> UserPermissions { get; }
        string PrivateKey { get; }
        string PublicKey { get; }
        string StorageAccountConnectionString { get; }
        string AzureB2CBaseUri { get; }
        string Port { get; }
        string SendGridAPIKey { get; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public Dictionary<string, List<string>> UserPermissions { get; set; }
        public string StorageAccountConnectionString { get; set; }
        public string AzureB2CBaseUri { get; set; }
        public string Port { get; set; }
        public string SendGridAPIKey { get; set; }
    }
}
