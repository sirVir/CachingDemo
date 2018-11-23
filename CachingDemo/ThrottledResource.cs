using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CachingDemo
{
    class ThrottledResource<T> : IResource<T>
    {
        private ConcurrentDictionary<Type, (object, Queue<DateTime>)> _cache;
        private IResource<T> _resource;
        private int _maxCalls;
        private TimeSpan _timeOut;

        public ThrottledResource(ConcurrentDictionary<Type, (object, Queue<DateTime>)> cache, IResource<T> resource, int maxCalls, TimeSpan timeOut)
        {
            _cache = cache;
            _resource = resource;
            _maxCalls = maxCalls;
            _timeOut = timeOut;
        }

        public T GetResource()
        {
            DateTime checkTime = DateTime.Now;

            T toReturn = default(T);

            _cache.AddOrUpdate(typeof(T), t =>
                {
                    toReturn = _resource.GetResource();
                    Queue<DateTime> timeQueue = new Queue<DateTime>();
                    timeQueue.Enqueue(checkTime);
                    return (toReturn, timeQueue);
                },
                (type, retrieved) =>
                {
                    (T cachedValue, Queue<DateTime> slidingWindow) cached = (ValueTuple<T, Queue<DateTime>>)retrieved;

                    while (cached.slidingWindow.TryPeek(out DateTime peeked) && checkTime.Subtract(peeked) > _timeOut)
                    {
                        // remove element if older than window
                        cached.slidingWindow.TryDequeue(out _);
                    }

                    // return cached value if we exceed the count
                    if (cached.slidingWindow.Count >= _maxCalls)
                    {
                        toReturn = cached.cachedValue;
                        return cached;
                    }

                    // call  the resource and save to the cache
                    toReturn = _resource.GetResource();
                    cached.cachedValue = toReturn;
                    cached.slidingWindow.Enqueue(checkTime);
                    return cached;
                }
            );

            return toReturn;
        }
    }
}
