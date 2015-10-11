using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace thor.Controllers
{
    public class ServerController : ApiController
    {
        [HttpGet, Route(@"api/server/{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Server
        [HttpPost, Route(@"api/server")]
        public void Post([FromBody]string value)
        {
        }

        [HttpPut, Route(@"api/server/{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        [HttpDelete, Route(@"api/server/{id}")]
        public void Delete(int id)
        {
        }
    }
}
