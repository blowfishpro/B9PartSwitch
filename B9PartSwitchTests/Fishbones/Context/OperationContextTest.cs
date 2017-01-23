using System;
using Xunit;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitchTests.Fishbones.Context
{
    public class OperationContextTest
    {
        [Fact]
        public void Test__Root()
        {
            object o = "an object";
            OperationContext context = new OperationContext(Operation.LoadPrefab, o);

            Assert.Equal(Operation.LoadPrefab, context.Operation);
            Assert.Same(o, context.Subject);
            Assert.Null(context.Parent);
            Assert.Same(o, context.Root);
        }

        [Fact]
        public void Test__Child()
        {
            object o1 = "parent object";
            object o2 = "child object";
            OperationContext parent = new OperationContext(Operation.Save, o1);
            OperationContext child = new OperationContext(parent, o2);

            Assert.Equal(Operation.Save, child.Operation);
            Assert.Same(o2, child.Subject);
            Assert.Same(o1, child.Parent);
            Assert.Same(o1, child.Root);
        }

        [Fact]
        public void Test__ChildChild()
        {
            object o1 = "parent object";
            object o2 = "intermediate object";
            object o3 = "child object";
            OperationContext parent = new OperationContext(Operation.Serialize, o1);
            OperationContext intermediate = new OperationContext(parent, o2);
            OperationContext child = new OperationContext(intermediate, o3);

            Assert.Equal(Operation.Serialize, child.Operation);
            Assert.Same(o3, child.Subject);
            Assert.Same(o2, child.Parent);
            Assert.Same(o1, child.Root);
        }

        [Fact]
        public void TestNew__Parent__Null()
        {
            Assert.Throws<ArgumentNullException>(() => new OperationContext(Operation.Deserialize, null));
        }

        [Fact]
        public void TestNew__Child__Null()
        {
            Assert.Throws<ArgumentNullException>(() => new OperationContext(null, "some object"));
            
            OperationContext context = new OperationContext(Operation.LoadPrefab, "some other object");
            Assert.Throws<ArgumentNullException>(() => new OperationContext(context, null));

            Assert.Throws<ArgumentNullException>(() => new OperationContext(null, null));
        }
    }
}
