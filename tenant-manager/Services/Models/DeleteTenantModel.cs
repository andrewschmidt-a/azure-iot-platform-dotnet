using System.Collections.Generic;
using System.Linq;

namespace MMM.Azure.IoTSolutions.TenantManager.Services.Models
{
    public class DeleteTenantModel
    {
        public string tenantId;
        public bool ensuredDeployment;
        public Dictionary<string, bool> deletionRecord;

        public DeleteTenantModel (string tenantGuid, Dictionary<string, bool> deletionRecord, bool ensuredDeployment)
        {
            this.tenantId = tenantGuid;
            this.ensuredDeployment = ensuredDeployment;
            this.deletionRecord = deletionRecord;
        }

        public DeleteTenantModel () { }

        public bool fullyDeleted
        {
            get
            {
                return this.deletionRecord.All(item => item.Value);
            }
        }
    }
}