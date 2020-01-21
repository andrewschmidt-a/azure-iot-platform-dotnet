// <copyright file="IDeviceSimulationClient.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Threading.Tasks;

namespace Mmm.Platform.IoT.Config.Services.External
{
    public interface IDeviceSimulationClient
    {
        Task<SimulationApiModel> GetDefaultSimulationAsync();

        Task UpdateSimulationAsync(SimulationApiModel model);
    }
}