// <copyright file="IAsaManagerClient.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Threading.Tasks;

namespace Mmm.Iot.Common.Services.External.AsaManager
{
    public interface IAsaManagerClient : IExternalServiceClient
    {
        Task<BeginConversionApiModel> BeginConversionAsync(string entity);
    }
}