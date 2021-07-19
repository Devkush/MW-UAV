using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace UAV_MW
{
    public class Memory
    {
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        public const uint PROCESS_VM_READ = 0x10;
        public const uint PROCESS_VM_WRITE = 0x20;
        public const uint PROCESS_VM_OPERATION = 8;
        public const uint PAGE_READWRITE = 4;
        private Process CurProcess;
        private IntPtr ProcessHandle;
        private string ProcessName;
        private int ProcessID;
        public IntPtr BaseModule;

        public bool AttackProcess(string _ProcessName)
        {
            Process[] processesByName = Process.GetProcessesByName(_ProcessName);
            if (processesByName.Length == 0)
            {
                return false;
            }
            BaseModule = processesByName[0].MainModule.BaseAddress;
            CurProcess = processesByName[0];
            ProcessID = processesByName[0].Id;
            ProcessName = _ProcessName;
            ProcessHandle = OpenProcess(0x38, false, ProcessID);
            return (ProcessHandle != IntPtr.Zero);
        }

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr handle);
        [DllImport("kernel32")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);
        ~Memory()
        {
            if (ProcessHandle != IntPtr.Zero)
            {
                CloseHandle(ProcessHandle);
            }
        }

        public static IntPtr GetModuleBaseAddress(Process proc, string modName)
        {
            IntPtr addr = IntPtr.Zero;

            foreach (ProcessModule m in proc.Modules)
            {
                if (m.ModuleName == modName)
                {
                    addr = m.BaseAddress;
                    break; ;
                }
            }
            return addr;
        }

        internal static IntPtr GetBaseAddress(string ProcessName)
        {
            try
            {
                return Process.GetProcessesByName(ProcessName)[0].MainModule.BaseAddress;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        public bool IsOpen() =>
            (ProcessName != string.Empty) ? AttackProcess(ProcessName) : false;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint dwAccess, bool inherit, int pid);
        public float ReadFloat(ulong _lpBaseAddress)
        {
            IntPtr ptr;
            byte[] lpBuffer = new byte[4];
            ReadProcessMemory(ProcessHandle, _lpBaseAddress, lpBuffer, 4UL, out ptr);
            return BitConverter.ToSingle(lpBuffer, 0);
        }

        public Int32 ReadInt32(ulong _lpBaseAddress)
        {
            IntPtr ptr;
            byte[] lpBuffer = new byte[4];
            ReadProcessMemory(ProcessHandle, _lpBaseAddress, lpBuffer, 4UL, out ptr);
            return BitConverter.ToInt32(lpBuffer, 0);
        }

        public ulong Read12Byte(ulong _lpBaseAddress, byte[] array)
        {
            IntPtr ptr;
            ReadProcessMemory(ProcessHandle, _lpBaseAddress, array, 12, out ptr);
            return BitConverter.ToUInt64(array, 0);
        }

        public Int32 ReadXInt32(ulong _lpBaseAddress, byte[] array)
        {
            IntPtr ptr;
            ReadProcessMemory(ProcessHandle, _lpBaseAddress, array, 4UL, out ptr);
            return BitConverter.ToInt32(array, 0);
        }

        public ulong ReadInt64(ulong _lpBaseAddress)
        {
            IntPtr ptr;
            byte[] lpBuffer = new byte[8];
            ReadProcessMemory(ProcessHandle, _lpBaseAddress, lpBuffer, 8UL, out ptr);
            return BitConverter.ToUInt64(lpBuffer, 0);
        }

        public ulong ReadPointerInt(ulong add, ulong[] offsets, int level)
        {
            ulong num = add;
            for (int i = 0; i < level; i++)
            {
                num = ReadInt64(num) + offsets[i];
            }
            return ReadInt64(num);
        }

        public ulong GetPointer(params ulong[] args)
        {
            ulong CurrentAddr = 0x0;
            for (int i = 0; i <= args.Length - 1; i++)
            {
                if (i != args.Length - 1)
                {
                    CurrentAddr = ReadInt64(CurrentAddr + args[i]);
                }
                else
                {
                    CurrentAddr += args[i];
                }
            }
            return CurrentAddr;
        }

        public ulong GetPointerInt(ulong add, ulong[] offsets, int level)
        {
            ulong num = add;

            for (int i = 0; i < level; i++)
            {
                num = ReadInt64(num) + offsets[i];
            }
            return num;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, ulong lpBaseAddress, [In, Out] byte[] lpBuffer, ulong dwSize, out IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory")]
        protected static extern bool ReadProcessMemory2(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesRead);

        public string ReadString(ulong _lpBaseAddress)
        {
            IntPtr ptr;
            byte[] lpBuffer = new byte[0x500];
            if (ReadProcessMemory(ProcessHandle, _lpBaseAddress, lpBuffer, 0x500, out ptr))
            {
                string str;
                string str1 = "";
                int index = 0;
                while (true)
                {
                    if (lpBuffer[index] == 0)
                    {
                        str = str1;
                        break;
                    }
                    str1 = str1 + ((char)lpBuffer[index]).ToString();
                    index++;
                }
                return str;
            }
            return "";
        }

        public uint ReadUInt32(ulong _lpBaseAddress)
        {
            IntPtr ptr;
            byte[] lpBuffer = new byte[4];
            ReadProcessMemory(ProcessHandle, _lpBaseAddress, lpBuffer, 4UL, out ptr);
            return BitConverter.ToUInt32(lpBuffer, 0);
        }

        public byte[] ReadBytes(ulong _lpBaseAddress, int Length)
        {
            byte[] lpBuffer = new byte[Length];
            IntPtr ptr;
            ReadProcessMemory(ProcessHandle, _lpBaseAddress, lpBuffer, 12UL, out ptr);
            return lpBuffer;
        }


        public ulong ReadUInt64(ulong _lpBaseAddress)
        {
            IntPtr ptr;
            byte[] lpBuffer = new byte[8];
            ReadProcessMemory(ProcessHandle, _lpBaseAddress, lpBuffer, 8UL, out ptr);
            return BitConverter.ToUInt64(lpBuffer, 0);
        }

        [DllImport("kernel32", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flNewProtect, out uint lpflOldProtect);


        public void WriteByte(ulong _lpBaseAddress, byte _Value)
        {
            byte[] bytes = BitConverter.GetBytes((short)_Value);
            IntPtr zero = IntPtr.Zero;
            WriteProcessMemory(ProcessHandle, _lpBaseAddress, bytes, (ulong)bytes.Length, out zero);
        }

        public void WriteBytes(ulong _lpBaseAddress, byte[] buffer)
        {
            IntPtr intPtr;
            WriteProcessMemory(ProcessHandle, _lpBaseAddress, buffer, (ulong)buffer.Length, out intPtr);
        }

 

        public void WriteFloat(ulong _lpBaseAddress, float _Value)
        {
            byte[] bytes = BitConverter.GetBytes(_Value);
            WriteMemory(_lpBaseAddress, bytes);
        }

        public void WriteInt16(ulong _lpBaseAddress, short _Value)
        {
            byte[] bytes = BitConverter.GetBytes(_Value);
            WriteMemory(_lpBaseAddress, bytes);
        }

        public void WriteInt32(ulong _lpBaseAddress, int _Value)
        {
            byte[] bytes = BitConverter.GetBytes(_Value);
            WriteMemory(_lpBaseAddress, bytes);
        }


        public void WriteInt64(ulong _lpBaseAddress, long _Value)
        {
            byte[] bytes = BitConverter.GetBytes(_Value);
            WriteMemory(_lpBaseAddress, bytes);
        }

        public void WriteMemory(ulong MemoryAddress, byte[] Buffer)
        {
            uint num;
            IntPtr ptr;
            VirtualProtectEx(ProcessHandle, (IntPtr)MemoryAddress, (uint)Buffer.Length, 4, out num);
            WriteProcessMemory(ProcessHandle, MemoryAddress, Buffer, (ulong)Buffer.Length, out ptr);
        }

        public void WriteNOP(ulong Address)
        {
            byte[] lpBuffer = new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90 };
            IntPtr zero = IntPtr.Zero;
            WriteProcessMemory(ProcessHandle, Address, lpBuffer, (ulong)lpBuffer.Length, out zero);
        }

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, ulong lpBaseAddress, [In, Out] byte[] lpBuffer, ulong dwSize, out IntPtr lpNumberOfBytesWritten);
        [DllImport("kernel32.dll", EntryPoint = "WriteProcessMemory")]
        private static extern bool WriteProcessMemory2(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, [Out] int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory3(IntPtr hProcess, IntPtr lpBaseAddress, [MarshalAs(UnmanagedType.AsAny)] object lpBuffer, Int64 nSize, out IntPtr lpNumberOfBytesWritten);

        public void WriteString(ulong Address, string Text)
        {
            byte[] bytes = new ASCIIEncoding().GetBytes(Text);
            IntPtr zero = IntPtr.Zero;
            WriteProcessMemory(ProcessHandle, Address, bytes, (ulong)ReadString(Address).Length, out zero);
        }

        public void WriteXString(ulong pAddress, string pString)
        {
            try
            {
                IntPtr num = IntPtr.Zero;
                if (WriteProcessMemory(ProcessHandle, pAddress, Encoding.UTF8.GetBytes(pString), (uint)pString.Length, out num))
                {
                    byte[] lpBuffer = new byte[1];
                    WriteProcessMemory(ProcessHandle, pAddress + (ulong)pString.Length, lpBuffer, 1, out num);
                }
            }
            catch (Exception)
            {
            }
        }

        public void WriteBool(ulong pAddress, bool value)
        {
            try
            {
                byte[] buff = new byte[] { value ? ((byte)1) : ((byte)0) };
                WriteProcessMemory(ProcessHandle, pAddress, buff, (uint)buff.Length, out IntPtr ptr);
            }
            catch (Exception)
            {
            }
        }

        public void WriteUInt32(ulong _lpBaseAddress, uint _Value)
        {
            byte[] bytes = BitConverter.GetBytes(_Value);
            WriteMemory(_lpBaseAddress, bytes);
        }

        public void WriteXBytes(ulong _lpBaseAddress, byte[] _Value)
        {
            byte[] lpBuffer = _Value;
            IntPtr zero = IntPtr.Zero;
            WriteProcessMemory(ProcessHandle, _lpBaseAddress, lpBuffer, (ulong)lpBuffer.Length, out zero);
        }
    }


}
