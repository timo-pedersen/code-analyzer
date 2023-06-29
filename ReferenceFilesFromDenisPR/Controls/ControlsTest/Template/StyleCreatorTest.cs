#if!VNEXT_TARGET
using System.Linq;
using Neo.ApplicationFramework.Controls.PropertyAdapters.Appearance;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.Template
{
    [TestFixture]
    public class StyleCreatorTest
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void GetClosestMatchingFontSizeReturnsSameAsDesiredFontSizeIfDesiredFontSizeExistsInDefaultFontSizes()
        {
            double match = StyleCreator.GetClosestMatchingFontSize(10.0, 16.0);
            Assert.That(match, Is.EqualTo(16));
        }

        [Test]
        public void GetClosestMatchingFontSizeReturnsMoreThanDesiredFontSizeIfDesiredFontSizeDoesNotExistsInDefaultFontSizesAndCurrentFontSizeIsLessThanDesiredFontSize()
        {
            double match = StyleCreator.GetClosestMatchingFontSize(10.0, 17.0);
            Assert.That(match, Is.EqualTo(18));
        }

        [Test]
        public void GetClosestMatchingFontSizeReturnsLessThanDesiredFontSizeIfDesiredFontSizeDoesNotExistsInDefaultFontSizesAndCurrentFontSizeIsMoreThanDesiredFontSize()
        {
            double match = StyleCreator.GetClosestMatchingFontSize(20.0, 17.0);
            Assert.That(match, Is.EqualTo(16));
        }

        [Test]
        public void GetClosestMatchingFontSizeReturnsMaxSizeIfDesiredSizeIsLargerThanMaxSize()
        {
            double match = StyleCreator.GetClosestMatchingFontSize(10.0, 2000.0);
            double biggestFont = AppearanceAdapter.DefaultFontSizes.Last();
            Assert.That(match, Is.EqualTo(biggestFont));
        }

        [Test]
        public void GetClosestMatchingFontSizeReturnsMinSizeIfDesiredSizeIsLessThanMinSize()
        {
            double match = StyleCreator.GetClosestMatchingFontSize(10.0, 1.0);
            double smallestFont = AppearanceAdapter.DefaultFontSizes.First();
            Assert.That(match, Is.EqualTo(smallestFont));
        }

        [Test]
        public void GetClosestMatchingFontSizeReturnsSameIfCurrentSizeAndDesiredSizeIsSame()
        {
            double match = StyleCreator.GetClosestMatchingFontSize(10.0, 10.0);
            
            Assert.That(match, Is.EqualTo(10.0));
        }
    }
}
#endif
