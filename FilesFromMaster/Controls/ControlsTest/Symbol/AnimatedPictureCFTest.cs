using System;
using System.Collections.Generic;
using Neo.ApplicationFramework.Common.Timers;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

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
            ISymbolFrameInfo symbolFrameInfo = MockRepository.GenerateMock<ISymbolFrameInfo>();
            symbolFrameInfo.Stub(x => x.Delay).Return(new TimeSpan(0, 0, 0, 0, inputDelay));

            ISymbolInfoCF symbolInfoCF = MockRepository.GenerateMock<ISymbolInfoCF>();
            symbolInfoCF.Stub(x => x.Frames).Return(
                new List<ISymbolFrameInfo>
                {
                    symbolFrameInfo
                });

            INativeBitmap nativeBitmap = MockRepository.GenerateMock<INativeBitmap>();
            nativeBitmap.Stub(x => x.Information).Return(symbolInfoCF);

            var symbolService = MockRepository.GenerateMock<ISymbolServiceCF>();
            symbolService.Stub(x => x.GetAnimationSymbol("MockName")).Return(nativeBitmap);

            var drawingService = MockRepository.GenerateMock<IDrawingServiceCF>().ToILazy();

            var timer = new ManualTimer();

            // ReSharper disable once UnusedVariable
            var animatedPicture = new AnimatedPictureCF(symbolService, drawingService, timer)
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