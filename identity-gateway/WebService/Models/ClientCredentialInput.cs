using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mmm.Platform.IoT.IdentityGateway.WebService.Models
{
    public class ClientCredentialInput
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string scope { get; set; }
    }
}
