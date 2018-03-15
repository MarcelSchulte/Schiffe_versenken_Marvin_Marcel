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
using System.Security.Cryptography;

namespace Client
{
    public static class Netzwerk
    {
        public static AsyncTcpClient client;
        private static bool connect;

        public static void Connect(TextBox txtip, TextBox txtport)
        {
            client = new AsyncTcpClient();
            IPAddress address = IPAddress.Parse(txtip.Text);

            
            try
            {
                client.Connect(address, Convert.ToInt16(txtport.Text));
            }
            catch (Exception)
            {
                MessageBox.Show("Fehler beim verbinden mit dem Server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void Disconnect()
        {
            client.Disconnect();
        }

        public static void IsConnectet(Label lblstatus)
        {
            if (client.IsConnected == true)
            {
                lblstatus.Invoke((MethodInvoker)(() => lblstatus.Text = "ist verbunden."));
                lblstatus.Invoke((MethodInvoker)(() => lblstatus.ForeColor = Color.Green));
                connect = true;
            }
            else if (connect == true)
            {

                lblstatus.Invoke((MethodInvoker)(() => lblstatus.Text = "ist nicht verbunden."));
                lblstatus.Invoke((MethodInvoker)(() => lblstatus.ForeColor = Color.Red));
                connect = false;
                MessageBox.Show("Verbindung zum Server verloren.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        public static string DecryptString(string cipherText)       //Entschlüsselt die Daten
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();

            // Erstellt die Sicherheitsschlüssel für die Verschlüsselung. Muss auf beiden Seiten gleich sein.
            byte[] IV = { 0x51, 0xa2, 0x0b, 0xcc, 0x45, 0x06, 0x07, 0x08, 0x99, 0x1a, 0x1d, 0x12, 0x13, 0x1e, 0x15, 0x16 };
            rijndaelCipher.Key = Convert.FromBase64String("ABCdfvdfknjvfdjknvfdknjfdkvfdmnj");
            rijndaelCipher.IV = IV;

            MemoryStream memoryStream = new MemoryStream(); // Erstellt ein Objekt das daten auf den Arbeitsspeicher schreiben kann.

            ICryptoTransform rijndaelDecryptor = rijndaelCipher.CreateDecryptor();

            CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndaelDecryptor, CryptoStreamMode.Write);

            string plainText = String.Empty;

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);

                cryptoStream.FlushFinalBlock();

                byte[] plainBytes = memoryStream.ToArray();

                plainText = Encoding.ASCII.GetString(plainBytes, 0, plainBytes.Length);
            }
            finally
            {
                memoryStream.Close();
                cryptoStream.Close();
            }

            return plainText;
        }
    }
}
