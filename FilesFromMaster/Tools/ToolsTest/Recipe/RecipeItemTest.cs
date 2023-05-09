using Core.Api.GlobalReference;
using Core.Api.Tools;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Tag;
using Neo.ApplicationFramework.Interop.DataSource;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcClient;
using NUnit.Framework;
using Rhino.Mocks;

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
            m_ToolManager.Stub(x => x.Runtime).Return(false).Repeat.Any();

            RecipeItem recipeItem = new RecipeItem();

            Assert.AreEqual(BEDATATYPE.DT_DEFAULT, recipeItem.DataType);
        }

        [Test]
        public void RecipeItemReturnsDefaultDataTypeIfConnectedDataItemDoesNotExist()
        {
            m_ToolManager.Stub(x => x.Runtime).Return(false).Repeat.Any();

            RecipeItem recipeItem = new RecipeItem();
            recipeItem.DataConnection = "Controller.Tag1";

            Assert.AreEqual(BEDATATYPE.DT_DEFAULT, recipeItem.DataType);
        }

        [Test]
        public void RecipeItemReturnsItsConnectedDataItemsDataTypeUponRequest()
        {
            m_ToolManager.Stub(x => x.Runtime).Return(false).Repeat.Any();

            var tag = MockRepository.GenerateStub<IBasicTag>();
            tag.Stub(x => x.GlobalDataTypeOrDataTypeIfDefault).Return(BEDATATYPE.DT_INTEGER4);
            m_GlobalReferenceService.Stub(x => x.GetObject<IBasicTag>(Arg<string>.Is.Equal("Controller.Tag1"))).Return(tag).Repeat.Any();

            RecipeItem recipeItem = new RecipeItem();
            recipeItem.DataConnection = "Controller.Tag1";

            Assert.AreEqual(BEDATATYPE.DT_INTEGER4, recipeItem.DataType);
        }

        [Test]
        public void RecipeItemIsNotifiedWhenItsConnectedDataItemsDataTypeIsChanged()
        {
            m_ToolManager.Stub(x => x.Runtime).Return(false).Repeat.Any();

            GlobalDataItem globalDataItem = new GlobalDataItem();
            m_GlobalReferenceService.Stub(x => x.GetObject<IBasicTag>(Arg<string>.Is.Equal("Controller.Tag1"))).Return(globalDataItem).Repeat.Any();

            RecipeItem recipeItem = new RecipeItem();
            recipeItem.DataConnection = "Controller.Tag1";

            globalDataItem.DataType = BEDATATYPE.DT_INTEGER4;

            Assert.AreEqual(BEDATATYPE.DT_INTEGER4, recipeItem.DataType);
        }

        [Test]
        public void RecipeItemReturnsItsConnectedDataItemsDataTypeUponRequestEvenWhenDataItemIsRemovedFromControllerAndLaterAddedAgain()
        {
            m_ToolManager.Stub(x => x.Runtime).Return(false).Repeat.Any();

            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataType = BEDATATYPE.DT_INTEGER4;
            m_GlobalReferenceService.Stub(x => x.GetObject<IGlobalDataItemBase>(Arg<string>.Is.Equal("Controller.Tag1"))).Return(globalDataItem).Repeat.Any();

            RecipeItem recipeItem = new RecipeItem();
            recipeItem.DataConnection = "Controller.Tag1";

            Assert.AreEqual(BEDATATYPE.DT_INTEGER4, recipeItem.DataType);

            // Clear previous expectation, by entering record mode.
            m_GlobalReferenceService.BackToRecord();

            // Simulate a change in controller, a new dataitem reference with the same name.
            globalDataItem = new GlobalDataItem();
            globalDataItem.DataType = BEDATATYPE.DT_REAL4;
            m_GlobalReferenceService.Stub(x => x.GetObject<IGlobalDataItemBase>(Arg<string>.Is.Equal("Controller.Tag1"))).Return(globalDataItem).Repeat.Any();

            // Make new expectation apply, by entering replay mode.
            m_GlobalReferenceService.Replay();

            Assert.AreEqual(BEDATATYPE.DT_REAL4, recipeItem.DataType);
        }

        [Test]
        public void RecipeItemIsNotifiedWhenItsConnectedDataItemsDataTypeIsChangedEvenWhenDataItemIsRemovedFromControllerAndLaterAddedAgain()
        {
            m_ToolManager.Stub(x => x.Runtime).Return(false).Repeat.Any();

            GlobalDataItem globalDataItem = new GlobalDataItem();
            globalDataItem.DataType = BEDATATYPE.DT_INTEGER4;
            m_GlobalReferenceService.Stub(x => x.GetObject<IGlobalDataItemBase>(Arg<string>.Is.Equal("Controller.Tag1"))).Return(globalDataItem).Repeat.Any();

            RecipeItem recipeItem = new RecipeItem();
            recipeItem.DataConnection = "Controller.Tag1";

            Assert.AreEqual(BEDATATYPE.DT_INTEGER4, recipeItem.DataType);

            // Clear previous expectation, by entering record mode.
            m_GlobalReferenceService.BackToRecord();

            // Simulate a change in controller, a new dataitem reference with the same name.
            globalDataItem = new GlobalDataItem();
            m_GlobalReferenceService.Stub(x => x.GetObject<IGlobalDataItemBase>(Arg<string>.Is.Equal("Controller.Tag1"))).Return(globalDataItem).Repeat.Any();

            // Make new expectation apply, by entering replay mode.
            m_GlobalReferenceService.Replay();

            Assert.AreEqual(BEDATATYPE.DT_DEFAULT, recipeItem.DataType);

            globalDataItem.DataType = BEDATATYPE.DT_REAL4;

            Assert.AreEqual(BEDATATYPE.DT_REAL4, recipeItem.DataType);
        }

        [Test]
        public void RecipeItemDoesNotHookUpPropertyChangedOnDataItemWhenSettingDataConnectionInRuntime()
        {
            m_ToolManager.Stub(x => x.Runtime).Return(true).Repeat.Any();

            IGlobalDataItem globalDataItem = MockRepository.GenerateStub<IGlobalDataItem>();
            m_GlobalReferenceService.Stub(x => x.GetObject<IBasicTag>(Arg<string>.Is.Equal("Controller.Tag1"))).Return(globalDataItem).Repeat.Any();

            RecipeItem recipeItem = new RecipeItem();
            recipeItem.DataConnection = "Controller.Tag1";

            globalDataItem.AssertWasNotCalled(x => x.PropertyChanged += null, options => options.IgnoreArguments());
        }

        /// <summary>
        /// This interface is needed since IGlobalDataItem today is not inheritinh IGlobalDataItemBase
        /// </summary>
        public interface IGlobalDataItemAndIGlobalDataItemBase : IGlobalDataItem, IGlobalDataItemBase
        {
             
        }

        [Test]
        public void RecipeItemDoesNotHookUpPropertyChangedOnDataItemWhenAccessingDataItemInRuntime()
        {
            m_ToolManager.Stub(x => x.Runtime).Return(true).Repeat.Any();

            m_GlobalReferenceService.Stub(x => x.GetObject<IBasicTag>(Arg<string>.Is.Equal("Controller.Tag1"))).Return(MockRepository.GenerateStub<IGlobalDataItemAndIGlobalDataItemBase>()).Repeat.Any();

            RecipeItem recipeItem = new RecipeItem();
            recipeItem.DataConnection = "Controller.Tag1";

            var globalDataItem = (IGlobalDataItem)recipeItem.DataItem;

            globalDataItem.AssertWasNotCalled(x => x.PropertyChanged += null, options => options.IgnoreArguments());
        }

        [Test]
        public void RecipeItemBindsToDataItemWhenSettingDataConnection()
        {
            IGlobalDataItem globalDataItem = MockRepository.GenerateStub<IGlobalDataItem>();
            m_GlobalReferenceService.Stub(x => x.GetObject<IBasicTag>(Arg<string>.Is.Equal("Controller.Tag1"))).Return(globalDataItem).Repeat.Any();

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

            m_ToolManager.Stub(x => x.Runtime).Return(true).Repeat.Any();

            IGlobalDataItem globalDataItem = MockRepository.GenerateStub<IGlobalDataItem>();
            m_GlobalReferenceService.Stub(x => x.GetObject<IBasicTag>(Arg<string>.Is.Equal("Controller.Tag1"))).Return(globalDataItem).Repeat.Any();

            RecipeItem recipeItem = new RecipeItem();
            recipeItem.DataConnection = "Controller.Tag1";

            Assert.AreEqual(0, recipeItem.DataBindings.Count);
        }
    }
}
