using System;
using Xunit;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitchTests.Fishbones.Context
{
    public class OperationTest
    {
        [Fact]
        public void TestLoadPrefab()
        {
            Assert.True(Operation.LoadPrefab.Loading);
            Assert.False(Operation.LoadPrefab.Saving);
            Assert.True(Operation.LoadPrefab.UseNonPersistentFields);
        }
        
        [Fact]
        public void TestLoadInstance()
        {
            Assert.True(Operation.LoadInstance.Loading);
            Assert.False(Operation.LoadInstance.Saving);
            Assert.True(Operation.LoadInstance.UseNonPersistentFields);
        }

        [Fact]
        public void TestSave()
        {
            Assert.False(Operation.Save.Loading);
            Assert.True(Operation.Save.Saving);
            Assert.False(Operation.Save.UseNonPersistentFields);
        }

        [Fact]
        public void TestDeserialize()
        {
            Assert.True(Operation.Deserialize.Loading);
            Assert.False(Operation.Deserialize.Saving);
            Assert.True(Operation.Deserialize.UseNonPersistentFields);
        }

        [Fact]
        public void TestSerialize()
        {
            Assert.False(Operation.Serialize.Loading);
            Assert.True(Operation.Serialize.Saving);
            Assert.True(Operation.LoadPrefab.UseNonPersistentFields);
        }

        [Fact]
        public void TestLoadUnknown()
        {
            Assert.True(Operation.LoadUnknown.Loading);
            Assert.False(Operation.LoadUnknown.Saving);
            Assert.True(Operation.LoadUnknown.UseNonPersistentFields);
        }

        [Fact]
        public void TestSaveUnknown()
        {
            Assert.False(Operation.SaveUnknown.Loading);
            Assert.True(Operation.SaveUnknown.Saving);
            Assert.True(Operation.SaveUnknown.UseNonPersistentFields);
        }

        [Fact]
        public void TestEquals()
        {
            Operation op = Operation.LoadPrefab;
            Assert.True(op == Operation.LoadPrefab);
            Assert.False(op == Operation.LoadInstance);

            Assert.False(op != Operation.LoadPrefab);
            Assert.True(op != Operation.LoadInstance);

            Assert.True(op.Equals(Operation.LoadPrefab));
            Assert.False(op.Equals(Operation.LoadInstance));

            Assert.True(op.Equals((object)Operation.LoadPrefab));
            Assert.False(op.Equals((object)Operation.LoadInstance));
        }
    }
}
