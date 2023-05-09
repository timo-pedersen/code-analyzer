using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Core.Api.CrossReference;
using System.Drawing;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Core.Component.Api.Design;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.vNext.Gaps;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;
using Neo.ApplicationFramework.Tools.Build.BuildManager.Implementations;
using Neo.ApplicationFramework.Tools.Build.BuildManager.Abstractions;

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

        private IResourceGenerator m_Generator;
        private DirectoryHelper m_DirectoryHelperStub;
        private FileHelper m_FileHelperStub;
        private BitmapHelperStub m_BitmapHelperMock;
        private IScreenRootDesigner m_ScreenRootDesignerStub;
        private INeoDesignerHost m_DesignerHostStub;
        private IRenderableControlServiceIde m_RenderableServiceStub;
        private IGapService m_GapService;
        private IDesignerProjectItem m_ProjectItemMock;
        private RenderableControlResourcesHelper m_RenderableControlResourceHelperStub;
        private MockRepository m_MockRepository;
        private List<ICrossReferenceItem> m_CrossReferenceItems;

        [SetUp]
        public void TestSetup()
        {
            m_MockRepository = new MockRepository();

            m_RenderableServiceStub = TestHelper.AddServiceStub<IRenderableControlServiceIde>();

            var targetInfo = MockRepository.GenerateStub<ITargetInfo>();
            targetInfo.TempPath = TempPath;
            targetInfo.ProjectPath = ProjectPath;

            var target = MockRepository.GenerateStub<ITarget>();
            target.Stub(x => x.Id).Return(TargetPlatform.WindowsCE);

            m_GapService = TestHelper.AddServiceStub<IGapService>();

            m_DirectoryHelperStub = MockRepository.GenerateStub<DirectoryHelper>();
            m_DirectoryHelperStub.Stub(x => x.Exists(TempPath)).Return(true);

            m_FileHelperStub = MockRepository.GenerateStub<FileHelper>();

            m_BitmapHelperMock = m_MockRepository.DynamicMock<BitmapHelperStub>();

            m_Generator = new RenderableControlResourcesGenerator(targetInfo, target, m_DirectoryHelperStub, m_FileHelperStub, m_BitmapHelperMock);

            m_ScreenRootDesignerStub = MockRepository.GenerateStub<IScreenRootDesigner>();

            m_DesignerHostStub = MockRepository.GenerateStub<INeoDesignerHost>();
            m_DesignerHostStub.Stub(x => x.RootDesigner).Return(m_ScreenRootDesignerStub);

            m_CrossReferenceItems = new List<ICrossReferenceItem>();
            AddCrossReferenceItem(DummyResourceFileName, DummyResourceObjectFullName);

            m_ProjectItemMock = m_MockRepository.DynamicMultiMock<IDesignerProjectItem>(typeof(ICompileUnit), typeof(ICrossReferenceItemSource));
            m_ProjectItemMock.Stub(x => x.DesignerHost).Return(m_DesignerHostStub);
            m_ProjectItemMock.Expect(x => x.Filename).Return(DesignerFileName).Repeat.Any();

            m_RenderableControlResourceHelperStub = MockRepository.GenerateStub<RenderableControlResourcesHelper>(m_ProjectItemMock);

            ((ICrossReferenceItemSource)m_ProjectItemMock).Expect(x => x.GetReferences<ICrossReferenceItem>(CrossReferenceTypes.Renderable.ToString())).Repeat.Once().Return(m_CrossReferenceItems);
            m_MockRepository.ReplayAll();
        }

        private void AddCrossReferenceItem(string sourceFullName, string targetFullName)
        {
            ICrossReferenceItem crossReferenceItem = MockRepository.GenerateStub<ICrossReferenceItem>();
            crossReferenceItem.Stub(x => x.SourceFullName).Return(sourceFullName);
            crossReferenceItem.Stub(x => x.TargetFullName).Return(targetFullName);
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
            var element = m_MockRepository.DynamicMultiMock<FrameworkElement>(typeof(IRenderableState), typeof(ISupportsTransparency));

            element.Width = 100;
            element.Height = 100;

            m_ScreenRootDesignerStub.Stub(x => x.FindElementByName(Arg<string>.Is.Anything)).Return(element);
            m_ScreenRootDesignerStub.Stub(x => x.Elements).Return(new List<FrameworkElement>() { element });

            m_BitmapHelperMock.Stub(x => x.ConvertToCETransparencyFormat(Arg<BitmapSource>.Is.Anything)).Return(new BitmapImage()).Repeat.Times(m_CrossReferenceItems.Count);
            m_BitmapHelperMock.Expect(x => x.WriteImageToStream(Arg<Stream>.Is.Anything, Arg<BitmapSource>.Is.Anything, Arg<BitmapEncoder>.Is.Anything)).Repeat.Times(m_CrossReferenceItems.Count);
            m_BitmapHelperMock.Expect(x => x.SaveStreamToBitmap(Arg<Stream>.Is.Anything, Arg<string>.Is.Anything)).Return(true).Repeat.Times(m_CrossReferenceItems.Count);

            m_RenderableServiceStub.Stub(x => x.TryGetLastModifiedDate(Arg<string>.Is.Anything, out Arg<DateTime>.Out(DateTime.MinValue).Dummy)).Return(false);
            m_FileHelperStub.Stub(x => x.Exists(m_DummyResourceFilePath)).Return(false);

            m_RenderableControlResourceHelperStub.Stub(x => x.GetLayoutSlot(Arg<FrameworkElement>.Is.Anything)).Return(new Rect(0, 0, 100, 100));
            m_RenderableControlResourceHelperStub.Stub(x => x.RenderBitmap(Arg<FrameworkElement>.Is.Anything)).IgnoreArguments().Return(new Bitmap(100, 100));

            m_Generator.GenerateResources(m_ProjectItemMock, false, m_RenderableControlResourceHelperStub);

            m_BitmapHelperMock.AssertWasCalled(x => x.ConvertToCETransparencyFormat(Arg<BitmapSource>.Is.Anything));

            m_ProjectItemMock.VerifyAllExpectations();
            m_BitmapHelperMock.VerifyAllExpectations();
        }

        [Test]
        public void WriteRenderStateResourceIfBothZipAndTempFileAreOutDated()
        {
            var element = m_MockRepository.DynamicMultiMock<FrameworkElement>(typeof(IRenderableState), typeof(ISupportsTransparency));

            element.Width = 100;
            element.Height = 100;

            m_ScreenRootDesignerStub.Stub(x => x.FindElementByName(Arg<string>.Is.Anything)).Return(element);
            m_ScreenRootDesignerStub.Stub(x => x.Elements).Return(new List<FrameworkElement>() { element });
            m_ScreenRootDesignerStub.Stub(x => x.FindElementByName(Arg<string>.Is.Anything)).Return(m_MockRepository.DynamicMultiMock<FrameworkElement>(typeof(IRenderable), typeof(ISupportsTransparency)));

            m_BitmapHelperMock.Stub(x => x.ConvertToCETransparencyFormat(Arg<BitmapSource>.Is.Anything)).Return(new BitmapImage()).Repeat.Times(m_CrossReferenceItems.Count);
            m_BitmapHelperMock.Expect(x => x.WriteImageToStream(Arg<Stream>.Is.Anything, Arg<BitmapSource>.Is.Anything, Arg<BitmapEncoder>.Is.Anything)).Repeat.Times(m_CrossReferenceItems.Count);
            m_BitmapHelperMock.Expect(x => x.SaveStreamToBitmap(Arg<Stream>.Is.Anything, Arg<string>.Is.Anything)).Return(true).Repeat.Times(m_CrossReferenceItems.Count);

            DateTime designerDate = DateTime.Now;

            m_RenderableServiceStub.Stub(x => x.TryGetLastModifiedDate(Arg<string>.Is.Anything, out Arg<DateTime>.Out(designerDate.Subtract(TimeSpan.FromDays(1))).Dummy)).Return(true);

            m_FileHelperStub.Stub(x => x.Exists(m_DummyResourceFilePath)).Return(true);
            m_FileHelperStub.Stub(x => x.GetLastWriteTime(m_DummyResourceFilePath)).Return(designerDate.Subtract(TimeSpan.FromDays(1)));
            m_FileHelperStub.Stub(x => x.Exists(m_DesignerFilePath)).Return(true);
            m_FileHelperStub.Stub(x => x.GetLastWriteTime(m_DesignerFilePath)).Return(designerDate);

            m_RenderableControlResourceHelperStub.Stub(x => x.GetLayoutSlot(Arg<FrameworkElement>.Is.Anything)).Return(new Rect(0, 0, 100, 100));
            m_RenderableControlResourceHelperStub.Stub(x => x.RenderBitmap(Arg<FrameworkElement>.Is.Anything)).IgnoreArguments().Return(new Bitmap(100, 100));

            m_Generator.GenerateResources(m_ProjectItemMock, false, m_RenderableControlResourceHelperStub);

            m_BitmapHelperMock.AssertWasCalled(x => x.ConvertToCETransparencyFormat(Arg<BitmapSource>.Is.Anything));

            m_ProjectItemMock.VerifyAllExpectations();
            m_BitmapHelperMock.VerifyAllExpectations();
        }

        [Test]
        public void WriteRenderStateResourceIfForcedEvenIfZipAndTempFileAreUpToDate()
        {
            var element = m_MockRepository.DynamicMultiMock<FrameworkElement>(typeof(IRenderableState), typeof(ISupportsTransparency));

            element.Width = 100;
            element.Height = 100;

            m_ScreenRootDesignerStub.Stub(x => x.FindElementByName(Arg<string>.Is.Anything)).Return(element);
            m_ScreenRootDesignerStub.Stub(x => x.Elements).Return(new List<FrameworkElement>() { element });

            m_BitmapHelperMock.Stub(x => x.ConvertToCETransparencyFormat(Arg<BitmapSource>.Is.Anything)).Return(new BitmapImage()).Repeat.Times(m_CrossReferenceItems.Count);
            m_BitmapHelperMock.Expect(x => x.WriteImageToStream(Arg<Stream>.Is.Anything, Arg<BitmapSource>.Is.Anything, Arg<BitmapEncoder>.Is.Anything)).Repeat.Times(m_CrossReferenceItems.Count);
            m_BitmapHelperMock.Expect(x => x.SaveStreamToBitmap(Arg<Stream>.Is.Anything, Arg<string>.Is.Anything)).Return(true).Repeat.Times(m_CrossReferenceItems.Count);

            DateTime designerDate = DateTime.Now;

            m_RenderableServiceStub.Stub(x => x.TryGetLastModifiedDate(Arg<string>.Is.Anything, out Arg<DateTime>.Out(designerDate.Add(TimeSpan.FromDays(1))).Dummy)).Return(true);

            m_FileHelperStub.Stub(x => x.Exists(m_DummyResourceFilePath)).Return(true);
            m_FileHelperStub.Stub(x => x.GetLastWriteTime(m_DummyResourceFilePath)).Return(designerDate.Add(TimeSpan.FromDays(1)));
            m_FileHelperStub.Stub(x => x.Exists(m_DesignerFilePath)).Return(true);
            m_FileHelperStub.Stub(x => x.GetLastWriteTime(m_DesignerFilePath)).Return(designerDate);

            m_RenderableControlResourceHelperStub.Stub(x => x.GetLayoutSlot(Arg<FrameworkElement>.Is.Anything)).Return(new Rect(0, 0, 100, 100));
            m_RenderableControlResourceHelperStub.Stub(x => x.RenderBitmap(Arg<FrameworkElement>.Is.Anything)).IgnoreArguments().Return(new Bitmap(100, 100));

            m_Generator.GenerateResources(m_ProjectItemMock, true, m_RenderableControlResourceHelperStub);

            m_BitmapHelperMock.AssertWasCalled(x => x.ConvertToCETransparencyFormat(Arg<BitmapSource>.Is.Anything));

            m_ProjectItemMock.VerifyAllExpectations();
            m_BitmapHelperMock.VerifyAllExpectations();
        }

        [Test]
        public void WriteResourceWithoutTransparencyIfResourceDoesNotExistInZipOrAsTempFile()
        {
            var element = m_MockRepository.DynamicMultiMock<FrameworkElement>(typeof(IRenderable), typeof(ISupportsTransparency));
            ((ISupportsTransparency)element).Stub(x => x.RequiresTransparency).PropertyBehavior();

            element.Width = 100;
            element.Height = 100;
            ((ISupportsTransparency)element).RequiresTransparency = true;

            m_ScreenRootDesignerStub.Stub(x => x.FindElementByName(Arg<string>.Is.Anything)).Return(element);
            m_ScreenRootDesignerStub.Stub(x => x.Elements).Return(new List<FrameworkElement>() { element });

            m_BitmapHelperMock.Stub(x => x.ConvertToCETransparencyFormat(Arg<BitmapSource>.Is.Anything)).Return(new BitmapImage()).Repeat.Times(m_CrossReferenceItems.Count);
            m_BitmapHelperMock.Expect(x => x.WriteImageToStream(Arg<Stream>.Is.Anything, Arg<BitmapSource>.Is.Anything, Arg<BitmapEncoder>.Is.Anything)).Repeat.Times(m_CrossReferenceItems.Count);
            m_BitmapHelperMock.Expect(x => x.SaveStreamToBitmap(Arg<Stream>.Is.Anything, Arg<string>.Is.Anything)).Return(true).Repeat.Times(m_CrossReferenceItems.Count);

            m_RenderableServiceStub.Stub(x => x.TryGetLastModifiedDate(Arg<string>.Is.Anything, out Arg<DateTime>.Out(DateTime.MinValue).Dummy)).Return(false);
            m_FileHelperStub.Stub(x => x.Exists(m_DummyResourceFilePath)).Return(false);

            m_RenderableControlResourceHelperStub.Stub(x => x.GetLayoutSlot(Arg<FrameworkElement>.Is.Anything)).Return(new Rect(0, 0, 100, 100));
            m_RenderableControlResourceHelperStub.Stub(x => x.RenderBitmap(Arg<FrameworkElement>.Is.Anything)).IgnoreArguments().Return(new Bitmap(100, 100));

            m_Generator.GenerateResources(m_ProjectItemMock, false, m_RenderableControlResourceHelperStub);

            m_BitmapHelperMock.AssertWasCalled(x => x.ConvertToCETransparencyFormat(Arg<BitmapSource>.Is.Anything));

            m_ProjectItemMock.VerifyAllExpectations();
            m_BitmapHelperMock.VerifyAllExpectations();
        }

        [Test]
        public void WriteResourceWithoutTransparencyIfBothZipAndTempFileAreOutDated()
        {
            var element = m_MockRepository.DynamicMultiMock<FrameworkElement>(typeof(IRenderable), typeof(ISupportsTransparency));
            ((ISupportsTransparency)element).Stub(x => x.RequiresTransparency).PropertyBehavior();

            element.Width = 100;
            element.Height = 100;
            ((ISupportsTransparency)element).RequiresTransparency = false;

            m_ScreenRootDesignerStub.Stub(x => x.FindElementByName(Arg<string>.Is.Anything)).Return(element);
            m_ScreenRootDesignerStub.Stub(x => x.Elements).Return(new List<FrameworkElement>() { element });
            m_ScreenRootDesignerStub.Stub(x => x.FindElementByName(Arg<string>.Is.Anything)).Return(m_MockRepository.DynamicMultiMock<FrameworkElement>(typeof(IRenderable), typeof(ISupportsTransparency)));

            m_BitmapHelperMock.Stub(x => x.ConvertToCETransparencyFormat(Arg<BitmapSource>.Is.Anything)).Return(new BitmapImage()).Repeat.Times(m_CrossReferenceItems.Count);
            m_BitmapHelperMock.Stub(x => x.ConvertBitmapToBitmapSource(Arg<Bitmap>.Is.Anything)).Return(new BitmapImage()).Repeat.Times(m_CrossReferenceItems.Count);
            m_BitmapHelperMock.Expect(x => x.WriteImageToStream(Arg<Stream>.Is.Anything, Arg<BitmapSource>.Is.Anything, Arg<BitmapEncoder>.Is.Anything)).Repeat.Times(m_CrossReferenceItems.Count);
            m_BitmapHelperMock.Expect(x => x.SaveStreamToBitmap(Arg<Stream>.Is.Anything, Arg<string>.Is.Anything)).Return(true).Repeat.Times(m_CrossReferenceItems.Count);

            DateTime designerDate = DateTime.Now;

            m_RenderableServiceStub.Stub(x => x.TryGetLastModifiedDate(Arg<string>.Is.Anything, out Arg<DateTime>.Out(designerDate.Subtract(TimeSpan.FromDays(1))).Dummy)).Return(true);

            m_FileHelperStub.Stub(x => x.Exists(m_DummyResourceFilePath)).Return(true);
            m_FileHelperStub.Stub(x => x.GetLastWriteTime(m_DummyResourceFilePath)).Return(designerDate.Subtract(TimeSpan.FromDays(1)));
            m_FileHelperStub.Stub(x => x.Exists(m_DesignerFilePath)).Return(true);
            m_FileHelperStub.Stub(x => x.GetLastWriteTime(m_DesignerFilePath)).Return(designerDate);

            m_RenderableControlResourceHelperStub.Stub(x => x.GetLayoutSlot(Arg<FrameworkElement>.Is.Anything)).Return(new Rect(0, 0, 100, 100));
            m_RenderableControlResourceHelperStub.Stub(x => x.RenderBitmap(Arg<FrameworkElement>.Is.Anything)).IgnoreArguments().Return(new Bitmap(100, 100));

            m_Generator.GenerateResources(m_ProjectItemMock, false, m_RenderableControlResourceHelperStub);

            m_BitmapHelperMock.AssertWasCalled(x => x.ConvertToCETransparencyFormat(Arg<BitmapSource>.Is.Anything));

            m_ProjectItemMock.VerifyAllExpectations();
            m_BitmapHelperMock.VerifyAllExpectations();
        }

        [Test]
        public void WriteResourceWithoutTransparencyIfForcedEvenIfZipAndTempFileAreUpToDate()
        {
            var element = m_MockRepository.DynamicMultiMock<FrameworkElement>(typeof(IRenderable), typeof(ISupportsTransparency));
            ((ISupportsTransparency)element).Stub(x => x.RequiresTransparency).PropertyBehavior();

            element.Width = 100;
            element.Height = 100;
            ((ISupportsTransparency)element).RequiresTransparency = false;

            m_ScreenRootDesignerStub.Stub(x => x.FindElementByName(Arg<string>.Is.Anything)).Return(element);
            m_ScreenRootDesignerStub.Stub(x => x.Elements).Return(new List<FrameworkElement>() { element });

            m_BitmapHelperMock.Stub(x => x.ConvertToCETransparencyFormat(Arg<BitmapSource>.Is.Anything)).Return(new BitmapImage()).Repeat.Times(m_CrossReferenceItems.Count);
            m_BitmapHelperMock.Stub(x => x.ConvertBitmapToBitmapSource(Arg<Bitmap>.Is.Anything)).Return(new BitmapImage()).Repeat.Times(m_CrossReferenceItems.Count);
            m_BitmapHelperMock.Expect(x => x.WriteImageToStream(Arg<Stream>.Is.Anything, Arg<BitmapSource>.Is.Anything, Arg<BitmapEncoder>.Is.Anything)).Repeat.Times(m_CrossReferenceItems.Count);
            m_BitmapHelperMock.Expect(x => x.SaveStreamToBitmap(Arg<Stream>.Is.Anything, Arg<string>.Is.Anything)).Return(true).Repeat.Times(m_CrossReferenceItems.Count);

            DateTime designerDate = DateTime.Now;

            m_RenderableServiceStub.Stub(x => x.TryGetLastModifiedDate(Arg<string>.Is.Anything, out Arg<DateTime>.Out(designerDate.Add(TimeSpan.FromDays(1))).Dummy)).Return(true);

            m_FileHelperStub.Stub(x => x.Exists(m_DummyResourceFilePath)).Return(true);
            m_FileHelperStub.Stub(x => x.GetLastWriteTime(m_DummyResourceFilePath)).Return(designerDate.Add(TimeSpan.FromDays(1)));
            m_FileHelperStub.Stub(x => x.Exists(m_DesignerFilePath)).Return(true);
            m_FileHelperStub.Stub(x => x.GetLastWriteTime(m_DesignerFilePath)).Return(designerDate);

            m_RenderableControlResourceHelperStub.Stub(x => x.GetLayoutSlot(Arg<FrameworkElement>.Is.Anything)).Return(new Rect(0, 0, 100, 100));
            m_RenderableControlResourceHelperStub.Stub(x => x.RenderBitmap(Arg<FrameworkElement>.Is.Anything)).IgnoreArguments().Return(new Bitmap(100, 100));

            m_Generator.GenerateResources(m_ProjectItemMock, true, m_RenderableControlResourceHelperStub);

            m_BitmapHelperMock.AssertWasCalled(x => x.ConvertToCETransparencyFormat(Arg<BitmapSource>.Is.Anything));

            m_ProjectItemMock.VerifyAllExpectations();
            m_BitmapHelperMock.VerifyAllExpectations();
        }

        [Test]
        public void WriteResourceWithTransparencyIfResourceDoesNotExistInZipOrAsTempFile()
        {
            var element = m_MockRepository.DynamicMultiMock<FrameworkElement>(typeof(IRenderable), typeof(ISupportsTransparency));
            ((ISupportsTransparency)element).Stub(x => x.RequiresTransparency).PropertyBehavior();

            element.Width = 100;
            element.Height = 100;
            ((ISupportsTransparency)element).RequiresTransparency = true;

            m_ScreenRootDesignerStub.Stub(x => x.FindElementByName(Arg<string>.Is.Anything)).Return(element);
            m_ScreenRootDesignerStub.Stub(x => x.Elements).Return(new List<FrameworkElement>() { element });

            m_BitmapHelperMock.Stub(x => x.ConvertToCETransparencyFormat(Arg<BitmapSource>.Is.Anything)).Return(new BitmapImage()).Repeat.Times(m_CrossReferenceItems.Count);
            m_BitmapHelperMock.Expect(x => x.WriteImageToStream(Arg<Stream>.Is.Anything, Arg<BitmapSource>.Is.Anything, Arg<BitmapEncoder>.Is.Anything)).Repeat.Times(m_CrossReferenceItems.Count);
            m_BitmapHelperMock.Expect(x => x.SaveStreamToBitmap(Arg<Stream>.Is.Anything, Arg<string>.Is.Anything)).Return(true).Repeat.Times(m_CrossReferenceItems.Count);

            m_RenderableServiceStub.Stub(x => x.TryGetLastModifiedDate(Arg<string>.Is.Anything, out Arg<DateTime>.Out(DateTime.MinValue).Dummy)).Return(false);
            m_FileHelperStub.Stub(x => x.Exists(m_DummyResourceFilePath)).Return(false);

            m_RenderableControlResourceHelperStub.Stub(x => x.GetLayoutSlot(Arg<FrameworkElement>.Is.Anything)).Return(new Rect(0, 0, 100, 100));
            m_RenderableControlResourceHelperStub.Stub(x => x.RenderBitmap(Arg<FrameworkElement>.Is.Anything)).IgnoreArguments().Return(new Bitmap(100, 100));

            m_Generator.GenerateResources(m_ProjectItemMock, false, m_RenderableControlResourceHelperStub);

            m_BitmapHelperMock.AssertWasCalled(x => x.ConvertToCETransparencyFormat(Arg<BitmapSource>.Is.Anything));

            m_ProjectItemMock.VerifyAllExpectations();
            m_BitmapHelperMock.VerifyAllExpectations();
        }

        [Test]
        public void WritesResourceWithTransparencyIfBothZipAndTempFileAreOutDated()
        {
            var element = m_MockRepository.DynamicMultiMock<FrameworkElement>(typeof(IRenderable), typeof(ISupportsTransparency));
            ((ISupportsTransparency)element).Stub(x => x.RequiresTransparency).PropertyBehavior();

            element.Width = 100;
            element.Height = 100;
            ((ISupportsTransparency)element).RequiresTransparency = true;

            m_ScreenRootDesignerStub.Stub(x => x.FindElementByName(Arg<string>.Is.Anything)).Return(element);
            m_ScreenRootDesignerStub.Stub(x => x.Elements).Return(new List<FrameworkElement>() { element });
            m_ScreenRootDesignerStub.Stub(x => x.FindElementByName(Arg<string>.Is.Anything)).Return(m_MockRepository.DynamicMultiMock<FrameworkElement>(typeof(IRenderable), typeof(ISupportsTransparency)));

            m_BitmapHelperMock.Stub(x => x.ConvertToCETransparencyFormat(Arg<BitmapSource>.Is.Anything)).Return(new BitmapImage()).Repeat.Times(m_CrossReferenceItems.Count);
            m_BitmapHelperMock.Expect(x => x.WriteImageToStream(Arg<Stream>.Is.Anything, Arg<BitmapSource>.Is.Anything, Arg<BitmapEncoder>.Is.Anything)).Repeat.Times(m_CrossReferenceItems.Count);
            m_BitmapHelperMock.Expect(x => x.SaveStreamToBitmap(Arg<Stream>.Is.Anything, Arg<string>.Is.Anything)).Return(true).Repeat.Times(m_CrossReferenceItems.Count);

            DateTime designerDate = DateTime.Now;

            m_RenderableServiceStub.Stub(x => x.TryGetLastModifiedDate(Arg<string>.Is.Anything, out Arg<DateTime>.Out(designerDate.Subtract(TimeSpan.FromDays(1))).Dummy)).Return(true);

            m_FileHelperStub.Stub(x => x.Exists(m_DummyResourceFilePath)).Return(true);
            m_FileHelperStub.Stub(x => x.GetLastWriteTime(m_DummyResourceFilePath)).Return(designerDate.Subtract(TimeSpan.FromDays(1)));
            m_FileHelperStub.Stub(x => x.Exists(m_DesignerFilePath)).Return(true);
            m_FileHelperStub.Stub(x => x.GetLastWriteTime(m_DesignerFilePath)).Return(designerDate);

            m_RenderableControlResourceHelperStub.Stub(x => x.GetLayoutSlot(Arg<FrameworkElement>.Is.Anything)).Return(new Rect(0, 0, 100, 100));
            m_RenderableControlResourceHelperStub.Stub(x => x.RenderBitmap(Arg<FrameworkElement>.Is.Anything)).IgnoreArguments().Return(new Bitmap(100, 100));

            m_Generator.GenerateResources(m_ProjectItemMock, false, m_RenderableControlResourceHelperStub);

            m_BitmapHelperMock.AssertWasCalled(x => x.ConvertToCETransparencyFormat(Arg<BitmapSource>.Is.Anything));

            m_ProjectItemMock.VerifyAllExpectations();
            m_BitmapHelperMock.VerifyAllExpectations();
        }

        [Test]
        public void WriteResourceWithTransparencyIfForcedEvenIfZipAndTempFileAreUpToDate()
        {
            var element = m_MockRepository.DynamicMultiMock<FrameworkElement>(typeof(IRenderable), typeof(ISupportsTransparency));
            ((ISupportsTransparency)element).Stub(x => x.RequiresTransparency).PropertyBehavior();

            element.Width = 100;
            element.Height = 100;
            ((ISupportsTransparency)element).RequiresTransparency = true;

            m_ScreenRootDesignerStub.Stub(x => x.FindElementByName(Arg<string>.Is.Anything)).Return(element);
            m_ScreenRootDesignerStub.Stub(x => x.Elements).Return(new List<FrameworkElement>() { element });

            m_BitmapHelperMock.Stub(x => x.ConvertToCETransparencyFormat(Arg<BitmapSource>.Is.Anything)).Return(new BitmapImage()).Repeat.Times(m_CrossReferenceItems.Count);
            m_BitmapHelperMock.Expect(x => x.WriteImageToStream(Arg<Stream>.Is.Anything, Arg<BitmapSource>.Is.Anything, Arg<BitmapEncoder>.Is.Anything)).Repeat.Times(m_CrossReferenceItems.Count);
            m_BitmapHelperMock.Expect(x => x.SaveStreamToBitmap(Arg<Stream>.Is.Anything, Arg<string>.Is.Anything)).Return(true).Repeat.Times(m_CrossReferenceItems.Count);

            DateTime designerDate = DateTime.Now;

            m_RenderableServiceStub.Stub(x => x.TryGetLastModifiedDate(Arg<string>.Is.Anything, out Arg<DateTime>.Out(designerDate.Add(TimeSpan.FromDays(1))).Dummy)).Return(true);

            m_FileHelperStub.Stub(x => x.Exists(m_DummyResourceFilePath)).Return(true);
            m_FileHelperStub.Stub(x => x.GetLastWriteTime(m_DummyResourceFilePath)).Return(designerDate.Add(TimeSpan.FromDays(1)));
            m_FileHelperStub.Stub(x => x.Exists(m_DesignerFilePath)).Return(true);
            m_FileHelperStub.Stub(x => x.GetLastWriteTime(m_DesignerFilePath)).Return(designerDate);

            m_RenderableControlResourceHelperStub.Stub(x => x.GetLayoutSlot(Arg<FrameworkElement>.Is.Anything)).Return(new Rect(0, 0, 100, 100));
            m_RenderableControlResourceHelperStub.Stub(x => x.RenderBitmap(Arg<FrameworkElement>.Is.Anything)).IgnoreArguments().Return(new Bitmap(100, 100));

            m_Generator.GenerateResources(m_ProjectItemMock, true, m_RenderableControlResourceHelperStub);

            m_BitmapHelperMock.AssertWasCalled(x => x.ConvertToCETransparencyFormat(Arg<BitmapSource>.Is.Anything));

            m_ProjectItemMock.VerifyAllExpectations();
            m_BitmapHelperMock.VerifyAllExpectations();
        }

        [Test]
        public void DoNotWriteResourceIfOnlyTempFileIsOutDated()
        {
            var screenRootDesignerStub = MockRepository.GenerateStub<IScreenRootDesigner>();
            screenRootDesignerStub.Stub(x => x.FindElementByName(Arg<string>.Is.Anything)).Return(m_MockRepository.DynamicMultiMock<FrameworkElement>(typeof(IRenderable)));

            var designerHostStub = MockRepository.GenerateStub<INeoDesignerHost>();
            designerHostStub.Stub(x => x.RootDesigner).Return(screenRootDesignerStub);

            m_BitmapHelperMock.Expect(x => x.ConvertToCETransparencyFormat(Arg<BitmapSource>.Is.Anything)).Return(null).Repeat.Never();
            m_BitmapHelperMock.Expect(x => x.WriteImageToStream(Arg<Stream>.Is.Anything, Arg<BitmapSource>.Is.Anything, Arg<BitmapEncoder>.Is.Anything)).Repeat.Never();
            m_BitmapHelperMock.Expect(x => x.SaveStreamToBitmap(Arg<Stream>.Is.Anything, Arg<string>.Is.Anything)).Return(true).Repeat.Never();

            DateTime designerDate = DateTime.Now;
            m_RenderableServiceStub.Stub(x => x.TryGetLastModifiedDate(Arg<string>.Is.Anything, out Arg<DateTime>.Out(designerDate.Add(TimeSpan.FromDays(1))).Dummy)).Return(true);
            m_FileHelperStub.Stub(x => x.Exists(m_DummyResourceFilePath)).Return(true);
            m_FileHelperStub.Stub(x => x.GetLastWriteTime(m_DummyResourceFilePath)).Return(designerDate.Subtract(TimeSpan.FromDays(1)));
            m_FileHelperStub.Stub(x => x.Exists(m_DesignerFilePath)).Return(true);
            m_FileHelperStub.Stub(x => x.GetLastWriteTime(m_DesignerFilePath)).Return(designerDate);

            m_Generator.GenerateResources(m_ProjectItemMock);

            m_ProjectItemMock.VerifyAllExpectations();
            m_BitmapHelperMock.VerifyAllExpectations();
        }

        [Test]
        public void DoNotWriteResourceIfOnlyZipFileIsOutDated()
        {
            var screenRootDesignerStub = MockRepository.GenerateStub<IScreenRootDesigner>();
            screenRootDesignerStub.Stub(x => x.FindElementByName(Arg<string>.Is.Anything)).Return(m_MockRepository.DynamicMultiMock<FrameworkElement>(typeof(IRenderable)));

            var designerHostStub = MockRepository.GenerateStub<INeoDesignerHost>();
            designerHostStub.Stub(x => x.RootDesigner).Return(screenRootDesignerStub);

            m_BitmapHelperMock.Expect(x => x.ConvertToCETransparencyFormat(Arg<BitmapSource>.Is.Anything)).Return(null).Repeat.Never();
            m_BitmapHelperMock.Expect(x => x.WriteImageToStream(Arg<Stream>.Is.Anything, Arg<BitmapSource>.Is.Anything, Arg<BitmapEncoder>.Is.Anything)).Repeat.Never();
            m_BitmapHelperMock.Expect(x => x.SaveStreamToBitmap(Arg<Stream>.Is.Anything, Arg<string>.Is.Anything)).Return(true).Repeat.Never();

            DateTime designerDate = DateTime.Now;
            m_RenderableServiceStub.Stub(x => x.TryGetLastModifiedDate(Arg<string>.Is.Anything, out Arg<DateTime>.Out(designerDate.Subtract(TimeSpan.FromDays(1))).Dummy)).Return(true);
            m_FileHelperStub.Stub(x => x.Exists(m_DummyResourceFilePath)).Return(true);
            m_FileHelperStub.Stub(x => x.GetLastWriteTime(m_DummyResourceFilePath)).Return(designerDate.Add(TimeSpan.FromDays(1)));
            m_FileHelperStub.Stub(x => x.Exists(m_DesignerFilePath)).Return(true);
            m_FileHelperStub.Stub(x => x.GetLastWriteTime(m_DesignerFilePath)).Return(designerDate);

            m_Generator.GenerateResources(m_ProjectItemMock);

            m_ProjectItemMock.VerifyAllExpectations();
            m_BitmapHelperMock.VerifyAllExpectations();
        }

        [Test]
        public void DoNotWriteResourceIfOnlyZipFileIsMissing()
        {
            m_ScreenRootDesignerStub.Stub(x => x.FindElementByName(Arg<string>.Is.Anything)).Return(m_MockRepository.DynamicMultiMock<FrameworkElement>(typeof(IRenderable)));

            m_BitmapHelperMock.Expect(x => x.ConvertToCETransparencyFormat(Arg<BitmapSource>.Is.Anything)).Return(null).Repeat.Never();
            m_BitmapHelperMock.Expect(x => x.WriteImageToStream(Arg<Stream>.Is.Anything, Arg<BitmapSource>.Is.Anything, Arg<BitmapEncoder>.Is.Anything)).Repeat.Never();
            m_BitmapHelperMock.Expect(x => x.SaveStreamToBitmap(Arg<Stream>.Is.Anything, Arg<string>.Is.Anything)).Return(true).Repeat.Never();

            DateTime designerDate = DateTime.Now;
            m_RenderableServiceStub.Stub(x => x.TryGetLastModifiedDate(Arg<string>.Is.Anything, out Arg<DateTime>.Out(DateTime.MinValue).Dummy)).Return(false);
            m_FileHelperStub.Stub(x => x.Exists(m_DummyResourceFilePath)).Return(true);
            m_FileHelperStub.Stub(x => x.GetLastWriteTime(m_DummyResourceFilePath)).Return(designerDate.Add(TimeSpan.FromDays(1)));
            m_FileHelperStub.Stub(x => x.Exists(m_DesignerFilePath)).Return(true);
            m_FileHelperStub.Stub(x => x.GetLastWriteTime(m_DesignerFilePath)).Return(designerDate);

            m_Generator.GenerateResources(m_ProjectItemMock);

            m_ProjectItemMock.VerifyAllExpectations();
            m_BitmapHelperMock.VerifyAllExpectations();
        }

        [Test]
        public void DoNotWriteResourceIfOnlyTempFileIsMissing()
        {
            m_ScreenRootDesignerStub.Stub(x => x.FindElementByName(Arg<string>.Is.Anything)).Return(m_MockRepository.DynamicMultiMock<FrameworkElement>(typeof(IRenderable)));

            m_BitmapHelperMock.Expect(x => x.ConvertToCETransparencyFormat(Arg<BitmapSource>.Is.Anything)).Return(null).Repeat.Never();
            m_BitmapHelperMock.Expect(x => x.WriteImageToStream(Arg<Stream>.Is.Anything, Arg<BitmapSource>.Is.Anything, Arg<BitmapEncoder>.Is.Anything)).Repeat.Never();
            m_BitmapHelperMock.Expect(x => x.SaveStreamToBitmap(Arg<Stream>.Is.Anything, Arg<string>.Is.Anything)).Return(true).Repeat.Never();

            DateTime designerDate = DateTime.Now;
            m_RenderableServiceStub.Stub(x => x.TryGetLastModifiedDate(Arg<string>.Is.Anything, out Arg<DateTime>.Out(designerDate.Add(TimeSpan.FromDays(1))).Dummy)).Return(true);
            m_FileHelperStub.Stub(x => x.Exists(m_DummyResourceFilePath)).Return(false);
            m_FileHelperStub.Stub(x => x.Exists(m_DesignerFilePath)).Return(true);
            m_FileHelperStub.Stub(x => x.GetLastWriteTime(m_DesignerFilePath)).Return(designerDate);

            m_Generator.GenerateResources(m_ProjectItemMock);

            m_ProjectItemMock.VerifyAllExpectations();
            m_BitmapHelperMock.VerifyAllExpectations();
        }
    }
}
