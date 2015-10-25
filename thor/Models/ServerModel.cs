using System;
using System.Collections.Generic;

namespace thor.Models
{
    //https://www.asp.net/mvc/overview/getting-started/getting-started-with-ef-using-mvc/creating-an-entity-framework-data-model-for-an-asp-net-mvc-application
    public class ServerModel
    {
        public Int64 ServerID { get; set; }
        public String ServerName { get; set; }
        public String Host { get; set; }
        public Int32 Port { get; set; }
        public String Password { get; set; }
    }
}