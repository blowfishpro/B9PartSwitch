using System;
using UnityEngine;

namespace B9PartSwitch.Fishbones.Parsers
{
    public class ScaleParser : ValueParser<Vector3>
    {
        private static readonly char[] splitChars = { ',', '\t', ' ' };

        public static Vector3 ParseScale(string value)
        {
            value.ThrowIfNullArgument(nameof(value));

            string[] splits = value.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);

            if (splits.Length == 1)
            {
                float scale = float.Parse(splits[0]);
                return new Vector3(scale, scale, scale);
            }
            else if (splits.Length == 3)
            {
                float x = float.Parse(splits[0]);
                float y = float.Parse(splits[1]);
                float z = float.Parse(splits[2]);
                return new Vector3(x, y, z);
            }
            else
            {
                throw new FormatException($"Could not parse value as scale because it split into {splits.Length} values: '{value}'");
            }
        }

        public ScaleParser() : base(ParseScale, ConfigNode.WriteVector) { }
    }
}
