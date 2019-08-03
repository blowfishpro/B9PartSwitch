using System;
using Xunit;
using B9PartSwitch;

namespace B9PartSwitchTests
{
    public class BestSubtypeDeterminatorTest
    {
        private static readonly BestSubtypeDeterminator determinator = new BestSubtypeDeterminator();

        [Fact]
        public void TestFindBestSubtype__FirstWhenNoTanks()
        {
            PartSubtype subtype0 = new PartSubtype { defaultSubtypePriority = 0 };
            PartSubtype subtype1 = new PartSubtype { defaultSubtypePriority = 1 };
            PartSubtype subtype2 = new PartSubtype { defaultSubtypePriority = 1 };
            PartSubtype[] subtypes = { subtype0, subtype1, subtype2 };
            string[] resourceNamesOnPart = { "SomeResource" };

            Assert.Same(subtype1, determinator.FindBestSubtype(subtypes, resourceNamesOnPart));
        }

        [Fact]
        public void TestFindBestSubtype__PreferStructuralWhenNoManagedResources()
        {
            PartSubtype subtype0 = CreateSubtype(1, "SomeResource1");
            PartSubtype subtype1 = CreateSubtype(0);
            PartSubtype subtype2 = CreateSubtype(1);
            PartSubtype subtype3 = CreateSubtype(1);

            PartSubtype[] subtypes = { subtype0, subtype1, subtype2, subtype3 };
            string[] resourceNamesOnPart = { "SomeResource0" };

            Assert.Same(subtype2, determinator.FindBestSubtype(subtypes, resourceNamesOnPart));
        }

        [Fact]
        public void TestFindBestSubtype__FirstWhenNoManagedResourcesAndNoStructural()
        {
            PartSubtype subtype0 = CreateSubtype(0, "SomeResource1");
            PartSubtype subtype1 = CreateSubtype(1, "SomeResource1");
            PartSubtype subtype2 = CreateSubtype(1, "SomeResource2");

            PartSubtype[] subtypes = { subtype0, subtype1, subtype2 };
            string[] resourceNamesOnPart = { "SomeResource0" };

            Assert.Same(subtype1, determinator.FindBestSubtype(subtypes, resourceNamesOnPart));
        }

        [Fact]
        public void TestFindBestSubtype__DetermineFromResources()
        {
            PartSubtype subtype0 = CreateSubtype(1, "SomeResource1");
            PartSubtype subtype1 = CreateSubtype(1, "SomeResource1", "SomeResource2", "SomeResource3");
            PartSubtype subtype2 = CreateSubtype(1, "SomeResource1", "SomeResource2");
            PartSubtype subtype3 = CreateSubtype(1, "SomeResource1", "SomeResource2");
            PartSubtype subtype4 = CreateSubtype(0, "SomeResource1", "SomeResource2");

            PartSubtype[] subtypes = { subtype0, subtype1, subtype2, subtype3, subtype4 };
            string[] resourceNamesOnPart = { "SomeResource0", "SomeResource1", "SomeResource2" };

            Assert.Same(subtype2, determinator.FindBestSubtype(subtypes, resourceNamesOnPart));
        }

        [Fact]
        public void TestFindBestSubtype__CantDetermineFromResources__First()
        {
            PartSubtype subtype0 = CreateSubtype(0, "SomeResource1");
            PartSubtype subtype1 = CreateSubtype(1, "SomeResource1");
            PartSubtype subtype2 = CreateSubtype(1, "SomeResource2");
            PartSubtype subtype3 = CreateSubtype(1, "SomeResource3");

            PartSubtype[] subtypes = { subtype0, subtype1, subtype2, subtype3 };
            string[] resourceNamesOnPart = { "SomeResource0", "SomeResource2", "SomeResource3" };

            Assert.Same(subtype1, determinator.FindBestSubtype(subtypes, resourceNamesOnPart));
        }

        private PartSubtype CreateSubtype(int defaultSubtypePriority, params string[] resourceNames)
        {
            PartSubtype subtype = new PartSubtype()
            {
                defaultSubtypePriority = defaultSubtypePriority,
                tankType = new TankType(),
            };

            foreach (string resourceName in resourceNames)
            {
                subtype.tankType.resources.Add(new TankResource()
                {
                    resourceDefinition = new PartResourceDefinition(resourceName)
                });
            }

            return subtype;
        }
    }
}
