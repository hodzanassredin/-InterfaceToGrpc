using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;

namespace UniformRpc
{
    public static class GrpcExtensions
    {
        public static IDisposable SubscribeAsync<TObj>(this IObservable<TObj> source, Func<TObj, Task> onNext, Action<Exception> onError, Action onCompleted)
        {
            return source.Select(e => Observable.Defer(() => onNext(e).ToObservable())).Concat()
                .Subscribe(
                e => { }, // empty
                onError,
                onCompleted);
        }
        public static IDisposable Write<T>(this IObservable<T> source, IClientStreamWriter<T> writer) {
            return SubscribeAsync(source,
                             onNext: writer.WriteAsync,
                             onError: ex => {

                             },
                             onCompleted: () => writer.CompleteAsync());
        }

        public static IObservable<T> ToObservable<T>(this IAsyncStreamReader<T> reader) {

            return Observable.Create<T>(observer =>
            {
                var isCancelled = false;
                ((Action)async delegate
                {
                    try
                    {
                        while (await reader.MoveNext() && !isCancelled)
                        {
                            observer.OnNext(reader.Current);
                        }
                    }
                    catch (Exception e)
                    {
                        observer.OnError(e);
                    }
                    finally
                    {
                        observer.OnCompleted();
                    }
                })();

                return () =>
                {
                    isCancelled = true;
                };

            });
        }
    }
}
