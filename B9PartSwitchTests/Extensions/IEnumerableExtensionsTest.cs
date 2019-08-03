using System;
using Xunit;
using B9PartSwitch;

namespace B9PartSwitchTests
{
    public class IEnumerableExtensionsTest
    {
        private class TestContainer
        {
            public int value;
        }

        [Fact]
        public void TestMaxBy()
        {
            TestContainer[] things =
            {
                new TestContainer { value = 1 },
                new TestContainer { value = 3 },
                new TestContainer { value = 3 },
                new TestContainer { value = 2 },
                new TestContainer { value = 2 },
            };

            Assert.Same(things[1], things.MaxBy(x => x.value));
        }

        [Fact]
        public void TestMaxBy__EmptyEnumerable()
        {

            TestContainer[] things = { };

            Assert.Throws<InvalidOperationException>(delegate
            {
                things.MaxBy(x => x.value);
            });
        }

        [Fact]
        public void TestMaxBy__EnumerableNull()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                IEnumerableExtensions.MaxBy<TestContainer, int>(null, x => x.value);
            });
        }

        [Fact]
        public void TestMaxBy__MapperNull()
        {
            TestContainer[] things =
            {
                new TestContainer { value = 1 },
            };

            Assert.Throws<ArgumentNullException>(delegate
            {
                things.MaxBy<TestContainer, int>(null);
            });
        }
    }
}
