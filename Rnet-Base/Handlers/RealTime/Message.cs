using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rnet_Base.Handlers.RealTime
{
    public class Message : IMessage
    {
        public String Type { get; set; }
        public String Content { get; set; }

        public Message() { }
        public Message(String msg) { SetMessage(msg); }
        public Message(List<String> msg) { SetMessage(msg); }

        private void SetMessage(string msg)
        {
            var r = (msg.Length > 0 && msg.Split(' ').Length > 0) ? msg.Split(' ').ToList() : null;

            if(r.Count > 1)
            {
                this.Type = r[0];
                this.Content = string.Join(" ", r.Skip(1).ToArray());
            }
        }

        private void SetMessage(List<String> msg)
        {
            this.Type = msg[0];
            this.Content = string.Join(" ", msg.Skip(1).ToArray());
        }
    }
}
