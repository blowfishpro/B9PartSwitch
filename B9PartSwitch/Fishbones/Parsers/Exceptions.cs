using System;

namespace B9PartSwitch.Fishbones.Parsers
{
    public class ParseTypeNotRegisteredException : Exception
    {
        public ParseTypeNotRegisteredException(Type parseType) :
            base($"Attempted to get the parser for type '{parseType}', but it has not been registered")
        {
            parseType.ThrowIfNullArgument(nameof(parseType));
        }
    }

    public class ParseTypeAlreadyRegisteredException : Exception
    {
        public ParseTypeAlreadyRegisteredException(Type parseType) :
            base($"Attempted to register perser for type '{parseType}', but it has already been registered")
        {
            parseType.ThrowIfNullArgument(nameof(parseType));
        }
    }
}
