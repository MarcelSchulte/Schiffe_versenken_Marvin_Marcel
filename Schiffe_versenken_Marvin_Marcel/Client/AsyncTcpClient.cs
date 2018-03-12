using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    public class AsyncTcpClient
    {
        #region Delegates

        public delegate void PacketReceivedEventHandler(object sender, PacketReceivedEventArgs e);
        public delegate void ConnectedEventHandler(object sender, ClientConnectedEventArgs e);
        public delegate void DisconnectedEventHandler(object sender, EventArgs e);
        public delegate void ConnectionFailedEventHandler(object sender, EventArgs e);

        #endregion Delegates

        #region Events

        public event PacketReceivedEventHandler PacketReceived;
        public event ConnectedEventHandler Connected;
        public event DisconnectedEventHandler Disconnected;
        public event ConnectionFailedEventHandler ConnectionFailed;

        #endregion Events

        #region Event Raiser

        protected void OnPacketReceived(PacketReceivedEventArgs e)
        {
            PacketReceivedEventHandler recHandler = PacketReceived;
            if (recHandler != null)
            {
                recHandler(this, e);
            }
        }

        protected void OnConnected(ClientConnectedEventArgs e)
        {
            ConnectedEventHandler conHandler = Connected;
            if (conHandler != null)
            {
                conHandler(this, e);
            }
        }

        protected void OnDisconnected(EventArgs e)
        {
            DisconnectedEventHandler disHandler = Disconnected;
            if (disHandler != null)
            {
                disHandler(this, e);
            }
        }

        protected void OnConnectionFailed(EventArgs e)
        {
            ConnectionFailedEventHandler failHandler = ConnectionFailed;
            if (failHandler != null)
            {
                failHandler(this, e);
            }
        }

        #endregion Event Raiser

        #region Fields

        /// <summary>
        /// Der zugrundeliegende TCPClient (System.Net.Sockets).
        /// </summary>
        public TcpClient TcpClient
        {
            get;
            private set;
        }

        /// <summary>
        /// Der Puffer zum Empfangen von Nachrichten.
        /// </summary>
        private byte[] buffer;

        #endregion Fields

        #region Properties

        public bool IsActive
        {
            get;
            private set;
        }

        /// <summary>
        /// gibt zurück, ob der Socket des Clients mit einem Host verbunden ist
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return TcpClient.Connected && IsActive;
            }
        }

        public string IPAdrStr
        {
            get
            {
                if (TcpClient.Connected)
                {
                    string[] part = TcpClient.Client.LocalEndPoint.ToString().Split(':');
                    return part[0];
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string PortStr
        {
            get
            {
                if (TcpClient.Connected)
                {
                    string[] part = TcpClient.Client.LocalEndPoint.ToString().Split(':');
                    return part[1];
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        #endregion Properties

        /// <summary>
        /// Standard Konstruktor
        /// </summary>
        public AsyncTcpClient()
        {
            this.TcpClient = new TcpClient();
            // Puffer wird auf maximale Empfangsgröße von Packeten festgelegt.
            this.buffer = new byte[TcpClient.ReceiveBufferSize];

            IsActive = true;
        }

        public void Destroy()
        {
            if (IsConnected)
            {
                Disconnect();
            }

            IsActive = false;
        }

        /// <summary>
        /// Startet einen Verbindungsversuch zum Server.
        /// Asynchron
        /// </summary>
        /// <param name="localaddr"></param>
        /// <param name="port"></param>
        public void Connect(IPAddress localaddr, int port)
        {
            // Wenn der Client verbunden ist, kein erneuter Verbindungsversuch.
            if (TcpClient.Connected)
                throw new InvalidOperationException();
            try
            {
                // Beginnt einen Asynchronen Verbindungsversuch in einem neuen Thread.
                this.TcpClient.BeginConnect(localaddr, port, ConnectCallback, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Verbinden zum Server.", ex);
            }
        }

        /// <summary>
        /// Trennt die Verbindung zum Server.
        /// Synchron
        /// </summary>
        public void Disconnect()
        {
            // Wenn der Client nicht verbunden ist kann nichts getrennt werden.
            if (!TcpClient.Connected)
                throw new InvalidOperationException();

            //try
            //{
            //tcpClient.ReceiveTimeout = 10;
            //tcpClient.SendTimeout = 10;

            // Schließt die Verbindung zum Server (Synchron, kein neuer Thread).
            TcpClient.Close();

            //Thread.Sleep(100);
            //Application.DoEvents();
            OnDisconnected(EventArgs.Empty);
            //
            //catch (Exception ex)
            //{
            //	throw new Exception("Fehler beim Schließen der Verbindung zum Server.", ex);
            //}
        }

        /// <summary>
        /// Sendet ein Paket (Byte-Array) an den Server.
        /// </summary>
        /// <param name="bytes"></param>
        public void SendPacket(byte[] bytes)
        {
            // Wenn der Client nicht verbunden ist kann nichts gesendet werden.
            if (!TcpClient.Connected)
                throw new InvalidOperationException();
            try
            {
                // Stream holen und Asynchron das Paket in den Stream schreiben (neuer Thread).
                NetworkStream networkStream = TcpClient.GetStream();
                networkStream.BeginWrite(bytes, 0, bytes.Length, sendCallback, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Senden des Paketes.", ex);
            }
        }

        #region Public Callbacks

        /// <summary>
        /// Callback-Methode die nach dem Verbinden zum Server aufgerufen wird.
        /// </summary>
        /// <param name="result"></param>
        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                // Beendet den Verbindungsversuch und schreibt benötigte Rückgabewerte in result.
                this.TcpClient.EndConnect(result);

                // Führt das Event Connected aus.

                // Holt den Stream und beginnt einen asynchronen Lesezyklus (neuer Thread).
                NetworkStream networkStream = TcpClient.GetStream();
                networkStream.BeginRead(this.buffer, 0, buffer.Length, readCallback, null);
            }
            catch (SocketException)
            {
                OnConnectionFailed(EventArgs.Empty);
            }
            catch (Exception ex)
            {

                throw new Exception("Fehler beim Verbinden zum Server.", ex);
            }
            OnConnected(new ClientConnectedEventArgs(TcpClient));
        }

        /// <summary>
        /// Callback-Methode die nach dem Senden einer Nachricht an den Server aufgerufen wird.
        /// </summary>
        /// <param name="result"></param>
        private void sendCallback(IAsyncResult result)
        {
            try
            {
                // Holt den Stream und beendet den asynchronen Sendevorgang (Nachricht wurde gesendet).
                NetworkStream networkStream = TcpClient.GetStream();
                networkStream.EndWrite(result);
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Senden des Paketes.", ex);
            }
        }

        /// <summary>
        /// Callback-Methode die aufgerufen wird sobald eine Nachricht vom Server ankommt.
        /// </summary>
        /// <param name="result"></param>
        private void readCallback(IAsyncResult result)
        {
            if (!TcpClient.Connected)
            {
                throw new Exception("Paket empfangen aber Client nicht verbunden");
            }
            byte[] actualBytes;
            int read;
            try
            {
                // Holt den Stream und beendet den asynchronen Lesevorgang (Nachricht empfangen).
                // Anzahl der empfangenen Bytes wird in read geschrieben.
                NetworkStream networkStream = TcpClient.GetStream();

               read = networkStream.EndRead(result);

                if (read > 0)
                {
                    // Kopiert die Anzahl empfangener Bytes aus dem Puffer (Puffer 8kb).
                    actualBytes = new byte[read];
                    Array.Copy(buffer, actualBytes, read);

                    // PacketReceived Event ausführen.

                    if (networkStream.CanRead)
                    {
                        // Neuen Lesevorgang starten.
                        networkStream.BeginRead(buffer, 0, buffer.Length, readCallback, null);
                    }
                    else
                    {
                        Disconnect();
                    }
                }
                else
                {
                    Disconnect();
                    return;
                }
            }
            catch (IOException)
            {
                OnDisconnected(EventArgs.Empty);
                return;
            }
            OnPacketReceived(new PacketReceivedEventArgs(null, actualBytes, read));
            /*catch (Exception ex)
            {
                throw new Exception("Fehler beim Empfangen eines Paketes.", ex);
            }*/
        }

        #endregion Public Callbacks
    }
}
