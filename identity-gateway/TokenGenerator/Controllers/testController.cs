using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TokenGenerator.Controllers
{
    [Route("test")]
    [ApiController]
    public class testController : ControllerBase
    {
        [Route("test")]
        [HttpGet]
        public ActionResult<string> Get()
        {
            return "test";
        }
    }
}