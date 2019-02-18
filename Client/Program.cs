using Grpc.Core;
using Shared;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using UniformRpc;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            RunAsync().Wait();
        }

        private static async Task RunAsync()
        {
            var channel = new Channel("127.0.0.1", 5000, ChannelCredentials.Insecure);
            var invoker = new DefaultCallInvoker(channel);
            var client = ClientBuilder.Build<IApi>(channel);
            var resStream = client.StreamToStream(Observable.Range(0, 10).Select(x => new Request { Value = x }));
            foreach (var item in resStream.ToEnumerable())
            {
                Console.WriteLine($"Response : {item.Value}");
            }

            Console.WriteLine("Press enter to stop...");
            Console.ReadLine();

            await channel.ShutdownAsync();
        }
    }
}

