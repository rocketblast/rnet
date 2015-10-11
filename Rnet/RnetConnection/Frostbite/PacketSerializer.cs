using System;
using System.Text;

namespace Rnet_Battlefield.RnetConnection.Frostbite
{
    public class PacketSerializer
    {
        public UInt32 PacketHeaderSize { get; protected set; }

        public PacketSerializer() { this.PacketHeaderSize = 12; }

        public byte[] Serialize(Packet packet)
        {
            UInt32 header = packet.SequenceId != null ? (UInt32)packet.SequenceId & 0x3fffffff : 0x3fffffff;

            if (packet.Origin == PacketOrigin.Server)
            {
                header |= 0x80000000;
            }

            if (packet.IsResponse == true)
            {
                header |= 0x40000000;
            }

            UInt32 packetSize = this.PacketHeaderSize;
            UInt32 wordCount = Convert.ToUInt32(packet.Message.Count);

            byte[] encodeWords = new byte[] { };
            foreach (string word in packet.Message)
            {
                string convertedWord = word;

                if (convertedWord.Length > UInt16.MaxValue - 1)
                {
                    convertedWord = convertedWord.Substring(0, UInt16.MaxValue - 1);
                }

                byte[] appendEncodedWords = new byte[encodeWords.Length + convertedWord.Length + 5];
                encodeWords.CopyTo(appendEncodedWords, 0);

                BitConverter.GetBytes(convertedWord.Length).CopyTo(appendEncodedWords, encodeWords.Length);
                Encoding.GetEncoding(1252).GetBytes(convertedWord + Convert.ToChar(0x00)).CopyTo(appendEncodedWords, encodeWords.Length + 4);

                encodeWords = appendEncodedWords;
            }

            packetSize += Convert.ToUInt32(encodeWords.Length);
            byte[] returnPacket = new byte[packetSize];

            BitConverter.GetBytes(header).CopyTo(returnPacket, 0);
            BitConverter.GetBytes(packetSize).CopyTo(returnPacket, 4);
            BitConverter.GetBytes(wordCount).CopyTo(returnPacket, 8);
            encodeWords.CopyTo(returnPacket, this.PacketHeaderSize);

            return returnPacket;
        }

        public Packet Deserialize(byte[] data)
        {
            Packet packet = new Packet();

            UInt32 header = BitConverter.ToUInt32(data, 0);
            UInt32 wordsTotal = BitConverter.ToUInt32(data, 8);

            // Try to figure out if message is from client or from server
            packet.Origin = Convert.ToBoolean(header & 0x80000000) == true ? PacketOrigin.Server : PacketOrigin.Client;

            packet.IsResponse = Convert.ToBoolean(header & 0x40000000);
            packet.SequenceId = header & 0x3fffffff;

            int wordOffset = 0;

            for (UInt32 wordCount = 0; wordCount < wordsTotal; wordCount++)
            {
                UInt32 wordLength = BitConverter.ToUInt32(data, (int)this.PacketHeaderSize + wordOffset);
                packet.Message.Add(Encoding.GetEncoding(1252).GetString(data, (int)this.PacketHeaderSize + wordOffset + 4, (int)wordLength));
                wordOffset += Convert.ToInt32(wordLength) + 5;
            }

            return packet;
        }

        public long ReadPacketSize(byte[] data)
        {
            long lenghth = 0;

            if (data.Length >= this.PacketHeaderSize)
            {
                lenghth = BitConverter.ToUInt32(data, 4);
            }

            return lenghth;
        }
    }
}
