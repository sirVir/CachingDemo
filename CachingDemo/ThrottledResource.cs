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
                (type, storedValue) =>
                {
                    while (storedValue.Item2.TryPeek(out DateTime peeked) && checkTime.Subtract(peeked) > _timeOut)
                    {
                        // remove element if older than window
                        storedValue.Item2.TryDequeue(out _);
                    }

                    if (storedValue.Item2.Count >= _maxCalls)
                    {
                        toReturn = (T)storedValue.Item1;
                        return storedValue;
                    }

                    toReturn = _resource.GetResource();
                    storedValue.Item1 = toReturn;
                    storedValue.Item2.Enqueue(checkTime);
                    return storedValue;
                }
            );

            return toReturn;
        }
    }
}
