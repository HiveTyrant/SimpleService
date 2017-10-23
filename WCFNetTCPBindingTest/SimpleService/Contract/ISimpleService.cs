using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SimpleService.Contract
{
    [ServiceContract(ProtectionLevel = ProtectionLevel.None,
        SessionMode = SessionMode.Required,
        CallbackContract = typeof(ISimpleServiceCallback),
        Namespace = "http://schemas.gibe.dk/services")]
    public interface ISimpleService
    {

        #region Construction and Initialization

        void Initialize();
        void Close();

        #endregion

        #region Status subscription

        /// <summary>
        /// Subscribe to Status update callbacks
        /// </summary>
        [OperationContract]
        void Subscribe();

        /// <summary>
        /// Unsubscribe from Status update callbacks
        /// </summary>
        [OperationContract]
        void Unsubscribe();

        #endregion
        
        #region Miscelanous

        /// <summary>
        /// Returns the current date and time of the server.
        /// Can be used as a keep-alive method
        /// </summary>
        /// <returns>The current date and time of the server</returns>
        [OperationContract]
        DateTime Ping();

        #endregion
    }
}
