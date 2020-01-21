// <copyright file="IAppConfigurationHelper.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Threading.Tasks;

namespace Mmm.Platform.IoT.Common.Services.Helpers
{
    public interface IAppConfigurationHelper
    {
        Task SetValueAsync(string key, string value);

        string GetValue(string key);

        Task DeleteKeyAsync(string key);
    }
}