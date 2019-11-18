// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Mmm.Platform.IoT.Common.Services
{
    public interface IStatusService
    {
        Task<StatusServiceModel> GetStatusAsync(bool authRequired);
    }
}
