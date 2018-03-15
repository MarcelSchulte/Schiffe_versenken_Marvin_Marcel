using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Drawing;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Xml;
using System.Security.Cryptography;

namespace Server
{
    public static class Netzwerk
    {
        public static AsyncTcpServer server;
        private static Label lblClients;
       
        public static void Starten(TextBox txtip, TextBox txtport, Label lblstatus, Label lblclients)
        {
            lblClients = lblclients;

            IPAddress address = IPAddress.Parse(txtip.Text);
            server = new AsyncTcpServer(address, Convert.ToInt16(txtport.Text));

            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;

            try
            {
                server.Start();
                lblstatus.Text = "Server gestartet";
                lblstatus.ForeColor = Color.Green;

            }
            catch (Exception)
            {
                MessageBox.Show("Fehler beim starten vom Server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        public static void Stoppen(Label lblstatus)
        {
            try
            {
                server.Stop();
                lblstatus.Text = "Server ist aus.";
                lblstatus.ForeColor = Color.Red;
            }
            catch (Exception)
            {
                MessageBox.Show("Fehler beim stoppen vom Server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void Server_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            lblClients.Invoke((MethodInvoker)(() => lblClients.Text = $"{server.NumberOfConnectedClients}"));
        }

        private static void Server_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            lblClients.Invoke((MethodInvoker)(() => lblClients.Text = $"{server.NumberOfConnectedClients}"));
        }

        public static void Senden_double(double data)
        {
            try
            {
                byte[] bytes = BitConverter.GetBytes(data);
                server.SendPacket(bytes);
            }
            catch (Exception)
            {

                MessageBox.Show("Fehler beim senden der Daten", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void Senden_string(string data)
        {
            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes(EncryptString(data));
                server.SendPacket(bytes);
            }
            catch (Exception)
            {

                MessageBox.Show("Fehler beim senden der Daten.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string EncryptString(string plainText)           //Verschlüsselung der Daten
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();

            byte[] IV = { 0x51, 0xa2, 0x0b, 0xcc, 0x45, 0x06, 0x07, 0x08, 0x99, 0x1a, 0x1d, 0x12, 0x13, 0x1e, 0x15, 0x16 };
            rijndaelCipher.Key = Convert.FromBase64String("ABCdfvdfknjvfdjknvfdknjfdkvfdmnj");
            rijndaelCipher.IV = IV;

            MemoryStream memoryStream = new MemoryStream();

            ICryptoTransform rijndaelEncryptor = rijndaelCipher.CreateEncryptor();

            CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndaelEncryptor, CryptoStreamMode.Write);

            byte[] plainBytes = Encoding.ASCII.GetBytes(plainText);

            cryptoStream.Write(plainBytes, 0, plainBytes.Length);

            cryptoStream.FlushFinalBlock();

            byte[] cipherBytes = memoryStream.ToArray();

            memoryStream.Close();
            cryptoStream.Close();

            string cipherText = Convert.ToBase64String(cipherBytes, 0, cipherBytes.Length);

            return cipherText;
        }
    }
}
