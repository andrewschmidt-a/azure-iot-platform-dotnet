// <copyright file="IDeviceTelemetryClient.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.Common.Services.External;

namespace Mmm.Platform.IoT.Config.Services.External
{
    public interface IDeviceTelemetryClient : IExternalServiceClient
    {
        Task UpdateRuleAsync(RuleApiModel rule, string etag);
    }
}