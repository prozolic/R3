﻿namespace R3;

public static partial class EventExtensions
{
    // TODO: TState

    public static Observable<T> Where<T>(this Observable<T> source, Func<T, bool> predicate)
    {
        if (source is Where<T> where)
        {
            // Optimize for Where.Where, create combined predicate.
            var p = where.predicate;
            return new Where<T>(where.source, x => p(x) && predicate(x));
        }

        return new Where<T>(source, predicate);
    }

    public static Observable<T> Where<T>(this Observable<T> source, Func<T, int, bool> predicate)
    {
        return new WhereIndexed<T>(source, predicate);
    }
}

internal sealed class Where<T>(Observable<T> source, Func<T, bool> predicate) : Observable<T>
{
    internal Observable<T> source = source;
    internal Func<T, bool> predicate = predicate;

    protected override IDisposable SubscribeCore(Observer<T> subscriber)
    {
        return source.Subscribe(new _Where(subscriber, predicate));
    }

    class _Where(Observer<T> subscriber, Func<T, bool> predicate) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            if (predicate(value))
            {
                subscriber.OnNext(value);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            subscriber.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            subscriber.OnCompleted(result);
        }
    }
}

internal sealed class WhereIndexed<T>(Observable<T> source, Func<T, int, bool> predicate) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> subscriber)
    {
        return source.Subscribe(new _Where(subscriber, predicate));
    }

    class _Where(Observer<T> subscriber, Func<T, int, bool> predicate) : Observer<T>
    {
        int index = 0;

        protected override void OnNextCore(T value)
        {
            if (predicate(value, index++))
            {
                subscriber.OnNext(value);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            subscriber.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            subscriber.OnCompleted(result);
        }
    }
}
