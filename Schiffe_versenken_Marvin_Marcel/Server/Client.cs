using System;
using System.Net.Sockets;

namespace Server
{
    class Client
    {
        public Client(TcpClient tcpClient, byte[] buffer)
        {
            if (tcpClient == null) throw new ArgumentNullException("tcpClient");
            if (buffer == null) throw new ArgumentNullException("buffer");
            this.Tcpclient = tcpClient;
            this.Buffer = buffer;
        }

        public TcpClient Tcpclient { get; private set; }

        public byte[] Buffer { get; private set; }

        public NetworkStream Networkstream { get { return Tcpclient.GetStream(); } }

        public void ClearBuffer()
        {
            Buffer = new byte[Tcpclient.ReceiveBufferSize];
        }
    }
}
