﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using UnityEngine;

namespace B9PartSwitch
{
    public static class CFGUtil
    {
        private static readonly Dictionary<Type, IParseType> ParseTypes = new Dictionary<Type, IParseType>();

        static CFGUtil()
        {
            RegisterParseType<bool>(bool.Parse, x => x.ToString(CultureInfo.InvariantCulture));
            RegisterParseType<char>(char.Parse, x => x.ToString(CultureInfo.InvariantCulture));
            RegisterParseType<byte>(byte.Parse, x => x.ToString(CultureInfo.InvariantCulture));
            RegisterParseType<sbyte>(sbyte.Parse, x => x.ToString(CultureInfo.InvariantCulture));
            RegisterParseType<short>(short.Parse, x => x.ToString(CultureInfo.InvariantCulture));
            RegisterParseType<ushort>(ushort.Parse, x => x.ToString(CultureInfo.InvariantCulture));
            RegisterParseType<int>(int.Parse, x => x.ToString(CultureInfo.InvariantCulture));
            RegisterParseType<uint>(uint.Parse, x => x.ToString(CultureInfo.InvariantCulture));
            RegisterParseType<long>(long.Parse, x => x.ToString(CultureInfo.InvariantCulture));
            RegisterParseType<ulong>(ulong.Parse, x => x.ToString(CultureInfo.InvariantCulture));
            RegisterParseType<float>(float.Parse, x => x.ToString(CultureInfo.InvariantCulture));
            RegisterParseType<decimal>(decimal.Parse, x => x.ToString(CultureInfo.InvariantCulture));
            RegisterParseType<double>(double.Parse, x => x.ToString(CultureInfo.InvariantCulture));

            RegisterParseType<Vector2>(ConfigNode.ParseVector2, ConfigNode.WriteVector);
            RegisterParseType<Vector3>(ConfigNode.ParseVector3, ConfigNode.WriteVector);
            RegisterParseType<Vector4>(ConfigNode.ParseVector4, ConfigNode.WriteVector);
            RegisterParseType<Quaternion>(ConfigNode.ParseQuaternion, ConfigNode.WriteQuaternion);
            RegisterParseType<QuaternionD>(ConfigNode.ParseQuaternionD, ConfigNode.WriteQuaternion);
            RegisterParseType<Vector3d>(ConfigNode.ParseVector3D, ConfigNode.WriteVector);
            RegisterParseType<Matrix4x4>(ConfigNode.ParseMatrix4x4, ConfigNode.WriteMatrix4x4);
            RegisterParseType<Color>(ConfigNode.ParseColor, ConfigNode.WriteColor);
            RegisterParseType<Color32>(ConfigNode.ParseColor32, ConfigNode.WriteColor);

            RegisterParseType<AttachNode>(ParseAttachNode, x => x.Format());
        }

        public static bool IsRegisteredParseType(Type type)
        {
            return ParseTypes.ContainsKey(type);
        }

        public static bool IsConfigParsableType(this Type type)
        {
            return type == typeof(string) || type.IsEnum || IsRegisteredParseType(type);
        }

        public static void RegisterParseType<T>(Func<string, T> parseFunction, Func<T, string> formatFunction)
        {
            Type type = typeof(T);
            if (IsConfigParsableType(type))
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

        public static void AssignConfigObject(ConfigFieldInfo field, string value, ref object result)
        {
            object parseResult;

            // if (field.Attribute.parseFunction != null)
            //     parseResult = field.Attribute.parseFunction(value);
            if (field.IsRegisteredParseType)
                parseResult = CFGUtil.ParseConfigValue(field.RealType, value);
            else
                throw new ArgumentException("Cannot find a way to parse field " + field.Name + " of type " + field.RealType + " from a config value");

            if (parseResult == null)
                return;

            if ((field.IsComponentType || field.IsScriptableObjectType) && field.Attribute.destroy)
                UnityEngine.Object.Destroy(result as UnityEngine.Object);

            result = parseResult;
            return;
        }

        public static void AssignConfigObject(ConfigFieldInfo field, ConfigNode value, ref IConfigNode result)
        {
            if (!field.IsConfigNodeType)
                throw new ArgumentException("Field is not a ConfigNode type: " + field.Name + " (type is " + field.RealType.Name + ")");
            if (result == null)
            {
                if (field.IsComponentType && field.Parent is Component)
                    result = (field.Parent as Component).gameObject.AddComponent(field.RealType) as IConfigNode;
                else if (field.IsScriptableObjectType)
                    result = ScriptableObject.CreateInstance(field.RealType) as IConfigNode;
                else
                {
                    try
                    {
                        result = (IConfigNode)Activator.CreateInstance(field.RealType);
                    }
                    catch(Exception e)
                    {
                        Debug.LogError("Error: Could not load field '" + field.Name + "' because an instance of " + field.RealType.FullName + "could not be created: " + e.Message);
                        return;
                    }
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
            return string.Join(",", new[] {
                node.position.x.ToString(),
                node.position.y.ToString(),
                node.position.z.ToString(),
                node.orientation.x.ToString(),
                node.orientation.y.ToString(),
                node.orientation.z.ToString(),
                node.size.ToString(),
                Enum.Format(typeof(AttachNodeMethod), node.attachMethod, "d")
            });
        }
    }
}
