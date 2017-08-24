#if DEBUG

using KSP.Testing;

namespace B9PartSwitch.Test
{
    public class ModuleB9PartSwitchTest : PartModuleTest<ModuleB9PartSwitch>
    {
        PartResourceDefinition resourceDef1, resourceDef2, resourceDef3;
        private PartSubtype subtype1, subtype2, subtype3;

        public override void TestStartUp()
        {
            base.TestStartUp();

            module.baseVolume = 1;

            resourceDef1 = new PartResourceDefinition("resource1");
            resourceDef2 = new PartResourceDefinition("resource2");
            resourceDef3 = new PartResourceDefinition("resource3");

            TankResource tankResource1 = new TankResource();
            tankResource1.resourceDefinition = resourceDef1;
            TankResource tankResource2 = new TankResource();
            tankResource2.resourceDefinition = resourceDef1;
            TankResource tankResource3 = new TankResource();
            tankResource3.resourceDefinition = resourceDef2;

            TankType tankType1 = new TankType();
            tankType1.resources.Add(tankResource1);
            TankType tankType2 = new TankType();
            tankType2.resources.Add(tankResource2);
            tankType2.resources.Add(tankResource3);

            subtype1 = new PartSubtype();
            subtype1.subtypeName = "subtype1";
            subtype1.tankType = tankType1;
            module.subtypes.Add(subtype1);

            subtype2 = new PartSubtype();
            subtype2.subtypeName = "subtype2";
            subtype2.tankType = tankType2;
            module.subtypes.Add(subtype2);

            subtype3 = new PartSubtype();
            subtype3.subtypeName = "subtype3";
            subtype3.tankType = B9TankSettings.StructuralTankType;
            module.subtypes.Add(subtype3);
        }

        [TestInfo("ModuleB9PartSwitch - OnStart - Find best subtype by name")]
        public void TestOnStart__FindBestSubtype__ByName()
        {
            module.currentSubtypeName = "subtype2";

            module.OnStart(PartModule.StartState.Editor);

            assertEquals("Should identify the subtype by name", module.CurrentSubtype, subtype2);
        }

        [TestInfo("ModuleB9PartSwitch - OnStart - Find best subtype by index")]
        public void TestOnStart__FindBestSubtype__ByIndex()
        {
            module.currentSubtypeName = null;
            module.currentSubtypeIndex = 2;

            module.OnStart(PartModule.StartState.Editor);

            assertEquals("Should identify the subtype by index", module.CurrentSubtype, subtype3);
        }

        [TestInfo("ModuleB9PartSwitch - OnStart - Find best subtype - structural when no resources present")]
        public void TestOnStart__FindBestSubtype__StructuralWhenNoResources()
        {
            module.currentSubtypeName = null;
            module.currentSubtypeIndex = -1;
            module.baseVolume = 1f;
            module.OnStart(PartModule.StartState.Editor);

            assertEquals("It identifies a structural subtype when no resources present", module.CurrentSubtype, subtype3);
        }

        [TestInfo("ModuleB9PartSwitch - OnStart - Find best subtype - structural when only unmanaged resources present")]
        public void TestOnStart__FindBestSubtype__UnmanagedResources()
        {
            PartResource partResource1 = part.AddResource(resourceDef3, 1f, 1f);

            module.currentSubtypeName = null;
            module.currentSubtypeIndex = -1;
            module.OnStart(PartModule.StartState.Editor);

            assertEquals("It identifies a structural subtype when only an unmanaged resource present", module.CurrentSubtype, subtype3);
        }

        [TestInfo("ModuleB9PartSwitch - OnStart - Find best subtype - correct when resources present")]
        public void TestOnStart__FindBestSubtype__ManagedResources()
        {
            PartResource partResource2 = part.AddResource(resourceDef1, 1f, 1f);
            PartResource partResource3 = part.AddResource(resourceDef2, 1f, 1f);

            module.currentSubtypeName = null;
            module.currentSubtypeIndex = -1;
            module.OnStart(PartModule.StartState.Editor);

            assertEquals("It identifies the correct subtype by resources when present", module.CurrentSubtype, subtype2);
        }

        [TestInfo("ModuleB9PartSwitch - OnStart - First subtype when subtype can't be determined from resources")]
        public void TestOnStart__FindBestSubtype__UnknownResources()
        {
            part.Resources.Clear();

            PartResource partResource1 = part.AddResource(resourceDef3, 1f, 1f);
            PartResource partResource3 = part.AddResource(resourceDef2, 1f, 1f);

            module.currentSubtypeName = null;
            module.currentSubtypeIndex = -1;
            module.OnStart(PartModule.StartState.Editor);

            assertEquals("It identifies the first subtype when subtype can't be determined from resources", module.CurrentSubtype, subtype1);
        }
    }
}

#endif
