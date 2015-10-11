using System;

namespace thor.Models
{
    public class NewServerModel
    {
        public String TargetGroup { get; set; }
        public String Host { get; set; }
        public Int32 Port { get; set; }
        public String Password { get; set; }
    }
}