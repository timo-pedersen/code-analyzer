#if!VNEXT_TARGET
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Core.Api.Service;
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interop.DataSource;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.DataItemBrowseControl.ViewModels
{
    [TestFixture]
    public class AddNewTagViewModelTest
    {
        private AddNewTagViewModel m_AddNewTagViewModel;
        private IOpcClientServiceIde m_OpcClientServiceStub;
        private IMultiNameCreationService m_ProjectNameCreationServiceStub;
        private IGlobalController m_GlobalController;
        private IDataSourceContainer m_ControllerOne;
        private IDataItemDataSource m_ControllerOneDataSource;
        private IProtectableItemServiceIde m_ProtectableItemServiceStub;

        public interface IMultiNameCreationService : INameCreationService, IProjectNameCreationServiceIde {}

        [SetUp]
        public void SetUp()
        {
            m_GlobalController = Substitute.For<IGlobalController, ISubItems>();
            m_GlobalController.Name = StringConstants.Tags;
            (m_GlobalController as ISubItems).Items.Returns(new List<object>());

            m_ControllerOneDataSource = Substitute.For<IDataItemDataSource>();

            m_ControllerOne = Substitute.For<IDataSourceContainer>();
            m_ControllerOne.Name = "Controller1";
            m_ControllerOne.DataSource.Returns(m_ControllerOneDataSource);

            m_OpcClientServiceStub = Substitute.For<IOpcClientServiceIde>();
            m_OpcClientServiceStub.GlobalController.Returns(m_GlobalController);
            m_OpcClientServiceStub.Controllers.Returns(new ExtendedBindingList<IDataSourceContainer>() { m_ControllerOne });

            m_ProtectableItemServiceStub = Substitute.For<IProtectableItemServiceIde>();
            m_ProtectableItemServiceStub.IsVisible(Arg.Any<IProtectableItem>()).Returns(true);

            m_ProjectNameCreationServiceStub = Substitute.For<IMultiNameCreationService>();
            ServiceContainerCF.Instance.AddService<INameCreationService>(m_ProjectNameCreationServiceStub);
            ServiceContainerCF.Instance.AddService<IProjectNameCreationServiceIde>(m_ProjectNameCreationServiceStub);

            m_AddNewTagViewModel = new AddNewTagViewModel(m_ProtectableItemServiceStub.ToILazy())
            {
                OpcClientService = m_OpcClientServiceStub,
                ProjectNameCreationService = m_ProjectNameCreationServiceStub
            };
        }

        [Test]
        public void InitalizeWithNameResultsWithThatName()
        {
            m_AddNewTagViewModel.InitializeName("MyTag");

            Assert.That(m_AddNewTagViewModel.Name, Is.EqualTo("MyTag"));
        }

        [Test]
        public void InitalizeWithEmptyNameResultsInDefaultNameWhenNoTagsExist()
        {
            m_GlobalController.DataItemBases.Returns(new ReadOnlyCollection<IDataItemBase>(new List<IDataItemBase>()));
            m_ProjectNameCreationServiceStub.CreateUniqueName(Arg.Any<string>(), Arg.Any<ISubItems>())
                .Returns(AddNewTagViewModel.DefaultTagName);

            m_AddNewTagViewModel.InitializeName(string.Empty);

            Assert.That(m_AddNewTagViewModel.Name, Is.EqualTo(AddNewTagViewModel.DefaultTagName));
        }

        [Test]
        public void InitalizeWithEmptyNameResultsInNextAvailableNameWhenTagsExist()
        {
            var dataItem = Substitute.For<IDataItemBase>();
            dataItem.Name = "MyTag1";

            m_GlobalController.DataItemBases.Returns(new ReadOnlyCollection<IDataItemBase>(new List<IDataItemBase>() { dataItem }));
            m_ProjectNameCreationServiceStub.CreateUniqueName(Arg.Is("MyTag1"), Arg.Any<ISubItems>()).Returns("MyTag2");

            m_AddNewTagViewModel.InitializeName(string.Empty);

            Assert.That(m_AddNewTagViewModel.Name, Is.EqualTo("MyTag2"));
        }

        [Test]
        public void SettingNameToEmptyResultsInValidationError()
        {
            m_ProjectNameCreationServiceStub.IsValidName(string.Empty).Returns(false);

            m_AddNewTagViewModel.Name = string.Empty;

            Assert.That(((IDataErrorInfo)m_AddNewTagViewModel)["Name"], Is.Not.Null);
        }

        [Test]
        public void SettingNameToInvalidNameResultsInValidationError()
        {
            m_ProjectNameCreationServiceStub.IsValidName("My Tag").Returns(false);

            m_AddNewTagViewModel.Name = "My Tag";

            Assert.That(((IDataErrorInfo)m_AddNewTagViewModel)["Name"], Is.Not.Null);
        }

        [Test]
        public void SettingNameToValidAvailableNameResultsInNoValidationError()
        {
            string name = "MyTag";
            m_ProjectNameCreationServiceStub.IsValidName(name).Returns(true);

            m_ProjectNameCreationServiceStub.IsUniqueAndValidName(Arg.Is(name), Arg.Any<ISubItems>(), ref Arg.Any<string>())
                .Returns(x => {
                    x[2] = string.Empty; 
                    return true; 
                });

            m_AddNewTagViewModel.Name = name;

            Assert.That(((IDataErrorInfo)m_AddNewTagViewModel)["Name"], Is.Null);
        }

        [Test]
        public void SettingNameToValidUnavailableNameResultsInValidationError()
        {
            string name = "MyTag1";
            m_ProjectNameCreationServiceStub.IsValidName(name).Returns(true);

            m_ProjectNameCreationServiceStub.IsUniqueAndValidName(Arg.Is(name), Arg.Any<ISubItems>(), ref Arg.Any<string>())
                .Returns(x => {
                    x[2] = string.Empty;
                    return false;
                });

            m_AddNewTagViewModel.Name = name;

            Assert.That(((IDataErrorInfo)m_AddNewTagViewModel)["Name"], Is.Not.Null);
        }

        [Test]
        public void SettingNameWhenAddressIsEmptyDoesNotSetAddressIfInternalTag()
        {
            m_AddNewTagViewModel.Name = "D0";

            Assert.That(m_AddNewTagViewModel.Address, Is.Null.Or.Empty);
        }

        [Test]
        public void SettingValidNameWhenAddressIsEmptySetsAddressIfValidForController()
        {
            m_AddNewTagViewModel.SelectedController = m_AddNewTagViewModel.Controllers.LastOrDefault();

            string newName = "D0";
            m_ProjectNameCreationServiceStub.RemoveIllegalCharactersInName(newName).Returns(newName);

            m_ControllerOne.ValidateAddress(Arg.Is(newName), out Arg.Any<string>())
                .Returns(x => {
                    x[1] = newName;
                    return true;
                });

            m_AddNewTagViewModel.Name = newName;

            Assert.That(m_AddNewTagViewModel.Address, Is.EqualTo(newName));
        }

        [Test]
        public void SettingInvalidNameWhenAddressIsEmptySetsCorrectedNameAsAddressIfValidForController()
        {
            m_AddNewTagViewModel.SelectedController = m_AddNewTagViewModel.Controllers.LastOrDefault();

            string newInvalidName = " D0 ";
            string newValidName = "D0";
            m_ProjectNameCreationServiceStub.RemoveIllegalCharactersInName(newInvalidName).Returns(newValidName);

            m_ControllerOne.ValidateAddress(Arg.Is(newValidName), out Arg.Any<string>())
                .Returns(x => {
                    x[1] = newValidName;
                    return true;
                });

            m_AddNewTagViewModel.Name = newInvalidName;

            Assert.That(m_AddNewTagViewModel.Address, Is.EqualTo(newValidName));
        }

        [Test]
        public void SettingValidNameWhenAddressIsEmptyDoesNotSetAddressIfInvalidForController()
        {
            m_AddNewTagViewModel.SelectedController = m_AddNewTagViewModel.Controllers.LastOrDefault();

            string newName = "MyTag";
            m_ProjectNameCreationServiceStub.RemoveIllegalCharactersInName(newName).Returns(newName);

            m_ControllerOne.ValidateAddress(Arg.Is(newName), out Arg.Any<string>())
                .Returns(x => {
                    x[1] = newName;
                    return false;
                });

            m_AddNewTagViewModel.Name = newName;

            Assert.That(m_AddNewTagViewModel.Address, Is.Null.Or.Empty);
        }

        [Test]
        public void SettingValidNameWhenAddressIsEmptySetsCorrectedAddressIfInvalidFromNameForController()
        {
            m_AddNewTagViewModel.SelectedController = m_AddNewTagViewModel.Controllers.Last();

            string newName = "d0";
            string correctedAddress = "D0";
            m_ProjectNameCreationServiceStub.RemoveIllegalCharactersInName(newName).Returns(newName);

            m_ControllerOne.ValidateAddress(Arg.Is(newName), out Arg.Any<string>())
                .Returns(x => {
                    x[1] = correctedAddress;
                    return true;
                });

            m_AddNewTagViewModel.Name = newName;

            Assert.That(m_AddNewTagViewModel.Address, Is.EqualTo(correctedAddress));
        }

        [Test]
        public void SettingAddressToEmptyResultsInNoValidationErrorForInternalTag()
        {
            string newAddress = string.Empty;

            m_AddNewTagViewModel.Address = newAddress;

            Assert.That(((IDataErrorInfo)m_AddNewTagViewModel)["Address"], Is.Null.Or.Empty);
        }

        [Test]
        public void SettingAddressToEmptyResultsInNoValidationErrorForController()
        {
            m_AddNewTagViewModel.SelectedController = m_AddNewTagViewModel.Controllers.Last();

            string newAddress = string.Empty;
            m_ControllerOne.ValidateAddress(newAddress).Returns(true);

            m_AddNewTagViewModel.Address = newAddress;

            Assert.That(((IDataErrorInfo)m_AddNewTagViewModel)["Address"], Is.Null);
        }

        [Test]
        public void SettingAddressToInvalidAddressForControllerResultsInValidationError()
        {
            m_AddNewTagViewModel.SelectedController = m_AddNewTagViewModel.Controllers.Last();

            string newAddress = "MyAddress";
            m_ProjectNameCreationServiceStub.IsValidName(newAddress).Returns(false);

            m_AddNewTagViewModel.Address = newAddress;

            Assert.That(((IDataErrorInfo)m_AddNewTagViewModel)["Address"], Is.Not.Null);
        }

        [Test]
        public void SettingValidAddressForControllerSetsDataType()
        {
            m_AddNewTagViewModel.SelectedController = m_AddNewTagViewModel.Controllers.Last();

            string newAddress = "D0";
            m_ControllerOne.ValidateAddress(newAddress).Returns(true);
            m_ControllerOneDataSource.GetDefaultDataType(Arg.Is(newAddress), out Arg.Any<BEDATATYPE>())
                .Returns(x => {
                    x[1] = BEDATATYPE.DT_INTEGER2;
                    return true;
                });

            m_AddNewTagViewModel.Address = newAddress;

            string dataTypeName = BEDATATYPENameConverter.DataTypeToFriendlyName(BEDATATYPE.DT_INTEGER2);
            Assert.That(m_AddNewTagViewModel.SelectedDataType.Name, Is.EqualTo(dataTypeName));
        }

        [Test]
        public void SettingInvalidAddressForControllerDoesNotSetDataType()
        {
            m_AddNewTagViewModel.SelectedController = m_AddNewTagViewModel.Controllers.Last();
            m_AddNewTagViewModel.SelectedDataType = m_AddNewTagViewModel.DataTypes.First();

            string newAddress = "MyTag";
            m_ControllerOne.ValidateAddress(newAddress).Returns(false);
            m_ControllerOneDataSource.GetDefaultDataType(Arg.Is(newAddress), out Arg.Any<BEDATATYPE>())
                .Returns(x => {
                    x[1] = BEDATATYPE.DT_INTEGER2;
                    return true;
                });

            m_AddNewTagViewModel.Address = newAddress;

            string dataTypeName = BEDATATYPENameConverter.DataTypeToFriendlyName(BEDATATYPE.DT_DEFAULT);
            Assert.That(m_AddNewTagViewModel.SelectedDataType.Name, Is.EqualTo(dataTypeName));
        }

        [Test]
        public void ChangingToNoControllerResetsAddressAndDataType()
        {
            m_AddNewTagViewModel.SelectedController = m_AddNewTagViewModel.Controllers.Last();
            m_AddNewTagViewModel.Address = "D0";
            m_AddNewTagViewModel.SelectedDataType = m_AddNewTagViewModel.DataTypes.Skip(1).First();

            m_AddNewTagViewModel.SelectedController = m_AddNewTagViewModel.Controllers.First();
            m_AddNewTagViewModel.SelectControllerCommand.Execute(m_AddNewTagViewModel.Controllers.First());

            string dataTypeName = BEDATATYPENameConverter.DataTypeToFriendlyName(BEDATATYPE.DT_DEFAULT);
            Assert.That(m_AddNewTagViewModel.SelectedDataType.Name, Is.EqualTo(dataTypeName));
            Assert.That(m_AddNewTagViewModel.Address, Is.Empty);
        }
    }
}
#endif
