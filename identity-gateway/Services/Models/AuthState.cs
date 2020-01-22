// <copyright file="AuthState.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

namespace Mmm.Iot.IdentityGateway.Services.Models
{
    public class AuthState
    {
        public string ReturnUrl { get; set; }

        public string State { get; set; }

        public string Tenant { get; set; }

        public string Nonce { get; set; }

        public string ClientId { get; set; }

        public string Invitation { get; set; }
    }
}