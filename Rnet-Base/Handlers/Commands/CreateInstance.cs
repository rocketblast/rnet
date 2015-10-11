using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rnet_Base.Handlers.Commands
{
    public class CreateInstance : ICreateInstance
    {
        public String Host { get; set; }
        public Int32 Port { get; set; }
        public String Password { get; set; }

        public CreateInstance() { }
    }
}
