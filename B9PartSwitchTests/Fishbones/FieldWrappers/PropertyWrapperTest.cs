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
            public bool B1 { get; set; }
            public bool B2 { private get; set; }
            public bool B3 { get; private set; } = false;
            public bool B4 => true;
            public bool B5 { set { } }

            public int I { get; set; }
        }

        private static readonly PropertyWrapper wrapper = new PropertyWrapper(typeof(DummyClass).GetProperty(nameof(DummyClass.B1)));

        [Fact]
        public void TestNew__Null()
        {
            Assert.Throws<ArgumentNullException>(() => new PropertyWrapper(null));
        }

        [Fact]
        public void TestNew__OnlyGetter()
        {
            PropertyInfo property = typeof(DummyClass).GetProperty(nameof(DummyClass.B4));
            Assert.Throws<ArgumentException>(() => new PropertyWrapper(property));
        }

        [Fact]
        public void TestNew__OnlySetter()
        {
            PropertyInfo property = typeof(DummyClass).GetProperty(nameof(DummyClass.B5));
            Assert.Throws<ArgumentException>(() => new PropertyWrapper(property));
        }

        [Fact]
        public void TestGetValue()
        {
            DummyClass c = new DummyClass { B1 = true };
            Assert.Equal(true, wrapper.GetValue(c));
        }

        [Fact]
        public void TestGetValue__Private()
        {
            PropertyInfo property = typeof(DummyClass).GetProperty(nameof(DummyClass.B2));
            PropertyWrapper wrapper2 = new PropertyWrapper(property);
            DummyClass c = new DummyClass { B2 = true };

            Assert.Equal(true, wrapper2.GetValue(c));
        }

        [Fact]
        public void TestGetValue__Null()
        {
            Assert.Throws<ArgumentNullException>(() => wrapper.GetValue(null));
        }

        [Fact]
        public void TestSetValue()
        {
            DummyClass c = new DummyClass { B1 = false };

            wrapper.SetValue(c, true);
            Assert.True(c.B1);
        }

        [Fact]
        public void TestSetValue__Private()
        {
            PropertyInfo property = typeof(DummyClass).GetProperty(nameof(DummyClass.B3));
            PropertyWrapper wrapper2 = new PropertyWrapper(property);
            DummyClass c = new DummyClass();

            wrapper2.SetValue(c, true);
            Assert.True(c.B3);
        }

        [Fact]
        public void TestSetValue__Null()
        {
            Assert.Throws<ArgumentNullException>(() => wrapper.SetValue(null, true));
        }

        [Fact]
        public void TestName()
        {
            Assert.Equal("B1", wrapper.Name);
        }

        [Fact]
        public void TestFieldType()
        {
            PropertyWrapper wrapper1 = new PropertyWrapper(typeof(DummyClass).GetProperty(nameof(DummyClass.B1)));
            PropertyWrapper wrapper2 = new PropertyWrapper(typeof(DummyClass).GetProperty(nameof(DummyClass.I)));

            Assert.Same(typeof(bool), wrapper1.FieldType);
            Assert.Same(typeof(int), wrapper2.FieldType);
        }

        [Fact]
        public void TestMemberInfo()
        {
            MemberInfo info = typeof(DummyClass).GetProperty(nameof(DummyClass.B1));

            Assert.Same(info, wrapper.MemberInfo);
        }
    }
}
