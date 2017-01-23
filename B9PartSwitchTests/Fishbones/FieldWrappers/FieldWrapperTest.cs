using System;
using System.Reflection;
using Xunit;
using B9PartSwitch.Fishbones.FieldWrappers;

namespace B9PartSwitchTests.Fishbones.FieldWrappers
{
    public class FieldWrapperTest
    {
        private class DummyClass
        {
            public bool b;
            public int i = 0;
        }

        private class DummyDerivedClass : DummyClass { }

        private static readonly FieldWrapper wrapper = new FieldWrapper(typeof(DummyClass).GetField(nameof(DummyClass.b)));

        [Fact]
        public void TestNew__Null()
        {
            Assert.Throws<ArgumentNullException>(() => new FieldWrapper(null));
        }

        [Fact]
        public void TestGetValue()
        {
            DummyClass dummy = new DummyClass { b = true };

            Assert.Equal(true, wrapper.GetValue(dummy));
        }

        [Fact]
        public void TestGetValue__Null()
        {
            Assert.Throws<ArgumentNullException>(() => wrapper.GetValue(null));
        }

        [Fact]
        public void TestSetValue()
        {
            DummyClass dummy = new DummyClass { b = false };

            wrapper.SetValue(dummy, true);

            Assert.Equal(true, dummy.b);
        }

        [Fact]
        public void TestSetValue__Null()
        {
            DummyClass dummy = new DummyClass();

            Assert.Throws<ArgumentNullException>(() => wrapper.SetValue(null, true));
        }

        [Fact]
        public void TestFieldType()
        {
            FieldWrapper wrapper1 = new FieldWrapper(typeof(DummyClass).GetField(nameof(DummyClass.b)));
            FieldWrapper wrapper2 = new FieldWrapper(typeof(DummyClass).GetField(nameof(DummyClass.i)));

            Assert.Same(typeof(bool), wrapper1.FieldType);
            Assert.Same(typeof(int), wrapper2.FieldType);
        }

        [Fact]
        public void TestMemberInfo()
        {
            MemberInfo info = typeof(DummyClass).GetField(nameof(DummyClass.b));

            Assert.Same(info, wrapper.MemberInfo);
        }
    }
}
