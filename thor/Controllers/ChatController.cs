using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace thor.Controllers
{
    public class ChatController : ApiController
    {
        [HttpGet, Route(@"api/chat/{playername}")]
        public Object GetChatByPlayername(String playername)
        {
            return Request.CreateResponse(HttpStatusCode.OK, "");
        }
    }
}
