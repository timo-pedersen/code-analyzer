using System.Collections.ObjectModel;
using Core.Api.DataSource;
using Neo.ApplicationFramework.Interfaces;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.TestUtilities
{
    public static class ControllerHelper
    {
     
        public static void CreateStubControllerWithDataItem(out IDataSourceContainer controller, out IDataItem dataItem, string controllerName, string dataItemName)
        {
            controller = MockRepository.GenerateStub<IDataSourceContainer>();
            controller.Name = controllerName;
            controller.IsActive = true;

            CreateStubDataItemInStubController(controller, out dataItem, dataItemName);
            ReadOnlyCollection<IDataItemBase> dataItems = new ReadOnlyCollection<IDataItemBase>(
                new IDataItemBase[]
                    {
                        dataItem
                    });
            controller.Stub(x => x.DataItemBases).Return(dataItems);
        }

        public static void CreateStubControllerWithDataItem(IGlobalDataItem globalDataItem, string controllerName, string dataItemName)
        {
            IDataSourceContainer controller;
            IDataItem dataItem;

            CreateStubControllerWithDataItem(out controller, out dataItem, controllerName, dataItemName);

            globalDataItem.DataItems.Add(dataItem);
            globalDataItem.AccessRights[controller.Name] = AccessRights.ReadWrite;
        }

        public static IDataSourceContainer CreateStubController(out IDataSourceContainer controller, string controllerName)
        {
            controller = MockRepository.GenerateStub<IDataSourceContainer>();
            controller.Name = controllerName;
            controller.IsActive = true;
            return controller;
        }

        public static void CreateStubDataItemInStubController(IDataSourceContainer controller, out IDataItem dataItem, string dataItemName)
        {
            string fullName = string.Format("{0}.{1}", controller.Name, dataItemName);
            IDataItem newDataItem = MockRepository.GenerateStub<IDataItem>();
            newDataItem.Stub(x => x.FullName).Return(fullName);
            newDataItem.Stub(x => x.SetValueForced(null)).IgnoreArguments().WhenCalled(invocation => newDataItem.Value = ((VariantValue)invocation.Arguments[0]));
            newDataItem.Stub(x => x.IncrementAnalogValue(null))
                .IgnoreArguments()
                .WhenCalled(invocation => newDataItem.Value = (new VariantValue(newDataItem.Value)).Double + ((VariantValue)invocation.Arguments[0]).Double);
            newDataItem.Stub(x => x.Toggle()).WhenCalled(invocation => newDataItem.Value = (new VariantValue(newDataItem.Value)).Double == 0 ? 1 : 0);
            newDataItem.Value = new VariantValue(0);
            newDataItem.DataSourceContainer = controller;

            dataItem = newDataItem;
        }

        public static IDataItem CreateStubDataItem(string dataItemName, string itemId)
        {
            IDataItem dataItem = MockRepository.GenerateStub<IDataItem>();
            dataItem.Name = dataItemName;
            dataItem.ItemID = itemId;
            dataItem.Value = new VariantValue(0);
            return dataItem;
        }
    }

}
