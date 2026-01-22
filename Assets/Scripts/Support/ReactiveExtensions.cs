using System;
using UniRx;

namespace Support
{
    public static class ReactiveExtensions
    {
        public static IDisposable EmptySubscribe<T>(this IObservable<T> source)
        {
            return source.SafeSubscribe(_ => { });
        }

        public static IDisposable SafeSubscribe<T>(this IObservable<T> source, Action<T> action)
        {
            return source
                .Catch<T, Exception>(exception =>
                {
                    UnityEngine.Debug.LogException(exception);

                    return Observable.Return<T>(default);
                })
                .Subscribe(action);
        }

        public static IDisposable SafeSubscribe<T>(this IObservable<T> source, Action<T> onNext, Action onCompleted)
        {
            return source
                .Catch<T, Exception>(exception =>
                {
                    UnityEngine.Debug.LogException(exception);

                    return Observable.Return<T>(default);
                })
                .Subscribe(onNext, onCompleted);
        }

        public static IObservable<T> SkipInitialization<T>(this IObservable<T> source)
        {
            return source.Skip(1);
        }
    }
}