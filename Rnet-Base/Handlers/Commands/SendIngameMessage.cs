using System;

namespace Rnet_Base.Handlers.Commands
{
    public class SendIngameMessage
    {
        public String ServerName { get; set; }
        public Target Target { get; set; }
        public String Message { get; set; }
        //public String Message 
        //{
        //    get
        //    {
        //        return this.Message.Length <= 200 ? this.Message : this.Message.Substring(0, 199);
        //    }
        //    set; 
        //}
    }

    public class Target
    {
        public String Name { get; set; }
        public String TargetName { get; set; }
    }
}
