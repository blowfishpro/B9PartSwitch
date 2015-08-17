using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using KSP;

namespace B9PartSwitch
{
    public static class CFGUtil
    {
        private static readonly Dictionary<Type, IParseType> ParseTypes = new Dictionary<Type, IParseType>();

        static CFGUtil()
        {
            RegisterParseType<bool>(bool.Parse, null);
            RegisterParseType<byte>(byte.Parse, null);
            RegisterParseType<ushort>(ushort.Parse, null);
            RegisterParseType<char>(char.Parse, null);
            RegisterParseType<ulong>(ulong.Parse, null);
            RegisterParseType<int>(int.Parse, null);
            RegisterParseType<long>(long.Parse, null);
            RegisterParseType<float>(float.Parse, null);
            RegisterParseType<uint>(uint.Parse, null);
            RegisterParseType<short>(short.Parse, null);
            RegisterParseType<double>(double.Parse, null);

            RegisterParseType<Vector2>(x => ParseVectorType<Vector2>(x, 2), x => x.ToString(null));
            RegisterParseType<Vector3>(x => ParseVectorType<Vector3>(x, 3), x => x.ToString(null));
            RegisterParseType<Vector4>(x => ParseVectorType<Vector4>(x, 4), x => x.ToString(null));
            RegisterParseType<Quaternion>(x => ParseVectorType<Quaternion>(x, 4), x => x.ToString(null));

            RegisterParseType<AttachNode>(ParseAttachNode, x => x.Format()); 

            // Not serializable
            // RegisterParseType<Vector2d>(x => ParseVectorDType<Vector2d>(x, 2), null);
            // RegisterParseType<Vector3d>(x => ParseVectorDType<Vector3d>(x, 3), null);
            // RegisterParseType<Vector4d>(x => ParseVectorDType<Vector4d>(x, 4), null);
        }

        public static bool IsRegisteredParseType(Type type)
        {
            return ParseTypes.ContainsKey(type);
        }

        public static bool IsParsableType(Type type)
        {
            return type == typeof(string) || type.IsEnum || IsRegisteredParseType(type);
        }

        public static void RegisterParseType<T>(Func<string, T> parseFunction, Func<T, string> formatFunction)
        {
            Type type = typeof(T);
            if (IsParsableType(type))
            {
                Debug.LogError("The type " + type.Name + " is already a registered parse type");
                return;
            }
            if (!type.IsUnitySerializableType())
                Debug.LogWarning("The type '" + type.Name + "' is being registed as a config parse type, but is not a Unity-serializable type.  Unexpected results may occur.");
            ParseTypes.Add(type, new ParseType<T>(parseFunction, formatFunction));

            Debug.Log("CFGUtil: Registered parse type " + type.Name);
        }

        public static ParseType<T> GetParseType<T>()
        {
            Type type = typeof(T);
            if (ParseTypes.ContainsKey(type))
                return ParseTypes[type] as ParseType<T>;
            else
                return null;
        }

        public static object ParseConfigValue(Type type, string value)
        {
            if (value == null)
                throw new ArgumentNullException("Cannot parse a null string");

            if (type == typeof(string))
            {
                return value.Clone();
            }
            else if (IsRegisteredParseType(type))
            {
                return ParseTypes[type].Parse(value);
            }
            else if (type.IsEnum)
            {
                try
                {
                    return Enum.Parse(type, value);
                }
                catch (Exception e)
                {
                    throw new FormatException("Cannot parse enum value of type " + type.Name + " from string '" + value + "': ", e);
                }
            }
            else
            {
                throw new NotImplementedException("No format converter has been implemented to parse type " + type.Name + " from string");
            }
        }

        public static void AssignConfigObject(ConfigFieldInfo info, string value, ref object result)
        {
            object parseResult;

            if (info.Attribute.parseFunction != null)
                parseResult = info.Attribute.parseFunction(value);
            else if (info.IsRegisteredParseType)
                parseResult = CFGUtil.ParseConfigValue(info.RealType, value);
            else
                throw new ArgumentException("Cannot find a way to parse field " + info.Name + " of type " + info.RealType + " from a config value");

            if (parseResult == null)
                return;

            if (info.IsCopyFieldsType)
            {
                if (result == null)
                {
                    if (info.IsComponentType)
                        result = info.Instance.gameObject.AddComponent(info.RealType);
                    else if (info.IsScriptableObjectType)
                        result = ScriptableObject.CreateInstance(info.RealType);
                    else if (info.Constructor != null)
                        result = info.Constructor.Invoke(null);
                    else
                    {
                        Debug.LogWarning("Field " + info.Name + " is ICopyFields, but the value is null and no default constructor could be found.  It will be assigned by referece rather than copying");
                        result = parseResult;
                        return;
                    }
                }

                (result as ICopyFields).CopyFrom(parseResult as ICopyFields);
            }
            else
            {
                result = parseResult;
                return;
            }
        }

        public static void AssignConfigObject(ConfigFieldInfo info, ConfigNode value, ref IConfigNode result)
        {
            if (!info.IsConfigNodeType)
                throw new ArgumentException("Field is not a ConfigNode type: " + info.Name + " (type is " + info.RealType.Name + ")");
            if (result == null)
            {
                if (info.IsComponentType)
                    result = info.Instance.gameObject.AddComponent(info.RealType) as IConfigNode;
                else if (info.IsScriptableObjectType)
                    result = ScriptableObject.CreateInstance(info.RealType) as IConfigNode;
                else if (info.Constructor != null)
                    result = info.Constructor.Invoke(null) as IConfigNode;
                else
                {
                    Debug.LogError("Error: Field " + info.Name + " is IConfigNode, but the value is null and no default constructor could be found.  It will be null");
                    result = null;
                    return;
                }
            }

            result.Load(value);
        }

        public static string FormatConfigValue(object value)
        {
            Type type = value.GetType();
            if (value is string)
            {
                return value as string;
            }
            else if (IsRegisteredParseType(type))
            {
                return ParseTypes[type].Format(value);
            }
            else if (type.IsEnum)
            {
                return Enum.Format(type, value, null);
            }
            else
            {
                return value.ToString();
            }
        }

        public static T ParseVectorType<T>(string value, ushort vectorLength)
        {
            Type type = typeof(T);
            string[] splitStr = value.Split(new char[] { '(', ',', ')' }, vectorLength);
            if (splitStr.Length != vectorLength)
                throw new FormatException("Attempting to parse " + type.Name + ": Expected " + vectorLength.ToString() + " values, but got " + splitStr.Length);
            object[] floatValues = new object[vectorLength];
            Type[] constructorArgs = new Type[vectorLength];
            for (int i = 0; i < vectorLength; i++)
            {
                try
                {
                    floatValues[i] = float.Parse(splitStr[i]);
                }
                catch (FormatException e)
                {
                    throw new FormatException("Cannot parse float from string '" + value + "': ", e);
                }
                constructorArgs[i] = typeof(float);
            }
            ConstructorInfo constructor = type.GetConstructor(constructorArgs);
            if (constructor == null)
                throw new MissingMethodException("Cannot find a constructor for type " + type.Name + " that takes " + vectorLength.ToString() + " floats as arguments");

            return (T)constructor.Invoke(floatValues);
        }

        public static T ParseVectorDType<T>(string value, ushort vectorLength)
        {
            Type type = typeof(T);
            string[] splitStr = value.Split(new char[] { '[', ',', ']' }, vectorLength);
            if (splitStr.Length != vectorLength)
                throw new FormatException("Attempting to parse " + type.Name + ": Expected " + vectorLength.ToString() + " values, but got " + splitStr.Length);
            object[] floatValues = new object[vectorLength];
            Type[] constructorArgs = new Type[vectorLength];
            for (int i = 0; i < vectorLength; i++)
            {
                try
                {
                    floatValues[i] = double.Parse(splitStr[i]);
                }
                catch (FormatException e)
                {
                    throw new FormatException("Cannot parse float from string '" + value + "': ", e);
                }
                constructorArgs[i] = typeof(double);
            }
            ConstructorInfo constructor = type.GetConstructor(constructorArgs);
            if (constructor == null)
                throw new MissingMethodException("Cannot find a constructor for type " + type.Name + " that takes " + vectorLength.ToString() + " floats as arguments");

            return (T)constructor.Invoke(floatValues);
        }

        public static AttachNode ParseAttachNode(string value)
        {
            string[] splitStr = value.Split(new char[] { ',' });
            int length = splitStr.Length;
            float[] floatValues = new float[6];

            if (length < 6)
            {
                throw new FormatException("Not enough values to parse an AttachNode: '" + value + "'");
            }

            for (int i = 0; i < 6; i++)
            {
                floatValues[i] = float.Parse(splitStr[i]);
            }

            AttachNode attachNode = new AttachNode();

            attachNode.id = "parsed-attach-node";

            attachNode.position = new Vector3(floatValues[0], floatValues[1], floatValues[2]);
            attachNode.orientation = new Vector3(floatValues[3], floatValues[4], floatValues[5]);

            attachNode.originalPosition = attachNode.position;
            attachNode.originalOrientation = attachNode.orientation;

            if (length > 6)
            {
                attachNode.size = int.Parse(splitStr[6]);

                if (length == 8)
                    attachNode.attachMethod = (AttachNodeMethod)int.Parse(splitStr[7]);

                if (length > 8)
                    Debug.LogWarning("Too many values encountered to parse an AttachNode: values after 8 will be ignored: '" + value + "'");
            }

            return attachNode;
        }

        public static string Format(this AttachNode node)
        {
            string outStr = "";
            outStr += node.position.ToString(null);
            outStr += ", ";
            outStr += node.orientation.ToString(null);
            outStr += ", ";
            outStr += node.size.ToString();
            outStr += ", ";
            outStr += Enum.Format(typeof(AttachNodeMethod), node.attachMethod, "d");

            return outStr;
        }
    }
}
