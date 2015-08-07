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

            RegisterParseType<Vector2d>(x => ParseVectorDType<Vector2d>(x, 2), null);
            RegisterParseType<Vector3d>(x => ParseVectorDType<Vector3d>(x, 3), null);
            RegisterParseType<Vector4d>(x => ParseVectorDType<Vector4d>(x, 4), null);
        }

        public static bool ParseTypeRegistered<T>()
        {
            return ParseTypeRegistered(typeof(T));
        }

        public static bool ParseTypeRegistered(Type type)
        {
            return type == typeof(string) || type.IsEnum || ParseTypes.ContainsKey(type);
        }

        public static void RegisterParseType<T>(Func<string, T> parseFunction, Func<T, string> formatFunction)
        {
            Type type = typeof(T);
            if (ParseTypeRegistered(type))
                throw new ArgumentException("The type " + type.Name + " is already a registered parse type");
            if (!type.IsUnitySerializableType())
                Debug.LogWarning("The type '" + type.Name + "' is being registed as a config parse type, but is not a Unity-serializable type.  Unexpected results may occur.");
            ParseTypes.Add(type, new ParseType<T>(parseFunction, formatFunction));
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
                return value;
            }
            else if (ParseTypeRegistered(type))
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

        public static string FormatConfigValue(object value)
        {
            Type type = value.GetType();

            if (value is string)
            {
                return value as string;
            }
            else if (ParseTypeRegistered(type))
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

        public static bool IsList(this object o)
        {
            if (o == null)
                return false;
            return IsListType(o.GetType());
        }

        public static bool IsListType(this Type t)
        {
            if (t == null)
                return false;
            return t.GetInterfaces().Contains(typeof(IList)) && t.IsGenericType && t.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        public static bool IsUnitySerializableType(this Type t)
        {
            if (t == null)
                return false;
            if (t.IsPrimitive) return true;
            if (t == typeof(string)) return true;
            if (t.IsSubclassOf(typeof(UnityEngine.Object))) return true;
            if (t.IsListType() && t.GetGenericArguments()[0].IsUnitySerializableType()) return true;
            if (t.IsArray && t.GetElementType().IsUnitySerializableType()) return true;

            // Unity serializable types
            if (t == typeof(Vector2)) return true;
            if (t == typeof(Vector3)) return true;
            if (t == typeof(Vector4)) return true;
            if (t == typeof(Quaternion)) return true;
            if (t == typeof(Matrix4x4)) return true;
            if (t == typeof(Color)) return true;
            if (t == typeof(Rect)) return true;
            if (t == typeof(LayerMask)) return true;

            // Serializable attribute
            object[] attributes = t.GetCustomAttributes(false);
            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i] is SerializableAttribute)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public interface IParseType
    {
        object Parse(string s);
        string Format(object o);
    }

    public class ParseType<T> : IParseType
    {
        public readonly Func<string, T> parseFunction;
        public readonly Func<T, string> formatFunction;

        public ParseType(Func<string, T> parseFunction, Func<T, string> formatFunction)
        {
            this.parseFunction = parseFunction;
            this.formatFunction = formatFunction;
        }

        public object Parse(string s)
        {
            return parseFunction(s);
        }

        public string Format(object o)
        {
            if (!(o is T))
                throw new ArgumentException("Object to format must be of type " + typeof(T).Name);
            if (formatFunction != null)
                return formatFunction((T)o);
            else
                return o.ToString();
        }
    }

    public abstract class CFGUtilObject : MonoBehaviour, IConfigNode
    {
        protected ConfigFieldList configFieldList;

        public void Awake()
        {
            CreateFieldList();
            OnAwake();
        }

        protected void CreateFieldList()
        {
            configFieldList = new ConfigFieldList(this);
        }

        public void Load(ConfigNode node)
        {
            configFieldList.Load(node);
            OnLoad(node);
        }

        public void Save(ConfigNode node)
        {
            configFieldList.Save(node);
            OnSave(node);
        }

        virtual public void OnAwake()
        {

        }

        virtual public void OnLoad(ConfigNode node)
        {

        }

        virtual public void OnSave(ConfigNode node)
        {

        }
    }

    public abstract class CFGUtilPartModule : PartModule
    {
        protected ConfigFieldList configFieldList;

        public override void OnAwake()
        {
            base.OnAwake();

            configFieldList = new ConfigFieldList(this);
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            configFieldList.Load(node);
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            configFieldList.Save(node);
        }
    }
}
