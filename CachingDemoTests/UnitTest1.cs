using CachingDemo;
using System;
using Xunit;

namespace CachingDemoTests
{
    public class UnitTest1
    {
        [Fact]
        public void CanThrottleWhenTooManyRequests()
        {
            var countingResource = new Moq.Mock<IResource<int>>();
            countingResource.SetupSequence(_ => _.GetResource())
                .Returns(0)
                .Returns(1)
                .Returns(2)
                .Returns(3)
                .Returns(4);

            CachingThrottler throttler = new CachingThrottler();
            var throttled = throttler.Throttle(countingResource.Object, 3, TimeSpan.FromSeconds(100));

            Assert.Equal(0, throttled.GetResource());
            Assert.Equal(1, throttled.GetResource());
            Assert.Equal(2, throttled.GetResource());
            Assert.Equal(2, throttled.GetResource());
            Assert.Equal(2, throttled.GetResource());
        }
    }
}
