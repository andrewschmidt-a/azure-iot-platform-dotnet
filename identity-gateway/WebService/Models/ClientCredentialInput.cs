// <copyright file="ClientCredentialInput.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

namespace Mmm.Iot.IdentityGateway.WebService.Models
{
    public class ClientCredentialInput
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string Scope { get; set; }
    }
}