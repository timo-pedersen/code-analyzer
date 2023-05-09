using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Resources.Texts;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcUaClient.ViewModels;
using NUnit.Framework;
using Rhino.Mocks;
using System.Windows.Forms;

namespace Neo.ApplicationFramework.Tools.OpcUaClient
{
    [TestFixture]
    public class OpcUaClientSettingsPropertyPageViewModelTest
    {
        private IMessageBoxServiceIde m_MessageBoxService;
        private OpcUaClientSettingsPropertyPageViewModel m_OpcUaClientSettingsPropertyPageViewModel;
        private IDataSourceContainer m_DataSourceContainer;

        [SetUp]
        public void SetUp()
        {
            m_MessageBoxService = TestHelper.CreateAndAddServiceMock<IMessageBoxServiceIde>();
            m_DataSourceContainer = MockRepository.GenerateStub<IDataSourceContainer>();
            GiveDataSourceContainerDummyValues();
            m_DataSourceContainer.Stub(x => x.ValidateAndCorrectUrl(Arg<string>.Is.Anything, out Arg<string>.Out(string.Empty).Dummy, out Arg<string>.Out(string.Empty).Dummy)).Return(true);
            m_OpcUaClientSettingsPropertyPageViewModel = new OpcUaClientSettingsPropertyPageViewModel();
        }

        [Test]
        public void AllowingNonSecureConnections_NotSet_ReturnsFalseAsDefaultValue()
        {
            //ARRANGE
            Initialize(false);
            //ASSERT
            Assert.IsFalse(m_OpcUaClientSettingsPropertyPageViewModel.AllowNonSecureConnections.Value);
            Assert.IsFalse(m_OpcUaClientSettingsPropertyPageViewModel.DataSourceContainer.OpcUaAllowNonSecureConnections);
        }

        [Test]
        public void AllowingNonSecureConnections_UsingUserNameAndPasswordAndAcceptingRisk_SavesTheValue()
        {
            //ARRANGE
            Initialize(true);
            m_MessageBoxService.Stub(x => x.Show(TextsIde.OpcUaClientSettingsPropertyPage_AllowNonSecureConnections_ValidationWarning, TextsIde.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.OK)).Return(DialogResult.Yes);
            m_OpcUaClientSettingsPropertyPageViewModel.AllowNonSecureConnections.Value = true;
            //ACT
            if (m_OpcUaClientSettingsPropertyPageViewModel.Validate())
                m_OpcUaClientSettingsPropertyPageViewModel.Apply();
            //ASSERT
            Assert.IsTrue(m_OpcUaClientSettingsPropertyPageViewModel.AllowNonSecureConnections.Value);
            Assert.IsTrue(m_OpcUaClientSettingsPropertyPageViewModel.DataSourceContainer.OpcUaAllowNonSecureConnections);
        }

        [Test]
        public void AllowingNonSecureConnections_UsingUserNameAndPasswordAndNotAcceptingRisk_DoesNotSaveTheValue()
        {
            //ARRANGE
            Initialize(true);
            m_MessageBoxService.Stub(x => x.Show(TextsIde.OpcUaClientSettingsPropertyPage_AllowNonSecureConnections_ValidationWarning, TextsIde.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.OK)).Return(DialogResult.No);
            m_OpcUaClientSettingsPropertyPageViewModel.AllowNonSecureConnections.Value = true;
            //ACT
            if (m_OpcUaClientSettingsPropertyPageViewModel.Validate())
                m_OpcUaClientSettingsPropertyPageViewModel.Apply();
            //ASSERT
            Assert.IsFalse(m_OpcUaClientSettingsPropertyPageViewModel.AllowNonSecureConnections.Value);
            Assert.IsFalse(m_OpcUaClientSettingsPropertyPageViewModel.DataSourceContainer.OpcUaAllowNonSecureConnections);
        }

        [Test]
        public void AllowingNonSecureConnections_WithoutUserNameAndPassword_DoesNotSaveTheValue()
        {
            //ARRANGE
            Initialize(false);
            m_OpcUaClientSettingsPropertyPageViewModel.AllowNonSecureConnections.Value = true;
            //ACT
            if (m_OpcUaClientSettingsPropertyPageViewModel.Validate())
                m_OpcUaClientSettingsPropertyPageViewModel.Apply();
            //ASSERT
            Assert.IsTrue(m_OpcUaClientSettingsPropertyPageViewModel.AllowNonSecureConnections.Value);
            Assert.IsFalse(m_OpcUaClientSettingsPropertyPageViewModel.DataSourceContainer.OpcUaAllowNonSecureConnections);
        }

        private void Initialize(bool withUserNameAndPassword)
        {
            if (withUserNameAndPassword)
            {
                m_DataSourceContainer.UserName = "user";
                m_DataSourceContainer.Password = "pass";
            }

            m_OpcUaClientSettingsPropertyPageViewModel.Initialize(m_DataSourceContainer);
        }

        private void GiveDataSourceContainerDummyValues()
        {
            m_DataSourceContainer.OpcUaNamespaceInfos = new OpcUaNamespaceInfos();
            m_DataSourceContainer.OpcUaNamespaceNameBrowseNameSeparator = ':';
            m_DataSourceContainer.OpcUaDefaultNamespaceName = string.Empty;
        }
    }
}
