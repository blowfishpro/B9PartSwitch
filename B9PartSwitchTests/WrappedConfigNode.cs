using System.Collections;

namespace B9PartSwitchTests
{
    internal class WrappedConfigNode : ConfigNode, IEnumerable
    {
        public void Add(string name, string value) => AddValue(name, value);

        public IEnumerator GetEnumerator() => values.GetEnumerator();
    }
}
