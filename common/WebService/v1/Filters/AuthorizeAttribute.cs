using Microsoft.AspNetCore.Mvc;

namespace Mmm.Platform.IoT.Common.WebService.v1.Filters
{
    public class AuthorizeAttribute : TypeFilterAttribute
    {
        public AuthorizeAttribute(string allowedActions)
            : base(typeof(AuthorizeActionFilterAttribute))
        {
            this.Arguments = new object[] { allowedActions };
        }
    }
}