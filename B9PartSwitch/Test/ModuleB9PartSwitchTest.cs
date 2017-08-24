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

            part.partInfo = new AvailablePart(); // So that it thinks it's loading from a save

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

        [TestInfo("ModuleB9PartSwitch - Load - Find best subtype by name")]
        public void TestLoad__FindBestSubtype__ByName()
        {
            Reset();

            ConfigNode node = new ConfigNode();
            node.AddValue("currentSubtype", "subtype2");
            
            module.Load(node);

            assertEquals("Should identify the subtype by name", module.CurrentSubtype, subtype2);
        }

        [TestInfo("ModuleB9PartSwitch - Load - Find best subtype by name (old field)")]
        public void TestLoad__FindBestSubtype__ByNameOldField()
        {
            Reset();

            ConfigNode node = new ConfigNode();
            node.AddValue("currentSubtypeName", "subtype2");

            module.Load(node);

            assertEquals("Should identify the subtype by name", module.CurrentSubtype, subtype2);
        }

        [TestInfo("ModuleB9PartSwitch - Load - Find best subtype by index")]
        public void TestLoad__FindBestSubtype__ByIndex()
        {
            Reset();

            ConfigNode node = new ConfigNode();
            node.AddValue("currentSubtypeIndex", "2");

            module.Load(node);

            assertEquals("Should identify the subtype by index", module.CurrentSubtype, subtype3);
        }

        [TestInfo("ModuleB9PartSwitch - Load - Find best subtype - structural when no resources present")]
        public void TestLoad__FindBestSubtype__StructuralWhenNoResources()
        {
            Reset();
            
            module.Load(new ConfigNode());

            assertEquals("It identifies a structural subtype when no resources present", module.CurrentSubtype, subtype3);
        }

        [TestInfo("ModuleB9PartSwitch - Load - Find best subtype - structural when only unmanaged resources present")]
        public void TestLoad__FindBestSubtype__UnmanagedResources()
        {
            Reset();

            part.AddResource(resourceDef3, 1f, 1f);

            module.Load(new ConfigNode());

            assertEquals("It identifies a structural subtype when only an unmanaged resource present", module.CurrentSubtype, subtype3);
        }

        [TestInfo("ModuleB9PartSwitch - Load - Find best subtype - correct when resources present")]
        public void TestLoad__FindBestSubtype__ManagedResources()
        {
            Reset();

            part.AddResource(resourceDef1, 1f, 1f);
            part.AddResource(resourceDef2, 1f, 1f);
            part.AddResource(resourceDef3, 1f, 1f);

            module.Load(new ConfigNode());

            assertEquals("It identifies the correct subtype by resources when present", module.CurrentSubtype, subtype2);
        }

        [TestInfo("ModuleB9PartSwitch - Load - First subtype when subtype can't be determined from resources")]
        public void TestLoad__FindBestSubtype__UnknownResources()
        {
            Reset();

            part.AddResource(resourceDef2, 1f, 1f);
            part.AddResource(resourceDef3, 1f, 1f);

            module.Load(new ConfigNode());

            assertEquals("It identifies the first subtype when subtype can't be determined from resources", module.CurrentSubtype, subtype1);
        }

        private void Reset()
        {
            module.currentSubtypeIndex = -1;

            part.Resources.Clear();
        }
    }
}

#endif
