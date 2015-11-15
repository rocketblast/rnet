using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace thor.Controllers
{
    public class DefaultController : ApiController
    {
        public HttpResponseMessage Get(string client)
        {
            return Request.CreateResponse(HttpStatusCode.OK, client);
        }
    }
}
