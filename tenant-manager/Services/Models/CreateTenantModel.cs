using System.Collections.Generic;
using System.Linq;

namespace Mmm.Platform.IoT.TenantManager.Services.Models
{
    public class CreateTenantModel
    {
        private string tenantId;
        private string message;

        public CreateTenantModel() { }

        public CreateTenantModel(string tenantGuid)
        {
            this.tenantId = tenantGuid;
            this.message = $"Your tenant is currently being deployed. This may take several minutes. You can check if your tenant is fully deployed using GET /api/tenantready";
        }
    }
}