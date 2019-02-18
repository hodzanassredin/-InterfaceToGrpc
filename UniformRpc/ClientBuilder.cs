using Grpc.Core;
using ImpromptuInterface;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using static UniformRpc.Descriptors;

namespace UniformRpc
{ //https://github.com/Horusiath/GrpcSample/blob/master/GrpcSample.Client/Program.cs
    //https://github.com/grpc/grpc/issues/15272
    public class ClientBuilder<T> : IMethodDesciptorVisitor
         where T : class
    {
        CallInvoker invoker;
        dynamic expando;
        IDictionary<string, object> dict;
        public ClientBuilder(Channel channel)
        {
            invoker = new DefaultCallInvoker(channel);
            expando = new ExpandoObject();
            dict = expando;
        }
        public void Fill()
        {
            foreach (var desc in Descriptors.GetMethods<T>())
            {
                desc.Visit(this);
                
            }
        }
        public T Build(){
            return Impromptu.ActLike<T>(expando);
        }
        

        public void Accept<TReq, TResp>(MethodDesciptorTyped<TReq, TResp> descr)
            where TReq : class
            where TResp : class
        {
            var method = descr.GetMethod();
            switch (descr.GetMethodType())
            {
                case MethodType.Unary:
                    dict.Add(descr.name, new Func<TReq, Task<TResp>>(x=>InvokeUnary(method,x)));
                    break;
                case MethodType.ClientStreaming:
                    dict.Add(descr.name, new Func<IObservable<TReq>, Task<TResp>>(x => InvokeClientStreaming(method, x)));
                    break;
                case MethodType.ServerStreaming:
                    dict.Add(descr.name, new Func<TReq, IObservable<TResp>>(x => InvokeServerStreaming(method, x)));
                    break;
                case MethodType.DuplexStreaming:
                    dict.Add(descr.name, new Func<IObservable<TReq>, IObservable<TResp>>(x => InvokeDuplexStreaming(method, x)));
                    break;
                default:
                    break;
            }

            
        }


        public IObservable<TResp> InvokeDuplexStreaming<TReq, TResp>(Method<TReq, TResp> method, IObservable<TReq> input)
            where TReq : class
            where TResp : class
        {
            var call = invoker.AsyncDuplexStreamingCall<TReq, TResp>(method, null, new CallOptions { });
            var cancel = input.Write(call.RequestStream);

            return call.ResponseStream.ToObservable();
        }

        public IObservable<TResp> InvokeServerStreaming<TReq, TResp>(Method<TReq, TResp> method, TReq input)
            where TReq : class
            where TResp : class
        {
            var call = invoker.AsyncServerStreamingCall<TReq, TResp>(method, null, new CallOptions { }, input);

            return call.ResponseStream.ToObservable();
        }

        public async Task<TResp> InvokeClientStreaming<TReq, TResp>(Method<TReq, TResp> method, IObservable<TReq> input)
            where TReq : class
            where TResp : class
        {
            var call = invoker.AsyncClientStreamingCall<TReq, TResp>(method, null, new CallOptions { });
            var cancel = input.Write(call.RequestStream);
            var res = await call.ResponseAsync;
            cancel.Dispose();
            return res;
        }

        public async Task<TResp> InvokeUnary<TReq, TResp>(Method<TReq, TResp> method, TReq input)
            where TReq : class
            where TResp : class
        {
            var call = invoker.AsyncUnaryCall<TReq, TResp>(method, null, new CallOptions { }, input);
            var res = await call.ResponseAsync;
            return res;
        }

        
    }
}
