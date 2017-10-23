using System.ServiceModel;

namespace SimpleService.Contract
{
    [ServiceContract(Namespace = "http://schemas.gibe.dk/services")]
    public interface ISimpleServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void StatusUpdated(string args);
    }
}