using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Simple.Config;
using Simple.Config.Domain;

using Rnet_Battlefield;
using Rnet_Battlefield.Helpers;
using Rnet_Battlefield.RnetConnection.Frostbite;
using Rnet_Base.Handlers.RealTime;
using Rnet_Base.Handlers.Commands;
using Rnet_Base.Handlers.Enums;

namespace Rnet_announcer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Rnet-announcer";
            Console.WriteLine("Rnet-announcer v0.1");

            #region ReadConfig
            var configManager = ConfigManager.GetInstance();
            var file = configManager.Load("servers.ini");

            var serverList = GetServerList(file);

            if (serverList != null)
            {
                foreach (var s in serverList)
                {
                    Console.WriteLine("Sending message to: " + s.Host + ":" + s.Port);
                    SendMessage(s, @"During this month we will shutdown all of our battlefield servers, for more information go to rocketblast.com");
                }
            }

            Console.WriteLine("All messages has been sent!");
            #endregion
        }

        private static Boolean SendMessage(Server server, String message) 
        {
            var con = new RConnection();
            con.HostName = server.Host;
            con.Port = (ushort)server.Port;
            con.PlaintextPassword = server.Password;

            con.Connect();

            while(!con.Client.Connected)
            {
                System.Threading.Thread.Sleep(200);
            }
            con.Login();
            con.Command("admin.say", message, "all");
            System.Threading.Thread.Sleep(200);
            con.Shutdown();

            return true;
        }

        private static IEnumerable<Server> GetServerList(IConfigFile file)
        {
            var result = new List<Server>();
            var servers = file.Namespaces.Where(x => x.Name.ToLower().StartsWith("server-"));

            foreach (var s in servers)
            {
                try
                {
                    result.Add(new Server()
                    {
                        Host = s.Properties.FirstOrDefault(x => x.Name.ToLower().Equals("host")).Value,
                        Password = s.Properties.FirstOrDefault(x => x.Name.ToLower().Equals("password")).Value,
                        Port = Convert.ToInt32(s.Properties.FirstOrDefault(x => x.Name.ToLower().Equals("port")).Value)
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Something went wrong parsing server");
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }

            return result;
        }
    }

    public class Server
    {
        public String Host { get; set; }
        public Int32 Port { get; set; }
        public String Password { get; set; }
    }
}
