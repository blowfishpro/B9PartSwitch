using System;

namespace B9PartSwitch
{
    public interface ISubtypePartField
    {
        bool ShouldUseOnSubtype(PartSubtypeContext context);

        void AssignValueOnSubtype(PartSubtypeContext context);

        string Name { get; }
    }

    public class SubtypePartField<T> : ISubtypePartField
    {
        public readonly Func<PartSubtypeContext, T> GetValueFromSubtype;
        public readonly Func<T, bool> ShouldUseValue;
        public readonly Func<Part, T> GetValueFromPart;
        public readonly Action<PartSubtypeContext, T> SetValueOnPart;

        public string Name { get; private set; }

        public SubtypePartField(
            string name,
            Func<PartSubtypeContext, T> getValueFromSubtype,
            Func<T, bool> shouldUseValue,
            Func<Part, T> getValueFromPart,
            Action<PartSubtypeContext, T> setValueOnPart)
        {
            Name = name;
            GetValueFromSubtype = getValueFromSubtype;
            ShouldUseValue = shouldUseValue;
            GetValueFromPart = getValueFromPart;
            SetValueOnPart = setValueOnPart;
        }

        public bool ShouldUseOnSubtype(PartSubtypeContext context) => ShouldUseValue(GetValueFromSubtype(context));

        public void AssignValueOnSubtype(PartSubtypeContext context)
        {
            T value = ShouldUseOnSubtype(context) ? GetValueFromSubtype(context) : GetValueFromPart(context.Part.GetPrefab());
            SetValueOnPart(context, value);
        }
    }
}
