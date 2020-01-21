// <copyright file="IAsaManagerClient.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Threading.Tasks;

namespace Mmm.Platform.IoT.Common.Services.External.AsaManager
{
    public interface IAsaManagerClient : IStatusOperation
    {
        Task<BeginConversionApiModel> BeginConversionAsync(string entity);
    }
}