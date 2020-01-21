// <copyright file="ITenantConnectionHelper.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using Microsoft.Azure.Devices;

namespace Mmm.Platform.IoT.IoTHubManager.Services.Helpers
{
    public interface ITenantConnectionHelper
    {
        string GetIotHubName();

        RegistryManager GetRegistry();

        string GetIotHubConnectionString();

        JobClient GetJobClient();
    }
}