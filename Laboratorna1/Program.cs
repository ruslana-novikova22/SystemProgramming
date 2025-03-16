using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinApi;

namespace Laboratorna1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var p1 = WinApiFuncs.CreateProcess("ConsoleTest Process1 arg11 arg12 arg13", WinApiFuncs.CREATE_NEW_CONSOLE);
            var p2 = WinApiFuncs.CreateProcess("ConsoleTest Process2 arg21 arg22 arg23 arg24",WinApiFuncs.CREATE_NEW_CONSOLE);
            var hp = new IntPtr[] { p1.hProcess, p2.hProcess };
            WinApiFuncs.CloseHandle(p1.hThread);
            WinApiFuncs.CloseHandle(p2.hThread);
            Console.WriteLine(" Processes started. Wait for 10 sec.to complete") ;
            var ret = WinApiFuncs.WaitForMultipleObjects(2, hp, false, 10000);
            if (ret == WinApiFuncs.WAIT_TIMEOUT)
            {
                Console.WriteLine(" Processes still working.Terminate it.");
                for (int i = 0; i < hp.Length; i++)
{
                    WinApiFuncs.TerminateProcess(hp[i], 0);
                    WinApiFuncs.CloseHandle(hp[i]);
                }
                Console.WriteLine(" All Done.");
                Console.ReadLine();
                return;
            }
            if (ret == WinApiFuncs.WAIT_OBJECT_0)
            {
                Console.WriteLine("First process terminated early. Close all");
                for (int i = 0; i < hp.Length; i++)
                {
                    WinApiFuncs.TerminateProcess(hp[i], 0);
                    WinApiFuncs.CloseHandle(hp[i]);
                }
            }
            else
            {
                Console.WriteLine("Second process terminated early. Restarting...");
                WinApiFuncs.TerminateProcess(hp[1], 0);
                WinApiFuncs.CloseHandle(hp[1]);
                p2 = WinApiFuncs.CreateProcess("ConsoleTest Process2 arg21 arg22 arg23 arg24", WinApiFuncs.CREATE_NEW_CONSOLE);
                hp[1] = p2.hProcess;
            }
            Console.WriteLine(" All Done.");
            Console.ReadLine();
        }
    }
}
