// Copyright (c) Microsoft. All rights reserved.

using MMM.Azure.IoTSolutions.TenantManager.Services.Models;
using System.Threading.Tasks;

namespace MMM.Azure.IoTSolutions.TenantManager.Services
{
    public interface IStatusService
    {
        Task<StatusServiceModel> GetStatusAsync();
    }
}
