using System;
using System.Reflection;

namespace B9PartSwitch.Fishbones.FieldWrappers
{
    public interface IFieldWrapper
    {
        object GetValue(object subject);
        void SetValue(object subject, object value);
        string Name { get; }
        Type FieldType { get; }
        MemberInfo MemberInfo { get; }
    }
}
