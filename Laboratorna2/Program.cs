using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Laboratorna2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var enc = Console.OutputEncoding;
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            var myProcess = new Process();
            myProcess.StartInfo.FileName = "ConsoleTest.exe";
            myProcess.StartInfo.CreateNoWindow = false;
            myProcess.StartInfo.Arguments = "Process1: arg11 arg12 arg13";
            myProcess.StartInfo.UseShellExecute = true;

            var myProcess1 = new Process();
            myProcess1.StartInfo.FileName = "ConsoleTest.exe";
            myProcess1.StartInfo.CreateNoWindow = false;
            myProcess1.StartInfo.Arguments = "Process2: arg21 arg22 arg23 arg24";
            myProcess1.StartInfo.UseShellExecute = true;

            var myProcess2 = new Process();
            myProcess2.StartInfo.FileName = "ConsoleTest.exe";
            myProcess2.StartInfo.CreateNoWindow = false;
            myProcess2.StartInfo.Arguments = "Process3: arg31 arg32";
            myProcess2.StartInfo.UseShellExecute = true;

            var waitsp = new Process[] { myProcess, myProcess1, myProcess2 };

            myProcess.Start();
            myProcess1.Start();
            myProcess2.Start();

            var pw = new ProcessWait(waitsp);

            Console.WriteLine("Wait for 10 sec.");
            Console.WriteLine($"ID:{myProcess.Id} Handle:{myProcess.Handle}");
            Console.WriteLine($"ID:{myProcess1.Id} Handle:{myProcess1.Handle}");
            Console.WriteLine($"ID:{myProcess2.Id} Handle:{myProcess2.Handle}");


            Console.WriteLine("Waiting exactly 10 seconds...");
            int result = pw.WaitAny(10000);

            if (result == 1) 
            {
                Console.WriteLine("Process 2 finished first within 10 seconds. Terminating Process 1 and Process 3...");
                if (!myProcess.HasExited)
                {
                    Console.WriteLine($"Process 1 (ID: {myProcess.Id}) is still running, terminating...");
                    myProcess.Kill();
                }
                else
                {
                    Console.WriteLine($"Process 1 (ID: {myProcess.Id}) has already exited.");
                }

                if (!myProcess2.HasExited)
                {
                    Console.WriteLine($"Process 3 (ID: {myProcess2.Id}) is still running, terminating...");
                    myProcess2.Kill();
                }
                else
                {
                    Console.WriteLine($"Process 3 (ID: {myProcess2.Id}) has already exited.");
                }
            }
            else // Якщо другий процес не завершився першим або минув тайм-аут
            {
                Console.WriteLine("Process 2 did not finish first within 10 seconds or another process finished.");
                Console.WriteLine("Waiting additional 10 seconds (total 20 seconds) to terminate all...");
                System.Threading.Thread.Sleep(10000); // Просто чекаємо ще 10 секунд

                foreach (var proc in waitsp)
                {
                    if (!proc.HasExited)
                    {
                        Console.WriteLine($"Terminating process ID: {proc.Id}");
                        proc.Kill();
                    }
                }
            }

            pw.Dispose(); 
            Console.WriteLine("All processes terminated. Press <Enter> to exit.");
            Console.ReadLine();
            Console.OutputEncoding = enc;
        }
    }
    internal class ProcessWait : IDisposable
    {
        public static uint INFINITE = 0xFFFFFFFF;
        public static uint WAIT_ABANDONED = 0x00000080;
        public static uint WAIT_OBJECT_0 = 0x00000000;
        public static uint WAIT_TIMEOUT = 0x00000102;
        public static uint WAIT_FAILED = 0x7FFFFFFF;
        private bool disposedValue;

        [DllImport("kernel32.dll")]
        static extern int WaitForMultipleObjects(uint nCount, IntPtr[] lpHandles, bool bWaitAll, uint dwMilliseconds);

        private readonly IntPtr[] _handles;
        private readonly Process[] _processes;
        private readonly uint _count = 0;
        public IntPtr[] Handles => _handles;
        public uint Count => _count;
        public ProcessWait(IntPtr[] handles)
        {
            _handles = handles;
            _count = (uint)_handles.Length;
        }
        public ProcessWait(Process[] processes)
        {
            _count = (uint)processes.Length;
            _handles = new IntPtr[_count];
            _processes = new Process[_count];
            for (var i = 0; i < _count; i++)
            {
                _processes[i] = processes[i];
                _handles[i] = processes[i].Handle;
            }
        }

        public int WaitAll()
        {
            return WaitForMultipleObjects(_count, _handles, true, INFINITE);
        }
        public int WaitAll(uint time)
        {
            return WaitForMultipleObjects(_count, _handles, true, time);
        }
        public int WaitAny()
        {
            return WaitForMultipleObjects(_count, _handles, false, INFINITE);
        }
        public int WaitAny(uint time)
        {
            return WaitForMultipleObjects(_count, _handles, false, time);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    for (var i = 0; i < _count; i++)
                    {
                        if (!_processes[i].HasExited)
                        {
                            _processes[i].Kill();
                            _processes[i].WaitForExit();
                        }
                    }
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
