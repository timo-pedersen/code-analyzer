using System.Collections.ObjectModel;
using Core.Api.DataSource;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;

namespace Neo.ApplicationFramework.TestUtilities
{
    public static class ControllerHelper
    {
     
        public static void CreateStubControllerWithDataItem(out IDataSourceContainer controller, out IDataItem dataItem, string controllerName, string dataItemName)
        {
            controller = Substitute.For<IDataSourceContainer>();
            controller.Name = controllerName;
            controller.IsActive = true;

            CreateStubDataItemInStubController(controller, out dataItem, dataItemName);
            ReadOnlyCollection<IDataItemBase> dataItems = new ReadOnlyCollection<IDataItemBase>(
                new IDataItemBase[]
                    {
                        dataItem
                    });
            controller.DataItemBases.Returns(dataItems);
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
            controller = Substitute.For<IDataSourceContainer>();
            controller.Name = controllerName;
            controller.IsActive = true;
            return controller;
        }

        public static void CreateStubDataItemInStubController(IDataSourceContainer controller, out IDataItem dataItem, string dataItemName)
        {
            string fullName = string.Format("{0}.{1}", controller.Name, dataItemName);
            IDataItem newDataItem = Substitute.For<IDataItem>();
            newDataItem.FullName.Returns(fullName);
            newDataItem.When(x => x.SetValueForced(Arg.Any<VariantValue>()))
                .Do(y => newDataItem.Value = (VariantValue)y[0]);
            newDataItem.When(x => x.IncrementAnalogValue(Arg.Any<VariantValue>()))
                .Do(y => newDataItem.Value = (new VariantValue(newDataItem.Value)).Double + ((VariantValue)y[0]).Double);
            newDataItem.When(x => x.Toggle())
                .Do(y => newDataItem.Value = new VariantValue(newDataItem.Value).Double == 0 ? 1 : 0);
            newDataItem.Value = new VariantValue(0);
            newDataItem.DataSourceContainer = controller;

            dataItem = newDataItem;
        }
    }
}
