using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TokenGenerator.Models
{
    public  class Configuration
    {
        private static HttpContext _httpContext;
        private string host
        {
            get
            {
                string forwardedFor = null;
                if(_httpContext.Request.Headers["X-Forwarded-For"].Count > 0){
                    forwardedFor = _httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                }
                return forwardedFor ?? "https://" + _httpContext.Request.Host.ToString();
            }
        }

        public Configuration(HttpContext context)
        {
            _httpContext = context;
        }
        public string issuer
        {
            get
            {
                return this.host;
            }
        }
        public string jwks_uri
        {
            get
            {
                return this.host + "/.well-known/openid-configuration/jwks";
            }
        }
        public string authorization_endpoint
        {
            get
            {
                return this.host + "/connect/authorize";
            }
        }
        /*
        public string token_endpoint
        {
            get
            {
                return this.host + "/connect/token";
            }
        }
        public string userinfo_endpoint
        {
            get
            {
                return this.host + "/connect/userinfo";
            }
        }
        public string endsession_endpoint
        {
            get
            {
                return this.host + "/connect/endsession";
            }
        }
        public string check_session_iframe
        {
            get
            {
                return this.host + "/connect/checksession";
            }
        }
        public string revocation_endpoint
        {
            get
            {
                return this.host + "/connect/endsession";
            }
        }
        public bool frontchannel_logout_supported
        {
            get
            {
                return false;
            }
        }
        public bool frontchannel_logout_session_supported
        {
            get
            {
                return false;
            }
        }

        public bool backchannel_logout_supported
        {
            get
            {
                return false;
            }
        }*/
        public List<string> scopes_supported
        {
            get
            {
                return new List<string> { "openid", "profile"};
            }
        }
        public List<string> claims_supported
        {
            get
            {
                return new List<string> { "sub", "name", "tenant", "role" };
            }
        }
        public List<string> grant_types_supported
        {
            get
            {
                return new List<string> { "implicit" };
            }
        }
        public List<string> response_types_supported
        {

            get
            {
                return new List<string> { "token" };
            }
        }
        public List<string> response_modes_supported
        {

            get
            {
                return new List<string> { "query" };
            }
        }


    }
}
