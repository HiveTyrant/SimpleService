using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using SimpleService.Contract;

namespace SimpleService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, Namespace = "http://schemas.gibe.dk/services")]
    public class SimpleService : ISimpleService
    {
        #region Private members

        private readonly List<ISimpleServiceCallback> _serviceCallbacks = new List<ISimpleServiceCallback>();
        private readonly AutoResetEvent _threadStop = new AutoResetEvent(false);
        private readonly AutoResetEvent _threadEnded = new AutoResetEvent(false);
        private Thread _thread;
        
        #endregion
        
        #region ISimpleService implementation

        public void Initialize()
        {
            // Add a new background thread, to simulate some kind of external event, that triggers subscriber callback
            _thread = new Thread(SucscriptionEventProducer) {IsBackground = true};
            _thread.Start();
        }

        public void Close()
        {
            // Shut down the background thread
            if (null == _thread) return;

            _threadStop.Set();
            _threadEnded.WaitOne(500);
            _thread = null;
        }

        public void Subscribe()
        {
            try
            {
                var serviceCallback = OperationContext.Current.GetCallbackChannel<ISimpleServiceCallback>();
                if (false == _serviceCallbacks.Contains(serviceCallback))
                {
                    _serviceCallbacks.Add(serviceCallback);
                    Console.WriteLine($"Added subscriber: {OperationContext.Current.Channel.RemoteAddress}, SessionId: {OperationContext.Current.Channel.SessionId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(SimpleService)}.{nameof(Subscribe)} failed. Error: {ex.Message}");
            }
        }

        public void Unsubscribe()
        {
            try
            {
                var serviceCallback = OperationContext.Current.GetCallbackChannel<ISimpleServiceCallback>();
                {
                    _serviceCallbacks.Remove(serviceCallback);
                    Console.WriteLine($"Unsubscribing subscriber: {OperationContext.Current.Channel.RemoteAddress}, SessionId: {OperationContext.Current.Channel.SessionId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(SimpleService)}.{nameof(Unsubscribe)} failed. Error: {ex.Message}");
            }
        }

        public DateTime Ping()
        {
            return DateTime.Now;
        }

        #endregion

        #region Callbacks

        /// <summary>
        /// Status of some check weigher/indicator has been updated. Notify clients.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void OnStatusUpdated(object sender, string args)
        {
            // Execute each callback in seperate task to avoid delay due to a timeout
            foreach (var serviceCallback in _serviceCallbacks.ToList())
            {
                Task.Run(() =>
                {
                    try
                    {
                        Console.WriteLine($"{nameof(SimpleService)}.{nameof(OnStatusUpdated)}: Calling StatusUpdated() for client: {serviceCallback}");
                        serviceCallback.StatusUpdated(args);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{nameof(SimpleService)}.{nameof(OnStatusUpdated)} failed: The client channel is no longer accessible and will be removed: " + ex.Message);
                        _serviceCallbacks.Remove(serviceCallback);
                    }
                });
            }
        }

        #endregion

        #region Helper methods

        private void SucscriptionEventProducer()
        {
            var nextRecTime = DateTime.MinValue;

            while (true)
            {
                if (_serviceCallbacks.Any() && DateTime.Now > nextRecTime)
                {
                    nextRecTime = DateTime.Now + TimeSpan.FromMilliseconds(250);

                    OnStatusUpdated(this, "Status update: " + DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff"));
                }
                else if (_threadStop.WaitOne(10))
                    break;
            }

            // Signal that thread has completed
            _threadEnded.Set();
        }

        #endregion
    }
}