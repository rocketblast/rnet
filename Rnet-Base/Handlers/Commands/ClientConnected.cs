using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rnet_Base.Handlers.Commands
{
    public class ClientConnected : IClientConnected
    {
        public String Type { get; set; }
        public Int32 MaxConnections { get; set; }
        public String Name { get; set; }

        public ClientConnected() { }
    }
}
