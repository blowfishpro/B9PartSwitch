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
            PartSubtype subtype0 = new PartSubtype();
            PartSubtype subtype1 = new PartSubtype();
            PartSubtype[] subtypes = { subtype0, subtype1 };
            string[] resourceNamesOnPart = { "SomeResource" };

            Assert.Same(subtype0, determinator.FindBestSubtype(subtypes, resourceNamesOnPart));
        }

        [Fact]
        public void TestFindBestSubtype__PreferStructuralWhenNoManagedResources()
        {
            PartSubtype subtype0 = CreateSubtype("SomeResource1");
            PartSubtype subtype1 = CreateSubtype();

            PartSubtype[] subtypes = { subtype0, subtype1 };
            string[] resourceNamesOnPart = { "SomeResource0" };

            Assert.Same(subtype1, determinator.FindBestSubtype(subtypes, resourceNamesOnPart));
        }

        [Fact]
        public void TestFindBestSubtype__FirstWhenNoManagedResourcesAndNoStructural()
        {
            PartSubtype subtype0 = CreateSubtype("SomeResource1");
            PartSubtype subtype1 = CreateSubtype("SomeResource2");

            PartSubtype[] subtypes = { subtype0, subtype1 };
            string[] resourceNamesOnPart = { "SomeResource0" };

            Assert.Same(subtype0, determinator.FindBestSubtype(subtypes, resourceNamesOnPart));
        }

        [Fact]
        public void TestFindBestSubtype__DetermineFromResources()
        {
            PartSubtype subtype0 = CreateSubtype("SomeResource1");
            PartSubtype subtype1 = CreateSubtype("SomeResource1", "SomeResource2", "SomeResource3");
            PartSubtype subtype2 = CreateSubtype("SomeResource1", "SomeResource2");

            PartSubtype[] subtypes = { subtype0, subtype1, subtype2 };
            string[] resourceNamesOnPart = { "SomeResource0", "SomeResource1", "SomeResource2" };

            Assert.Same(subtype2, determinator.FindBestSubtype(subtypes, resourceNamesOnPart));
        }

        [Fact]
        public void TestFindBestSubtype__CantDetermineFromResources__First()
        {
            PartSubtype subtype0 = CreateSubtype("SomeResource1");
            PartSubtype subtype1 = CreateSubtype("SomeResource2");
            PartSubtype subtype2 = CreateSubtype("SomeResource3");

            PartSubtype[] subtypes = { subtype0, subtype1, subtype2 };
            string[] resourceNamesOnPart = { "SomeResource0", "SomeResource2", "SomeResource3" };

            Assert.Same(subtype0, determinator.FindBestSubtype(subtypes, resourceNamesOnPart));
        }

        private PartSubtype CreateSubtype(params string[] resourceNames)
        {
            PartSubtype subtype = new PartSubtype()
            {
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
