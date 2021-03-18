using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace B9PartSwitch.Utils
{
    public static class ColorParser
    {
        private static readonly Dictionary<string, Color> namedColors = new Dictionary<string, Color>();

        private static readonly char[] commaSeparator = { ',' };
        private static readonly char[] spaceSeparator = { ' ', '\t' };

        static ColorParser()
        {
            foreach (PropertyInfo propertyInfo in typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                if (!propertyInfo.CanRead) continue;
                if (propertyInfo.PropertyType != typeof(Color)) continue;

                namedColors.Add(propertyInfo.Name, (Color)propertyInfo.GetValue(null, null));
            }

            foreach (PropertyInfo propertyInfo in typeof(XKCDColors).GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                if (!propertyInfo.CanRead) continue;
                if (propertyInfo.PropertyType != typeof(Color)) continue;

                if (namedColors.ContainsKey(propertyInfo.Name)) throw new Exception("duplicate key " + propertyInfo.Name);

                namedColors.Add(propertyInfo.Name, (Color)propertyInfo.GetValue(null, null));
            }

            namedColors.Add("ResourceColorLiquidFuel", ResourceColors.LiquidFuel);
            namedColors.Add("ResourceColorLqdHydrogen", ResourceColors.LqdHydrogen);
            namedColors.Add("ResourceColorLqdMethane", ResourceColors.LqdMethane);
            namedColors.Add("ResourceColorOxidizer", ResourceColors.Oxidizer);
            namedColors.Add("ResourceColorMonoPropellant", ResourceColors.MonoPropellant);
            namedColors.Add("ResourceColorXenonGas", ResourceColors.XenonGas);
            namedColors.Add("ResourceColorElectricChargePrimary", ResourceColors.ElectricChargePrimary);
            namedColors.Add("ResourceColorElectricChargeSecondary", ResourceColors.ElectricChargeSecondary);
            namedColors.Add("ResourceColorOre", ResourceColors.Ore);
        }

        public static Color Parse(string colorStr)
        {
            colorStr.ThrowIfNullArgument(nameof(colorStr));
            if (colorStr == string.Empty) throw new FormatException("Cannot parse empty color");

            if (colorStr[0] == '#')
            {
                switch (colorStr.Length)
                {
                    case 4: // #RGB
                        byte red1 = ParseHex(colorStr[1]);
                        byte green1 = ParseHex(colorStr[2]);
                        byte blue1 = ParseHex(colorStr[3]);
                        return new Color(red1 / 15f, green1 / 15f, blue1 / 15f);

                    case 5: // #RGBA
                        byte red2 = ParseHex(colorStr[1]);
                        byte green2 = ParseHex(colorStr[2]);
                        byte blue2 = ParseHex(colorStr[3]);
                        byte alpha2 = ParseHex(colorStr[4]);
                        return new Color(red2 / 15f, green2 / 15f, blue2 / 15f, alpha2 / 15f);

                    case 7: // #RRGGBB
                        int red3 = (ParseHex(colorStr[1]) << 4) | ParseHex(colorStr[2]);
                        int green3 = (ParseHex(colorStr[3]) << 4) | ParseHex(colorStr[4]);
                        int blue3 = (ParseHex(colorStr[5]) << 4) | ParseHex(colorStr[6]);
                        return new Color(red3 / 255f, green3 / 255f, blue3 / 255f);

                    case 9: // #RRGGBBAA
                        int red4 = (ParseHex(colorStr[1]) << 4) | ParseHex(colorStr[2]);
                        int green4 = (ParseHex(colorStr[3]) << 4) | ParseHex(colorStr[4]);
                        int blue4 = (ParseHex(colorStr[5]) << 4) | ParseHex(colorStr[6]);
                        int alpha4 = (ParseHex(colorStr[7]) << 4) | ParseHex(colorStr[8]);
                        return new Color(red4 / 255f, green4 / 255f, blue4 / 255f, alpha4 / 255f);

                    default:
                        throw new FormatException("Value looks like HTML color (begins with #) but has wrong number of digits (must be 3, 4, 6, or 8): " + colorStr);
                }
            }
            else if (namedColors.TryGetValue(colorStr, out Color namedColor))
            {
                return namedColor;
            }

            string[] splits;

            if (colorStr.IndexOf(',') != -1)
            {
                splits = colorStr.Split(commaSeparator, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < splits.Length; i++)
                {
                    splits[i] = splits[i].Trim();
                }
            }
            else
            {
                splits = colorStr.Split(spaceSeparator, StringSplitOptions.RemoveEmptyEntries);
            }

            if (splits.Length == 3)
            {
                float red = ParseFloatValue(splits[0]);
                float green = ParseFloatValue(splits[1]);
                float blue = ParseFloatValue(splits[2]);
                return new Color(red, green, blue);
            }
            else if (splits.Length == 4)
            {
                float red = ParseFloatValue(splits[0]);
                float green = ParseFloatValue(splits[1]);
                float blue = ParseFloatValue(splits[2]);
                float alpha = ParseFloatValue(splits[3]);
                return new Color(red, green, blue, alpha);
            }
            else
            {
                throw new FormatException("Could not value parse as color: " + colorStr);
            }
        }

        private static byte ParseHex(char value)
        {
            return value switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 2,
                '3' => 3,
                '4' => 4,
                '5' => 5,
                '6' => 6,
                '7' => 7,
                '8' => 8,
                '9' => 9,
                'A' or 'a' => 10,
                'B' or 'b' => 11,
                'C' or 'c' => 12,
                'D' or 'd' => 13,
                'E' or 'e' => 14,
                'F' or 'f' => 15,
                _ => throw new FormatException("Invalid hexadecimal character: " + value),
            };
        }

        private static float ParseFloatValue(string valueStr)
        {
            if (!float.TryParse(valueStr, out float value) || float.IsNaN(value) || value < 0 || value > 1)
                throw new FormatException("Invalid float value when parsing color (should be 0-1): " + valueStr);

            return value;
        }
    }
}
