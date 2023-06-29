#if !VNEXT_TARGET
using System.Windows.Forms;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Resources.Texts;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcUaClient.ViewModels;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.OpcUaClient
{
    [TestFixture]
    class OpcUaClientSettingsPropertyPageViewModelTest
    {
        private IMessageBoxServiceIde m_MessageBoxService;
        private OpcUaClientSettingsPropertyPageViewModel m_OpcUaClientSettingsPropertyPageViewModel;
        private IDataSourceContainer m_DataSourceContainer;

        [SetUp]
        public void SetUp()
        {
            m_MessageBoxService = TestHelper.CreateAndAddServiceStub<IMessageBoxServiceIde>();
            m_DataSourceContainer = Substitute.For<IDataSourceContainer>();
            GiveDataSourceContainerDummyValues();
            m_DataSourceContainer.ValidateAndCorrectUrl(Arg.Any<string>(), out Arg.Any<string>(),
                out Arg.Any<string>()).Returns(x => 
                {
                    x[1] = string.Empty;
                    x[2] = string.Empty;
                    return true;
                });
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
            m_MessageBoxService.Show(TextsIde.OpcUaClientSettingsPropertyPage_AllowNonSecureConnections_ValidationWarning, TextsIde.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.OK).Returns(DialogResult.Yes);
            m_OpcUaClientSettingsPropertyPageViewModel.AllowNonSecureConnections.Value = true;
            //ACT
            m_OpcUaClientSettingsPropertyPageViewModel.ValidateAndSaveValues();
            //ASSERT
            Assert.IsTrue(m_OpcUaClientSettingsPropertyPageViewModel.AllowNonSecureConnections.Value);
            Assert.IsTrue(m_OpcUaClientSettingsPropertyPageViewModel.DataSourceContainer.OpcUaAllowNonSecureConnections);
        }

        [Test]
        public void AllowingNonSecureConnections_UsingUserNameAndPasswordAndNotAcceptingRisk_DoesNotSaveTheValue()
        {
            //ARRANGE
            Initialize(true);
            m_MessageBoxService.Show(TextsIde.OpcUaClientSettingsPropertyPage_AllowNonSecureConnections_ValidationWarning, TextsIde.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.OK).Returns(DialogResult.No);
            m_OpcUaClientSettingsPropertyPageViewModel.AllowNonSecureConnections.Value = true;
            //ACT
            m_OpcUaClientSettingsPropertyPageViewModel.ValidateAndSaveValues();
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
            m_OpcUaClientSettingsPropertyPageViewModel.ValidateAndSaveValues();
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
            m_DataSourceContainer.OpcUaMaxSubscriptions = 1;
            m_DataSourceContainer.OpcUaMaxSubscriptionItems = 1;
        }
    }
}
#endif
