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
            // access or add new value to the cache
            bool firstEntry = false;
            var retrieved = _cache.GetOrAdd(typeof(T), t =>
            {
                var fromResource = _resource.GetResource();
                var timeQueue = new Queue<DateTime>();
                timeQueue.Enqueue(DateTime.Now);
                firstEntry = true;
                return (fromResource, timeQueue);
            });

            // move the cache window
            while (!firstEntry && (checkTime.Subtract(retrieved.Item2.Peek()) > _timeOut) )
            {
                // remove element if older than window
                retrieved.Item2.TryDequeue(out _);
            }

            if (firstEntry || retrieved.Item2.Count >= _maxCalls)
            {
                return (T)retrieved.Item1;
            }
            else
            {
                var fromResource = _resource.GetResource();
                retrieved.Item2.Enqueue(checkTime);

                _cache[typeof(T)] =  (fromResource, retrieved.Item2);
                return fromResource;
            }
        }
    }
}
