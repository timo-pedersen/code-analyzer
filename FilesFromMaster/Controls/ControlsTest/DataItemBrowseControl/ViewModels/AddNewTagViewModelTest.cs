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
using NUnit.Framework;
using Rhino.Mocks;

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
            m_GlobalController = MockRepository.GenerateMock<IGlobalController, ISubItems>();
            m_GlobalController.Name = StringConstants.Tags;
            (m_GlobalController as ISubItems).Stub(inv => inv.Items).Return(new List<object>());

            m_ControllerOneDataSource = MockRepository.GenerateStub<IDataItemDataSource>();

            m_ControllerOne = MockRepository.GenerateStub<IDataSourceContainer>();
            m_ControllerOne.Name = "Controller1";
            m_ControllerOne.Stub(x => x.DataSource).Return(m_ControllerOneDataSource);

            m_OpcClientServiceStub = MockRepository.GenerateStub<IOpcClientServiceIde>();
            m_OpcClientServiceStub.Stub(x => x.GlobalController).Return(m_GlobalController);
            m_OpcClientServiceStub.Stub(x => x.Controllers).Return(new ExtendedBindingList<IDataSourceContainer>() { m_ControllerOne });

            m_ProtectableItemServiceStub = MockRepository.GenerateStub<IProtectableItemServiceIde>();
            m_ProtectableItemServiceStub.Stub(x => x.IsVisible(Arg<IProtectableItem>.Is.Anything)).Return(true);

            m_ProjectNameCreationServiceStub = MockRepository.GenerateStub<IMultiNameCreationService>();
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
            m_GlobalController.Stub(x => x.DataItemBases).Return(new ReadOnlyCollection<IDataItemBase>(new List<IDataItemBase>()));
            m_ProjectNameCreationServiceStub.Stub<IProjectNameCreationServiceIde>(x => x.CreateUniqueName(Arg<string>.Is.Anything, Arg<ISubItems>.Is.Anything)).Return(AddNewTagViewModel.DefaultTagName);

            m_AddNewTagViewModel.InitializeName(string.Empty);

            Assert.That(m_AddNewTagViewModel.Name, Is.EqualTo(AddNewTagViewModel.DefaultTagName));
        }

        [Test]
        public void InitalizeWithEmptyNameResultsInNextAvailableNameWhenTagsExist()
        {
            var dataItem = MockRepository.GenerateStub<IDataItemBase>();
            dataItem.Name = "MyTag1";

            m_GlobalController.Stub(x => x.DataItemBases).Return(new ReadOnlyCollection<IDataItemBase>(new List<IDataItemBase>() { dataItem }));
            m_ProjectNameCreationServiceStub.Stub(x => x.CreateUniqueName(Arg<string>.Is.Equal("MyTag1"), Arg<ISubItems>.Is.Anything)).Return("MyTag2");

            m_AddNewTagViewModel.InitializeName(string.Empty);

            Assert.That(m_AddNewTagViewModel.Name, Is.EqualTo("MyTag2"));
        }

        [Test]
        public void SettingNameToEmptyResultsInValidationError()
        {
            m_ProjectNameCreationServiceStub.Stub(x => x.IsValidName(string.Empty)).Return(false);

            m_AddNewTagViewModel.Name = string.Empty;

            Assert.That(((IDataErrorInfo)m_AddNewTagViewModel)["Name"], Is.Not.Null);
        }

        [Test]
        public void SettingNameToInvalidNameResultsInValidationError()
        {
            m_ProjectNameCreationServiceStub.Stub(x => x.IsValidName("My Tag")).Return(false);

            m_AddNewTagViewModel.Name = "My Tag";

            Assert.That(((IDataErrorInfo)m_AddNewTagViewModel)["Name"], Is.Not.Null);
        }

        [Test]
        public void SettingNameToValidAvailableNameResultsInNoValidationError()
        {
            string name = "MyTag";
            m_ProjectNameCreationServiceStub.Stub(x => x.IsValidName(name)).Return(true);

            m_ProjectNameCreationServiceStub.Stub(x => x.IsUniqueAndValidName(
                                                    Arg<string>.Is.Equal(name),
                                                    Arg<ISubItems>.Is.Anything,
                                                    ref Arg<string>.Ref(Rhino.Mocks.Constraints.Is.Anything(), string.Empty).Dummy)).Return(true);

            m_AddNewTagViewModel.Name = name;

            Assert.That(((IDataErrorInfo)m_AddNewTagViewModel)["Name"], Is.Null);
        }

        [Test]
        public void SettingNameToValidUnavailableNameResultsInValidationError()
        {
            string name = "MyTag1";
            m_ProjectNameCreationServiceStub.Stub(x => x.IsValidName(name)).Return(true);

            m_ProjectNameCreationServiceStub.Stub(x => x.IsUniqueAndValidName(
                                                    Arg<string>.Is.Equal(name),
                                                    Arg<ISubItems>.Is.Anything,
                                                    ref Arg<string>.Ref(Rhino.Mocks.Constraints.Is.Anything(), string.Empty).Dummy)).Return(false);

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
            m_ProjectNameCreationServiceStub.Stub(x => x.RemoveIllegalCharactersInName(newName)).Return(newName);

            m_ControllerOne.Stub(x => x.ValidateAddress(Arg<string>.Is.Equal(newName), out Arg<string>.Out(newName).Dummy)).Return(true);

            m_AddNewTagViewModel.Name = newName;

            Assert.That(m_AddNewTagViewModel.Address, Is.EqualTo(newName));
        }

        [Test]
        public void SettingInvalidNameWhenAddressIsEmptySetsCorrectedNameAsAddressIfValidForController()
        {
            m_AddNewTagViewModel.SelectedController = m_AddNewTagViewModel.Controllers.LastOrDefault();

            string newInvalidName = " D0 ";
            string newValidName = "D0";
            m_ProjectNameCreationServiceStub.Stub(x => x.RemoveIllegalCharactersInName(newInvalidName)).Return(newValidName);

            m_ControllerOne.Stub(x => x.ValidateAddress(Arg<string>.Is.Equal(newValidName), out Arg<string>.Out(newValidName).Dummy)).Return(true);

            m_AddNewTagViewModel.Name = newInvalidName;

            Assert.That(m_AddNewTagViewModel.Address, Is.EqualTo(newValidName));
        }

        [Test]
        public void SettingValidNameWhenAddressIsEmptyDoesNotSetAddressIfInvalidForController()
        {
            m_AddNewTagViewModel.SelectedController = m_AddNewTagViewModel.Controllers.LastOrDefault();

            string newName = "MyTag";
            m_ProjectNameCreationServiceStub.Stub(x => x.RemoveIllegalCharactersInName(newName)).Return(newName);

            m_ControllerOne.Stub(x => x.ValidateAddress(Arg<string>.Is.Equal(newName), out Arg<string>.Out(newName).Dummy)).Return(false);

            m_AddNewTagViewModel.Name = newName;

            Assert.That(m_AddNewTagViewModel.Address, Is.Null.Or.Empty);
        }

        [Test]
        public void SettingValidNameWhenAddressIsEmptySetsCorrectedAddressIfInvalidFromNameForController()
        {
            m_AddNewTagViewModel.SelectedController = m_AddNewTagViewModel.Controllers.Last();

            string newName = "d0";
            string correctedAddress = "D0";
            m_ProjectNameCreationServiceStub.Stub(x => x.RemoveIllegalCharactersInName(newName)).Return(newName);

            m_ControllerOne.Stub(x => x.ValidateAddress(Arg<string>.Is.Equal(newName), out Arg<string>.Out(correctedAddress).Dummy)).Return(true);

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
            m_ControllerOne.Stub(x => x.ValidateAddress(newAddress)).Return(true);

            m_AddNewTagViewModel.Address = newAddress;

            Assert.That(((IDataErrorInfo)m_AddNewTagViewModel)["Address"], Is.Null);
        }

        [Test]
        public void SettingAddressToInvalidAddressForControllerResultsInValidationError()
        {
            m_AddNewTagViewModel.SelectedController = m_AddNewTagViewModel.Controllers.Last();

            string newAddress = "MyAddress";
            m_ProjectNameCreationServiceStub.Stub(x => x.IsValidName(newAddress)).Return(false);

            m_AddNewTagViewModel.Address = newAddress;

            Assert.That(((IDataErrorInfo)m_AddNewTagViewModel)["Address"], Is.Not.Null);
        }

        [Test]
        public void SettingValidAddressForControllerSetsDataType()
        {
            m_AddNewTagViewModel.SelectedController = m_AddNewTagViewModel.Controllers.Last();

            string newAddress = "D0";
            m_ControllerOne.Stub(x => x.ValidateAddress(newAddress)).Return(true);
            m_ControllerOneDataSource.Stub(x => x.GetDefaultDataType(Arg<string>.Is.Equal(newAddress), out Arg<BEDATATYPE>.Out(BEDATATYPE.DT_INTEGER2).Dummy)).Return(true);

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
            m_ControllerOne.Stub(x => x.ValidateAddress(newAddress)).Return(false);
            m_ControllerOneDataSource.Stub(x => x.GetDefaultDataType(Arg<string>.Is.Equal(newAddress), out Arg<BEDATATYPE>.Out(BEDATATYPE.DT_INTEGER2).Dummy)).Return(true);

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
