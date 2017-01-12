using System;
using System.Reflection;

namespace B9PartSwitch.Fishbones.FieldWrappers
{
    public class FieldWrapper : IFieldWrapper
    {
        private readonly FieldInfo field;

        public FieldWrapper(FieldInfo field)
        {
            field.ThrowIfNullArgument(nameof(field));

            this.field = field;
        }

        public object GetValue(object subject)
        {
            subject.ThrowIfNullArgument(nameof(subject));

            return field.GetValue(subject);
        }

        public void SetValue(object subject, object value)
        {
            subject.ThrowIfNullArgument(nameof(subject));

            field.SetValue(subject, value);
        }

        public Type FieldType => field.FieldType;
    }
}
