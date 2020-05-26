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
    }
}
