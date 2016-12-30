using System;
using System.Reflection;
using Xunit;
using B9PartSwitch.Fishbones.FieldWrappers;

namespace B9PartSwitchTests.Fishbones.FieldWrappers
{
    public class PropertyWrapperTest
    {
        private class DummyClass
        {
            public bool b1 { get; set; }
            public bool b2 { private get; set; }
            public bool b3 { get; private set; } = false;
            public bool b4 { get { return true; } }
            public bool b5 { set { } }
        }

        private static readonly PropertyWrapper wrapper = new PropertyWrapper(typeof(DummyClass).GetProperty(nameof(DummyClass.b1)));

        [Fact]
        public void TestNew__Null()
        {
            Assert.Throws<ArgumentNullException>(() => new PropertyWrapper(null));
        }

        [Fact]
        public void TestNew__OnlyGetter()
        {
            PropertyInfo property = typeof(DummyClass).GetProperty(nameof(DummyClass.b4));
            Assert.Throws<ArgumentException>(() => new PropertyWrapper(property));
        }

        [Fact]
        public void TestNew__OnlySetter()
        {
            PropertyInfo property = typeof(DummyClass).GetProperty(nameof(DummyClass.b5));
            Assert.Throws<ArgumentException>(() => new PropertyWrapper(property));
        }

        [Fact]
        public void TestGetValue()
        {
            DummyClass c = new DummyClass { b1 = true };
            Assert.Equal(true, wrapper.GetValue(c));
        }

        [Fact]
        public void TestGetValue__Private()
        {
            PropertyInfo property = typeof(DummyClass).GetProperty(nameof(DummyClass.b2));
            PropertyWrapper wrapper2 = new PropertyWrapper(property);
            DummyClass c = new DummyClass { b1 = true };

            Assert.Equal(true, wrapper.GetValue(c));
        }

        [Fact]
        public void TestGetValue__Null()
        {
            Assert.Throws<ArgumentNullException>(() => wrapper.GetValue(null));
        }

        [Fact]
        public void TestSetValue()
        {
            DummyClass c = new DummyClass { b1 = false };

            wrapper.SetValue(c, true);
            Assert.Equal(true, c.b1);
        }

        [Fact]
        public void TestSetValue__Private()
        {
            PropertyInfo property = typeof(DummyClass).GetProperty(nameof(DummyClass.b3));
            PropertyWrapper wrapper2 = new PropertyWrapper(property);
            DummyClass c = new DummyClass();

            wrapper.SetValue(c, true);
            Assert.Equal(true, c.b1);
        }

        [Fact]
        public void TestSetValue__Null()
        {
            Assert.Throws<ArgumentNullException>(() => wrapper.SetValue(null, true));
        }
    }
}
