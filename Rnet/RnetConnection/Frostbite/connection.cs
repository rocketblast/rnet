using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Rnet_Battlefield.RnetConnection.Frostbite
{
    public class RConnection
    {
        #region Class Variables
        protected byte[] Buffer;
        protected byte[] ReadData;
        #endregion

        #region Class properties
        public TcpClient Client { get; private set; }
        public String HostName { get; set; }
        public ushort Port { get; set; }
        public String PlaintextPassword { get; set; }
        public String LastCommand { get; set; }
        public List<string> LastResponse { get; set; }
        public String Servername { get; set; }
        #endregion

        #region Class protected properties
        protected NetworkStream Stream { get; set; }
        protected PacketSerializer PacketSerializer { get; set; }
        protected UInt32 SequenceNumber { get; set; }
        protected readonly Object AcquireSequenceNumberLock = new object();
        #endregion

        #region Events
        public delegate void ErrorHandler(RConnection sender, Exception e);
        public delegate void EmptyParameterHandler(RConnection sender);
        public delegate void PacketHandler(RConnection sender, Packet packet);

        public event ErrorHandler Error;
        public event EmptyParameterHandler Connected;
        public event EmptyParameterHandler Disconnected;
        public event PacketHandler PacketReceived;
        public event PacketHandler PacketSent;

        protected void OnError(Exception e)
        {
            var handler = this.Error;
            if (handler != null) { handler(this, e); }
        }

        protected void OnConnected()
        {
            var handler = this.Connected;
            if (handler != null) { handler(this); }

            this.GetServerInfo();
        }

        protected void OnDisconnected()
        {
            var handler = this.Disconnected;
            if (handler != null) { handler(this); }
        }

        protected void OnPacketReceived(Packet packet)
        {
            var handler = this.PacketReceived;
            if (handler != null) { handler(this, packet); }
        }

        protected void OnPacketSent(Packet packet)
        {
            var handler = this.PacketSent;
            if (handler != null) { handler(this, packet); }
        }
        #endregion

        public RConnection()
        {
            this.Client = new TcpClient();
            this.HostName = String.Empty;

            this.Buffer = new byte[0];
            this.ReadData = new byte[1024];
            this.PacketSerializer = new PacketSerializer();
            this.SequenceNumber = 0;
        }

        #region Public methods and functions
        public UInt32 AcquireSequenceNumber()
        {
            lock (this.AcquireSequenceNumberLock)
            {
                return ++this.SequenceNumber;
            }
        }

        public bool Connect()
        {
            bool connected = false;

            try
            {
                this.Client.Connect(this.HostName, this.Port);

                if (this.Client.Connected == true)
                {
                    this.Stream = this.Client.GetStream();
                    this.Stream.BeginRead(this.ReadData, 0, this.ReadData.Length, this.ReceiveCallback, null);
                    this.OnConnected();
                    connected = true;
                }
            }
            catch (Exception e)
            {
                this.OnError(e);
                this.Shutdown();
            }

            return connected;
        }

        public void Shutdown()
        {
            if (this.Client != null && this.Client.Connected == true)
            {
                this.Client.Close();
                this.OnDisconnected();
            }
        }

        public void Login()
        {
            this.Command("login.plainText", this.PlaintextPassword);
        }

        public void EnableEvents()
        {
            this.Command("admin.eventsEnabled", "true");
        }

        public void GetServerInfo()
        {
            this.Command(new List<string>(){ "serverInfo" });
        }

        public void Respond(Packet packet, params String[] msg)
        {
            this.Respond(packet, new List<String>(msg));
        }

        public void Respond(Packet packet, List<String> msg)
        {
            this.Send(new Packet()
            {
                Origin = packet.Origin,
                IsResponse = true,
                SequenceId = packet.SequenceId,
                Message = msg
            });
        }

        public void Command(params String[] msg)
        {
            this.LastCommand = msg.Count() > 0 ? string.Join(" ", msg) : string.Empty;
            this.Command(new List<String>(msg));
        }

        public void Command(List<String> msg)
        {
            this.LastCommand = msg.Count() > 0 ? string.Join(" ", msg) : string.Empty;
            this.Send(new Packet()
            {
                Origin = PacketOrigin.Client,
                IsResponse = false,
                SequenceId = this.AcquireSequenceNumber(),
                Message = msg
            });
        }
        #endregion

        #region Private methods and functions
        private bool Send(Packet packet)
        {
            bool sent = false;

            try
            {
                if (this.Client.Connected == true && this.Stream != null)
                {
                    // 1. Encode the packet to byte[] for sending
                    byte[] data = this.PacketSerializer.Serialize(packet);

                    // 2. Write it to the stream
                    this.Stream.Write(data, 0, data.Length);

                    // 3. Alert the application a packet has been successfully sent.
                    this.OnPacketSent(packet);

                    sent = true;
                }
            }
            catch (Exception e)
            {
                this.OnError(e);

                this.Shutdown();
            }

            return sent;
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            if (this.Client.Connected == true)
            {
                try
                {
                    int bytesRead = this.Stream.EndRead(ar);

                    if (bytesRead > 0)
                    {

                        // 1. Resize our buffer to store the additional data just read
                        Array.Resize(ref this.Buffer, this.Buffer.Length + bytesRead);

                        // 2. Append the data just read onto the buffer.
                        Array.Copy(this.ReadData, 0, this.Buffer, this.Buffer.Length - bytesRead, bytesRead);

                        // 3. Find the size of the first packet on the buffer
                        long packetSize = this.PacketSerializer.ReadPacketSize(this.Buffer);

                        // WHILE we have enough data AND that data at least covers the header of the packet
                        while (this.Buffer.Length >= packetSize && this.Buffer.Length > this.PacketSerializer.PacketHeaderSize)
                        {

                            // 4. Create a packet by removing the first X bytes from the buffer
                            Packet packet = this.PacketSerializer.Deserialize(this.Buffer.Take((int)packetSize).ToArray());

                            // 5. Alert application to new packet.
                            this.ProcessRecievedPacket(packet);

                            // 6. Remove all bytes from the buffer that were used to create the packet in step 4.
                            this.Buffer = this.Buffer.Skip((int)packetSize).ToArray();

                            // 7. Find out if we've got enough for another packet.
                            packetSize = this.PacketSerializer.ReadPacketSize(this.Buffer);
                        }

                        // Now wait for more data to read
                        this.Stream.BeginRead(this.ReadData, 0, this.ReadData.Length, this.ReceiveCallback, null);
                    }
                    else if (bytesRead == 0)
                    {
                        // Nothing was read, time to shut down the connection.
                        this.Shutdown();
                    }
                }
                catch (Exception e)
                {
                    this.OnError(e);
                    this.Shutdown();
                }
            }
        }
        #endregion

        protected void ProcessRecievedPacket(Packet packet)
        {
            this.OnPacketReceived(packet);
            this.LastResponse = packet.Message.Count > 0 ? packet.Message : null;

            if(this.LastCommand.ToLower() == "serverinfo")
            {
                if(packet.Message.Count > 10)
                {
                    this.Servername = packet.Message[1];
                    this.LastCommand = "";
                }
            }

            // If this originated from the server then make sure we send back an OK message with
            // the same sequence id. If we don't do this then the server may disconnect us.
            // This only occurs after some connection time
            if (packet.Origin == PacketOrigin.Server && packet.IsResponse == false)
            {
                this.Respond(packet, "OK");
            }
        }
    }
}
