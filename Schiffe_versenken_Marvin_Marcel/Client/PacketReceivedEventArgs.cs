using System;
using System.Net.Sockets;

namespace Client
{
    public class PacketReceivedEventArgs : EventArgs
    {
        public byte[] Packet { get; set; }

        public TcpClient Sender { get; set; }

        public PacketReceivedEventArgs(TcpClient sender, byte[] buffer, int length)
        {
            this.Sender = sender;
            this.Packet = new byte[length];
            // Kopiert vom Puffer die tatsächliche Anzahl bytes ins Packetarray.
            Array.Copy(buffer, this.Packet, length);
        }
    }
}
