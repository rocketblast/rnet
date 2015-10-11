using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

using Simple.Config;
using Simple.Config.Domain;
using Newtonsoft.Json;

using Rnet_Base;
using Rnet_Base.Handlers.Commands;
using System.Threading;
using Newtonsoft.Json;
using Rnet_Base.Handlers.RealTime;

namespace Rnet_Cli
{
    class Program
    {
        static HubConnection conn = null as HubConnection;
        static IHubProxy skynet;
        static List<Thread> Threadlist = new List<Thread>();

        static void Main(string[] args)
        {
            #region Welcome message etc
            Console.WriteLine("");
            Console.WriteLine("Rnet-cli tool v0.1 Alpha");
            WriteLine(Console.WindowWidth);
            Console.WriteLine("");
            #endregion

            #region Variables and properties
            var configManger = ConfigManager.GetInstance();
            var baseurl = string.Empty;
            var port = string.Empty;
            #endregion

            #region Readconfig and setting up connection to signalr
            Console.WriteLine("Current config: ");
            var file = configManger.Load("config-sample.ini");

            // Prints before continues
            PrintConfig(file);
            Console.WriteLine("");
            WriteLine(Console.WindowWidth);

            var url = string.Empty;
            baseurl = GetProperty(file, "node", "baseurl");
            port = GetProperty(file, "node", "baseport");

            if(port == null || port == string.Empty) { url = baseurl; }
            else { url = string.Concat(baseurl, ":", port, "/"); }

            conn = new HubConnection(url);

            Console.WriteLine("");
            Console.WriteLine("Connecting to remote signalr server...");
            skynet = conn.CreateHubProxy("Skynet");

            conn.Start().ContinueWith(task =>
            {
                if(task.IsFaulted)
                {
                    Console.WriteLine("There was an error opening the connection: {0}", task.Exception);
                }
                else
                {
                    Console.WriteLine("Connected to signalr!");
                }
            }).Wait();
            #endregion

            if (conn.State == ConnectionState.Connected)
            {
                #region Sending node information
                //var info = new ClientConnected()
                //{
                //    MaxConnections = Convert.ToInt32(GetProperty(file, "node", "maxconnections")),
                //    Name = GetProperty(file, "node", "name"),
                //    Type = GetProperty(file, "node", "type")
                //};

                //skynet.Invoke<ClientConnected>("ClientConnected", info).ContinueWith(task =>
                //{
                //    if (task.IsFaulted)
                //    {
                //        Console.WriteLine("There was an error calling ClientConnected: {0}", task.Exception.GetBaseException());
                //    }
                //    else
                //    {
                //        Console.WriteLine(task.Result);
                //    }
                //});
                #endregion

                // Try to join group nodes and waits for answer before continues
                skynet.Invoke("JoinGroup", "nodes").ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Console.WriteLine("Unable to join group: nodes");
                        Console.WriteLine(task.Exception.GetBaseException());
                    }
                    else
                    {
                        Console.WriteLine("Is now part of the group: nodes");
                    }
                }).Wait();

                #region Handlers
                var CreateInstanceHandler = skynet.On<CreateInstance>("CreateInstance", instance =>
                {
                    Console.WriteLine("Instance information: ");
                    Console.WriteLine("Host: " + instance.Host);
                    Console.WriteLine("Port: " + instance.Port);
                    Console.WriteLine("Password: *********");
                    WriteLine(Console.WindowWidth);

                    Console.WriteLine("");
                    Console.WriteLine("Trying to spawn new instance");

                    RconClient client = new RconClient(skynet);
                    Thread oThread = new Thread(() => client.Connect(instance.Host, instance.Port, instance.Password));
                    oThread.Name = string.Concat(instance.Host, ":", instance.Port);

                    if(Threadlist.FirstOrDefault(x => x.Name.Equals(oThread.Name)) != null)
                    {
                        // TODO: Add realtime response to this
                        Console.WriteLine("Instance already exsists! Unable to add it");
                        WriteLine(Console.WindowWidth);
                    }
                    else
                    {
                        Console.WriteLine("Instance has been created");
                        Console.WriteLine("Booting up Rnet client");
                        WriteLine(Console.WindowWidth);

                        Threadlist.Add(oThread);
                        oThread.Start();
                    }
                });
                #endregion
            }

            Console.ReadLine();
        }

        #region Help methods
        static object Serialize(object obj)
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
        #endregion

        #region Console methods
        private static String GetProperty(IConfigFile file, String section, String property)
        {
            var iniNamespace = file.Namespaces.FirstOrDefault(x => x.Name.ToLower().Equals(section));
            var prop = iniNamespace.Properties.FirstOrDefault(x => x.Name.ToLower().Equals(property));

            if (prop != null) { return String.Join(", ", prop.Values); }
            else { return String.Empty; }
        }

        private static void WriteLine(Int32 chars = 0)
        {
            for(var i = 0; i < chars; i++)
            {
                if (i != chars) { Console.Write("-"); }
                else { Console.WriteLine("-"); }
            }
        }

        private static void PrintConfig(IConfigFile file)
        {
            var iniNamespace = file.Namespaces.FirstOrDefault(x => x.Name.ToLower().Equals("node"));
            foreach(var prop in iniNamespace.Properties)
            {
                var values = String.Join(", ", prop.Values);
                Console.WriteLine("{0} = {1}", prop.Name, values);
            }
        }
        #endregion
    }
}
