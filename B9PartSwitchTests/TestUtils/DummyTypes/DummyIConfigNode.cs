namespace B9PartSwitchTests.TestUtils.DummyTypes
{
    public class DummyIConfigNode : IConfigNode
    {
        public string value;

        public void Load(ConfigNode node)
        {
            value = node.GetValue("value");
        }

        public void Save(ConfigNode node)
        {
            node.SetValue("value", value, true);
        }
    }
}
