using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using SimpleService.Contract;

namespace ServiceConsoleHost
{
    public class SimpleServiceHost
    {
        private ServiceHost _serviceHost;
        private ISimpleService _serviceInstance;

        public event EventHandler<string> Log;


        public void Start()
        {
            OnLog("SimpleServiceHost starting...");

            _serviceHost?.Close(new TimeSpan(0, 0, 0, 5));
            try
            {
                // Setup Service as Singleton service implementation
                _serviceInstance = new SimpleService.SimpleService();
                _serviceInstance.Initialize();

                var tcpAddress = $"net.tcp://localhost:8081/SimpleService";
#if DEBUG

                // On DEBUG builds, also listen to HTTP endpoint. This makes it easier to get WSDL for testing
                var httpAddress = $"http://localhost:8080/SimpleService";
                var baseAdresses = new[] { new Uri(httpAddress), new Uri(tcpAddress) };
                const bool httpGetEnabled = true;
#else
                var baseAdresses = new[] { new Uri(tcpAddress) };
                const bool httpGetEnabled = false;
#endif

                _serviceHost = new ServiceHost(_serviceInstance, baseAdresses);

                // Enable metadata exchange for Service
                _serviceHost.Description.Behaviors.Add(new ServiceMetadataBehavior { HttpGetEnabled = httpGetEnabled });

#if DEBUG
                // On DEBUG builds, include exception details in faults
                var sdb = _serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>();
                if (null != sdb)
                    sdb.IncludeExceptionDetailInFaults = true;
                else
                    _serviceHost.Description.Behaviors.Add(new ServiceDebugBehavior
                    {
                        IncludeExceptionDetailInFaults = true
                    });
#endif
                // Add discoverable endpoints, using Dual HTTP and MEX bindings, for Service
                _serviceHost.AddServiceEndpoint(typeof(ISimpleService), GetTcpBinding, tcpAddress);
                _serviceHost.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexTcpBinding(), "mex");

                _serviceHost.Open();

                OnLog($"SimpleService listening on:\n - {string.Join("\n - ", _serviceHost.BaseAddresses)}");
            }
            catch (Exception ex)
            {
                OnLog("Error: initializing SimpleService failed: " + ex.Message);
                if (ex.Message.Contains("HTTP could not register URL http"))
                    OnLog("Try useing the command: netsh http add urlacl http://+:8080/SimpleService/ user=Everyone");
            }
        }

        public void Stop()
        {
            OnLog("SimpleServiceHost stopping...");

            if (_serviceHost != null && _serviceHost.State != CommunicationState.Closed)

                // Make sure, that allocated ressources gets released/disposed
                _serviceHost.Close(new TimeSpan(0, 0, 0, 5));
            _serviceHost = null;

            _serviceInstance.Close();
            _serviceInstance = null;
        }

        protected virtual void OnLog(string e)
        {
            Log?.Invoke(this, e);
        }
        private static NetTcpBinding GetTcpBinding
        {
            get
            {
                var binding = new NetTcpBinding
                {
                    Namespace = "http://schemas.gibe.dk/services",
                    SendTimeout = new TimeSpan(0, 0, 0, 25),
                    ReceiveTimeout = new TimeSpan(0, 0, 1, 00),
                    OpenTimeout = new TimeSpan(0, 0, 0, 15),
                    CloseTimeout = new TimeSpan(0, 0, 1, 0),
                    TransactionFlow = true,
                    Security = { Mode = SecurityMode.None },
                    MaxReceivedMessageSize = 655360
                };

                return binding;
            }
        }
    }
}
