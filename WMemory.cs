using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace MemoryHandler
{
    class WMemory
    {
        int PROCESS_VM_OPERATION = 0x00000008;
        int PROCESS_VM_READ = 0x00000010;
        int PROCESS_VM_WRITE = 0x00000020;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(
            int processAccess,
            bool bInheritHandle,
            int processId
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int nSize,
            out IntPtr lpNumberOfBytesRead
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
          IntPtr hProcess,
          IntPtr lpBaseAddress,
          byte[] lpBuffer,
          Int32 nSize,
          out int lpNumberOfBytesWritten
        );

        private IntPtr baseAddress = IntPtr.Zero;
        private IntPtr processHandler = IntPtr.Zero;
        private int processId = 0;

        public WMemory(string processName)
        {
            Process process = Process.GetProcessesByName(processName).FirstOrDefault();

            if (process == null)
                throw new Exception("Process " + processName + " not found.", null);

            processId = process.Id;
            baseAddress = process.MainModule.BaseAddress;
            processHandler = handleOpen();
        }

        private IntPtr handleOpen()
        {
            int openingMode = PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE;
            return OpenProcess(openingMode, false, processId);
        }

        public bool WriteByteArray(int pOffset, byte[] bytes)
        {
            int bytesWritten = 0;
            IntPtr finalAddress = baseAddress + pOffset;
            WriteProcessMemory(processHandler, finalAddress, bytes, bytes.Length, out bytesWritten);
            return bytesWritten > 0;
        }

        public Int32 ReadInt32(IntPtr address)
        {
            IntPtr bytesRead;
            byte[] buffer = new byte[4];
            ReadProcessMemory(processHandler, address, buffer, buffer.Length, out bytesRead);

            return BitConverter.ToInt32(buffer, 0);
        }

        public float ReadFloat(IntPtr address)
        {
            IntPtr bytesRead;
            byte[] buffer = new byte[4];
            ReadProcessMemory(processHandler, address, buffer, buffer.Length, out bytesRead);

            return BitConverter.ToSingle(buffer, 0);
        }

        public bool WriteFloat(IntPtr address, float value)
        {
            byte[] floatBytes = BitConverter.GetBytes(value);
            int bytesWritten = 0;

            WriteProcessMemory(processHandler, address, floatBytes, floatBytes.Length, out bytesWritten);
            return bytesWritten > 0;
        }

        public long getFinalAddressFromOffset(int startAddress, int[] offsets)
        {
            long address = (baseAddress + startAddress).ToInt32();

            foreach (int offset in offsets)
            {
                address = ReadInt32((IntPtr)address) + offset;
            }

            return address;
        }
    }
}
