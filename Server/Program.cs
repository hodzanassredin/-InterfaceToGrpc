using Grpc.Core;
using Shared;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using UniformRpc;

namespace Server
{
    public class EchoApi : IApi
    {
        public IObservable<Response> StreamToStream(IObservable<Request> request)
        {
            return request.Select(Map);
        }

        public async Task<Response> Unary(Request request)
        {
            Console.WriteLine($"Unary invoked {request.Value}");
            return Map(request);
        }

        public IObservable<Response> UnaryToStream(Request request)
        {
            Console.WriteLine($"UnaryToStream invoked {request.Value}");
            return Observable.Repeat(Map(request));
        }

        public async Task<Response> StreamToUnary(IObservable<Request> request)
        {
            Console.WriteLine($"StreamToUnary invoked");
            return await request.Select(Map).LastAsync();
        }

        private static Response Map(Request request)
        {
            Console.WriteLine($"Mapping request {request.Value}");
            return new Response { Value = $"Echo: {request.Value}" };
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            RunAsync().Wait();
        }

        private static async Task RunAsync()
        {
            var builder = new ServerBuilder();
            var server = new Grpc.Core.Server()
            {
                Ports = { { "127.0.0.1", 5000, ServerCredentials.Insecure } },
                Services = { builder.GetService<IApi>(new EchoApi()) }
            };

            server.Start();

            Console.WriteLine($"Server started under [127.0.0.1:5000]. Press Enter to stop it...");
            Console.ReadLine();

            await server.ShutdownAsync();
        }
    }
}
}
