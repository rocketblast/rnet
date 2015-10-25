using System;
using Rnet_Base.Handlers.Enums;

namespace Rnet_Base.Handlers.Commands
{
    public class CreateInstance : ICreateInstance
    {
        public String Host { get; set; }
        public Int32 Port { get; set; }
        public String Password { get; set; }
        public GameType GameType { get; set; }

        public CreateInstance() { }
    }
}
