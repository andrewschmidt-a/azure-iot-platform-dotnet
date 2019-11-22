using System.Collections.Generic;
using System.Linq;

namespace Mmm.Platform.IoT.TenantManager.Services.Models
{
    public class DeleteTenantModel
    {
        public string TenantId;
        public bool EnsuredDeployment;
        public Dictionary<string, bool> DeletionRecord;

        public DeleteTenantModel (string tenantGuid, Dictionary<string, bool> deletionRecord, bool ensuredDeployment)
        {
            this.TenantId = tenantGuid;
            this.EnsuredDeployment = ensuredDeployment;
            this.DeletionRecord = deletionRecord;
        }

        public DeleteTenantModel () { }

        public bool fullyDeleted
        {
            get
            {
                return this.DeletionRecord.All(item => item.Value);
            }
        }
    }
}