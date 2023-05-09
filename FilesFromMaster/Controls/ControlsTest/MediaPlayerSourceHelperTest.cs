﻿using System;
using System.IO;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Common.Constants;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls.Commands;
using Neo.ApplicationFramework.Controls.ControlsIde.TestHelpers;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls
{
    [TestFixture]
    public class MediaPlayerSourceHelperTest
    {
        private const string ProjectFolderPath = @"c:\neo\MyProject";
        private const string ProjectFilesFolderPath = @"c:\neo\MyProject\Project Files";
        private const string NonProjectFolderPath = @"c:\other\folder";
        private const string FileName = @"MyVideo.wmv";
        private const string InternetAddress = @"http://www.beijer.se/Videos/MyVideo.mpeg";

        private IProject m_ProjectStub;
        private IProjectManager m_ProjectManagerStub;
        private MediaPlayerSourceHelper m_MediaPlayerSourceHelper;
        private FileHelper m_FileHelperMock;
        private IGlobalCommandService m_GlobalCommandServiceStub;
        private ITargetService m_TargetServiceStub;
        private ITarget m_CurrentTargetStub;
        private IMessageBoxServiceIde m_MessageBoxServiceIdeStub;

        [SetUp]
        public void SetUp()
        {
            m_ProjectStub = MockRepository.GenerateStub<IProject>();
            m_ProjectStub.FolderPath = ProjectFolderPath;

            m_ProjectManagerStub = TestHelper.CreateAndAddServiceStub<IProjectManager>();
            m_ProjectManagerStub.Project = m_ProjectStub;

            m_CurrentTargetStub = MockRepository.GenerateStub<ITarget>();

            m_TargetServiceStub = TestHelper.CreateAndAddServiceStub<ITargetService>();
            m_TargetServiceStub.CurrentTarget = m_CurrentTargetStub;

            m_GlobalCommandServiceStub = new GlobalCommandServiceFake();
            m_FileHelperMock = MockRepository.GenerateMock<FileHelper>();
            m_MessageBoxServiceIdeStub = MockRepository.GenerateStub<IMessageBoxServiceIde>();

            m_MediaPlayerSourceHelper = new MediaPlayerSourceHelper(
                m_TargetServiceStub.ToILazy(),
                m_ProjectManagerStub.ToILazy(),
                m_GlobalCommandServiceStub.ToILazy(),
                m_MessageBoxServiceIdeStub.ToILazy(),
                m_FileHelperMock.ToLazy());
        }

        [Test]
        public void SettingNullReturnsNullWithoutCopying()
        {
            Uri uriIn = null;

            m_FileHelperMock.Expect(fileHelper => fileHelper.Copy(string.Empty, string.Empty, true)).IgnoreArguments().Repeat.Never();

            m_MediaPlayerSourceHelper.MediaSource = uriIn;

            Assert.AreEqual(uriIn, m_MediaPlayerSourceHelper.MediaSource);

            m_FileHelperMock.VerifyAllExpectations();
        }

        [Test]
        public void SettingFileUriNotInProjectFolderCopiesFileAndReturnsRelativeUri()
        {
            string sourcePath = Path.Combine(NonProjectFolderPath, FileName);
            Uri uriIn = new Uri(sourcePath);

            m_FileHelperMock.Expect(fileHelper => fileHelper.Copy(string.Empty, string.Empty, true)).IgnoreArguments().Repeat.Once();

            m_MediaPlayerSourceHelper.MediaSource = uriIn;

            Uri uriOut = m_MediaPlayerSourceHelper.MediaSource;
            Assert.IsNotNull(uriOut);
            Assert.AreEqual(false, uriOut.IsAbsoluteUri);
            Assert.AreEqual(FileName, uriOut.OriginalString);

            m_FileHelperMock.VerifyAllExpectations();
        }

        [Test]
        public void SettingFileUriInProjectFolderDoesNotCopyFileAndReturnsRelativeUri()
        {
            string sourcePath = Path.Combine(ProjectFilesFolderPath, FileName);
            Uri uriIn = new Uri(sourcePath);

            m_FileHelperMock.Expect(fileHelper => fileHelper.Copy(string.Empty, string.Empty, true)).IgnoreArguments().Repeat.Never();

            m_MediaPlayerSourceHelper.MediaSource = uriIn;

            Uri uriOut = m_MediaPlayerSourceHelper.MediaSource;
            Assert.IsNotNull(uriOut);
            Assert.AreEqual(false, uriOut.IsAbsoluteUri);
            Assert.AreEqual(FileName, uriOut.OriginalString);

            m_FileHelperMock.VerifyAllExpectations();
        }

        [Test]
        public void SettingRelativeFileUriDoesNotCopyFileAndReturnsRelativeUri()
        {
            Uri uriIn = new Uri(FileName, UriKind.Relative);

            m_FileHelperMock.Expect(fileHelper => fileHelper.Copy(string.Empty, string.Empty, true)).IgnoreArguments().Repeat.Never();

            m_MediaPlayerSourceHelper.MediaSource = uriIn;

            Uri uriOut = m_MediaPlayerSourceHelper.MediaSource;
            Assert.IsNotNull(uriOut);
            Assert.AreEqual(false, uriOut.IsAbsoluteUri);
            Assert.AreEqual(FileName, uriOut.OriginalString);

            m_FileHelperMock.VerifyAllExpectations();
        }

        [Test]
        public void SettingInternetUriDoesNotCopyFileAndReturnsAbsoluteUri()
        {
            Uri uriIn = new Uri(InternetAddress);

            m_FileHelperMock.Expect(fileHelper => fileHelper.Copy(string.Empty, string.Empty, true)).IgnoreArguments().Repeat.Never();

            m_MediaPlayerSourceHelper.MediaSource = uriIn;

            Uri uriOut = m_MediaPlayerSourceHelper.MediaSource;
            Assert.IsNotNull(uriOut);
            Assert.AreEqual(true, uriOut.IsAbsoluteUri);
            Assert.AreEqual(InternetAddress, uriOut.OriginalString);

            m_FileHelperMock.VerifyAllExpectations();
        }

        [Test]
        public void SearchPatternForPC()
        {
            //ASSIGN
            m_CurrentTargetStub.Stub(x => x.Id).Return(TargetPlatform.Windows);

            //ACT
            var searchPattern = m_MediaPlayerSourceHelper.FileSearchPatterns;

            //ASSERT
            CollectionAssert.AreEqual(FileDialogFiltersIde.MediaSearchPatterns, searchPattern);
        }

        [Test]
        public void SearchPatternForCE()
        {
            //ASSIGN
            m_CurrentTargetStub.Stub(x => x.Id).Return(TargetPlatform.WindowsCE);
            m_CurrentTargetStub.Stub(x => x.PlatformVersion).Return(TargetPlatformVersion.CE6);

            //ACT
            var searchPattern = m_MediaPlayerSourceHelper.FileSearchPatterns;

            //ASSERT
            CollectionAssert.AreEqual(FileDialogFiltersIde.MediaSearchPatternsCE, searchPattern);
        }

        [Test]
        public void SearchPatternForCE8()
        {
            //ASSIGN
            m_CurrentTargetStub.Stub(x => x.Id).Return(TargetPlatform.WindowsCE);
            m_CurrentTargetStub.Stub(x => x.PlatformVersion).Return(TargetPlatformVersion.CE8);

            //ACT
            var searchPattern = m_MediaPlayerSourceHelper.FileSearchPatterns;

            //ASSERT
            CollectionAssert.AreEqual(FileDialogFiltersIde.MediaSearchPatternsCE8, searchPattern);
        }

        [Test]
        public void OpenFileDialogFilterForPC()
        {
            //ASSIGN
            m_CurrentTargetStub.Stub(x => x.Id).Return(TargetPlatform.Windows);

            //ACT
            var openFileDialogFilter = m_MediaPlayerSourceHelper.OpenFileDialogFilter;

            //ASSERT
            CollectionAssert.AreEqual(FileDialogFiltersIde.MediaFileDialogFilter, openFileDialogFilter);
        }

        [Test]
        public void OpenFileDialogFilterForCE()
        {
            //ASSIGN
            m_CurrentTargetStub.Stub(x => x.Id).Return(TargetPlatform.WindowsCE);
            m_CurrentTargetStub.Stub(x => x.PlatformVersion).Return(TargetPlatformVersion.CE6);

            //ACT
            var openFileDialogFilter = m_MediaPlayerSourceHelper.OpenFileDialogFilter;

            //ASSERT
            CollectionAssert.AreEqual(FileDialogFiltersIde.MediaFileDialogFilterCE, openFileDialogFilter);
        }

        [Test]
        public void OpenFileDialogFilterForCE8()
        {
            //ASSIGN
            m_CurrentTargetStub.Stub(x => x.Id).Return(TargetPlatform.WindowsCE);
            m_CurrentTargetStub.Stub(x => x.PlatformVersion).Return(TargetPlatformVersion.CE8);

            //ACT
            var openFileDialogFilter = m_MediaPlayerSourceHelper.OpenFileDialogFilter;

            //ASSERT
            CollectionAssert.AreEqual(FileDialogFiltersIde.MediaFileDialogFilterCE8, openFileDialogFilter);
        }
    }
}
