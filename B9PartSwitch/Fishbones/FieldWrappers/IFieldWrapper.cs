using System;
using System.Reflection;

namespace B9PartSwitch.Fishbones.FieldWrappers
{
    public interface IFieldWrapper
    {
        object GetValue(object subject);
        void SetValue(object subject, object value);
        Type FieldType { get; }
        MemberInfo MemberInfo { get; }
    }
}
