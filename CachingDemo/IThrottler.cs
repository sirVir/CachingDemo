using System;
using System.Collections.Generic;
using System.Text;

namespace CachingDemo
{
    interface IThrottler 
    {
        IResource<T> Throttle<T>(IResource<T> resource);

        IResource<T> Throttle<T>(IResource<T> resource, int maxCalls, TimeSpan time);
    }
}
