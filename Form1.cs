using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UAV_MW
{
    public partial class Form1 : Form
    {

        Memory m = new Memory();
        public Threads Threads;
        public static bool GameAttached = true;
        public static Form1 main = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void UAV_ON_Click(object sender, EventArgs e)
        {
            if (GameAttached)
            {
                Threads.m.WriteBytes(Offsets.BaseAddress + Offsets.UAV_Offset1, new byte[] { 0x01, 0x00, 0x00, 0x00 });
                Threads.m.WriteBytes(Offsets.BaseAddress + Offsets.UAV_Offset2, new byte[] { 0x01, 0x00, 0x00, 0x00 });
                Threads.m.WriteBytes(Offsets.BaseAddress + Offsets.UAV_Offset3, new byte[] { 0x4f, 0xbb, 0x55, 0x51 });
                Threads.m.WriteBytes(Offsets.BaseAddress + Offsets.UAV_Offset4, new byte[] { 0xf6, 0x6d, 0xae, 0x79 });
                Threads.m.WriteBytes(Offsets.BaseAddress + Offsets.UAV_Offset5, new byte[] { 0x4f, 0xbb, 0x55, 0x51 });
                Threads.m.WriteBytes(Offsets.BaseAddress + Offsets.UAV_Offset6, new byte[] { 0xf6, 0x6d, 0xae, 0x79 });
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            start();
        }

        public void start()
        {
            main = this;
            if (!backgroundWorker1.IsBusy) backgroundWorker1.RunWorkerAsync();
            Threads = new Threads();

            var PointerThread = new Thread(Threads.PointerThread);
            PointerThread.IsBackground = true;
            PointerThread.Start();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    if (Offsets.BaseAddress != 0)
                    {
                        GameAttached = true;
                        label1.ForeColor = Color.DarkGreen;
                        label1.Text = "ATTACHED";
                    }
                    else
                    {
                        GameAttached = false;
                        for (int i = 0; i < 3; i++)
                        {
                            label1.Text = "WAITING FOR GAME";
                            Thread.Sleep(500);
                            label1.Text = "WAITING FOR GAME.";
                            Thread.Sleep(500);
                            label1.Text = "WAITING FOR GAME..";
                            Thread.Sleep(500);
                            label1.Text = "WAITING FOR GAME...";
                            Thread.Sleep(500);
                        }
                    }
                }
                  
                
                catch { }
            }
        }
    }
}
