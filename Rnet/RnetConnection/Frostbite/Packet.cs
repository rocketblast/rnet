using System;
using System.Collections.Generic;

namespace Rnet_Battlefield.RnetConnection.Frostbite
{
    public class Packet
    {
        public DateTime Tick { get; set; }

        public PacketOrigin Origin { get; set; }

        public Boolean IsResponse { get; set; }

        public List<String> Message { get; set; }

        public UInt32? SequenceId { get; set; }

        public Packet()
        {
            this.SequenceId = null;
            this.Message = new List<String>();
            this.Origin = PacketOrigin.None;
            this.IsResponse = false;
            this.Tick = DateTime.Now;
        }

        public override string ToString()
        {
            return this.Message.Count > 0 ? String.Join(" ", this.Message.ToArray()) : String.Empty;
        }
    }
}
