using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UAV_MW
{
    public class Threads
    {
        public Memory m = new Memory();
        public Form1 main = Form1.main;

        public void PointerThread()
        {
            while (true)
            {
                try
                {
                    if (!m.IsOpen())
                    {
                        m.AttackProcess("ModernWarfare");
                    }
                    else
                    {
                        var gameProcs = Process.GetProcessesByName("ModernWarfare");
                        Offsets.gameProc = gameProcs[0];
                        Offsets.Proc = Memory.OpenProcess((uint)Memory.ProcessAccessFlags.All, false, Offsets.gameProc.Id);

                        Offsets.BaseAddress = (ulong)Memory.GetBaseAddress("ModernWarfare");
                        m.AttackProcess("ModernWarfare");

                        Offsets.UAVPtr = m.ReadInt64(Offsets.BaseAddress + Offsets.UAV_Offset1);

                    }
                    Thread.Sleep(1000);
                }
                catch { }
            }
        }
    }
}
