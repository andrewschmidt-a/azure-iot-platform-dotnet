// <copyright file="IIdentityGatewayClient.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.TenantManager.Services.Models;

namespace Mmm.Platform.IoT.TenantManager.Services.External
{
    public interface IIdentityGatewayClient : IStatusOperation
    {
        Task<IdentityGatewayApiModel> AddTenantForUserAsync(string userId, string tenantId, string roles);

        Task<IdentityGatewayApiModel> GetTenantForUserAsync(string userId, string tenantId);

        Task<IdentityGatewayApiSettingModel> AddSettingsForUserAsync(string userId, string settingKey, string settingValue);

        Task<IdentityGatewayApiSettingModel> GetSettingsForUserAsync(string userId, string settingKey);

        Task<IdentityGatewayApiSettingModel> UpdateSettingsForUserAsync(string userId, string settingKey, string settingValue);

        Task<IdentityGatewayApiModel> DeleteTenantForAllUsersAsync(string tenantId);
    }
}