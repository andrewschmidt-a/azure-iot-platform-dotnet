using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TokenGenerator.Models
{
    public class AuthState
    {
        public string returnUrl;
        public string state;
        public string tenant;
    }
}
