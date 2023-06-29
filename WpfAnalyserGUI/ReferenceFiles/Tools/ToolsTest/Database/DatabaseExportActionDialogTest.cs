﻿using System;
using System.Collections.Generic;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Common.Constants;
using Neo.ApplicationFramework.Controls.DatabaseImportExport.ViewModels;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Storage;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Database
{
    [TestFixture]
    public class DatabaseExportActionDialogTest
    {
        private DatabaseActionBaseViewModel m_DatabaseActionBaseViewModel;
        private ITargetService m_TargetService;
        private ITarget m_CurrentTarget;

        [SetUp]
        public void Setup()
        {
            TestHelper.ClearServices();
            TestHelper.CreateAndAddServiceStub<ICommandManagerService>();

            m_TargetService = TestHelper.CreateAndAddServiceStub<ITargetService>();
            m_CurrentTarget = Substitute.For<ITarget>();
            m_TargetService.CurrentTarget = m_CurrentTarget;

            m_DatabaseActionBaseViewModel = new DatabaseActionBaseViewModel();
        }

        [Test]
        [TestCase(ApplicationConstantsCF.RecipeReferenceName, TargetPlatform.Windows, false)]
        [TestCase(ApplicationConstantsCF.AlarmServerReferenceName, TargetPlatform.Windows, false)]
        [TestCase(ApplicationConstantsCF.DataLoggerReferenceName, TargetPlatform.Windows, false)]
        [TestCase(ApplicationConstantsCF.AuditTrailReferenceName, TargetPlatform.Windows, false)]
        [TestCase(ApplicationConstantsCF.DataLoggerReferenceName, TargetPlatform.Windows, false)]
        [TestCase(ApplicationConstantsCF.RecipeReferenceName, TargetPlatform.WindowsCE, false)]
        [TestCase(ApplicationConstantsCF.AlarmServerReferenceName, TargetPlatform.WindowsCE, true)]
        [TestCase(ApplicationConstantsCF.DataLoggerReferenceName, TargetPlatform.WindowsCE, true)]
        [TestCase(ApplicationConstantsCF.AuditTrailReferenceName, TargetPlatform.WindowsCE, true)]
        [TestCase(ApplicationConstantsCF.DataLoggerReferenceName, TargetPlatform.WindowsCE, true)]
        public void FolderOptionsVisibility(string designerType, TargetPlatform targetPlatform, bool isVisible)
        {
            //ARRANGE
            IStorageSourceItemInfo selectedStorage = GetSelectedStorage(designerType, targetPlatform);

            //ACT
            m_DatabaseActionBaseViewModel.SelectedStorageSource = selectedStorage;

            //ASSERT
            Assert.AreEqual(isVisible, m_DatabaseActionBaseViewModel.IsFolderLimitOptionsVisible);
            if (!m_DatabaseActionBaseViewModel.IsFolderLimitOptionsVisible)
                Assert.IsTrue(m_DatabaseActionBaseViewModel.OverwriteOlderFiles);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void FolderOptionsWhenOverwriteIsSelected(bool overwrite)
        {
            m_DatabaseActionBaseViewModel.OverWrite = overwrite;

            Assert.AreEqual(!overwrite, m_DatabaseActionBaseViewModel.IsFolderLimitOptionsEnabled);
            if (!m_DatabaseActionBaseViewModel.IsFolderLimitOptionsEnabled)
                Assert.IsTrue(m_DatabaseActionBaseViewModel.OverwriteOlderFiles);
        }

        private IStorageSourceItemInfo GetSelectedStorage(string designerType, TargetPlatform targetPlatform)
        {
            m_CurrentTarget.Id.Returns(targetPlatform);

            IStorageSourceItemInfo selectedStorage = Substitute.For<IStorageSourceItemInfo>();
            selectedStorage.Name = designerType;
            var type = Substitute.For<Type>();
            type.Name.Returns(designerType);
            selectedStorage.DesignerType.Returns(type);
            m_DatabaseActionBaseViewModel.DatabaseTableNames = new List<IStorageSourceItemInfo>
            {
                selectedStorage
            };
            return selectedStorage;
        }
    }
}
