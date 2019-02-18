using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Shared
{
    [DataContract]
    public class Request {
        [DataMember]
        public string Value { get; set; }
    }

    [DataContract]
    public class Response
    {
        [DataMember]
        public string Value { get; set; }
    }

    public interface IApi
    {
        Task<Response> Unary(Request request);
        IObservable<Response> UnaryToStream(Request request);
        IObservable<Response> StreamToStream(IObservable<Request> request);
        Task<Response> StreamToUnary(IObservable<Request> request);
    }
}
