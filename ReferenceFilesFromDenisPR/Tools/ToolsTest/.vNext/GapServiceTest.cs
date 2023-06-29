#region vNext
using System;
using System.Collections.Generic;
using System.Reflection;
using Core.Api.Feature;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Core.Api.Utilities;
using Neo.ApplicationFramework.Attributes;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.vNext.Gaps;
using Neo.ApplicationFramework.Tools.vNext.Gaps;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.vNext
{
    [TestFixture]
    public sealed class GapServiceTest
    {
        private readonly Type[] m_FactoriesForSubjectsToTest = {
            typeof(TypeWithGapFactory),
            typeof(PropertyWithGapFactory),
            typeof(TypeWithoutGapFactory),
            typeof(PropertyWithoutGapFactory),
            typeof(TypeWithWinFormsGapFalseFactory),
            typeof(TypeWithWpfGapFalseFactory)
        };
        private readonly bool[] m_WpfVNextFeatureActivatedOptions = { true, false };
        private readonly TargetPlatform[] m_TargetPlatforms = { TargetPlatform.WindowsCE, TargetPlatform.Windows };

        [TestCaseSource(nameof(IsSubjectConsideredGapTestData))]
        public void TestIsSubjectConsideredGap(
            string testCaseName,
            Type subjectFactory,
            TargetPlatform targetPlatform,
            bool isWpfVNextTargetFeatureActivated,
            bool expectedResult)
        {
            AssertIsSubjectConsideredGap(testCaseName, subjectFactory, targetPlatform, isWpfVNextTargetFeatureActivated, false, false, "vNext", expectedResult);
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(false, false)]
        public void WhenTypeIsRegisteredAsGapItsAGap(bool useRegisteredTypes, bool expectedResultIsGap)
        {
            // ARRANGE
            Type subjectFactory = typeof(TypeWithoutGapFactory);
            TargetPlatform targetPlatform = TargetPlatform.WindowsCE;
            bool isWpfVNextTargetFeatureActivated = false;
            bool isDisableGapsFeatureActivated = false;
            string brandName = "vNext";

            // ACT, ASSERT
            AssertIsSubjectConsideredGap(nameof(WhenTypeIsRegisteredAsGapItsAGap), subjectFactory, targetPlatform, isWpfVNextTargetFeatureActivated, isDisableGapsFeatureActivated, useRegisteredTypes, brandName, expectedResultIsGap);
        }

        [Test]
        public void WhenNotVNextBrandThenSubjectNeverIsGap()
        {
            // ARRANGE 
            var disableGapFeatureActivatedOptions = new[] { true, false };

            // ACT, ASSERT
            foreach (bool isDisableGapsFeatureActivated in disableGapFeatureActivatedOptions)
            {
                foreach (bool isWpfVNextTargetFeatureActivated in m_WpfVNextFeatureActivatedOptions)
                {
                    foreach (TargetPlatform targetPlatform in m_TargetPlatforms)
                    {
                        foreach (Type subjectFactory in m_FactoriesForSubjectsToTest)
                        {
                            AssertIsSubjectConsideredGap(nameof(WhenNotVNextBrandThenSubjectNeverIsGap), subjectFactory, targetPlatform, isWpfVNextTargetFeatureActivated, isDisableGapsFeatureActivated, false, "iX", false);
                        }
                    }
                }
            }
        }

        [Test]
        public void WhenDisableGapsFeatureActivatedThenSubjectNeverIsGap()
        {
            // ARRANGE 
            var brands = new[] { "iX", "vNext" };

            // ACT, ASSERT
            foreach (string brandName in brands)
            {
                foreach (bool isWpfVNextTargetFeatureActivated in m_WpfVNextFeatureActivatedOptions)
                {
                    foreach (TargetPlatform targetPlatform in m_TargetPlatforms)
                    {
                        foreach (Type subjectFactory in m_FactoriesForSubjectsToTest)
                        {
                            AssertIsSubjectConsideredGap(nameof(WhenDisableGapsFeatureActivatedThenSubjectNeverIsGap), subjectFactory, targetPlatform, isWpfVNextTargetFeatureActivated, true, false, brandName, false);
                        }
                    }
                }
            }
        }

        [TestCaseSource(nameof(ConfigurationRespectsGapsTestData))]
        public void TestConfigurationRespectsGaps(
            string testCaseName,
            string brandName,
            TargetPlatform targetPlatform,
            bool isDisableGapsFeatureActivated,
            bool isWpfVNextTargetFeatureActivated,
            bool expectedResult)
        {
            // ARRANGE
            IGapService gapService = ConfigureGapService(brandName, targetPlatform, isDisableGapsFeatureActivated, isWpfVNextTargetFeatureActivated);
            
            // ACT
            bool result = gapService.ConfigurationRespectsGaps();

            // ASSERT
            Assert.AreEqual(expectedResult, result, testCaseName);
        }

        [TestCaseSource(nameof(IsSystemHidingvNextGapsTestData))]
        public void TestIsSystemHidingvNextGaps(
            string testCaseName,
            string brandName,
            TargetPlatform targetPlatform,
            bool isDisableGapsFeatureActivated,
            bool isWpfVNextTargetFeatureActivated,
            bool expectedResult)
        {
            // ARRANGE
            IGapService gapService = ConfigureGapService(brandName, targetPlatform, isDisableGapsFeatureActivated, isWpfVNextTargetFeatureActivated);

            // ACT
            bool result = gapService.IsSystemHidingGaps();

            // ASSERT
            Assert.AreEqual(expectedResult, result, testCaseName);
        }

        private static IGapService ConfigureGapService(string brandName, TargetPlatform targetPlatform, bool isDisableGapsFeatureActivated, bool isWpfVNextTargetFeatureActivated)
        {
            IFeatureSecurityServiceIde featureService = CreateMockFeatureServiceIde(isWpfVNextTargetFeatureActivated, isDisableGapsFeatureActivated);
            ILazy<IBrandService> brandService = CreateMockBrandService(brandName);
            ILazy<ITargetService> targetService = CreateMockTargetService(targetPlatform);

            return new GapService(brandService, featureService, targetService);
        }

        private static ILazy<IBrandService> CreateMockBrandService(string brandName)
        {
            ILazy<IBrandService> brandService = CreateLazyStub<IBrandService>();
            brandService.Value.BrandName.Returns(brandName);
            return brandService;
        }

        private static IFeatureSecurityServiceIde CreateMockFeatureServiceIde(
            bool isWpfVNextTargetFeatureActivated,
            bool isDisableGapsFeatureActivated)
        {
            var featureService = Substitute.For<IFeatureSecurityServiceIde>();
            ILazy<IBrandService> brandService = CreateLazyStub<IBrandService>();
            ILazy<ITargetService> targetService = CreateLazyStub<ITargetService>();
            featureService.IsActivated<DisableGapsFilteringFeature>().Returns(isDisableGapsFeatureActivated);
            featureService.IsActivated<ExperimentalWPFvNextTargetFeature>().Returns(isWpfVNextTargetFeatureActivated);

            return featureService;
        }

        private static ILazy<ITargetService> CreateMockTargetService(TargetPlatform targetPlatform)
        {
            ILazy<ITargetService> targetService = CreateLazyStub<ITargetService>();
            ILazy<ITarget> target = CreateLazyStub<ITarget>();
            target.Value.Id.Returns(targetPlatform);
            targetService.Value.CurrentTarget = target.Value;

            return targetService;
        }

        private static void AssertIsSubjectConsideredGap(
            string testCaseName,
            Type subjectFactory,
            TargetPlatform targetPlatform,
            bool isWpfVNextTargetFeatureActivated,
            bool isDisableGapsFeatureActivated,
            bool useRegisteredTypes,
            string brandName,
            bool expectedResult)
        {
            // ARRANGE
            MemberInfo subject = ((IMemberInfoFactory)Activator.CreateInstance(subjectFactory)).Create();
            IGapService gapService = ConfigureGapService(brandName, targetPlatform, isDisableGapsFeatureActivated, isWpfVNextTargetFeatureActivated);

            if (useRegisteredTypes)
            {
                IGapServiceSetup gapServiceSetup = (IGapServiceSetup)gapService;
                gapServiceSetup.RegisterGap(typeof(TypeWithoutGapFactory.TypeWithoutGap).AssemblyQualifiedName, true, true);
            }

            // ACT 
            bool result = gapService.IsSubjectConsideredGap(subject);

            // ASSERT
            Assert.AreEqual(expectedResult, result, testCaseName);
        }

        private static ILazy<T> CreateLazyStub<T>() where T : class
        {
            var lazy = Substitute.For<ILazy<T>>();
            lazy.Value.Returns(Substitute.For<T>());
            return lazy;
        }
        
        private static IEnumerable<TestCaseData> IsSubjectConsideredGapTestData
        {
            get
            {
                yield return new TestCaseData("01", typeof(TypeWithGapFactory), TargetPlatform.WindowsCE, true, true);
                yield return new TestCaseData("02", typeof(TypeWithGapFactory), TargetPlatform.WindowsCE, false, true);
                yield return new TestCaseData("03", typeof(TypeWithGapFactory), TargetPlatform.Windows, true, true);
                yield return new TestCaseData("04", typeof(TypeWithGapFactory), TargetPlatform.Windows, false, false);

                yield return new TestCaseData("05", typeof(PropertyWithGapFactory), TargetPlatform.WindowsCE, true, true);
                yield return new TestCaseData("06", typeof(PropertyWithGapFactory), TargetPlatform.WindowsCE, false, true);
                yield return new TestCaseData("07", typeof(PropertyWithGapFactory), TargetPlatform.Windows, true, true);
                yield return new TestCaseData("08", typeof(PropertyWithGapFactory), TargetPlatform.Windows, false, false);
                
                yield return new TestCaseData("09", typeof(TypeWithoutGapFactory), TargetPlatform.WindowsCE, true, false);
                yield return new TestCaseData("10", typeof(TypeWithoutGapFactory), TargetPlatform.WindowsCE, false, false);
                yield return new TestCaseData("11", typeof(TypeWithoutGapFactory), TargetPlatform.Windows, true, false);
                yield return new TestCaseData("12", typeof(TypeWithoutGapFactory), TargetPlatform.Windows, false, false);

                yield return new TestCaseData("13", typeof(PropertyWithoutGapFactory), TargetPlatform.WindowsCE, true, false);
                yield return new TestCaseData("14", typeof(PropertyWithoutGapFactory), TargetPlatform.WindowsCE, false, false);
                yield return new TestCaseData("15", typeof(PropertyWithoutGapFactory), TargetPlatform.Windows, true, false);
                yield return new TestCaseData("16", typeof(PropertyWithoutGapFactory), TargetPlatform.Windows, false, false);

                yield return new TestCaseData("17", typeof(TypeWithWinFormsGapFalseFactory), TargetPlatform.WindowsCE, true, false);
                yield return new TestCaseData("18", typeof(TypeWithWinFormsGapFalseFactory), TargetPlatform.WindowsCE, false, false);
                yield return new TestCaseData("19", typeof(TypeWithWinFormsGapFalseFactory), TargetPlatform.Windows, true, true);
                yield return new TestCaseData("20", typeof(TypeWithWinFormsGapFalseFactory), TargetPlatform.Windows, false, false);

                yield return new TestCaseData("21", typeof(TypeWithWpfGapFalseFactory), TargetPlatform.WindowsCE, true, true);
                yield return new TestCaseData("22", typeof(TypeWithWpfGapFalseFactory), TargetPlatform.WindowsCE, false, true);
                yield return new TestCaseData("23", typeof(TypeWithWpfGapFalseFactory), TargetPlatform.Windows, true, false);
                yield return new TestCaseData("24", typeof(TypeWithWpfGapFalseFactory), TargetPlatform.Windows, false, false);
            }
        }

        private static IEnumerable<TestCaseData> ConfigurationRespectsGapsTestData
        {
            // testCaseName, brandName, targetPlatform, isDisableGapsFeatureActivated, isWpfVNextTargetFeatureActivated, expectedResult
            get
            {
                yield return new TestCaseData("01", "iX", TargetPlatform.WindowsCE, false, false, false);
                yield return new TestCaseData("02", "iX", TargetPlatform.WindowsCE, true, false, false);
                yield return new TestCaseData("03", "iX", TargetPlatform.WindowsCE, false, true, false);
                yield return new TestCaseData("04", "iX", TargetPlatform.WindowsCE, true, true, false);
                yield return new TestCaseData("05", "iX", TargetPlatform.Windows, false, false, false);
                yield return new TestCaseData("06", "iX", TargetPlatform.Windows, true, false, false);
                yield return new TestCaseData("07", "iX", TargetPlatform.Windows, false, true, false);
                yield return new TestCaseData("08", "iX", TargetPlatform.Windows, true, true, false);

                yield return new TestCaseData("09", "vNext", TargetPlatform.WindowsCE, false, false, true);
                yield return new TestCaseData("10", "vNext", TargetPlatform.WindowsCE, true, false, true);
                yield return new TestCaseData("11", "vNext", TargetPlatform.WindowsCE, false, true, true);
                yield return new TestCaseData("12", "vNext", TargetPlatform.WindowsCE, true, true, true);
                yield return new TestCaseData("13", "vNext", TargetPlatform.Windows, false, false, false);
                yield return new TestCaseData("14", "vNext", TargetPlatform.Windows, true, false, false);
                yield return new TestCaseData("15", "vNext", TargetPlatform.Windows, false, true, true);
                yield return new TestCaseData("16", "vNext", TargetPlatform.Windows, true, true, true);
            }
        }

        private static IEnumerable<TestCaseData> IsSystemHidingvNextGapsTestData
        {
            // testCaseName, brandName, targetPlatform, isDisableGapsFeatureActivated, isWpfVNextTargetFeatureActivated, expectedResult
            get
            {
                yield return new TestCaseData("01", "iX", TargetPlatform.WindowsCE, false, false, false);
                yield return new TestCaseData("02", "iX", TargetPlatform.WindowsCE, true, false, false);
                yield return new TestCaseData("03", "iX", TargetPlatform.WindowsCE, false, true, false);
                yield return new TestCaseData("04", "iX", TargetPlatform.WindowsCE, true, true, false);
                yield return new TestCaseData("05", "iX", TargetPlatform.Windows, false, false, false);
                yield return new TestCaseData("06", "iX", TargetPlatform.Windows, true, false, false);
                yield return new TestCaseData("07", "iX", TargetPlatform.Windows, false, true, false);
                yield return new TestCaseData("08", "iX", TargetPlatform.Windows, true, true, false);

                yield return new TestCaseData("09", "vNext", TargetPlatform.WindowsCE, false, false, true);
                yield return new TestCaseData("10", "vNext", TargetPlatform.WindowsCE, true, false, false);
                yield return new TestCaseData("11", "vNext", TargetPlatform.WindowsCE, false, true, true);
                yield return new TestCaseData("12", "vNext", TargetPlatform.WindowsCE, true, true, false);
                yield return new TestCaseData("13", "vNext", TargetPlatform.Windows, false, false, false);
                yield return new TestCaseData("14", "vNext", TargetPlatform.Windows, true, false, false);
                yield return new TestCaseData("15", "vNext", TargetPlatform.Windows, false, true, true);
                yield return new TestCaseData("16", "vNext", TargetPlatform.Windows, true, true, false);
            }
        }

        private interface IMemberInfoFactory
        {
            MemberInfo Create();
        }

        private class TypeWithGapFactory : IMemberInfoFactory
        {
            public MemberInfo Create()
            {
                return typeof(TypeWithGap);
            }

            [VNextGap]
            private class TypeWithGap
            { }
        }

        private class PropertyWithGapFactory : IMemberInfoFactory
        {
            public MemberInfo Create()
            {
                return typeof(PropertyWithGap).GetProperty(nameof(PropertyWithGap.Property));
            }

            private class PropertyWithGap
            {
                [VNextGap]
                public bool Property { get; set; }
            }
        }

        private class PropertyWithoutGapFactory : IMemberInfoFactory
        {
            public MemberInfo Create()
            {
                return typeof(PropertyWithoutGap).GetProperty(nameof(PropertyWithoutGap.Property));
            }

            private class PropertyWithoutGap
            {
                public bool Property { get; set; }
            }
        }

        private class TypeWithoutGapFactory : IMemberInfoFactory
        {
            public MemberInfo Create()
            {
                return typeof(TypeWithoutGap);
            }

            internal sealed class TypeWithoutGap
            { }
        }

        private class TypeWithWinFormsGapFalseFactory : IMemberInfoFactory
        {
            public MemberInfo Create()
            {
                return typeof(TypeWithWinFormsGapFalse);
            }

            [VNextGap(winFormsIsGap:false)]
            private class TypeWithWinFormsGapFalse
            { }
        }

        private class TypeWithWpfGapFalseFactory : IMemberInfoFactory
        {
            public MemberInfo Create()
            {
                return typeof(TypeWithWpfGapFalse);
            }

            [VNextGap(wpfIsGap:false)]
            private class TypeWithWpfGapFalse
            { }
        }
    }
}
#endregion