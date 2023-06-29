using Core.Api.GlobalReference;
using Core.Api.Tools;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Tag;
using Neo.ApplicationFramework.Interop.DataSource;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcClient;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Recipe
{
    [TestFixture]
    public class RecipeItemTest
    {
        private IToolManager m_ToolManager;
        private IGlobalReferenceService m_GlobalReferenceService;

        [SetUp]
        public void SetUp()
        {
            NeoDesignerProperties.IsInDesignMode = true;

            m_ToolManager = TestHelper.CreateAndAddServiceStub<IToolManager>();
            m_GlobalReferenceService = TestHelper.CreateAndAddServiceStub<IGlobalReferenceService>();
        }

        [TearDown]
        public void TearDown()
        {
            NeoDesignerProperties.IsInDesignMode = false;

            TestHelper.ClearServices();
        }

        [Test]
        public void CanCreateRecipeItemInContainer()
        {
            RecipeItem recipeItem = new RecipeItem();

            Assert.IsNotNull(recipeItem, "Can not create RecipeItem");
        }

        [Test]
        public void RecipeItemReturnsDefaultDataTypeIfNotConnectedToAnyDataItem()
        {
            m_ToolManager.Runtime.Returns(false);

            RecipeItem recipeItem = new RecipeItem();

            Assert.AreEqual(BEDATATYPE.DT_DEFAULT, recipeItem.DataType);
        }

        [Test]
        public void RecipeItemReturnsDefaultDataTypeIfConnectedDataItemDoesNotExist()
        {
            m_ToolManager.Runtime.Returns(false);

            RecipeItem recipeItem = new RecipeItem();
            recipeItem.DataConnection = "Controller.Tag1";

            Assert.AreEqual(BEDATATYPE.DT_DEFAULT, recipeItem.DataType);
        }

        [Test]
        public void RecipeItemReturnsItsConnectedDataItemsDataTypeUponRequest()
        {
            m_ToolManager.Runtime.Returns(false);

            var dataItem = Substitute.For<IGlobalDataItemBase>();
            dataItem.GlobalDataTypeOrDataTypeIfDefault.Returns(BEDATATYPE.DT_INTEGER4);
            m_GlobalReferenceService.GetObject<IGlobalDataItemBase>("Controller.Tag1").Returns(dataItem);

            RecipeItem recipeItem = new RecipeItem();
            recipeItem.DataConnection = "Controller.Tag1";

            Assert.AreEqual(BEDATATYPE.DT_INTEGER4, recipeItem.DataType);
        }

        [Test]
        public void RecipeItemIsNotifiedWhenItsConnectedDataItemsDataTypeIsChanged()
        {
            m_ToolManager.Runtime.Returns(false);

            GlobalDataItem globalDataItem = new GlobalDataItem();
            m_GlobalReferenceService.GetObject<IGlobalDataItemBase>(Arg.Is("Controller.Tag1")).Returns(globalDataItem);

            RecipeItem recipeItem = new RecipeItem();
            recipeItem.DataConnection = "Controller.Tag1";

            globalDataItem.DataType = BEDATATYPE.DT_INTEGER4;

            Assert.AreEqual(BEDATATYPE.DT_INTEGER4, recipeItem.DataType);
        }

        [Test]
        public void RecipeItemReturnsItsConnectedDataItemsDataTypeUponRequestEvenWhenDataItemIsRemovedFromControllerAndLaterAddedAgain()
        {
            m_ToolManager.Runtime.Returns(false);

            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataType = BEDATATYPE.DT_INTEGER4;
            m_GlobalReferenceService.GetObject<IGlobalDataItemBase>(Arg.Is("Controller.Tag1")).Returns(globalDataItem);

            RecipeItem recipeItem = new RecipeItem();
            recipeItem.DataConnection = "Controller.Tag1";

            Assert.AreEqual(BEDATATYPE.DT_INTEGER4, recipeItem.DataType);

            // Simulate a change in controller, a new dataitem reference with the same name.
            globalDataItem = new GlobalDataItem();
            globalDataItem.DataType = BEDATATYPE.DT_REAL4;
            m_GlobalReferenceService.GetObject<IGlobalDataItemBase>(Arg.Is("Controller.Tag1")).Returns(globalDataItem);

            Assert.AreEqual(BEDATATYPE.DT_REAL4, recipeItem.DataType);
        }

        [Test]
        public void RecipeItemIsNotifiedWhenItsConnectedDataItemsDataTypeIsChangedEvenWhenDataItemIsRemovedFromControllerAndLaterAddedAgain()
        {
            m_ToolManager.Runtime.Returns(false);

            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataType = BEDATATYPE.DT_INTEGER4;
            m_GlobalReferenceService.GetObject<IGlobalDataItemBase>(Arg.Is("Controller.Tag1")).Returns(globalDataItem);

            RecipeItem recipeItem = new RecipeItem();
            recipeItem.DataConnection = "Controller.Tag1";

            Assert.AreEqual(BEDATATYPE.DT_INTEGER4, recipeItem.DataType);

            // Simulate a change in controller, a new dataitem reference with the same name.
            globalDataItem = new GlobalDataItem();
            m_GlobalReferenceService.GetObject<IGlobalDataItemBase>(Arg.Is("Controller.Tag1")).Returns(globalDataItem);

            Assert.AreEqual(BEDATATYPE.DT_DEFAULT, recipeItem.DataType);

            globalDataItem.DataType = BEDATATYPE.DT_REAL4;

            Assert.AreEqual(BEDATATYPE.DT_REAL4, recipeItem.DataType);
        }

        [Test]
        public void RecipeItemDoesNotHookUpPropertyChangedOnDataItemWhenSettingDataConnectionInRuntime()
        {
            m_ToolManager.Runtime.Returns(true);

            IGlobalDataItem globalDataItem = Substitute.For<IGlobalDataItem>();
            m_GlobalReferenceService.GetObject<IBasicTag>(Arg.Is("Controller.Tag1")).Returns(globalDataItem);

            RecipeItem recipeItem = new RecipeItem();
            recipeItem.DataConnection = "Controller.Tag1";

            globalDataItem.DidNotReceiveWithAnyArgs().PropertyChanged += null;
        }

        [Test]
        public void RecipeItemDoesNotHookUpPropertyChangedOnDataItemWhenAccessingDataItemInRuntime()
        {
            m_ToolManager.Runtime.Returns(true);

            var tag = Substitute.For<IGlobalDataItem, IGlobalDataItemBase>();
            m_GlobalReferenceService.GetObject<IBasicTag>(Arg.Any<string>()).Returns(tag);

            RecipeItem recipeItem = new RecipeItem();
            recipeItem.DataConnection = "Controller.Tag1";

            var globalDataItem = (IGlobalDataItem)recipeItem.DataItem;

            globalDataItem.DidNotReceiveWithAnyArgs().PropertyChanged += null;
        }

        [Test]
        public void RecipeItemBindsToDataItemWhenSettingDataConnection()
        {
            IGlobalDataItem globalDataItem = Substitute.For<IGlobalDataItem>();
            m_GlobalReferenceService.GetObject<IBasicTag>(Arg.Is("Controller.Tag1")).Returns(globalDataItem);

            RecipeItem recipeItem = new RecipeItem();
            recipeItem.DataConnection = "Controller.Tag1";

            Assert.AreEqual(1, recipeItem.DataBindings.Count);
            IDataItemProxy dataItemProxy = recipeItem.DataBindings[0].DataSource as IDataItemProxy;
            Assert.IsNotNull(dataItemProxy);
            Assert.AreEqual("Controller.Tag1", dataItemProxy.FullName);
        }

        [Test]
        public void RecipeItemDoesNotBindToDataItemWhenSettingDataConnectionInRuntime()
        {
            NeoDesignerProperties.IsInDesignMode = false;

            m_ToolManager.Runtime.Returns(true);

            IGlobalDataItem globalDataItem = Substitute.For<IGlobalDataItem>();
            m_GlobalReferenceService.GetObject<IBasicTag>(Arg.Is("Controller.Tag1")).Returns(globalDataItem);

            RecipeItem recipeItem = new RecipeItem();
            recipeItem.DataConnection = "Controller.Tag1";

            Assert.AreEqual(0, recipeItem.DataBindings.Count);
        }
    }
}
