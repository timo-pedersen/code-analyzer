using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Core.Api.CrossReference;
using Core.Api.CrossReference.Storage;
using Core.Api.Feature;
using Core.Api.GlobalReference;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Core.Api.Tools;
using Core.Api.Utilities;
using Core.Component.Engine.Design;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Common.Serialization.Encryption;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls;
using Neo.ApplicationFramework.Controls.Chart;
using Neo.ApplicationFramework.Controls.ComponentLibrary.Model;
using Neo.ApplicationFramework.Controls.PropertyAdapters.Appearance;
using Neo.ApplicationFramework.Controls.Screen;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Constants;
using Neo.ApplicationFramework.Interfaces.Tag;
using Neo.ApplicationFramework.Interfaces.vNext.Gaps;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.TestUtilities.Utilities.DependencyObjectPropertyBinderTests.MockObjects;
using Neo.ApplicationFramework.Tools.Action;
using Neo.ApplicationFramework.Tools.CrossReference.Providers;
using Neo.ApplicationFramework.Tools.CrossReference.Resources;
using Neo.ApplicationFramework.Tools.FontUsageManager;
using Neo.ApplicationFramework.Tools.ProjectManager;
using Neo.ApplicationFramework.Tools.Symbol.Service;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.CrossReference
{
    [TestFixture]
    public class CrossReferenceStorageTest
    {
        private const string ArialFontName = "Arial"; 
        private const string ArialFontFileName = "arial.ttf";
        private const string TahomaFontName = "Tahoma";
        private const string TahomaFontFileName = "tahoma.ttf";
        private const string TimesNewRomanFontName = "Times New Roman";
        private const string TimesNewRomanFontFileName = "times.ttf";

        private const string ScreenName = "Screen1";
        private const string SymbolName = "MySymbol";

        private ElementCanvas m_ElementCanvas;
        private CrossReferenceStorage m_CrossReferenceStorage;
        private IActionService m_ActionService;
        private IScreenDesignerProjectItemCrossReferenceItemSource m_CrossReferenceItemSource;
        private ICrossReferenceService m_CrossReferenceService;
        private IObjectPropertyService m_ObjectPropertyService;

        [SetUp]
        public void SetUp()
        {
            NeoDesignerProperties.IsInDesignMode = true;

            TestHelper.AddServiceStub<IProjectManager>();
            TestHelper.AddServiceStub<IEncryptionStrategyFactory>();
            TestHelper.AddServiceStub<IOpcClientServiceIde>();

            TestHelper.AddService<IPropertyBinderFactory>(new PropertyBinderFactory());
            
            var featureSecurityServiceIdeStub = Substitute.For<IFeatureSecurityServiceIde>();
            featureSecurityServiceIdeStub.IsActivated(Arg.Any<Type>()).Returns(true);

            var gapServiceStubLazy = Substitute.For<ILazy<IGapService>>();
            gapServiceStubLazy.Value.Returns(Substitute.For<IGapService>());
            gapServiceStubLazy.Value.IsSubjectConsideredGap(Arg.Any<MemberInfo>()).Returns(false);

            TestHelper.AddService<IComponentInfoFactory>(new ComponentInfoFactory());

            IToolManager toolManagerMock = Substitute.For<IToolManager>();
            TestHelper.AddService<IToolManager>(toolManagerMock);
            TestHelper.AddService<IFontService>(new FontService());

            var symbolServiceIde = new TestableSymbolServiceIde();
            TestHelper.AddService<ISymbolServiceIde>(symbolServiceIde);

            symbolServiceIde.AddSymbol(SymbolName, new SymbolInfo(true));

            ITarget target = new Target(TargetPlatform.WindowsCE, string.Empty, string.Empty);
            ITargetService targetServiceMock = Substitute.For<ITargetService>();
            targetServiceMock.CurrentTarget = target;
            TestHelper.AddService<ITargetService>(targetServiceMock);

            
            BitmapSource bitmapSourceMock = Substitute.For<BitmapSource>();
            ISymbolService symbolServiceMock = Substitute.For<ISymbolService>();
            symbolServiceMock.GetSymbolFx(SymbolName).Returns(bitmapSourceMock);
            TestHelper.AddService<ISymbolService>(symbolServiceMock);

            m_ActionService = new ActionService(featureSecurityServiceIdeStub, gapServiceStubLazy);
            m_ActionService.AddNoneAction();
            m_ActionService.AddActionType(typeof(ITagActions));
            m_ActionService.AddActionType(typeof(Screen.ScreenDesign.Screen));
            m_ActionService.AddActionType(typeof(Recipe.Recipe));
            TestHelper.AddService<IActionService>(m_ActionService);

            IServiceContainer serviceContainer = new ServiceContainer();
            serviceContainer.AddService(typeof(IAppearanceAdapterService), new AppearanceAdapterService());

            var designerHost = Substitute.For<INeoDesignerHost>();
            designerHost.GetService(Arg.Any<Type>()).ReturnsForAnyArgs(x => serviceContainer.GetService((Type)x[0]));
            serviceContainer.AddService(typeof(INeoDesignerHost), designerHost);
            serviceContainer.AddService(typeof(IDesignerHost), designerHost);

            var rootComponent = Substitute.For<IComponent>();
            var site = Substitute.For<ISite>();
            site.Name = ScreenName;
            rootComponent.Site = site;
            designerHost.RootComponent.Returns(rootComponent);

            var globalReferenceService = Substitute.For<IGlobalReferenceService>();
            var globalDataItem = Substitute.For<IGlobalDataItem>();
            globalReferenceService.GetObject<IGlobalDataItem>(Arg.Any<String>()).Returns(globalDataItem);
            TestHelper.AddService(typeof(IGlobalReferenceService), globalReferenceService);

            m_ElementCanvas = new ElementCanvas();
            m_ElementCanvas.ServiceProvider = serviceContainer;

            m_CrossReferenceItemSource = Substitute.For<IScreenDesignerProjectItemCrossReferenceItemSource>(); 
            (m_CrossReferenceItemSource as IProjectItem).Name = ScreenName;
            (m_CrossReferenceItemSource as ICrossReferenceItemSource).Name.Returns(ScreenName);

            m_CrossReferenceService = TestHelper.CreateAndAddServiceStub<ICrossReferenceService>();

            // the categories used within the tests
            string[] categoryNames =
            {
                CrossReferenceTypes.GlobalDataItem.ToString(), 
                CrossReferenceTypes.Font.ToString(), 
                CrossReferenceTypes.Symbol.ToString(),
                CrossReferenceTypes.Recipe.ToString(),
                CrossReferenceTypes.Screen.ToString()
            };
            m_CrossReferenceService.GetCategoryNames().Returns(categoryNames);
            m_CrossReferenceService.GetCategoryNames(Arg.Any<Func<ICrossReferenceProvider, bool>>())
                .Returns(categoryNames);
            
            List<ICrossReferenceProvider> providers = new List<ICrossReferenceProvider>();

            m_CrossReferenceService.WhenForAnyArgs(x => x.RegisterCrossReferenceProvider(Arg.Any<ICrossReferenceProvider>()))
                .Do(inv => providers.Add((ICrossReferenceProvider)inv[0]));

            m_CrossReferenceService.GetAllProviders(Arg.Any<Func<ICrossReferenceProvider, bool>>())
                .Returns(inv => providers.Where((Func<ICrossReferenceProvider, bool>)inv[0]));
            m_CrossReferenceService.CreateSupportedFinders(Arg.Any<ICrossReferenceItemSource>())
                .Returns(providers.Select(item => item.CreateFinder(m_CrossReferenceItemSource)));

            ProviderHelper.RegisterAll(m_CrossReferenceService);

            var detailedCrossReferenceTargetProvider = new IDetailedCrossReferenceTargetProvider[]
            {
                new StringIntervalsCrossReferenceTargetProvider(),
                new SubItemsCrossReferenceTargetProvider()
            };
            m_CrossReferenceStorage = new CrossReferenceStorage(m_CrossReferenceItemSource, m_CrossReferenceService, detailedCrossReferenceTargetProvider);

            TestHelper.Bindings.Wpf.RegisterSimpleDataItemBindingSourceProvider();

            m_ObjectPropertyService = TestHelper.CreateAndAddServiceStub<IObjectPropertyService>();

            TestHelper.AddServiceStub<IEventBrokerService>();

            TestHelper.SetupServicePlatformFactory(Substitute.For<IKeyboardHelper>());
        }

        [TearDown]
        public void TearDown()
        {
            NeoDesignerProperties.IsInDesignMode = false;

            m_ElementCanvas.Children.Clear();
            m_CrossReferenceStorage.ClearAllReferences();

        }

        [Test]
        public void FindFontReferences()
        {
            CreateAndAddTextBox("TextBox1", ArialFontName);

            m_CrossReferenceStorage.FindReferences<object, object>(Elements, null, _ => true);

            ICrossReferenceContainer fontReferenceContainer = m_CrossReferenceStorage[CrossReferenceTypes.Font.ToString()];

            Assert.AreEqual(1, fontReferenceContainer.Count());

            AssertFontReferenceItemEquals(fontReferenceContainer[0], "TextBox1", ArialFontFileName);
        }


        private static class ProviderHelper
        {

            public static void RegisterAll(ICrossReferenceServiceSignature crossReferenceService)
            {
                RegisterProvidersForAllDesigners(crossReferenceService);
                RegisterProvidersForScreenDesigners(crossReferenceService);
            }

            private static void RegisterProvidersForScreenDesigners(ICrossReferenceServiceSignature crossReferenceService)
            {
                crossReferenceService.RegisterCrossReferenceProvider(new SymbolDesignerCrossReferenceProvider());
                crossReferenceService.RegisterCrossReferenceProvider(new UriDesignerCrossReferenceProvider());
                crossReferenceService.RegisterCrossReferenceProvider(new AlarmViewerDesignerCrossReferenceProvider());
                crossReferenceService.RegisterCrossReferenceProvider(new TrendViewerDesignerCrossReferenceProvider());
                crossReferenceService.RegisterCrossReferenceProvider(new AliasDataItemDesignerCrossReferenceProvider());
                crossReferenceService.RegisterCrossReferenceProvider(new ScreenDesignerIdCrossReferenceProvider());
                crossReferenceService.RegisterCrossReferenceProvider(new TextLibraryDesignerCrossReferenceProvider());
                crossReferenceService.RegisterCrossReferenceProvider(new FontDesignerCrossReferenceProvider());
                crossReferenceService.RegisterCrossReferenceProvider(new RenderableDesignerCrossReferenceProvider());

            }

            private static void RegisterProvidersForAllDesigners(ICrossReferenceServiceSignature crossReferenceService)
            {
                crossReferenceService.RegisterCrossReferenceProvider(new DataItemCrossReferenceProvider());
                crossReferenceService.RegisterCrossReferenceProvider(new DatabaseServiceActionCrossReferenceProvider());
                crossReferenceService.RegisterCrossReferenceProvider(new ActionAliasCrossReferenceProvider());
                crossReferenceService.RegisterCrossReferenceProvider(new DataConnectionStringCrossReferenceProvider());
                crossReferenceService.RegisterCrossReferenceProvider(new DataConnectionStringLogItemCrossReferenceProvider());
                crossReferenceService.RegisterCrossReferenceProvider(new ScriptTagCrossReferenceProvider());
                crossReferenceService.RegisterCrossReferenceProvider(new EventHookedScriptTagCrossReferenceProvider());
                crossReferenceService.RegisterCrossReferenceProvider(new RecipeActionCrossReferenceProvider());
                crossReferenceService.RegisterCrossReferenceProvider(new ScreenActionCrossReferenceProvider());
                crossReferenceService.RegisterCrossReferenceProvider(new DataLoggerActionCrossReferenceProvider());
                crossReferenceService.RegisterCrossReferenceProvider(new ReportCrossReferenceProvider());
                crossReferenceService.RegisterCrossReferenceProvider(new ExpressionCrossReferenceProvider());
                crossReferenceService.RegisterCrossReferenceProvider(new StructuredTagCrossReferenceProvider());
                crossReferenceService.RegisterCrossReferenceProvider(new DDXCrossReferenceProvider());
                crossReferenceService.RegisterCrossReferenceProvider(new TagsInExpressionsCrossReferenceProvider());
                crossReferenceService.RegisterCrossReferenceProvider(new ObjectDesignerCrossReferenceProvider());
            }
        }
        
        [Test]
        public void FindDataItems()
        {
            const string dataItemFullName = "Controller1.D0";

            CreateAndAddAnalogNumeric("AnalogNumeric1", ArialFontName, dataItemFullName);

            m_CrossReferenceStorage.FindReferences<object, object>(Elements, null, _ => true);

            ICrossReferenceContainer dataItemReferenceContainer = m_CrossReferenceStorage[CrossReferenceTypes.GlobalDataItem.ToString()];

            Assert.AreEqual(1, dataItemReferenceContainer.Count());
            AssertCrossReferenceItemEquals(dataItemReferenceContainer[0], dataItemFullName, GetTargetFullName("AnalogNumeric1"), AnalogNumericFX.ValueProperty.Name);
        }

        [Test]
        public void FindDataItemsInSubItems()
        {
            const string dataItemFullName = "Controller1.X0";

            var chart = new ChartHost();
            chart.Name = "Chart1";
            var series = new Series { DisplayName = "MySeries" };
            chart.Series.Add(series);

            var dataItemProxyProviderMock = new DataItemProxyProviderMock();
            dataItemProxyProviderMock.ProxyList.Add(dataItemFullName, new DataItemProxyMock<int>(dataItemFullName));

            IPropertyBinderWpf propertyBinder = new DependencyObjectPropertyBinder(dataItemProxyProviderMock);
            propertyBinder.BindToDataItem(series, Series.XValuesProperty, dataItemFullName, null);

            AddElement(chart);

            m_CrossReferenceStorage.FindReferences<object, object>(Elements, null, _ => true);

            ICrossReferenceContainer dataItemReferenceContainer = m_CrossReferenceStorage[CrossReferenceTypes.GlobalDataItem.ToString()];

            Assert.AreEqual(1, dataItemReferenceContainer.Count());
            AssertCrossReferenceItemEquals(dataItemReferenceContainer[0], dataItemFullName, GetTargetFullName("Chart1", series.DisplayName), Series.XValuesProperty.Name);
        }

        [Test]
        public void FindSymbols()
        {
            Button button = CreateAndAddButtonWithSymbol("Button1", ArialFontName);

            m_CrossReferenceStorage.FindReferences<object, object>(Elements, null, _ => true);

            ICrossReferenceContainer symbolReferenceContainer = m_CrossReferenceStorage[CrossReferenceTypes.Symbol.ToString()];

            Assert.AreEqual(1, symbolReferenceContainer.Count());
            AssertCrossReferenceItemEquals(symbolReferenceContainer[0], button.SymbolName, GetTargetFullName("Button1"), string.Empty);
        }

        [Test]
        public void FindFontsForMultipleElements()
        {
            CreateAndAddButtonWithSymbol("Button1", ArialFontName);
            CreateAndAddButtonWithSymbol("Button2", TimesNewRomanFontName);
            CreateAndAddAnalogNumeric("AnalogNumeric1", ArialFontName, "Controller1.D0");
            CreateAndAddAnalogNumeric("AnalogNumeric2", ArialFontName, "Controller1.C0");
            CreateAndAddTextBox("TextBox1", ArialFontName);
            CreateAndAddTextBox("TextBox2", TahomaFontName);

            m_CrossReferenceStorage.FindReferences<object, object>(Elements, null, _ => true);

            ICrossReferenceContainer fontReferenceContainer = m_CrossReferenceStorage[CrossReferenceTypes.Font.ToString()];

            Assert.AreEqual(6, fontReferenceContainer.Count());

            AssertFontReferenceItemEquals(fontReferenceContainer[0], "Button1", ArialFontFileName);
            AssertFontReferenceItemEquals(fontReferenceContainer[1], "Button2", TimesNewRomanFontFileName);
            AssertFontReferenceItemEquals(fontReferenceContainer[2], "AnalogNumeric1", ArialFontFileName);
            AssertFontReferenceItemEquals(fontReferenceContainer[3], "AnalogNumeric2", ArialFontFileName);
            AssertFontReferenceItemEquals(fontReferenceContainer[4], "TextBox1", ArialFontFileName);
            AssertFontReferenceItemEquals(fontReferenceContainer[5], "TextBox2", TahomaFontFileName);
        }

        [Test]
        public void FindDataItemsForMultipleElements()
        {
            CreateAndAddButtonWithSymbol("Button1", ArialFontName);
            CreateAndAddButtonWithSymbol("Button2", TimesNewRomanFontName);
            CreateAndAddAnalogNumeric("AnalogNumeric1", ArialFontName, "Controller1.D0");
            CreateAndAddAnalogNumeric("AnalogNumeric2", ArialFontName, "Controller1.C0");
            CreateAndAddTextBox("TextBox1", ArialFontName);
            CreateAndAddTextBox("TextBox2", TahomaFontName);

            m_CrossReferenceStorage.FindReferences<object, object>(Elements, null, _ => true);

            ICrossReferenceContainer dataItemReferenceContainer = m_CrossReferenceStorage[CrossReferenceTypes.GlobalDataItem.ToString()];

            Assert.AreEqual(2, dataItemReferenceContainer.Count());

            AssertCrossReferenceItemEquals(dataItemReferenceContainer[0], "Controller1.D0", GetTargetFullName("AnalogNumeric1"), AnalogNumericFX.ValueProperty.Name);
            AssertCrossReferenceItemEquals(dataItemReferenceContainer[1], "Controller1.C0", GetTargetFullName("AnalogNumeric2"), AnalogNumericFX.ValueProperty.Name);
        }      

        [Test]
        public void SaveThenLoadResultIsSame()
        {
            CreateAndAddButtonWithSymbol("Button1", ArialFontName);
            CreateAndAddButtonWithSymbol("Button2", TimesNewRomanFontName);
            CreateAndAddAnalogNumeric("AnalogNumeric1", ArialFontName, "Controller1.D0");
            CreateAndAddAnalogNumeric("AnalogNumeric2", ArialFontName, "Controller1.C0");
            CreateAndAddTextBox("TextBox1", ArialFontName);
            CreateAndAddTextBox("TextBox2", TahomaFontName);

            m_CrossReferenceStorage.FindReferences<object, object>(Elements, null, _ => true);


            var detailedCrossReferenceTargetProvider = new IDetailedCrossReferenceTargetProvider[]
            {
                new StringIntervalsCrossReferenceTargetProvider(),
                new SubItemsCrossReferenceTargetProvider()
            };

            var crossReferenceStorage = new CrossReferenceStorage(m_CrossReferenceItemSource, m_CrossReferenceService, detailedCrossReferenceTargetProvider);
            byte[] savedData;
            byte[] resavedData;

            MemoryStream currentMemoryStream = null;

            Func<Stream> streamProvider = () => {
                currentMemoryStream = new MemoryStream();
                return currentMemoryStream;
            };
            m_CrossReferenceItemSource.GetStreamWrite().Returns(a => streamProvider());
            
            m_CrossReferenceStorage.Save();
            savedData = currentMemoryStream.ToArray();

            MemoryStream loadStream = new MemoryStream(savedData);
            m_CrossReferenceItemSource.GetStreamRead().Returns(loadStream);
            crossReferenceStorage.Load(checkXmlFileValidity:false);

            crossReferenceStorage.Save();
            resavedData = currentMemoryStream.ToArray();

            Assert.AreEqual(true, ArraysIdentical(savedData, resavedData));
        }

        [Test]
        public void FindDataItemsInAction()
        {
            const string dataItemFullName = "Controller1.D0";
            const string actionName = ActionConstants.IncrementAnalogActionName;
            const string eventName = "Click";
            const string elementName = "AnalogNumeric1";

            AnalogNumericFX analogNumeric = CreateAndAddAnalogNumeric(elementName, ArialFontName, null);

            CreateAndAddAction(dataItemFullName, actionName, eventName, analogNumeric);

            m_CrossReferenceStorage.FindReferences<object, object>(Elements, null, _ => true);

            ICrossReferenceContainer dataItemReferenceContainer = m_CrossReferenceStorage[CrossReferenceTypes.GlobalDataItem.ToString()];

            Assert.AreEqual(1, dataItemReferenceContainer.Count());
            AssertActionCrossReferenceItemEquals(
                dataItemReferenceContainer[0],
                dataItemFullName,
                GetTargetFullName(elementName),
                ActionProperties.ActionsProperty.Name,
                actionName, eventName);
        }

        [Test]
        public void FindScreenReferencesInAction()
        {
            const string screenName = "Screen2";
            const string actionName = ActionConstants.ShowScreenActionName;
            const string eventName = "Click";
            const string elementName = "Button1";

            Button button = CreateAndAddButtonWithSymbol(elementName, TahomaFontName);

            CreateAndAddAction(screenName, actionName, eventName, button);

            m_CrossReferenceStorage.FindReferences<object, object>(Elements, null, _ => true);

            ICrossReferenceContainer dataItemReferenceContainer = m_CrossReferenceStorage[CrossReferenceTypes.Screen.ToString()];

            Assert.AreEqual(1, dataItemReferenceContainer.Count());
            AssertActionCrossReferenceItemEquals(
                dataItemReferenceContainer[0],
                screenName,
                GetTargetFullName(elementName),
                ActionProperties.ActionsProperty.Name,
                actionName, eventName);
        }

        [Test]
        public void FindRecipeReferencesInAction()
        {
            const string recipeName = "Recipe2.Field23";
            const string actionName = ActionConstants.LoadRecipeActionName;
            const string eventName = "Click";
            const string elementName = "Button1";

            Button button = CreateAndAddButtonWithSymbol(elementName, TahomaFontName);

            CreateAndAddAction(recipeName, actionName, eventName, button);

            m_CrossReferenceStorage.FindReferences<object, object>(Elements, null, _ => true); ;

            ICrossReferenceContainer dataItemReferenceContainer = m_CrossReferenceStorage[CrossReferenceTypes.Recipe.ToString()];

            Assert.AreEqual(1, dataItemReferenceContainer.Count());
            AssertActionCrossReferenceItemEquals(
                dataItemReferenceContainer[0],
                recipeName,
                GetTargetFullName(elementName),
                ActionProperties.ActionsProperty.Name,
                actionName, eventName);
        }

        [Test]
        public void ISupportStringIntervalsContainsIntervalsMember()
        {
            //If this test fails due to renaming of Intervals, StringInterval.DisplayName must be updated
            Assert.IsNotNull(typeof(ISupportStringIntervals).GetProperty("Intervals"));
        }

        [Test]
        public void LoadAndFindReferencesForCategoryNotInXml()
        {
            // Arrange
            Func<Stream> streamProvider = () => new MemoryStream(Encoding.UTF8.GetBytes(FileResources.CrossReferencesNoTextLibraryCategory));
            m_CrossReferenceItemSource.GetStreamRead().Returns(streamProvider());
            // Act
            m_CrossReferenceStorage.Load();

            // Assert
            Assert.IsNotNull(m_CrossReferenceStorage[CrossReferenceTypes.TextLibrary.ToString()]);
            m_CrossReferenceItemSource.Received(1).GetStreamRead();
        }

        private void CreateAndAddAction(string sourceName, string actionName, string eventName, FrameworkElement element)
        {
            IAction action = m_ActionService.CreateAction(actionName);
            action.ActionMethodInfo.ObjectName = sourceName;
            action.ActionMethodInfo.EventName = eventName;

            var actionList = new ActionList();
            actionList.Add(action);
            m_ActionService.SetActionList(element, actionList);
        }

        private static string GetTargetFullName(string elementName)
        {
            return string.Format("{0}{1}{2}", ScreenName, StringConstants.ObjectNameSeparator, elementName);
        }

        private string GetTargetFullName(string elementName, string propertyName)
        {
            return string.Format("{0}{1}{2}", GetTargetFullName(elementName), StringConstants.ObjectNameSeparator, propertyName);
        }

        private ICollection Elements
        {
            get { return m_ElementCanvas.Children; }
        }

        private void AddElement(FrameworkElement element)
        {
            m_ElementCanvas.Children.Add(element);
        }

        private TextBox CreateAndAddTextBox(string name, string fontFamilyName)
        {
            var textBox = new TextBox();
            textBox.Name = name;
            textBox.FontFamily = new FontFamily(fontFamilyName);

            AddElement(textBox);

            return textBox;
        }

        private AnalogNumericFX CreateAndAddAnalogNumeric(string name, string fontFamilyName, string dataItemFullName)
        {
            var analogNumeric = new AnalogNumericFX();
            analogNumeric.Name = name;
            analogNumeric.FontFamily = new FontFamily(fontFamilyName);

            if (!string.IsNullOrEmpty(dataItemFullName))
            {
                var dataItemProxyProviderMock = new DataItemProxyProviderMock();
                dataItemProxyProviderMock.ProxyList.Add(dataItemFullName, new DataItemProxyMock<int>(dataItemFullName));

                IPropertyBinderWpf propertyBinder = new DependencyObjectPropertyBinder(dataItemProxyProviderMock);
                propertyBinder.BindToDataItem(analogNumeric, AnalogNumericFX.ValueProperty, dataItemFullName, null);
            }

            AddElement(analogNumeric);

            return analogNumeric;
        }

        private Button CreateAndAddButtonWithSymbol(string name, string fontFamilyName)
        {
            var button = new Button();
            button.Name = name;
            button.FontFamily = new FontFamily(fontFamilyName);
            button.SymbolName = SymbolName;

            AddElement(button);

            return button;
        }

        private static void AssertFontReferenceItemEquals(ICrossReferenceItem crossReferenceItem, string elementName, string fontFileName)
        {
            AssertCrossReferenceItemEquals(crossReferenceItem, null, GetTargetFullName(elementName), TextBox.FontFamilyProperty.Name);
            Assert.IsTrue(crossReferenceItem.SourceFullName.ToLower().Contains(fontFileName.ToLower()));
        }

        private static void AssertCrossReferenceItemEquals(ICrossReferenceItem crossReferenceItem, string expectedSourceFullName, string expectedTargetFullName, string expectedPropertyName)
        {
            if (expectedSourceFullName != null)
            {
                Assert.That(crossReferenceItem.SourceFullName, Is.EqualTo(expectedSourceFullName));
            }

            Assert.That(expectedTargetFullName, Is.EqualTo(crossReferenceItem.TargetFullName));
            Assert.That(expectedPropertyName, Is.EqualTo(crossReferenceItem.TargetPropertyName));
        }

        private static void AssertActionCrossReferenceItemEquals(ICrossReferenceItem crossReferenceItem, string expectedSourceFullName, string expectedTargetFullName, string expectedPropertyName, string expectedActionName, string expectedEventName)
        {
            AssertCrossReferenceItemEquals(crossReferenceItem, expectedSourceFullName, expectedTargetFullName, expectedPropertyName);

            var actionCrossReferenceItem = crossReferenceItem as ActionCrossReferenceItem;
            Assert.IsNotNull(actionCrossReferenceItem);
            Assert.AreEqual(expectedActionName, actionCrossReferenceItem.ActionName);
            Assert.AreEqual(expectedEventName, actionCrossReferenceItem.EventName);
        }

        private static bool ArraysIdentical(byte[] dataOne, byte[] dataTwo)
        {
            if (dataOne.Length != dataTwo.Length)
                return false;

            for (int index = 0; index < dataOne.Length; index++)
            {
                if (dataTwo[index] != dataOne[index])
                    return false;
            }

            return true;
        }

        public interface IScreenDesignerProjectItemCrossReferenceItemSource : IScreenDesignerProjectItem, ICrossReferenceItemSource { }

        private class TestableSymbolServiceIde : SymbolServiceIde
        {
            public void AddSymbol(string name, SymbolInfo symbolInfo)
            {
                m_SymbolDictionary.Add(name, symbolInfo);
            }
        }
    }
}
