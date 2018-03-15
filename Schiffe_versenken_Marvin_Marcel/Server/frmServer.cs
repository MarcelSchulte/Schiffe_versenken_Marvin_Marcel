using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class frmServer : Form
    {
        private int Groesse = 30;
        private int Abstand = 4;

        List<PictureBox> pboxen = new List<PictureBox>();

        public frmServer()
        {
            InitializeComponent();
        }

        private void frmServer_Load(object sender, EventArgs e)
        {
            

            for (int x = 0; x < 11; x++)
            {
                for (int y = 0; y < 11; y++)
                {
                    PictureBox pb2 = new PictureBox();
                    pb2.Top = y * (Groesse + Abstand) + 60;
                    pb2.Left = x * (Groesse + Abstand) + 5;
                    pb2.Size = new Size(Groesse, Groesse);
                    if (y == 0 || x == 0) { pb2.BackColor = Color.Purple; }
                    else { pb2.BackColor = Color.White; }
                    pb2.BorderStyle = BorderStyle.FixedSingle;
                    this.Controls.Add(pb2);
                    pboxen.Add(pb2);
                }
                PictureBox pb1 = new PictureBox();
                pb1.Top = 60;
                pb1.Left = x * (Groesse + Abstand) + 5;
                pb1.Size = new Size(Groesse, Groesse);
                pb1.BackColor = Color.White;
                pb1.BorderStyle = BorderStyle.FixedSingle;
                this.Controls.Add(pb1);
                pboxen.Add(pb1);
            }
        }
    }
}
