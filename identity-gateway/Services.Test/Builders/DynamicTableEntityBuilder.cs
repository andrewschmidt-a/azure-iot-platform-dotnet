// <copyright file="DynamicTableEntityBuilder.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Table;
using TestStack.Dossier;
using TestStack.Dossier.EquivalenceClasses;

namespace Mmm.Iot.IdentityGateway.Services.Test.Helpers.Builders
{
    public class DynamicTableEntityBuilder : TestDataBuilder<DynamicTableEntity, DynamicTableEntityBuilder>
    {
        public virtual DynamicTableEntityBuilder WithRandomValueProperty()
        {
            return Set(dte => dte.Properties, new Dictionary<string, EntityProperty> { { "Value", new EntityProperty(Any.String()) } });
        }

        public virtual DynamicTableEntityBuilder WithRandomRolesProperty()
        {
            return Set(dte => dte.Properties, new Dictionary<string, EntityProperty> { { "Roles", new EntityProperty(Any.String()) } });
        }

        protected override DynamicTableEntity BuildObject()
        {
            return new DynamicTableEntity(Get(dte => dte.PartitionKey), Get(dte => dte.RowKey), Get(dte => dte.ETag), Get(dte => dte.Properties));
        }
    }
}