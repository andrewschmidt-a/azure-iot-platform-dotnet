// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services.Models;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Actions
{
    public interface IActionExecutor
    {
        Task Execute(IAction action, object metadata);
    }
}
