#if DEBUG

using UnityEngine;
using KSP.Testing;

namespace B9PartSwitch.Test
{
    public abstract class PartModuleTest<T> : UnitTest where T : PartModule
    {
        protected GameObject gameObject;
        protected GameObject gameObject2;
        protected Part part;
        protected T module;

        public override void TestStartUp()
        {
            base.TestStartUp();

            gameObject = new GameObject("part");
            gameObject2 = new GameObject("model");
            gameObject2.transform.parent = gameObject.transform;
            part = gameObject.AddComponent<Part>();
            part.enabled = false;
            module = AddPartModule<T>();
        }

        protected U AddPartModule<U>() where U : PartModule
        {
            U module = gameObject.AddComponent<U>();
            module.enabled = false;
            part.Modules.Add(module);
            return module;
        }

        public override void TestTearDown()
        {
            base.TestTearDown();

            UnityEngine.Object.Destroy(gameObject);
            UnityEngine.Object.Destroy(gameObject2);
        }
    }
}

#endif