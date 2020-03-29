using System;
using Xunit;
using UnityEngine;
using B9PartSwitch.Fishbones.Parsers;
using B9PartSwitch.Utils;

namespace B9PartSwitchTests.Fishbones.Parsers
{
    public class DefaultValueParseMapTest
    {
        private class DummyClass { }

        private struct DummyStruct { }

        private enum DummyEnum
        {
            ZERO = 0,
            ONE = 1,
            TWO = 2,
        }

        [Fact]
        public void Test__DefaultParsersRegistered()
        {
            Type[] defaultParseTypes =
            {
                typeof(string),
                typeof(bool),
                typeof(char),
                typeof(byte),
                typeof(sbyte),
                typeof(short),
                typeof(ushort),
                typeof(int),
                typeof(uint),
                typeof(long),
                typeof(ulong),
                typeof(float),
                typeof(decimal),
                typeof(double),
                typeof(Vector2),
                typeof(Vector3),
                typeof(Vector4),
                typeof(Quaternion),
                typeof(QuaternionD),
                typeof(Vector3d),
                typeof(Matrix4x4),
                typeof(Color),
                typeof(Color32),
                typeof(IStringMatcher),
                typeof(AttachNode),
                typeof(PartResourceDefinition),
            };

            DefaultValueParseMap map = new DefaultValueParseMap();

            foreach(Type parseType in defaultParseTypes)
            {
                Assert.True(map.CanParse(parseType), $"Expected to be able to parse type '{parseType}' but could not");
                Assert.NotNull(map.GetParser(parseType));
            }
        }

        #region Instance

        [Fact]
        public void TestInstance()
        {
            IValueParseMap instance = DefaultValueParseMap.Instance;

            Assert.NotNull(instance);
            Assert.Null(instance as IMutableValueParseMap);
        }

        #endregion

        #region GetParseType

        [Fact]
        public void TestGetParseType()
        {
            DefaultValueParseMap map = new DefaultValueParseMap();
            IValueParser dummyParser = Exemplars.DummyValueParser<DummyClass>();
            map.AddParser(dummyParser);

            Assert.Same(dummyParser, map.GetParser(typeof(DummyClass)));
        }

        [Fact]
        public void TestGetParseType__Enum()
        {
            DefaultValueParseMap map = new DefaultValueParseMap();

            IValueParser parser = map.GetParser(typeof(DummyEnum));
            Assert.IsType<EnumValueParser>(parser);
            Assert.Same(typeof(DummyEnum), parser.ParseType);
        }

        [Fact]
        public void TestGetParseType__Nullable()
        {
            DefaultValueParseMap map = new DefaultValueParseMap();

            Assert.Same(DefaultValueParseMap.BoolParser, map.GetParser(typeof(bool?)));

            IValueParser dummyParser = Exemplars.DummyValueParser<DummyStruct>();
            map.AddParser(dummyParser);

            Assert.Same(dummyParser, map.GetParser(typeof(DummyStruct?)));
        }

        [Fact]
        public void TestGetParseType__NullableEnum()
        {
            DefaultValueParseMap map = new DefaultValueParseMap();

            IValueParser parser = map.GetParser(typeof(DummyEnum?));
            Assert.IsType<EnumValueParser>(parser);
            Assert.Same(typeof(DummyEnum), parser.ParseType);
        }

        [Fact]
        public void TestGetParseType__RegisteredEnum()
        {
            DefaultValueParseMap map = new DefaultValueParseMap();
            IValueParser dummyParser = Exemplars.DummyValueParser<DummyEnum>();
            map.AddParser(dummyParser);

            Assert.Same(dummyParser, map.GetParser(typeof(DummyEnum)));
            Assert.Same(dummyParser, map.GetParser(typeof(DummyEnum?)));
        }

        [Fact]
        public void TestGetParseType__RegisteredNullable()
        {
            DefaultValueParseMap map = new DefaultValueParseMap();
            IValueParser dummyParser = Exemplars.DummyValueParser<DummyStruct?>();
            map.AddParser(dummyParser);

            Assert.Same(dummyParser, map.GetParser(typeof(DummyStruct?)));
            Assert.Throws<ParseTypeNotRegisteredException>(() => map.GetParser(typeof(DummyStruct)));

            IValueParser dummyParser2 = Exemplars.DummyValueParser<DummyStruct>();
            map.AddParser(dummyParser2);

            Assert.Same(dummyParser, map.GetParser(typeof(DummyStruct?)));
            Assert.Same(dummyParser2, map.GetParser(typeof(DummyStruct)));
        }

        [Fact]
        public void TestGetParseType__RegisteredNullableEnum()
        {
            DefaultValueParseMap map = new DefaultValueParseMap();
            IValueParser dummyParser = Exemplars.DummyValueParser<DummyEnum?>();
            map.AddParser(dummyParser);

            Assert.Same(dummyParser, map.GetParser(typeof(DummyEnum?)));

            IValueParser someParser = map.GetParser(typeof(DummyEnum));
            Assert.IsType<EnumValueParser>(someParser);
            Assert.Same(typeof(DummyEnum), someParser.ParseType);
            Assert.NotSame(dummyParser, someParser);

            IValueParser dummyParser2 = Exemplars.DummyValueParser<DummyEnum>();
            map.AddParser(dummyParser2);

            Assert.Same(dummyParser, map.GetParser(typeof(DummyEnum?)));
            Assert.Same(dummyParser2, map.GetParser(typeof(DummyEnum)));
        }

        #endregion

        #region CanParse

        [Fact]
        public void TestCanParse()
        {
            DefaultValueParseMap map = new DefaultValueParseMap();

            Assert.False(map.CanParse(typeof(DummyClass)));

            map.AddParser(Exemplars.DummyValueParser<DummyClass>());

            Assert.True(map.CanParse(typeof(DummyClass)));
        }

        [Fact]
        public void TestCanParse__Enum()
        {
            DefaultValueParseMap map = new DefaultValueParseMap();
            Assert.True(map.CanParse(typeof(DummyEnum)));
            Assert.True(map.CanParse(typeof(DummyEnum?)));
        }

        [Fact]
        public void TestCanParse__Nullable()
        {
            DefaultValueParseMap map = new DefaultValueParseMap();

            Assert.False(map.CanParse(typeof(DummyStruct)));
            Assert.False(map.CanParse(typeof(DummyStruct?)));

            map.AddParser(Exemplars.DummyValueParser<DummyStruct>());

            Assert.True(map.CanParse(typeof(DummyStruct)));
            Assert.True(map.CanParse(typeof(DummyStruct?)));
        }

        [Fact]
        public void TestCanParse__RegisteredEnum()
        {
            DefaultValueParseMap map = new DefaultValueParseMap();
            Assert.True(map.CanParse(typeof(DummyEnum)));
            Assert.True(map.CanParse(typeof(DummyEnum?)));

            map.AddParser(Exemplars.DummyValueParser<DummyEnum>());
            Assert.True(map.CanParse(typeof(DummyEnum)));
            Assert.True(map.CanParse(typeof(DummyEnum?)));

            map.AddParser(Exemplars.DummyValueParser<DummyEnum?>());
            Assert.True(map.CanParse(typeof(DummyEnum)));
            Assert.True(map.CanParse(typeof(DummyEnum?)));
        }

        [Fact]
        public void TestCanParse__RegisteredNullable()
        {
            DefaultValueParseMap map = new DefaultValueParseMap();

            Assert.False(map.CanParse(typeof(DummyStruct)));
            Assert.False(map.CanParse(typeof(DummyStruct?)));

            map.AddParser(Exemplars.DummyValueParser<DummyStruct?>());

            Assert.False(map.CanParse(typeof(DummyStruct)));
            Assert.True(map.CanParse(typeof(DummyStruct?)));
        }

        #endregion
    }
}
