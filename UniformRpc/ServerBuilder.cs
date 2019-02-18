using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using static Grpc.Core.Server;
using static UniformRpc.Descriptors;

namespace UniformRpc
{
    public class ServerBuilder : IMethodDesciptorVisitor
    {
        ServerServiceDefinition.Builder builder;
        public ServerBuilder()
        {
            builder = ServerServiceDefinition.CreateBuilder();
        }

        public void Accept<TReq, TResp>(MethodDesciptorTyped<TReq, TResp> desc)
             where TReq : class
             where TResp : class
        {
            builder.AddMethod(desc.GetMethod(), async (requestStream, responseStream, context) =>
            {
                var reqStream = requestStream.ToObservable();


                await requestStream.ForEachAsync(async request =>
                {
                    // handle incoming request
                    // push response into stream
                    await responseStream.WriteAsync(new CustomResponse { Payload = request.Payload });
                });
            });
        }

        public void AddService<T>(T implementation)
        {
            foreach (var desc in Descriptors.GetMethods<T>())
            {
                desc.Visit(this);
            }

        }

        public ServerServiceDefinition Build()
        {
            return builder.Build();
        }

        
    }
}
