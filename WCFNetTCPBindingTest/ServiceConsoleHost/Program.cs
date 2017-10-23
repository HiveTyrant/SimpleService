using System;

namespace ServiceConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new SimpleServiceHost();
            host.Log += (sender, e) => { Console.WriteLine($"{DateTime.Now:HH.mm.ss.fff}: {e}"); };
            host.Start();

            Console.WriteLine("Service host attempt completed.  Press Enter to exit...");
            Console.ReadLine();

            host.Stop();
        }
    }
}
