using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Core.Api.CrossReference;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Core.Component.Engine.Design;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Build.BuildManager
{
    [TestFixture]
    public class RenderableControlResourcesGeneratorTest
    {
        private const string TempPath = @"drive:\temppath";
        private const string ProjectPath = @"drive:\projectpath";
        private const string DesignerFileName = "somescreen.neo";
        private readonly string m_DesignerFilePath = Path.Combine(ProjectPath, DesignerFileName);
        private const string DummyResourceFileName = @"somescreen.somecontrol_state.bmp";
        private const string DummyResourceObjectFullName = @"somescreen.somecontrol";
        private readonly string m_DummyResourceFilePath = Path.Combine(TempPath, DummyResourceFileName);

        private RenderableControlResourcesGenerator m_Generator;
        private DirectoryHelper m_DirectoryHelperStub;
        private FileHelper m_FileHelperStub;
        private BitmapHelperStub m_BitmapHelperMock;
        private IScreenRootDesigner m_ScreenRootDesignerStub;
        private INeoDesignerHost m_DesignerHostStub;
        private IRenderableControlServiceIde m_RenderableServiceStub;
        private IDesignerProjectItem m_ProjectItemMock;
        private RenderableControlResourcesHelper m_RenderableControlResourceHelperStub;
        private List<ICrossReferenceItem> m_CrossReferenceItems;

        [SetUp]
        public void TestSetup()
        {
            m_RenderableServiceStub = TestHelper.AddServiceStub<IRenderableControlServiceIde>();

            var targetInfo = Substitute.For<ITargetInfo>();
            targetInfo.TempPath = TempPath;
            targetInfo.ProjectPath = ProjectPath;

            var target = Substitute.For<ITarget>();
            target.Id.Returns(TargetPlatform.WindowsCE);

            m_DirectoryHelperStub = Substitute.For<DirectoryHelper>();
            m_DirectoryHelperStub.Exists(TempPath).Returns(true);

            m_FileHelperStub = Substitute.For<FileHelper>();

            m_BitmapHelperMock = Substitute.For<BitmapHelperStub>();

            m_Generator = new RenderableControlResourcesGenerator(targetInfo, target, m_DirectoryHelperStub, m_FileHelperStub, m_BitmapHelperMock);

            m_ScreenRootDesignerStub = Substitute.For<IScreenRootDesigner>();

            m_DesignerHostStub = Substitute.For<INeoDesignerHost>();
            m_DesignerHostStub.RootDesigner.Returns(m_ScreenRootDesignerStub);

            m_CrossReferenceItems = new List<ICrossReferenceItem>();
            AddCrossReferenceItem(DummyResourceFileName, DummyResourceObjectFullName);

            m_ProjectItemMock = Substitute.For<IDesignerProjectItem, ICompileUnit, ICrossReferenceItemSource>(); // ???
            m_ProjectItemMock.DesignerHost.Returns(m_DesignerHostStub);
            m_ProjectItemMock.Filename.Returns(DesignerFileName);

            m_RenderableControlResourceHelperStub = Substitute.For<RenderableControlResourcesHelper>(m_ProjectItemMock);

            ((ICrossReferenceItemSource)m_ProjectItemMock).GetReferences<ICrossReferenceItem>(CrossReferenceTypes.Renderable.ToString())
                .Returns(m_CrossReferenceItems);
        }

        private void AddCrossReferenceItem(string sourceFullName, string targetFullName)
        {
            ICrossReferenceItem crossReferenceItem = Substitute.For<ICrossReferenceItem>();
            crossReferenceItem.SourceFullName.Returns(sourceFullName);
            crossReferenceItem.TargetFullName.Returns(targetFullName);
            m_CrossReferenceItems.Add(crossReferenceItem);
        }

        [TearDown]
        public void TestTearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void WriteRenderStateResourceIfResourceDoesNotExistInZipOrAsTempFile()
        {
            var element = Substitute.For<FrameworkElement, IRenderableState, ISupportsTransparency>();

            element.Width = 100;
            element.Height = 100;

            m_ScreenRootDesignerStub.FindElementByName(Arg.Any<string>()).Returns(element);
            m_ScreenRootDesignerStub.Elements.Returns(new List<FrameworkElement>() { element });

            m_BitmapHelperMock.ConvertToCETransparencyFormat(Arg.Any<BitmapSource>()).Returns(new BitmapImage());
            m_BitmapHelperMock.SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>()).Returns(true);

            m_RenderableServiceStub.TryGetLastModifiedDate(Arg.Any<string>(), out Arg.Any<DateTime>())
                .Returns(x => {
                    x[1] = DateTime.MinValue;
                    return false; 
                });
            m_FileHelperStub.Exists(m_DummyResourceFilePath).Returns(false);

            ((IRenderableState)element).Render(Arg.Any<string>()).Returns(new BitmapImage());

            m_Generator.GenerateResources(m_ProjectItemMock, false, m_RenderableControlResourceHelperStub);

            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).ConvertToCETransparencyFormat(Arg.Any<BitmapSource>());
            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).WriteImageToStream(Arg.Any<Stream>(), Arg.Any<BitmapSource>(), Arg.Any<BitmapEncoder>());
            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>());
        }

        [Test]
        public void WriteRenderStateResourceIfBothZipAndTempFileAreOutDated()
        {
            var element = Substitute.For<FrameworkElement, IRenderableState, ISupportsTransparency>();
            var anElement = Substitute.For<FrameworkElement, IRenderable, ISupportsTransparency>();

            element.Width = 100;
            element.Height = 100;

            m_ScreenRootDesignerStub.FindElementByName(Arg.Any<string>()).Returns(element, anElement);
            m_ScreenRootDesignerStub.Elements.Returns(new List<FrameworkElement>() { element });

            m_BitmapHelperMock.ConvertToCETransparencyFormat(Arg.Any<BitmapSource>()).Returns(new BitmapImage());
            m_BitmapHelperMock.SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>()).Returns(true);

            DateTime designerDate = DateTime.Now;

            m_RenderableServiceStub.TryGetLastModifiedDate(Arg.Any<string>(), out Arg.Any<DateTime>())
                .Returns(x => {
                    x[1] = designerDate.Subtract(TimeSpan.FromDays(1));
                    return true; 
                });

            m_FileHelperStub.Exists(m_DummyResourceFilePath).Returns(true);
            m_FileHelperStub.GetLastWriteTime(m_DummyResourceFilePath).Returns(designerDate.Subtract(TimeSpan.FromDays(1)));
            m_FileHelperStub.Exists(m_DesignerFilePath).Returns(true);
            m_FileHelperStub.GetLastWriteTime(m_DesignerFilePath).Returns(designerDate);

            ((IRenderableState)element).Render(Arg.Any<string>()).Returns(new BitmapImage());

            m_Generator.GenerateResources(m_ProjectItemMock, false, m_RenderableControlResourceHelperStub);

            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).ConvertToCETransparencyFormat(Arg.Any<BitmapSource>());
            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).WriteImageToStream(Arg.Any<Stream>(), Arg.Any<BitmapSource>(), Arg.Any<BitmapEncoder>());
            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>());
        }

        [Test]
        public void WriteRenderStateResourceIfForcedEvenIfZipAndTempFileAreUpToDate()
        {
            var element = Substitute.For<FrameworkElement, IRenderableState, ISupportsTransparency>();

            element.Width = 100;
            element.Height = 100;

            m_ScreenRootDesignerStub.FindElementByName(Arg.Any<string>()).Returns(element);
            m_ScreenRootDesignerStub.Elements.Returns(new List<FrameworkElement>() { element });

            m_BitmapHelperMock.ConvertToCETransparencyFormat(Arg.Any<BitmapSource>()).Returns(new BitmapImage());
            m_BitmapHelperMock.SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>()).Returns(true);

            DateTime designerDate = DateTime.Now;

            m_RenderableServiceStub.TryGetLastModifiedDate(Arg.Any<string>(), out Arg.Any<DateTime>())
                .Returns(x => {
                    x[1] = designerDate.Add(TimeSpan.FromDays(1));
                    return true;
                });

            m_FileHelperStub.Exists(m_DummyResourceFilePath).Returns(true);
            m_FileHelperStub.GetLastWriteTime(m_DummyResourceFilePath).Returns(designerDate.Add(TimeSpan.FromDays(1)));
            m_FileHelperStub.Exists(m_DesignerFilePath).Returns(true);
            m_FileHelperStub.GetLastWriteTime(m_DesignerFilePath).Returns(designerDate);

            ((IRenderableState)element).Render(Arg.Any<string>()).Returns(new BitmapImage());

            m_Generator.GenerateResources(m_ProjectItemMock, true, m_RenderableControlResourceHelperStub);

            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).ConvertToCETransparencyFormat(Arg.Any<BitmapSource>());
            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).WriteImageToStream(Arg.Any<Stream>(), Arg.Any<BitmapSource>(), Arg.Any<BitmapEncoder>());
            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>());
        }

        [Test]
        public void WriteResourceWithoutTransparencyIfResourceDoesNotExistInZipOrAsTempFile()
        {
            var element = Substitute.For<FrameworkElement, IRenderable, ISupportsTransparency>();

            element.Width = 100;
            element.Height = 100;
            ((ISupportsTransparency)element).RequiresTransparency = true;

            m_ScreenRootDesignerStub.FindElementByName(Arg.Any<string>()).Returns(element);
            m_ScreenRootDesignerStub.Elements.Returns(new List<FrameworkElement>() { element });

            m_BitmapHelperMock.ConvertToCETransparencyFormat(Arg.Any<BitmapSource>()).Returns(new BitmapImage());
            m_BitmapHelperMock.WriteImageToStream(Arg.Any<Stream>(), Arg.Any<BitmapSource>(), Arg.Any<BitmapEncoder>());
            m_BitmapHelperMock.SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>()).Returns(true);

            m_RenderableServiceStub.TryGetLastModifiedDate(Arg.Any<string>(), out Arg.Any<DateTime>())
                .Returns(x => {
                    x[1] = DateTime.MinValue;
                    return false;
                });
            m_FileHelperStub.Exists(m_DummyResourceFilePath).Returns(false);

            ((IRenderable)element).Render().Returns(new BitmapImage());

            m_Generator.GenerateResources(m_ProjectItemMock, false, m_RenderableControlResourceHelperStub);

            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).ConvertToCETransparencyFormat(Arg.Any<BitmapSource>());
            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).WriteImageToStream(Arg.Any<Stream>(), Arg.Any<BitmapSource>(), Arg.Any<BitmapEncoder>());
            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>());
        }

        [Test]
        public void WriteResourceWithoutTransparencyIfBothZipAndTempFileAreOutDated()
        {
            var element = Substitute.For<FrameworkElement, IRenderable, ISupportsTransparency>();
            var anElement = Substitute.For<FrameworkElement, IRenderable, ISupportsTransparency>();

            element.Width = 100;
            element.Height = 100;
            ((ISupportsTransparency)element).RequiresTransparency = false;

            m_ScreenRootDesignerStub.FindElementByName(Arg.Any<string>()).Returns(element);
            m_ScreenRootDesignerStub.Elements.Returns(new List<FrameworkElement>() { element });
            m_ScreenRootDesignerStub.FindElementByName(Arg.Any<string>()).Returns(anElement);

            m_BitmapHelperMock.ConvertToCETransparencyFormat(Arg.Any<BitmapSource>()).Returns(new BitmapImage());
            m_BitmapHelperMock.ConvertBitmapToBitmapSource(Arg.Any<Bitmap>()).Returns(new BitmapImage());
            m_BitmapHelperMock.SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>()).Returns(true);

            DateTime designerDate = DateTime.Now;

            m_RenderableServiceStub.TryGetLastModifiedDate(Arg.Any<string>(), out Arg.Any<DateTime>())
                .Returns(x =>
                {
                    x[1] = designerDate.Subtract(TimeSpan.FromDays(1));
                    return true;
                });

            m_FileHelperStub.Exists(m_DummyResourceFilePath).Returns(true);
            m_FileHelperStub.GetLastWriteTime(m_DummyResourceFilePath).Returns(designerDate.Subtract(TimeSpan.FromDays(1)));
            m_FileHelperStub.Exists(m_DesignerFilePath).Returns(true);
            m_FileHelperStub.GetLastWriteTime(m_DesignerFilePath).Returns(designerDate);

            m_RenderableControlResourceHelperStub.GetLayoutSlot(Arg.Any<FrameworkElement>()).Returns(new Rect(0, 0, 100, 100));
            m_RenderableControlResourceHelperStub.RenderBitmap(Arg.Any<FrameworkElement>()).Returns(new Bitmap(100, 100));

            m_Generator.GenerateResources(m_ProjectItemMock, false, m_RenderableControlResourceHelperStub);

            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).ConvertToCETransparencyFormat(Arg.Any<BitmapSource>());
            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).ConvertBitmapToBitmapSource(Arg.Any<Bitmap>());
            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).WriteImageToStream(Arg.Any<Stream>(), Arg.Any<BitmapSource>(), Arg.Any<BitmapEncoder>());
            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>());
        }

        [Test]
        public void WriteResourceWithoutTransparencyIfForcedEvenIfZipAndTempFileAreUpToDate()
        {
            var element = Substitute.For<FrameworkElement, IRenderable, ISupportsTransparency>();

            element.Width = 100;
            element.Height = 100;
            ((ISupportsTransparency)element).RequiresTransparency = false;

            m_ScreenRootDesignerStub.FindElementByName(Arg.Any<string>()).Returns(element);
            m_ScreenRootDesignerStub.Elements.Returns(new List<FrameworkElement>() { element });

            m_BitmapHelperMock.ConvertToCETransparencyFormat(Arg.Any<BitmapSource>()).Returns(new BitmapImage());
            m_BitmapHelperMock.ConvertBitmapToBitmapSource(Arg.Any<Bitmap>()).Returns(new BitmapImage());
            m_BitmapHelperMock.SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>()).Returns(true);

            DateTime designerDate = DateTime.Now;

            m_RenderableServiceStub.TryGetLastModifiedDate(Arg.Any<string>(), out Arg.Any<DateTime>())
                .Returns(x =>
                {
                    x[1] = designerDate.Add(TimeSpan.FromDays(1));
                    return true;
                });

            m_FileHelperStub.Exists(m_DummyResourceFilePath).Returns(true);
            m_FileHelperStub.GetLastWriteTime(m_DummyResourceFilePath).Returns(designerDate.Add(TimeSpan.FromDays(1)));
            m_FileHelperStub.Exists(m_DesignerFilePath).Returns(true);
            m_FileHelperStub.GetLastWriteTime(m_DesignerFilePath).Returns(designerDate);

            m_RenderableControlResourceHelperStub.GetLayoutSlot(Arg.Any<FrameworkElement>()).Returns(new Rect(0, 0, 100, 100));
            m_RenderableControlResourceHelperStub.RenderBitmap(Arg.Any<FrameworkElement>()).Returns(new Bitmap(100, 100));

            m_Generator.GenerateResources(m_ProjectItemMock, true, m_RenderableControlResourceHelperStub);

            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).ConvertToCETransparencyFormat(Arg.Any<BitmapSource>());
            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).ConvertBitmapToBitmapSource(Arg.Any<Bitmap>());
            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).WriteImageToStream(Arg.Any<Stream>(), Arg.Any<BitmapSource>(), Arg.Any<BitmapEncoder>());
            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>());
        }

        [Test]
        public void WriteResourceWithTransparencyIfResourceDoesNotExistInZipOrAsTempFile()
        {
            var element = Substitute.For<FrameworkElement, IRenderable, ISupportsTransparency>();

            element.Width = 100;
            element.Height = 100;
            ((ISupportsTransparency)element).RequiresTransparency = true;

            m_ScreenRootDesignerStub.FindElementByName(Arg.Any<string>()).Returns(element);
            m_ScreenRootDesignerStub.Elements.Returns(new List<FrameworkElement>() { element });

            m_BitmapHelperMock.ConvertToCETransparencyFormat(Arg.Any<BitmapSource>()).Returns(new BitmapImage());
            m_BitmapHelperMock.WriteImageToStream(Arg.Any<Stream>(), Arg.Any<BitmapSource>(), Arg.Any<BitmapEncoder>());
            m_BitmapHelperMock.SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>()).Returns(true);

            m_RenderableServiceStub.TryGetLastModifiedDate(Arg.Any<string>(), out Arg.Any<DateTime>())
                .Returns(x =>
                {
                    x[1] = DateTime.MinValue;
                    return false;
                });
            m_FileHelperStub.Exists(m_DummyResourceFilePath).Returns(false);

            ((IRenderable)element).Render().Returns(new BitmapImage());

            m_Generator.GenerateResources(m_ProjectItemMock, false, m_RenderableControlResourceHelperStub);

            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).ConvertToCETransparencyFormat(Arg.Any<BitmapSource>());
            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).WriteImageToStream(Arg.Any<Stream>(), Arg.Any<BitmapSource>(), Arg.Any<BitmapEncoder>());
            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>());
        }

        [Test]
        public void WritesResourceWithTransparencyIfBothZipAndTempFileAreOutDated()
        {
            var element = Substitute.For<FrameworkElement, IRenderable, ISupportsTransparency>();
            var anElement = Substitute.For<FrameworkElement, IRenderable, ISupportsTransparency>();

            element.Width = 100;
            element.Height = 100;
            ((ISupportsTransparency)element).RequiresTransparency = true;

            m_ScreenRootDesignerStub.FindElementByName(Arg.Any<string>()).Returns(element, anElement);
            m_ScreenRootDesignerStub.Elements.Returns(new List<FrameworkElement>() { element });

            m_BitmapHelperMock.ConvertToCETransparencyFormat(Arg.Any<BitmapSource>()).Returns(new BitmapImage());
            m_BitmapHelperMock.SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>()).Returns(true);

            DateTime designerDate = DateTime.Now;

            m_RenderableServiceStub.TryGetLastModifiedDate(Arg.Any<string>(), out Arg.Any<DateTime>())
                .Returns(x =>
                {
                    x[1] = designerDate.Subtract(TimeSpan.FromDays(1));
                    return true;
                });

            m_FileHelperStub.Exists(m_DummyResourceFilePath).Returns(true);
            m_FileHelperStub.GetLastWriteTime(m_DummyResourceFilePath).Returns(designerDate.Subtract(TimeSpan.FromDays(1)));
            m_FileHelperStub.Exists(m_DesignerFilePath).Returns(true);
            m_FileHelperStub.GetLastWriteTime(m_DesignerFilePath).Returns(designerDate);

            ((IRenderable)element).Render().Returns(new BitmapImage());

            m_Generator.GenerateResources(m_ProjectItemMock, false, m_RenderableControlResourceHelperStub);

            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).ConvertToCETransparencyFormat(Arg.Any<BitmapSource>());
            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).WriteImageToStream(Arg.Any<Stream>(), Arg.Any<BitmapSource>(), Arg.Any<BitmapEncoder>());
            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>());
        }

        [Test]
        public void WriteResourceWithTransparencyIfForcedEvenIfZipAndTempFileAreUpToDate()
        {
            var element = Substitute.For<FrameworkElement, IRenderable, ISupportsTransparency>();

            element.Width = 100;
            element.Height = 100;
            ((ISupportsTransparency)element).RequiresTransparency = true;

            m_ScreenRootDesignerStub.FindElementByName(Arg.Any<string>()).Returns(element);
            m_ScreenRootDesignerStub.Elements.Returns(new List<FrameworkElement>() { element });

            m_BitmapHelperMock.ConvertToCETransparencyFormat(Arg.Any<BitmapSource>()).Returns(new BitmapImage());
            m_BitmapHelperMock.SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>()).Returns(true);

            DateTime designerDate = DateTime.Now;

            m_RenderableServiceStub.TryGetLastModifiedDate(Arg.Any<string>(), out Arg.Any<DateTime>())
                .Returns(x =>
                {
                    x[1] = designerDate.Add(TimeSpan.FromDays(1));
                    return true;
                });

            m_FileHelperStub.Exists(m_DummyResourceFilePath).Returns(true);
            m_FileHelperStub.GetLastWriteTime(m_DummyResourceFilePath).Returns(designerDate.Add(TimeSpan.FromDays(1)));
            m_FileHelperStub.Exists(m_DesignerFilePath).Returns(true);
            m_FileHelperStub.GetLastWriteTime(m_DesignerFilePath).Returns(designerDate);

            ((IRenderable)element).Render().Returns(new BitmapImage());

            m_Generator.GenerateResources(m_ProjectItemMock, true, m_RenderableControlResourceHelperStub);

            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).ConvertToCETransparencyFormat(Arg.Any<BitmapSource>());
            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).WriteImageToStream(Arg.Any<Stream>(), Arg.Any<BitmapSource>(), Arg.Any<BitmapEncoder>());
            m_BitmapHelperMock.ReceivedWithAnyArgs(m_CrossReferenceItems.Count).SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>());
        }

        [Test]
        public void DoNotWriteResourceIfOnlyTempFileIsOutDated()
        {
            var screenRootDesignerStub = Substitute.For<IScreenRootDesigner>();
            screenRootDesignerStub.FindElementByName(Arg.Any<string>()).Returns(Substitute.For<FrameworkElement, IRenderable>());

            var designerHostStub = Substitute.For<INeoDesignerHost>();
            designerHostStub.RootDesigner.Returns(screenRootDesignerStub);

            m_BitmapHelperMock.ConvertToCETransparencyFormat(Arg.Any<BitmapSource>()).Returns(x => null);
            m_BitmapHelperMock.SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>()).Returns(true);

            DateTime designerDate = DateTime.Now;
            m_RenderableServiceStub.TryGetLastModifiedDate(Arg.Any<string>(), out Arg.Any<DateTime>())
                .Returns(x =>
                {
                    x[1] = designerDate.Add(TimeSpan.FromDays(1));
                    return true;
                });
            m_FileHelperStub.Exists(m_DummyResourceFilePath).Returns(true);
            m_FileHelperStub.GetLastWriteTime(m_DummyResourceFilePath).Returns(designerDate.Subtract(TimeSpan.FromDays(1)));
            m_FileHelperStub.Exists(m_DesignerFilePath).Returns(true);
            m_FileHelperStub.GetLastWriteTime(m_DesignerFilePath).Returns(designerDate);

            m_Generator.GenerateResources(m_ProjectItemMock);

            m_BitmapHelperMock.DidNotReceiveWithAnyArgs().ConvertToCETransparencyFormat(Arg.Any<BitmapSource>());
            m_BitmapHelperMock.DidNotReceiveWithAnyArgs().WriteImageToStream(Arg.Any<Stream>(), Arg.Any<BitmapSource>(), Arg.Any<BitmapEncoder>());
            m_BitmapHelperMock.DidNotReceiveWithAnyArgs().SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>());
        }

        [Test]
        public void DoNotWriteResourceIfOnlyZipFileIsOutDated()
        {
            var screenRootDesignerStub = Substitute.For<IScreenRootDesigner>();
            screenRootDesignerStub.FindElementByName(Arg.Any<string>()).Returns(Substitute.For<FrameworkElement, IRenderable>());

            var designerHostStub = Substitute.For<INeoDesignerHost>();
            designerHostStub.RootDesigner.Returns(screenRootDesignerStub);

            m_BitmapHelperMock.ConvertToCETransparencyFormat(Arg.Any<BitmapSource>()).Returns(x => null);
            m_BitmapHelperMock.SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>()).Returns(true);

            DateTime designerDate = DateTime.Now;
            m_RenderableServiceStub.TryGetLastModifiedDate(Arg.Any<string>(), out Arg.Any<DateTime>())
                .Returns(x =>
                {
                    x[1] = designerDate.Add(TimeSpan.FromDays(1));
                    return true;
                });
            m_FileHelperStub.Exists(m_DummyResourceFilePath).Returns(true);
            m_FileHelperStub.GetLastWriteTime(m_DummyResourceFilePath).Returns(designerDate.Add(TimeSpan.FromDays(1)));
            m_FileHelperStub.Exists(m_DesignerFilePath).Returns(true);
            m_FileHelperStub.GetLastWriteTime(m_DesignerFilePath).Returns(designerDate);

            m_Generator.GenerateResources(m_ProjectItemMock);

            m_BitmapHelperMock.DidNotReceiveWithAnyArgs().ConvertToCETransparencyFormat(Arg.Any<BitmapSource>());
            m_BitmapHelperMock.DidNotReceiveWithAnyArgs().WriteImageToStream(Arg.Any<Stream>(), Arg.Any<BitmapSource>(), Arg.Any<BitmapEncoder>());
            m_BitmapHelperMock.DidNotReceiveWithAnyArgs().SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>());
        }

        [Test]
        public void DoNotWriteResourceIfOnlyZipFileIsMissing()
        {
            m_ScreenRootDesignerStub.FindElementByName(Arg.Any<string>()).Returns(Substitute.For<FrameworkElement, IRenderable>());

            m_BitmapHelperMock.ConvertToCETransparencyFormat(Arg.Any<BitmapSource>()).Returns(x => null);
            m_BitmapHelperMock.SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>()).Returns(true);

            DateTime designerDate = DateTime.Now;
            m_RenderableServiceStub.TryGetLastModifiedDate(Arg.Any<string>(), out Arg.Any<DateTime>())
                .Returns(x =>
                {
                    x[1] = DateTime.MinValue;
                    return false;
                });
            m_FileHelperStub.Exists(m_DummyResourceFilePath).Returns(true);
            m_FileHelperStub.GetLastWriteTime(m_DummyResourceFilePath).Returns(designerDate.Add(TimeSpan.FromDays(1)));
            m_FileHelperStub.Exists(m_DesignerFilePath).Returns(true);
            m_FileHelperStub.GetLastWriteTime(m_DesignerFilePath).Returns(designerDate);

            m_Generator.GenerateResources(m_ProjectItemMock);

            m_BitmapHelperMock.DidNotReceiveWithAnyArgs().ConvertToCETransparencyFormat(Arg.Any<BitmapSource>());
            m_BitmapHelperMock.DidNotReceiveWithAnyArgs().WriteImageToStream(Arg.Any<Stream>(), Arg.Any<BitmapSource>(), Arg.Any<BitmapEncoder>());
            m_BitmapHelperMock.DidNotReceiveWithAnyArgs().SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>());
        }

        [Test]
        public void DoNotWriteResourceIfOnlyTempFileIsMissing()
        {
            m_ScreenRootDesignerStub.FindElementByName(Arg.Any<string>()).Returns(Substitute.For<FrameworkElement, IRenderable>());

            m_BitmapHelperMock.ConvertToCETransparencyFormat(Arg.Any<BitmapSource>()).Returns(x => null);
            m_BitmapHelperMock.SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>()).Returns(true);

            DateTime designerDate = DateTime.Now;
            m_RenderableServiceStub.TryGetLastModifiedDate(Arg.Any<string>(), out Arg.Any<DateTime>())
                .Returns(x =>
                 {
                     x[1] = designerDate.Add(TimeSpan.FromDays(1));
                     return true;
                 });
            m_FileHelperStub.Exists(m_DummyResourceFilePath).Returns(false);
            m_FileHelperStub.Exists(m_DesignerFilePath).Returns(true);
            m_FileHelperStub.GetLastWriteTime(m_DesignerFilePath).Returns(designerDate);

            m_Generator.GenerateResources(m_ProjectItemMock);

            m_BitmapHelperMock.DidNotReceiveWithAnyArgs().ConvertToCETransparencyFormat(Arg.Any<BitmapSource>());
            m_BitmapHelperMock.DidNotReceiveWithAnyArgs().WriteImageToStream(Arg.Any<Stream>(), Arg.Any<BitmapSource>(), Arg.Any<BitmapEncoder>());
            m_BitmapHelperMock.DidNotReceiveWithAnyArgs().SaveStreamToBitmap(Arg.Any<Stream>(), Arg.Any<string>());
        }
    }
}
