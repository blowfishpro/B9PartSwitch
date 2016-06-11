#if DEBUG

using KSP.Testing;

namespace B9PartSwitch.Test
{
    public class ModuleB9PartSwitchTest : PartModuleTest<ModuleB9PartSwitch>
    {
        [TestInfo("TestFindBestSubtype")]
        public void TestFindBestSubtype()
        {
            PartSubtype subtype1 = new PartSubtype();
            subtype1.subtypeName = "subtype1";
            subtype1.tankType = B9TankSettings.StructuralTankType;
            module.subtypes.Add(subtype1);

            PartSubtype subtype2 = new PartSubtype();
            subtype2.subtypeName = "subtype2";
            subtype2.tankType = B9TankSettings.StructuralTankType;
            module.subtypes.Add(subtype2);

            PartSubtype subtype3 = new PartSubtype();
            subtype3.subtypeName = "subtype3";
            subtype3.tankType = B9TankSettings.StructuralTankType;
            module.subtypes.Add(subtype3);

            module.currentSubtypeName = "subtype2";

            module.OnStart(PartModule.StartState.Editor);

            assertEquals("Should identify the subtype by name", module.CurrentSubtype, subtype2);

            module.currentSubtypeName = null;
            module.currentSubtypeIndex = 2;

            module.OnStart(PartModule.StartState.Editor);

            assertEquals("Should identify the subtype by index", module.CurrentSubtype, subtype3);

            PartResourceDefinition resourceDef1 = new PartResourceDefinition("resource1");
            PartResourceDefinition resourceDef2 = new PartResourceDefinition("resource2");
            PartResourceDefinition resourceDef3 = new PartResourceDefinition("resource3");

            TankResource tankResource1 = new TankResource();
            tankResource1.resourceDefinition = resourceDef1;
            TankResource tankResource2 = new TankResource();
            tankResource2.resourceDefinition = resourceDef1;
            TankResource tankResource3 = new TankResource();
            tankResource3.resourceDefinition = resourceDef2;

            TankType tankType1 = new TankType();
            tankType1.resources.Add(tankResource1);
            subtype1.tankType = tankType1;
            TankType tankType2 = new TankType();
            tankType2.resources.Add(tankResource2);
            tankType2.resources.Add(tankResource3);
            subtype2.tankType = tankType2;

            module.currentSubtypeName = null;
            module.currentSubtypeIndex = -1;
            module.baseVolume = 1f;
            module.OnStart(PartModule.StartState.Editor);

            assertEquals("It identifies a structural subtype when no resources present", module.CurrentSubtype, subtype3);

            PartResource partResource1 = part.AddResource(resourceDef3, 1f, 1f);

            module.currentSubtypeName = null;
            module.currentSubtypeIndex = -1;
            module.OnStart(PartModule.StartState.Editor);

            assertEquals("It identifies a structural subtype when only an unmanaged resource present", module.CurrentSubtype, subtype3);

            PartResource partResource2 = part.AddResource(resourceDef1, 1f, 1f);
            PartResource partResource3 = part.AddResource(resourceDef2, 1f, 1f);

            module.currentSubtypeName = null;
            module.currentSubtypeIndex = -1;
            module.OnStart(PartModule.StartState.Editor);

            assertEquals("It identifies the correct subtype by resources when present", module.CurrentSubtype, subtype2);

            part.RemoveResource(partResource2);

            module.currentSubtypeName = null;
            module.currentSubtypeIndex = -1;
            module.OnStart(PartModule.StartState.Editor);

            assertEquals("It identifies the first subtype when subtype can't be determined from resources", module.CurrentSubtype, subtype1);
        }
    }
}

#endif
