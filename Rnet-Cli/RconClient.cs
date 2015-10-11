using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rnet_Battlefield;
using Rnet_Battlefield.Helpers;
using Rnet_Battlefield.RnetConnection.Frostbite;
using Newtonsoft.Json;
using Microsoft.AspNet.SignalR.Client;
using Rnet_Base.Handlers.RealTime;


namespace Rnet_Cli
{
    public class RconClient
    {
        #region Public properties
        public String Host { get; set; }
        public Int32 Port { get; set; }
        public String Password { get; set; }

        public IHubProxy skynet = null;
        #endregion

        #region Private properties
        private RConnection Connection = null;
        #endregion

        public RconClient(IHubProxy proxy) { this.skynet = proxy; }

        public RconClient(IHubProxy proxy, string host, int port, string password)
        {
            this.Host = host;
            this.Port = port;
            this.Password = password;
            this.skynet = proxy;

            Initialize();
        }

        #region Private methods
        private void Initialize()
        {
            Connection = new RConnection();

            Connection.HostName = this.Host;
            Connection.Port = (ushort)this.Port;
            Connection.PlaintextPassword = this.Password;

            Connection.Connected += Connection_Connected;
            Connection.Error += Connection_Error;
            Connection.Disconnected += Connection_Disconnected;
            Connection.PacketReceived += Connection_PacketReceived;
            Connection.PacketSent += Connection_PacketSent;

            // The actual method call for the connection to remote server
            Connection.Connect();

            while(Connection.Client.Connected == true)
            {
                String messageInput = Console.ReadLine();

                if(String.Compare(messageInput, "exit", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Connection.Shutdown();
                }
                else
                {
                    Connection.Command(messageInput.Wordify());
                }
            }
        }
        #endregion

        #region Public methods
        public void Connect(string host, int port, string password)
        {
            this.Host = host;
            this.Port = port;
            this.Password = password;

            this.Initialize();
        }
        #endregion

        #region Events
        void Connection_Connected(RConnection sender)
        {
            Console.WriteLine("{0} Connected", DateTime.Now);
            sender.Login();
            sender.EnableEvents();
        }

        void Connection_Disconnected(RConnection sender)
        {
            Console.WriteLine("{0} Disconnected!", DateTime.Now);
        }

        void Connection_Error(RConnection sender, Exception e)
        {
            Console.WriteLine("ERROR: {0}", e.Message);
        }

        void Connection_PacketReceived(RConnection sender, Packet packet)
        {
            Console.WriteLine(">> {0} {1}", packet.Tick, packet);

            var servername = (sender.Servername != null && sender.Servername.Count() > 0) ? sender.Servername : string.Concat(sender.HostName, ":", sender.Port);
            if(!packet.Message.Contains("OK"))
            {
                var msg = new Message(packet.Message);

                var m = new Event
                {
                    isResponse = packet.IsResponse,
                    Message = new Message(packet.Message),
                    Origin = (Int32)packet.Origin,
                    SequenceId = (Int64)packet.SequenceId,
                    ServerName = servername,
                    Tick = packet.Tick
                };

                skynet.Invoke("SendEvent", Serialize(m)).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Console.WriteLine(">>> {0} {1} {2}", DateTime.Now, "Unable to call remote method send: ", task.Exception.GetBaseException());
                    }
                    else
                    {
                        // Do something here later if everything is successful
                        //Console.WriteLine(task.IsCompleted);
                    }
                });
            }
        }

        void Connection_PacketSent(RConnection sender, Packet packet)
        {
            if(!packet.Message.Contains("OK"))
            {
                Console.WriteLine(">> {0} {1}", packet.Tick, packet);
            }
        }
        #endregion

        private object Serialize(object obj)
        {
            try
            {
                obj = JsonConvert.SerializeObject(obj);
            }
            catch(Exception ex)
            {
                throw ex;
            }

            return obj;
        }
    }
}
