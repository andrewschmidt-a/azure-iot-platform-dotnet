using System.Collections.Generic;
using System.Linq;

namespace MMM.Azure.IoTSolutions.TenantManager.Services.Models
{
    public class CreateTenantModel
    {
        public string tenantId;
        public string message;

        public CreateTenantModel (string tenantGuid)
        {
            this.tenantId = tenantGuid;
            this.message = $"Your tenant is currently being deployed. This may take several minutes. You can check if your tenant is fully deployed using GET /api/tenantready";
        }

        public CreateTenantModel () { }
    }
}