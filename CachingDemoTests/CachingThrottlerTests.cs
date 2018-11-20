using CachingDemo;
using System;
using System.Threading;
using Xunit;

namespace CachingDemoTests
{
    public class CachingThrottlerTests
    {
        [Theory]
        [InlineData(3, 100, 0,   0, 1, 2, 3, 4, 0, 1, 2, 2, 2)]
        [InlineData(3, 3,   0,   0, 1, 2, 3, 4, 0, 1, 2, 2, 2)]
        [InlineData(3, 1,   0,   0, 1, 2, 3, 4, 0, 1, 2, 2, 2)]
        [InlineData(3, 0,   0,   0, 1, 2, 3, 4, 0, 1, 2, 3, 4)]
        [InlineData(0, 0,   0,   0, 1, 2, 3, 4, 0, 0, 0, 0, 0)]
        [InlineData(1, 0,   0,   0, 1, 2, 3, 4, 0, 1, 2, 3, 4)]
        [InlineData(3, 1,   300, 0, 1, 2, 3, 4, 0, 1, 2, 2, 2)]

        public void CanThrottle(
            int maxElem, int  timeWindowSeconds, int delayBetweenInMs,
            int in1,  int in2,  int in3,  int in4,  int in5,
            int out1, int out2, int out3, int out4, int out5)
        {
            var countingResource = new Moq.Mock<IResource<int>>();
            countingResource.SetupSequence(_ => _.GetResource())
                .Returns(() =>
                {
                    Thread.Sleep(delayBetweenInMs);
                    return in1;
                })
                .Returns(() =>
                {
                    Thread.Sleep(delayBetweenInMs);
                    return in2;
                })
                .Returns(() =>
                {
                    Thread.Sleep(delayBetweenInMs);
                    return in3;
                })
                .Returns(() =>
                {
                    Thread.Sleep(delayBetweenInMs);
                    return in4;
                })
                .Returns(() =>
                {
                    Thread.Sleep(delayBetweenInMs);
                    return in5;
                });

            CachingThrottler throttler = new CachingThrottler();
            var throttled = throttler.Throttle(countingResource.Object, maxElem, TimeSpan.FromSeconds(timeWindowSeconds));

            Assert.Equal(out1, throttled.GetResource());
            Assert.Equal(out2, throttled.GetResource());
            Assert.Equal(out3, throttled.GetResource());
            Assert.Equal(out4, throttled.GetResource());
            Assert.Equal(out5, throttled.GetResource());
        }

        [Fact]
        public void CanThrottleMultipleResources()
        {
            var countingResource = new Moq.Mock<IResource<int>>();
            countingResource.SetupSequence(_ => _.GetResource())
                .Returns(0)
                .Returns(1)
                .Returns(2)
                .Returns(3)
                .Returns(4);

            var stringResource = new Moq.Mock<IResource<string>>();
            stringResource.SetupSequence(_ => _.GetResource())
                .Returns("zero")
                .Returns("one")
                .Returns("two")
                .Returns("three")
                .Returns("four");

            CachingThrottler throttler = new CachingThrottler();
            var throttledCount = throttler.Throttle(countingResource.Object, 1, TimeSpan.FromSeconds(0));
            var throttledString = throttler.Throttle(stringResource.Object, 1, TimeSpan.FromSeconds(0));

            Assert.Equal(0, throttledCount.GetResource());
            Assert.Equal(1, throttledCount.GetResource());
            Assert.Equal(2, throttledCount.GetResource());
            Assert.Equal(3, throttledCount.GetResource());
            Assert.Equal(4, throttledCount.GetResource());

            Assert.Equal("zero", throttledString.GetResource());
            Assert.Equal("one", throttledString.GetResource());
            Assert.Equal("two", throttledString.GetResource());
            Assert.Equal("three", throttledString.GetResource());
            Assert.Equal("four", throttledString.GetResource());
        }
    }
}
