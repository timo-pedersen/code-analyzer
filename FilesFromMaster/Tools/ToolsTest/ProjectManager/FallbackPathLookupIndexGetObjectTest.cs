using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Windows;
using Core.Api.GlobalReference;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Core.Component.Api.Design;
using Core.Component.Engine.Design;
using Core.Controls.Api.Designer;
using Core.TestUtilities.Utilities;
using Neo.ApplicationFramework.Common.Dynamics;
using Neo.ApplicationFramework.Common.Keyboard;
using Neo.ApplicationFramework.Common.Test;
using Neo.ApplicationFramework.Controls.FunctionKey;
using Neo.ApplicationFramework.Controls.Trend;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Alarm;
using Neo.ApplicationFramework.Tools.Design;
using Neo.ApplicationFramework.Tools.FunctionKey;
using Neo.ApplicationFramework.Tools.MultiLanguage;
using Neo.ApplicationFramework.Tools.Selection;
using NUnit.Framework;
using Rhino.Mocks;
using Button = Neo.ApplicationFramework.Controls.Button;

namespace Neo.ApplicationFramework.Tools.ProjectManager
{
    [TestFixture]
    public class FallbackPathLookupIndexGetObjectTest
    {
        private IDesignerHost m_DesignerHost;
        private IFallbackPathLookupIndex m_PathLookupIndex;
        private ISubItemsServiceIde m_SubItemsServiceIde;

        private IProject m_ProjectStub;
        private IProjectManager m_ProjectManagerStub;

        private FunctionKeyManager m_FunctionKeyManager;
        private IFunctionKey m_GlobalFunctionKey;
        private IFunctionKey m_ScreenOneFunctionKey;
        private IFunctionKey m_ScreenTwoFunctionKey;

        private AlarmServer m_AlarmServer;
        private IAlarmItem m_AlarmItemGroupZero;
        private IAlarmItem m_AlarmItemGroupOne;
        private IAlarmItem m_AlarmItemGroupOneSameNameAsGroup;
        private DynamicStringItem m_DynamicStringItem;

        private ICurve m_Curve;

        private StringInterval m_StringInterval;

        private FrameworkElement[] m_ScreenElements;

        #region Test Setup

        [SetUp]
        public void SetUp()
        {
            NeoDesignerProperties.IsInDesignMode = true;

            TestHelperExtensions.AddServiceToolManager(false);

            TestHelper.SetupServicePlatformFactory<IKeyboardHelper>(new KeyboardHelper());

            m_ProjectStub = MockRepository.GenerateStub<IProject>();
            m_ProjectManagerStub = MockRepository.GenerateStub<IProjectManager>();

            m_ProjectManagerStub.Project = m_ProjectStub;

            m_PathLookupIndex = new FallbackPathLookupIndex(new LazyWrapper<IProjectManager>(() => m_ProjectManagerStub));
            TestHelper.AddService(m_PathLookupIndex);

            m_SubItemsServiceIde = new SubItemsServiceIde();
            TestHelper.AddService(m_SubItemsServiceIde);

            var securityService = MockRepository.GenerateStub<ISecurityServiceCF>();
            securityService.Stub(x => x.GetSecurityGroups(null)).IgnoreArguments().Return(SecurityGroups.None);
            TestHelper.AddService(securityService);

            var target = new Target(TargetPlatform.WindowsCE, string.Empty, string.Empty);

            var targetService = MockRepository.GenerateMock<ITargetService>();
            targetService.Stub(x => x.CurrentTarget).Return(target);
            TestHelper.AddService(targetService);

            CreateDesignerProjectItems();
        }

        [TearDown]
        public void TearDown()
        {
            NeoDesignerProperties.IsInDesignMode = false;
        }

        private void CreateDesignerProjectItems()
        {
            SetupDesignerHost();

            List<IDesignerProjectItem> designerProjectItems = new List<IDesignerProjectItem>();

            IDesignerProjectItem designerProjectItemFunctionKeys = CreateFunctionKeysDesignerProjectItem();
            designerProjectItems.Add(designerProjectItemFunctionKeys);

            IDesignerProjectItem designerProjectItemAlarmServer = CreateAlarmServerDesignerProjectItem();
            designerProjectItems.Add(designerProjectItemAlarmServer);

            IDesignerProjectItem screenDesignerProject = CreateScreenDesignerProjectItem();
            designerProjectItems.Add(screenDesignerProject);

            AddTrendViewerToScreen();
            AddButtonToScreen();

            m_ProjectStub.Stub(x => x.GetDesignerProjectItems()).Return(designerProjectItems.ToArray());
        }

        private IDesignerProjectItem CreateScreenDesignerProjectItem()
        {
            const string screenName = "Screen";
            var testSite = new TestSite
            {
                Name = screenName
            };
            var containedScreenObject = MockRepository.GenerateStub<ITestSubItemsComponent>();
            containedScreenObject.Site = testSite;

            var screenDesignerProjectItem = MockRepository.GenerateStub<IDesignerProjectItem>();
            screenDesignerProjectItem.Name = screenName;
            screenDesignerProjectItem.Stub(x => x.ContainedObject).Return(containedScreenObject);
            screenDesignerProjectItem.Stub(x => x.ProjectItems).Return(new IProjectItem[0]);
            screenDesignerProjectItem.Stub(x => x.DesignerType).Return(typeof(Screen.ScreenDesign.Screen));

            m_ScreenElements = new FrameworkElement[10];
            containedScreenObject.Stub(x => x.Items).Return(m_ScreenElements);

            return screenDesignerProjectItem;
        }

        private void SetupDesignerHost()
        {
            var testSite = new TestSite {Name = "TestSite"};
            IDesignerDocument designerDocument = new DesignerDocument(
                testSite,
                MockRepository.GenerateStub<IDesignerPersistenceService>(),
                MockRepository.GenerateStub<System.ComponentModel.Design.Serialization.INameCreationService>().ToILazy(),
                () => new SelectionService(),
                new LazyWrapper<IReferenceProvider>(
                    () => new GlobalReferenceToReferenceAdapter(Core.Api.Service.ServiceContainerCF.GetService<IGlobalReferenceService>())),
                new IDesignerSerializationProvider[] { new CodeDomMultiLanguageProvider(CodeDomLocalizationModel.PropertyReflection) }
            );
            m_DesignerHost = designerDocument.DesignerHost;
        }

        private IDesignerProjectItem CreateFunctionKeysDesignerProjectItem()
        {
            m_FunctionKeyManager = m_DesignerHost.CreateComponent(typeof(FunctionKeyManager)) as FunctionKeyManager;
            m_FunctionKeyManager.Name = "FunctionKeys";

            m_GlobalFunctionKey = new ApplicationFramework.Controls.FunctionKey.FunctionKey(System.Windows.Forms.Keys.F1);
            m_FunctionKeyManager.GlobalFunctionKeys.Add(m_GlobalFunctionKey);

            m_ScreenOneFunctionKey = new ApplicationFramework.Controls.FunctionKey.FunctionKey(System.Windows.Forms.Keys.F1);
            m_ScreenTwoFunctionKey = new ApplicationFramework.Controls.FunctionKey.FunctionKey(System.Windows.Forms.Keys.F1);

            var functionKeyContextScreenOne = m_DesignerHost.CreateComponent(typeof(FunctionKeyContext)) as IFunctionKeyContext;
            functionKeyContextScreenOne.ScreenName = "Screen1";
            functionKeyContextScreenOne.Name = "Screen1";

            var functionKeyContextScreenTwo = m_DesignerHost.CreateComponent(typeof(FunctionKeyContext)) as IFunctionKeyContext;
            functionKeyContextScreenTwo.ScreenName = "Screen2";
            functionKeyContextScreenTwo.Name = "Screen2";

            functionKeyContextScreenOne.FunctionKeys.Add(m_ScreenOneFunctionKey);
            functionKeyContextScreenTwo.FunctionKeys.Add(m_ScreenTwoFunctionKey);

            m_FunctionKeyManager.LocalFunctionKeys.Add(functionKeyContextScreenOne as FunctionKeyContext);
            m_FunctionKeyManager.LocalFunctionKeys.Add(functionKeyContextScreenTwo as FunctionKeyContext);

            IDesignerProjectItem designerProjectItem = CreateDesignerProjectItemStub("FunctionKeys", "Functions", m_FunctionKeyManager);

            return designerProjectItem;
        }

        private IDesignerProjectItem CreateAlarmServerDesignerProjectItem()
        {
            m_AlarmServer = m_DesignerHost.CreateComponent(typeof(AlarmServer)) as AlarmServer;
            m_AlarmServer.Name = "AlarmServer";

            var alarmGroupZero = new AlarmGroup
            {
                Name = "AlarmGroup0"
            };
            m_AlarmServer.AlarmGroups.Add(alarmGroupZero);

            var alarmGroupOne = new AlarmGroup
            {
                Name = "AlarmGroup1"
            };
            m_AlarmServer.AlarmGroups.Add(alarmGroupOne);

            m_AlarmItemGroupZero = new AlarmItem();
            alarmGroupZero.AlarmItems.Add(m_AlarmItemGroupZero);
            m_AlarmItemGroupZero.DisplayName = "AlarmItem0";

            m_AlarmItemGroupOne = new AlarmItem();
            alarmGroupOne.AlarmItems.Add(m_AlarmItemGroupOne);
            m_AlarmItemGroupOne.DisplayName = "AlarmItem0";

            m_AlarmItemGroupOneSameNameAsGroup = new AlarmItem();
            alarmGroupOne.AlarmItems.Add(m_AlarmItemGroupOneSameNameAsGroup);
            m_AlarmItemGroupOneSameNameAsGroup.DisplayName = alarmGroupOne.Name;

            m_DynamicStringItem = new DynamicStringItem(0, 0);

            m_AlarmItemGroupZero.DynamicString.DynamicItems.Add(m_DynamicStringItem);

            IDesignerProjectItem designerProjectItem = CreateDesignerProjectItemStub("AlarmServer", "Functions", m_AlarmServer);
            return designerProjectItem;
        }

        private IDesignerProjectItem CreateDesignerProjectItemStub(string name, string group, object containedObject)
        {
            var designerProjectItem = MockRepository.GenerateStub<IDesignerProjectItem>();
            designerProjectItem.Name = name;
            designerProjectItem.Group = group;
            designerProjectItem.Stub(x => x.ProjectItems).Return(new IProjectItem[0]);
            designerProjectItem.Stub(x => x.ContainedObject).Return(containedObject);

            return designerProjectItem;
        }

        private void AddTrendViewerToScreen()
        {
            var trendViewer = new ApplicationFramework.Controls.Trend.TrendViewer("TrendViewer");
            m_Curve = new Curve
            {
                DisplayName = "Curve1"
            };
            trendViewer.Curves.Add(m_Curve);

            m_ScreenElements[0] = trendViewer;
        }

        private void AddButtonToScreen()
        {
            var button = new Button
            {
                Name = "Button"
            };

            var mapper = new StringIntervalMapper();
            m_StringInterval = new StringInterval();
            mapper.Intervals.Add(m_StringInterval);
            //order of adding is important
            button.TextIntervalMapper = mapper;
            m_ScreenElements[1] = button;
        }

        #endregion

        [Test]
        public void VerifyFunctionKeyManagerFound()
        {
            GetObjectExpectedToBeFoundTestHelper("FunctionKeys", m_FunctionKeyManager);
        }

        [Test]
        public void VerifyGlobalFunctionKeyFound()
        {
            GetObjectExpectedToBeFoundTestHelper("FunctionKeys.F1", m_GlobalFunctionKey);
        }

        [Test]
        public void VerifyLocalFunctionKeysFound()
        {
            GetObjectExpectedToBeFoundTestHelper("FunctionKeys.Screen1_F1", m_ScreenOneFunctionKey);
            GetObjectExpectedToBeFoundTestHelper("FunctionKeys.Screen2_F1", m_ScreenTwoFunctionKey);
        }

        [Test]
        public void VerifyAlarmServerFound()
        {
            GetObjectExpectedToBeFoundTestHelper("AlarmServer", m_AlarmServer);
        }

        [Test]
        public void VerifyAlarmItemsFound()
        {
            GetObjectExpectedToBeFoundTestHelper("AlarmServer.AlarmGroup0_AlarmItem0", m_AlarmItemGroupZero);
            GetObjectExpectedToBeFoundTestHelper("AlarmServer.AlarmGroup1_AlarmItem0", m_AlarmItemGroupOne);
        }

        [Test]
        public void VerifyAlarmItemWithSameNameAsGroupFound()
        {
            GetObjectExpectedToBeFoundTestHelper("AlarmServer.AlarmGroup1_AlarmGroup1", m_AlarmItemGroupOneSameNameAsGroup);
        }

        [Test]
        public void VerifyDynamicStringInAlarmIsFound()
        {
            GetObjectExpectedToBeFoundTestHelper($"AlarmServer.AlarmGroup0_AlarmItem0.DynamicString.{((IDisplayName)m_DynamicStringItem).DisplayName}", m_DynamicStringItem);
        }

        [Test]
        public void VerifyTrendCurveIsFound()
        {
            GetObjectExpectedToBeFoundTestHelper("Screen.TrendViewer.Curve1", m_Curve);
        }

        [Test]
        public void VerifyStringIntervalIsFound()
        {
            GetObjectExpectedToBeFoundTestHelper("Screen.Button.Intervals[0]", m_StringInterval);
        }

        private void GetObjectExpectedToBeFoundTestHelper(string objectName, object expected)
        {
            var actual = m_PathLookupIndex.GetObject<object>(objectName);
            Assert.IsNotNull(actual, $"{objectName} is null");
            Assert.AreEqual(expected, actual);
        }

    }
}
