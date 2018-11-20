using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CachingDemo
{
    public class CachingThrottler : IThrottler
    {
        static ConcurrentDictionary<Type, (object, Queue<DateTime>)> Cache => new ConcurrentDictionary<Type, (object, Queue<DateTime>)>();

        public IResource<T> Throttle<T>(IResource<T> resource, int maxCalls, TimeSpan time)
        {
            return new ThrottledResource<T>(Cache, resource, maxCalls, time);
        }

        public IResource<T> Throttle<T>(IResource<T> resource)
        {
            return Throttle<T>(resource, 5, TimeSpan.FromSeconds(3));
        }
    }
}
