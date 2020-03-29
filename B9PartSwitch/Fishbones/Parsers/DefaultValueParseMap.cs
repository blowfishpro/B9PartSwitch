using System;
using System.Globalization;
using UnityEngine;
using B9PartSwitch.Utils;

namespace B9PartSwitch.Fishbones.Parsers
{
    public class DefaultValueParseMap : ValueParseMap
    {
        public static readonly IValueParser StringParser = new ValueParser<string>(s => s, s => s);
        public static readonly IValueParser BoolParser = new ValueParser<bool>(bool.Parse, x => x.ToString(CultureInfo.InvariantCulture));
        public static readonly IValueParser CharParser = new ValueParser<char>(char.Parse, x => x.ToString(CultureInfo.InvariantCulture));
        public static readonly IValueParser ByteParser = new ValueParser<byte>(byte.Parse, x => x.ToString(CultureInfo.InvariantCulture));
        public static readonly IValueParser SByteParser = new ValueParser<sbyte>(sbyte.Parse, x => x.ToString(CultureInfo.InvariantCulture));
        public static readonly IValueParser ShortParser = new ValueParser<short>(short.Parse, x => x.ToString(CultureInfo.InvariantCulture));
        public static readonly IValueParser UShortParser = new ValueParser<ushort>(ushort.Parse, x => x.ToString(CultureInfo.InvariantCulture));
        public static readonly IValueParser IntParser = new ValueParser<int>(int.Parse, x => x.ToString(CultureInfo.InvariantCulture));
        public static readonly IValueParser UIntParser = new ValueParser<uint>(uint.Parse, x => x.ToString(CultureInfo.InvariantCulture));
        public static readonly IValueParser LongParser = new ValueParser<long>(long.Parse, x => x.ToString(CultureInfo.InvariantCulture));
        public static readonly IValueParser ULongParser = new ValueParser<ulong>(ulong.Parse, x => x.ToString(CultureInfo.InvariantCulture));
        public static readonly IValueParser FloatParser = new ValueParser<float>(float.Parse, x => x.ToString(CultureInfo.InvariantCulture));
        public static readonly IValueParser DecimalParser = new ValueParser<decimal>(decimal.Parse, x => x.ToString(CultureInfo.InvariantCulture));
        public static readonly IValueParser DoubleParser = new ValueParser<double>(double.Parse, x => x.ToString(CultureInfo.InvariantCulture));

        public static readonly IValueParser Vector2Parser = new ValueParser<Vector2>(ConfigNode.ParseVector2, ConfigNode.WriteVector);
        public static readonly IValueParser Vector3Parser = new ValueParser<Vector3>(ConfigNode.ParseVector3, ConfigNode.WriteVector);
        public static readonly IValueParser Vector4Parser = new ValueParser<Vector4>(ConfigNode.ParseVector4, ConfigNode.WriteVector);
        public static readonly IValueParser QuaternionParser = new ValueParser<Quaternion>(ConfigNode.ParseQuaternion, ConfigNode.WriteQuaternion);
        public static readonly IValueParser QuaternionDParser = new ValueParser<QuaternionD>(ConfigNode.ParseQuaternionD, ConfigNode.WriteQuaternion);
        public static readonly IValueParser Vector3dParser = new ValueParser<Vector3d>(ConfigNode.ParseVector3D, ConfigNode.WriteVector);
        public static readonly IValueParser Matrix4x4Parser = new ValueParser<Matrix4x4>(ConfigNode.ParseMatrix4x4, ConfigNode.WriteMatrix4x4);
        public static readonly IValueParser ColorParser = new ValueParser<Color>(Utils.ColorParser.Parse, ConfigNode.WriteColor);
        public static readonly IValueParser Color32Parser = new ValueParser<Color32>(ConfigNode.ParseColor32, ConfigNode.WriteColor);

        public static readonly IValueParser StringMatcherParser = new ValueParser<IStringMatcher>(StringMatcher.Parse, s => s.ToString());

        public static readonly IValueParser AttachNodeParser = new AttachNodeValueParser();
        public static readonly IValueParser PartResourceDefinitionParser = new PartResourceDefinitionValueParser();

        public static readonly IValueParseMap Instance = new ValueParseMapWrapper(new DefaultValueParseMap());

        public DefaultValueParseMap()
        {
            AddParser(StringParser);
            AddParser(BoolParser);
            AddParser(CharParser);
            AddParser(ByteParser);
            AddParser(SByteParser);
            AddParser(ShortParser);
            AddParser(UShortParser);
            AddParser(IntParser);
            AddParser(UIntParser);
            AddParser(LongParser);
            AddParser(ULongParser);
            AddParser(FloatParser);
            AddParser(DecimalParser);
            AddParser(DoubleParser);

            AddParser(Vector2Parser);
            AddParser(Vector3Parser);
            AddParser(Vector4Parser);
            AddParser(QuaternionParser);
            AddParser(QuaternionDParser);
            AddParser(Vector3dParser);
            AddParser(Matrix4x4Parser);
            AddParser(ColorParser);
            AddParser(Color32Parser);

            AddParser(StringMatcherParser);

            AddParser(AttachNodeParser);
            AddParser(PartResourceDefinitionParser);
        }

        public override IValueParser GetParser(Type parseType)
        {
            parseType.ThrowIfNullArgument(nameof(parseType));

            if (IsRegisteredParserOrEnum(parseType))
                return GetRegisteredParserOrEnum(parseType);

            if (parseType.IsNullableValueType())
            {
                Type nullableArgument = parseType.GetGenericArguments()[0];
                if (IsRegisteredParserOrEnum(nullableArgument))
                    return GetRegisteredParserOrEnum(nullableArgument);
            }

            throw new ParseTypeNotRegisteredException(parseType);
        }

        public override bool CanParse(Type parseType)
        {
            parseType.ThrowIfNullArgument(nameof(parseType));

            if (IsRegisteredParserOrEnum(parseType))
                return true;
            else if (parseType.IsNullableValueType() && IsRegisteredParserOrEnum(parseType.GetGenericArguments()[0]))
                return true;
            else
                return false;
        }

        private IValueParser GetRegisteredParserOrEnum(Type parseType)
        {
            if (base.CanParse(parseType))
                return base.GetParser(parseType);
            else if (parseType.IsEnum)
                return new EnumValueParser(parseType);

            throw new NotImplementedException();
        }

        private bool IsRegisteredParserOrEnum(Type parseType)
        {
            return base.CanParse(parseType) || parseType.IsEnum;
        }
    }
}
