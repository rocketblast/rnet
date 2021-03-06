﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;

using Rnet_Battlefield;
using Rnet_Battlefield.Helpers;
using Rnet_Battlefield.RnetConnection.Frostbite;
using Rnet_Base.Handlers.RealTime;
using Rnet_Base.Handlers.Commands;
using Rnet_Base.Handlers.Enums;
using System.Net.Sockets;
using System.Threading;

namespace Rnet_Cli
{
    public class RconClient
    {
        #region Public properties
        public String Host { get; set; }
        public Int32 Port { get; set; }
        public String Password { get; set; }
        public String InstanceName { get; set; }
        public GameType GameType { get; set; }

        public IHubProxy skynet = null;
        #endregion

        #region Private properties
        private RConnection Connection = null;
        #endregion

        public RconClient(IHubProxy proxy) { this.skynet = proxy; }

        public RconClient(IHubProxy proxy, GameType type, string host, int port, string password)
        {
            this.Host = host;
            this.Port = port;
            this.Password = password;
            this.skynet = proxy;
            this.GameType = type;

            Initialize();
            SetupSkynet();
        }

        #region Private methods
        private void Initialize(CreateInstance instance = null)
        {
            Connection = new RConnection();
            this.InstanceName = string.Concat(this.Host, ":", this.Port);

            Connection.HostName = this.Host;
            Connection.Port = (ushort)this.Port;
            Connection.PlaintextPassword = this.Password;

            Connection.Connected += Connection_Connected;
            Connection.Error += Connection_Error;
            Connection.Disconnected += Connection_Disconnected;
            Connection.PacketReceived += Connection_PacketReceived;
            Connection.PacketSent += Connection_PacketSent;

            RunClient();
        }

        private void SetupSkynet(CreateInstance instance = null)
        {
            if(instance == null)
            {
                instance = new CreateInstance()
                {
                    GameType = this.GameType,
                    Host = this.Host,
                    Password = this.Password,
                    Port = this.Port
                };
            }

            #region Realtime handlers
            // The actual method call for the connection to remote server
            var inGameMessage = skynet.On<SendIngameMessage>("IngameMessage", msg =>
            {
                if (msg.ServerName.Equals(this.InstanceName))
                {
                    if (msg.Target.Name.ToLower().Equals("all"))
                    {
                        Connection.Command("admin.say", msg.Message, msg.Target.Name.ToLower());
                        msg = null;
                    }
                    else
                    {
                        Connection.Command("admin.say", msg.Message, msg.Target.Name.ToLower(), msg.Target.TargetName);
                        msg = null;
                    }
                }
            });
            #endregion

            #region Startup calls
            skynet.Invoke<CreateInstance>("InstanceCreated", instance).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("{0} {1}", DateTime.Now, "Unable to notify thor about instance");
                    Console.WriteLine(task.Exception.GetBaseException());
                }
                else
                {
                    Console.WriteLine("{0} {1}", DateTime.Now, "thor has been notified about instance");
                }
            }).Wait();
            #endregion
        }

        private void RunClient()
        {
            Connection.Connect();

            while (Connection.Client.Connected == true)
            {
                // Reconnect logic should be here later on

                #region Shutdown app
                String messageInput = Console.ReadLine();
                if (String.Compare(messageInput, "exit", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Connection.Shutdown();
                }
                else
                {
                    Connection.Command(messageInput.Wordify());
                }
                #endregion
            }

            //Console.WriteLine("{0} {1}", DateTime.Now, "[INFO] - Lost connection to game server");
        }
        #endregion

        #region Public methods
        public void Connect(CreateInstance instance)
        {
            this.Host = instance.Host;
            this.Port = instance.Port;
            this.Password = instance.Password;
            this.GameType = instance.GameType;

            this.Initialize(instance);
        }

        public void Connect(GameType type, string host, int port, string password)
        {
            this.Host = host;
            this.Port = port;
            this.Password = password;
            this.GameType = type;

            this.Initialize();
        }
        #endregion

        #region Events
        void Connection_Connected(RConnection sender)
        {
            Console.WriteLine("{0} {1}", DateTime.Now, "Node is now connected to game server");
            sender.Login();
            sender.EnableEvents();
        }

        void Connection_Disconnected(RConnection sender)
        {
            Console.WriteLine("{0} {1}", DateTime.Now, "[INFO] Node has been disconnected from game server");

            ReConnect(sender, 100);
        }

        void Connection_Error(RConnection sender, Exception e)
        {
            Console.WriteLine("{0} {1}", DateTime.Now, "[ERROR] An error occured!");
            Console.WriteLine("Exception: {0}", e.Message);

            ReConnect(sender);
        }

        void Connection_PacketReceived(RConnection sender, Packet packet)
        {
            Console.WriteLine("<< {0} {1}", packet.Tick, packet);

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

        private void ReConnect(RConnection sender, int tries = 5, int wait = 10000)
        {
            var numberOfTime = 0;
            while (!sender.Client.Connected)
            {
                try
                {
                    Console.WriteLine("{0} {1}", DateTime.Now, "[INFO] Trying reconnect to game server");
                    Initialize();
                }
                catch (SocketException s)
                {
                    Console.WriteLine("{0} {1}", DateTime.Now, "[WARNING] Unable to connect, will try to reconnect in 10seconds");
                    Thread.Sleep(wait);

                    if (numberOfTime == tries)
                    {
                        Console.WriteLine("{0} {1}", DateTime.Now, "[WARNING] Reached the maximum number of tries, will no longer try to reconnect");

                        // TODO: Add more logic for sending this information to the hub and inform master
                        break;
                    }
                    else
                    {
                        numberOfTime++;
                        continue;
                    }
                }
            }
        }

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
