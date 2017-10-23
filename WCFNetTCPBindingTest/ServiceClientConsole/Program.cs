using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceClientConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new ServiceClient();
            client.Run();

            Console.WriteLine("Service host attempt completed.  Press Enter to exit...");
            Console.ReadLine();
        }
    }

    public class ServiceClient : SimpleServiceReference.ISimpleServiceCallback
    {
        public int StatusCounts { get; set; } = 0;

        public void Run()
        {
            var binding = new NetTcpBinding(SecurityMode.None);
            var serverName = "localhost";
            var portNo = 8081;
            var endpointAdr = new EndpointAddress($"net.tcp://{serverName}:{8081}/SimpleService");

            Console.WriteLine($"Attempting to connect to SimpleService at {endpointAdr.Uri}...");
            using (var client = new SimpleServiceReference.SimpleServiceClient(new InstanceContext(this), binding, endpointAdr))
            {
                var serverTime = client.Ping();
                Console.WriteLine($"Servertime: {serverTime:yyyy.MM.dd HH.mm.ss.fff}");

                Console.WriteLine("Waiting for atleast 5 Status updates...");

                client.Subscribe();
                while (StatusCounts < 5) Thread.Sleep(10);
                client.Unsubscribe();
            }
        }

        public void StatusUpdated(string args)
        {
            StatusCounts++;
            Console.WriteLine($"Status update received: {args}...");
        }
    }
}
