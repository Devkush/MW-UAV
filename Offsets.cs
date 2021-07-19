using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAV_MW
{
    public class Offsets
    {
        public static IntPtr Proc;
        public static Process gameProc;
        public static ulong BaseAddress = 0x0;
        public static ulong UAVPtr; 

        public static ulong UAV_Offset1 = 0x16CA0FB8;
        public static ulong UAV_Offset2 = UAV_Offset1 + 0x20;
        public static ulong UAV_Offset3 = UAV_Offset1 + 0x38;
        public static ulong UAV_Offset4 = UAV_Offset1 + 0x1C;
        public static ulong UAV_Offset5 = UAV_Offset1 + 0x38;
        public static ulong UAV_Offset6 = UAV_Offset1 + 0x3C;

    }
}
