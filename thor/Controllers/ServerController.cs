using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using thor.Models;
using thor.Hubs;
using Rnet_Base.Handlers.Commands;

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
        public Object Post(NewServerModel value)
        {
            #region Errorhandling
            if (string.IsNullOrWhiteSpace(value.Host))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No host given");

            if (value.Port < 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No port was given");

            if (string.IsNullOrWhiteSpace(value.Password))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No password was given");

            if (string.IsNullOrWhiteSpace(value.TargetGroup))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No target group was given");
            #endregion

            var instance = new CreateInstance()
            {
                Host = value.Host,
                Password = value.Password,
                Port = value.Port
            };

            var hubContext = GlobalHost.ConnectionManager.GetHubContext<SkynetHub>();
            var result = hubContext.Clients.Group(value.TargetGroup).createInstance(instance);

            return Request.CreateResponse(HttpStatusCode.OK, "Instance was added");
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
