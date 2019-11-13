using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new[] {"value1", "value2"};
        }

    }
}