using RevitChangesMonitor.WebApp.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RevitChangesMonitor.WebApp.Controllers
{
    [IdentityBasicAuthentication]
    [Authorize]
    public class AuthenticationController : ApiController
    {
        [Route("api/Authentication/")]
        public IHttpActionResult DoTest() => Ok();

    }
}
