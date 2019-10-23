using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;
using TestStack.Dossier;
using TestStack.Dossier.EquivalenceClasses;

namespace Services.Test.Helpers.Builders
{
    public class DynamicTableEntityBuilder : TestDataBuilder<DynamicTableEntity, DynamicTableEntityBuilder>
    {
        public virtual DynamicTableEntityBuilder WithRandomValueProperty()
        {
            return Set(dte => dte.Properties, new Dictionary<string, EntityProperty> { { "Value", new EntityProperty(Any.String()) } });
        }

        protected override DynamicTableEntity BuildObject()
        {
            return new DynamicTableEntity(Get(dte => dte.PartitionKey), Get(dte => dte.RowKey), Get(dte => dte.ETag), Get(dte => dte.Properties));
        }
    }
}