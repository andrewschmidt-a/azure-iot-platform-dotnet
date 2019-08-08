using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityGateway.Services.Models;

namespace IdentityGateway.Services
{

    public interface IStatusService
    {
        Task<StatusServiceModel> GetStatusAsync();
    }
}
