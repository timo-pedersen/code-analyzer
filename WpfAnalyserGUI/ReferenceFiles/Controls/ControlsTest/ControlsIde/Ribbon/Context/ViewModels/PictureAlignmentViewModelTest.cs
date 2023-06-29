﻿using System.Windows;
using Neo.ApplicationFramework.Controls.Ribbon.Context.ViewModels.Button;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.ControlsIde.Ribbon.Context.ViewModels
{
    [TestFixture]
    public class PictureAlignmentViewModelTest : RibbonViewModelTestBase
    {
        private const string PictureVerticalAlignmentPropertyName = "ImageVerticalAlignment";
        private const string PictureHorizontalAlignmentPropertyName = "ImageHorizontalAlignment";

        [SetUp]
        public void SetUp()
        {
            TestHelper.CreateAndAddServiceStub<ICommandManagerService>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [TestCase(HorizontalAlignment.Left, true, false, false, false)]
        [TestCase(HorizontalAlignment.Center, false, true, false, false)]
        [TestCase(HorizontalAlignment.Right, false, false, true, false)]
        [TestCase(HorizontalAlignment.Stretch, false, false, false, true)]
        public void UpdateContentSetsTheHorizontalAlignmentProperties(HorizontalAlignment horizontalAlignment,
            bool expectedIsHorizontalLeft,
            bool expectedIsHorizontalCenter,
            bool expectedIsHorizontalRight,
            bool expectedIsHorizontalStretch)
        {
            GlobalCommandServiceStub.GetProperty(PictureHorizontalAlignmentPropertyName, HorizontalAlignment.Center).Returns(horizontalAlignment);
            GlobalCommandServiceStub.GetProperty(PictureVerticalAlignmentPropertyName, VerticalAlignment.Center).Returns(VerticalAlignment.Center);

            ExtendedPictureAlignmentViewModel viewModel = new ExtendedPictureAlignmentViewModel().ExecuteUpdateContent();

            Assert.That(viewModel.IsHorizontalLeftAligned, Is.EqualTo(expectedIsHorizontalLeft));
            Assert.That(viewModel.IsHorizontalCenterAligned, Is.EqualTo(expectedIsHorizontalCenter));
            Assert.That(viewModel.IsHorizontalRightAligned, Is.EqualTo(expectedIsHorizontalRight));
            Assert.That(viewModel.IsHorizontalStretchAligned, Is.EqualTo(expectedIsHorizontalStretch));
        }

        [TestCase(HorizontalAlignment.Left, true, false, false, false)]
        [TestCase(HorizontalAlignment.Center, false, true, false, false)]
        [TestCase(HorizontalAlignment.Right, false, false, true, false)]
        [TestCase(HorizontalAlignment.Stretch, false, false, false, true)]
        public void UpdateContentSetsTheVerticalAlignmentProperties(VerticalAlignment verticalAlignment,
            bool expectedIsVerticalTop,
            bool expectedIsVerticalCenter,
            bool expectedIsVerticalBottom,
            bool expectedIsVerticalStretch)
        {
            GlobalCommandServiceStub.GetProperty(PictureHorizontalAlignmentPropertyName, HorizontalAlignment.Center).Returns(HorizontalAlignment.Center);
            GlobalCommandServiceStub.GetProperty(PictureVerticalAlignmentPropertyName, VerticalAlignment.Center).Returns(verticalAlignment);

            ExtendedPictureAlignmentViewModel viewModel = new ExtendedPictureAlignmentViewModel().ExecuteUpdateContent();

            Assert.That(viewModel.IsVerticalTopAligned, Is.EqualTo(expectedIsVerticalTop));
            Assert.That(viewModel.IsVerticalCenterAligned, Is.EqualTo(expectedIsVerticalCenter));
            Assert.That(viewModel.IsVerticalBottomAligned, Is.EqualTo(expectedIsVerticalBottom));
            Assert.That(viewModel.IsVerticalStretchAligned, Is.EqualTo(expectedIsVerticalStretch));
        }

        internal class ExtendedPictureAlignmentViewModel : ButtonPictureAlignmentViewModel
        {
            public ExtendedPictureAlignmentViewModel ExecuteUpdateContent()
            {
                UpdateContent();
                return this;
            }
        }

    }
}
