using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Common.Graphics
{
    [TestFixture]
    public class AnimatedGifHelperTest
    {
        private readonly string m_AnimatedDogFilePath = Environment.ExpandEnvironmentVariables(@"%temp%\animated dog.gif");
        private readonly string m_AnimatedClockFilePath = Environment.ExpandEnvironmentVariables(@"%temp%\animated clock.gif");
        private const string SymbolName = "symbolName";
        private BitmapHelper m_BitmapHelper;

        [OneTimeSetUp]
        public void Setup()
        {
            ExtractAnimationFile(m_AnimatedDogFilePath);
            ExtractAnimationFile(m_AnimatedClockFilePath);
        }

        [Test]
        public void CanParseAnimatedDogGif()
        {
            AnimatedGifHelper helper = new AnimatedGifHelper();
            IList<FrameInfo> frameInfos = helper.GetFrames(m_AnimatedDogFilePath, SymbolName);

            Assert.AreEqual(27, frameInfos.Count);

            Assert.AreEqual(TimeSpan.FromSeconds(0.1), frameInfos[0].Delay);
            Assert.AreEqual(141, frameInfos[0].Height);
            Assert.AreEqual(117, frameInfos[0].Width);
            Assert.AreEqual(0, frameInfos[0].Index);

            Assert.AreEqual(TimeSpan.FromSeconds(0.1), frameInfos[1].Delay);
            Assert.AreEqual(141, frameInfos[1].Height);
            Assert.AreEqual(117, frameInfos[1].Width);
            Assert.AreEqual(1, frameInfos[1].Index);
        }

        [Test]
        public void CanParseAnimatedClockGif()
        {
            AnimatedGifHelper helper = new AnimatedGifHelper();
            IList<FrameInfo> frameInfos = helper.GetFrames(m_AnimatedClockFilePath, SymbolName);

            Assert.AreEqual(13, frameInfos.Count);

            Assert.AreEqual(TimeSpan.FromSeconds(0.1), frameInfos[0].Delay);
            Assert.AreEqual(200, frameInfos[0].Height);
            Assert.AreEqual(200, frameInfos[0].Width);
            Assert.AreEqual(0, frameInfos[0].Index);

            Assert.AreEqual(TimeSpan.FromSeconds(1), frameInfos[1].Delay);
            Assert.AreEqual(200, frameInfos[1].Height);
            Assert.AreEqual(200, frameInfos[1].Width);
            Assert.AreEqual(1, frameInfos[1].Index);
        }

        [Test]
        public void CanGenerateSingleComposedImageOfAllFramesOfAnimatedDog()
        {
            AnimatedGifHelper helper = new AnimatedGifHelper();
            BitmapSource composition = null;
#if !VNEXT_TARGET
            using (new Neo.ApplicationFramework.Utilities.Measurement.Stopwatch("It took {0} to generate animated dog composed bitmap"))
#endif
            {
                using (FileStream file = new FileStream(m_AnimatedDogFilePath, FileMode.Open, FileAccess.Read))
                {
                    composition = helper.GetComposedBitmap(file, SymbolName, Size.Empty);
                }

                Assert.IsNotNull(composition);
                Assert.AreEqual(117 * 27, composition.PixelWidth);
                Assert.AreEqual(141, composition.PixelHeight);
            }
        }

        [Test]
        [TestCase(0, 20)]
        [TestCase(10, 20)]
        [TestCase(20, 20)]
        [TestCase(50, 50)]
        [TestCase(1001, 1001)]
        [TestCase(int.MaxValue, int.MaxValue)]
        public void FrameInfoDelay(int inputDelay, int outputDelay)
        {
            var frameInfo = new FrameInfo
            {
                Delay = new TimeSpan(0, 0, 0, 0, inputDelay)
            };

            Assert.AreEqual(frameInfo.Delay.TotalMilliseconds, outputDelay);
        }

        private void ExtractAnimationFile(string destinationFilePath)
        {
            string fileName = Path.GetFileName(destinationFilePath);
            Stream animatedImageStream = GetEmbeddedResourceStream(fileName);
            using (FileStream temporaryFile = new FileStream(destinationFilePath, FileMode.Create))
            {
                animatedImageStream.CopyTo(temporaryFile);
            }
        }

        private Stream GetEmbeddedResourceStream(string fileName)
        {
            Stream animatedImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GetType(), fileName);
            return animatedImageStream;
        }

        internal BitmapHelper BitmapHelper
        {
            get
            {
                if (m_BitmapHelper == null)
                {
                    m_BitmapHelper = new BitmapHelper();
                }

                return m_BitmapHelper;
            }
            set { m_BitmapHelper = value; }
        }

    }
}
