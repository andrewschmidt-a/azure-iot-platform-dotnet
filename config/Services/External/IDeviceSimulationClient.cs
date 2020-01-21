// <copyright file="IDeviceSimulationClient.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.External;

namespace Mmm.Platform.IoT.Config.Services.External
{
    public interface IDeviceSimulationClient : IExternalServiceClient
    {
        Task<SimulationApiModel> GetDefaultSimulationAsync();

        Task UpdateSimulationAsync(SimulationApiModel model);
    }
}