using System;

namespace Rnet_Base.Handlers.RealTime
{
    public class Event : IEvent
    {
        public Int64 SequenceId { get; set; }
        public Boolean isResponse { get; set; }
        public DateTime Tick { get; set; }
        public Int32 Origin { get; set; }
        public String ServerName { get; set; }
        public Message Message { get; set; }

        public Event()
        {

        }
    }
}
