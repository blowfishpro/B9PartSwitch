using System.Collections;
using System;
namespace B9PartSwitchTests
{
    internal class WrappedConfigNode : ConfigNode, IEnumerable
    {
        public void Add(string name, string value) => AddValue(name, value);
        public void Add(string name, ConfigNode node) => AddNode(name, node);

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
