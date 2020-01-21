// <copyright file="BeginConversionApiModel.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

namespace Mmm.Platform.IoT.Common.Services.External.AsaManager
{
    public class BeginConversionApiModel
    {
        public BeginConversionApiModel()
        {
        }

        public string TenantId { get; set; }

        public string OperationId { get; set; }
    }
}