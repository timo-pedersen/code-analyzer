using System;
using System.Collections.Generic;
using Neo.ApplicationFramework.Common.Timers;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.Symbol
{
    [TestFixture]
    public class AnimatedPictureCFTest
    {
        [Test]
        [TestCase(0, 20)]
        [TestCase(10, 20)]
        [TestCase(20, 20)]
        [TestCase(30, 30)]
        [TestCase(1000, 1000)]
        [TestCase(1001, 1001)]
        [TestCase(int.MaxValue, int.MaxValue)]
        public void TickTest(int inputDelay, int outputDelay)
        {
            // ARRANGE
            ISymbolFrameInfo symbolFrameInfo = Substitute.For<ISymbolFrameInfo>();
            symbolFrameInfo.Delay.Returns(new TimeSpan(0, 0, 0, 0, inputDelay));

            ISymbolInfoCF symbolInfoCF = Substitute.For<ISymbolInfoCF>();
            symbolInfoCF.Frames.Returns(
                new List<ISymbolFrameInfo>
                {
                    symbolFrameInfo
                });

            INativeBitmap nativeBitmap = Substitute.For<INativeBitmap>();
            nativeBitmap.Information.Returns(symbolInfoCF);

            var symbolService = Substitute.For<ISymbolServiceCF>();
            symbolService.GetAnimationSymbol("MockName").Returns(nativeBitmap);

            var timer = new ManualTimer();

            // ReSharper disable once UnusedVariable
            var animatedPicture = new AnimatedPictureCF(symbolService, timer)
            {
                SymbolName = "MockName",
                IsAnimationEnabled = true
            };

            // ACT
            timer.FireTick(this, new EventArgs());

            // ASSERT
            Assert.AreEqual(timer.Interval, outputDelay);
        }
    }
}