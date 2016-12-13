namespace B9PartSwitch.Fishbones.Parsers
{
    public interface IValueParser
    {
        object Parse(string value);
        string Format(object value);
    }
}
