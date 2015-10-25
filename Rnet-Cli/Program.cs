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
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Rnet_Cli
{
    class Program
    {
        const int SWP_NOZORDER = 0x4;
        const int SWP_NOACTIVATE = 0x10;

        [DllImport("kernel32")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

        static HubConnection conn = null as HubConnection;
        static IHubProxy skynet;
        static List<Thread> Threadlist = new List<Thread>();

        public static void SetWindowPosition(int x, int y, int width, int height)
        {
            SetWindowPos(Handle, IntPtr.Zero, x, y, width, height, SWP_NOZORDER | SWP_NOACTIVATE);
        }

        public static IntPtr Handle
        {
            get
            {
                return GetConsoleWindow();
            }
        }

        static void Main(string[] args)
        {
            #region Variables and properties
            var configManger = ConfigManager.GetInstance();
            var file = configManger.Load("config-sample.ini");
            var baseurl = string.Empty;
            var port = string.Empty;
            #endregion

            #region Console configuration
            var height = GetProperty(file, "node", "height");
            var width = GetProperty(file, "node", "width");
            Console.WindowHeight = Convert.ToInt32(height);
            Console.WindowWidth = Convert.ToInt32(width);
            //Console.BufferHeight = Convert.ToInt32(height);
            //Console.BufferWidth = Convert.ToInt32(width);
            //Console.SetWindowPosition(0, 0);

            //var screen = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            //var w = screen.Width;
            //var h = screen.Height;

            //SetWindowPosition(100, h - 300, 500, 100);
            Console.Title = "Rnet-cli";
            #endregion

            #region Welcome message etc
            Console.WriteLine("");
            Console.WriteLine("Rnet-cli tool v0.1 Alpha");
            WriteLine(Console.WindowWidth);
            Console.WriteLine("");
            #endregion

            #region Readconfig and setting up connection to signalr
            Console.WriteLine("Current config: ");

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
            Console.WriteLine("{0} {1}", DateTime.Now, "Connecting to remote signalr server...");
            skynet = conn.CreateHubProxy("Skynet");

            // Connects to signalr server and prints status
            conn.Start().ContinueWith(task =>
            {
                if(task.IsFaulted)
                {
                    Console.WriteLine("{0} {1} {2}", DateTime.Now, "There was an error opening the connection: ", task.Exception);
                }
                else
                {
                    Console.WriteLine("{0} {1}", DateTime.Now, "Connected to thor!");
                }
            }).Wait();
            #endregion

            if (conn.State == ConnectionState.Connected)
            {
                // Try to join group nodes and waits for answer before continues
                skynet.Invoke("JoinGroup", "nodes").ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Console.WriteLine("{0} {1}", DateTime.Now, "Unable to join group: nodes");
                        Console.WriteLine(task.Exception.GetBaseException());
                    }
                    else
                    {
                        Console.WriteLine("{0} {1}", DateTime.Now, "Is now part of the group: nodes");
                    }
                }).Wait();

                #region General Handlers
                var CreateInstanceHandler = skynet.On<CreateInstance>("CreateInstance", instance =>
                {
                    Console.WriteLine("Instance information: ");
                    Console.WriteLine("Host: " + instance.Host);
                    Console.WriteLine("Port: " + instance.Port);
                    Console.WriteLine("Password: *********");
                    WriteLine(Console.WindowWidth);

                    Console.WriteLine("");
                    Console.WriteLine("{0} {1}", DateTime.Now, "Trying to spawn new instance");

                    var threadName = string.Concat(instance.Host, ":", instance.Port);
                    if(Threadlist.FirstOrDefault(x => x.Name.Equals(threadName)) != null)
                    {
                        // TODO: Add realtime response to this
                        Console.WriteLine("{0} {1}", DateTime.Now, "Instance already exsists! Unable to add it");
                        WriteLine(Console.WindowWidth);
                    }
                    else
                    {
                        RconClient client = new RconClient(skynet);
                        //Thread oThread = new Thread(() => client.Connect(instance.GameType, instance.Host, instance.Port, instance.Password));
                        Thread oThread = new Thread(() => client.Connect(instance));
                        oThread.Name = threadName;

                        Console.WriteLine("{0} {1}", DateTime.Now, "Instance has been created");
                        Console.WriteLine("{0} {1}", DateTime.Now, "Booting up Rnet client");
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
