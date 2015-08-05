using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B9PartSwitch
{
    public class PartModuleMissingException<T> : Exception where T : PartModule
    {
        public Part Part { get; private set; }
        public PartModuleMissingException(Part part) : base("The part " + part.name + " has no " + typeof(T).Name + " module")
        {
            this.Part = part;
        }
    }

    public class ConfigValueException : Exception
    {
        public ConfigValueException(string message) : base(message) { }
    }
}
