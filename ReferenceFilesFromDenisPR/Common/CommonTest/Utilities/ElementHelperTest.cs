#if !VNEXT_TARGET
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Controls;
using System.Windows.Forms;
using Core.Api.DataSource;
using Core.Api.GlobalReference;
using Core.Component.Api.Instantiation;
using Core.Component.Engine.Design;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Common.Dynamics;
using Neo.ApplicationFramework.Controls;
using Neo.ApplicationFramework.Controls.FunctionKey;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Threading;
using Neo.ApplicationFramework.Tools.Alarm;
using Neo.ApplicationFramework.Tools.FunctionKey;
using Neo.ApplicationFramework.Tools.Screen.ScreenEditor;
using NSubstitute;
using NUnit.Framework;
using Button = Neo.ApplicationFramework.Controls.Button;

namespace Neo.ApplicationFramework.Common.Utilities
{
    [TestFixture]
    public class ElementHelperTest
    {
        private IServiceProvider m_ServiceProvider;

        private const string FunctionKeyManager = "FunctionKeys";
        
        private const string ScreenName = "Screen1";
        private const string ButtonName = "Button1";

        [SetUp]
        public void TestFixtureSetUp()
        {
            ISite siteStub = Substitute.For<ISite>();
            siteStub.Name = ScreenName;
            IComponent rootComponentStub = Substitute.For<IComponent>();
            rootComponentStub.Site = siteStub;
            var designerHostStub = Substitute.For<INeoDesignerHost>();
            designerHostStub.RootComponent.Returns(rootComponentStub);
            
            m_ServiceProvider = Substitute.For<IServiceProvider>();
            m_ServiceProvider.GetService(typeof(IDesignerHost)).Returns(designerHostStub);
            m_ServiceProvider.GetService(typeof(INeoDesignerHost)).Returns(designerHostStub);

            var securityService = Substitute.For<ISecurityServiceCF>();
            securityService.GetSecurityGroups(null).ReturnsForAnyArgs(SecurityGroups.None);
            TestHelper.AddService(securityService);
        }

        [Test]
        public void GetFullNameForTrendCurve()
        {
            const string curveName = "Curve1";
            const string trendName = "Trend1";

            ISubItems trend = Substitute.For<ISubItems>();
            ISubItems curve = Substitute.For<ISubItems>();

            trend.Items.Returns(new object[] { curve });
            trend.Parent.Returns(m_ServiceProvider);
            trend.DisplayName.Returns(trendName);

            curve.Items.Returns(new object[0]);
            curve.Parent.Returns(trend);

            curve.DisplayName.Returns(curveName);

            string fullName = ElementHelper.GetFullName(curve);

            Assert.That(fullName, Is.EqualTo($"{ScreenName}.{trendName}.{curveName}"));
        }

        [Test]
        public void GetFullNameForAnalogNumericInScreen()
        {
            Canvas canvas = new Canvas {Name = ScreenName};
            AnalogNumericFX analogNumericFx = new AnalogNumericFX {Name = "AnalogNumeric1"};
            canvas.Children.Add(analogNumericFx);

            string fullName = ElementHelper.GetFullName(analogNumericFx);
            Assert.That(fullName,Is.EqualTo($"{ScreenName}.AnalogNumeric1"));
        }

        [Test]
        public void GetFullNameForGroupedElementInScreen()
        {
            Canvas canvas = new Canvas {Name = ScreenName};
            Group group = new Group {Name = "Group1"};
            AnalogNumericFX analogNumericFx = new AnalogNumericFX {Name = "AnalogNumeric1"};

            group.Items.Add(analogNumericFx);
            canvas.Children.Add(group);

            string fullName = ElementHelper.GetFullName(analogNumericFx);
            Assert.That(fullName, Is.EqualTo($"{ScreenName}.Group1.AnalogNumeric1"));
        }

        [Test]
        public void GetFullNameForTrippledGroupedElementInScreen()
        {
            AnalogNumericFX analogNumericFx = GetTrippledGroupedAnalogNumericFx();

            string fullName = ElementHelper.GetFullName(analogNumericFx);
            Assert.That(fullName, Is.EqualTo($"{ScreenName}.Group3.Group2.Group1.AnalogNumeric1"));
        }

        [Test]
        public void GetFullNameForAlarmItem()
        {
            AlarmItem alarmItem = GetAlarmItem();
            string fullName = ElementHelper.GetFullName(alarmItem);
            Assert.That(fullName, Is.EqualTo("AlarmServer.Default_AlarmItem0"));
        }

        [Test]
        public void GetFullNameForAlarmItemDynamicString()
        {
            IDynamicStringItem dynamicStringItem = GetAlarmItemDynamicString();
            string fullName = ElementHelper.GetFullName(dynamicStringItem);
            Assert.That(fullName, Is.EqualTo("AlarmServer.Default_AlarmItem0.DynamicString.DynamicItem0"));
        }

        [Test]
        public void GetFullNameForButtonInScreen()
        {
            Button button = GetButton();
            string fullName = ElementHelper.GetFullName(button);
            Assert.That(fullName, Is.EqualTo($"{ScreenName}.{ButtonName}"));
        }

        [Test]
        public void GetFullNameForStringIntervalButton()
        {
            StringInterval stringInterval = GetStringInterval();
            string fullName = ElementHelper.GetFullName(stringInterval);
            Assert.That(fullName, Is.EqualTo($"{ScreenName}.{ButtonName}.Intervals[0]"));
        }

        [Test]
        public void GetFullNameForGlobalFunctionKey()
        {
            IFunctionKey functionKey = GetFunctionKey(Keys.F10);
            
            string fullName = ElementHelper.GetFullName(functionKey);
            Assert.That(fullName, Is.EqualTo($"FunctionKeys.{nameof(Keys.F10)}"));
        }

        [Test]
        public void GetFullNameForScreenFunctionKey()
        {
            IFunctionKey functionKey = GetFunctionKey(Keys.F13, ScreenName);

            string fullName = ElementHelper.GetFullName(functionKey);
            Assert.That(fullName, Is.EqualTo($"FunctionKeys.{ScreenName}_{nameof(Keys.F13)}"));
        }

        private AnalogNumericFX GetTrippledGroupedAnalogNumericFx()
        {
            Canvas canvas = new Canvas { Name = ScreenName };
            Group group = new Group { Name = "Group1" };
            Group secondGroup = new Group { Name = "Group2" };
            Group thirdGroup = new Group { Name = "Group3" };
            AnalogNumericFX analogNumericFx = new AnalogNumericFX { Name = "AnalogNumeric1" };

            group.Items.Add(analogNumericFx);
            secondGroup.Items.Add(group);
            thirdGroup.Items.Add(secondGroup);
            canvas.Children.Add(thirdGroup);
            return analogNumericFx;
        }

        private IDynamicStringItem GetAlarmItemDynamicString()
        {
            AlarmItem alarmItem = GetAlarmItem();

            alarmItem.DynamicString = new DynamicString(alarmItem);

            var dynamicString = new DynamicStringItem
            {
                DynamicIndex = 0,
                Value = new VariantValue("0", DataQuality.Good),
                DataConnection = "Tags.Tag1"
            };

            alarmItem.DynamicString.DynamicItems.Add(dynamicString);

            return dynamicString;
        }

        private AlarmItem GetAlarmItem()
        {
            NeoDesignerProperties.IsInDesignMode = true;

            TestHelper.AddServiceStub<IGlobalReferenceService>();

            var alarmServerStorage = Substitute.For<IAlarmServerStorage>();
            var alarmServer = new AlarmServer(alarmServerStorage, 
                Substitute.For<IActionConsumer>().ToLazyCF(), 
                Substitute.For<IRootComponentService>().ToILazy(),
                Substitute.For<ISystemTagServiceCF>().ToILazy(),
                Substitute.For<IAlarmEventFactory>().ToILazy())
            {
                Name = "AlarmServer",
                IsEnabled = true
            };

            IAlarmGroup alarmServerAlarmGroup = alarmServer.AlarmGroups.AddNew();
            if (alarmServerAlarmGroup == null)
            {
                return null;
            }

            alarmServerAlarmGroup.Name = "Default";
            var site = Substitute.For<ISite>();
            site.Name = alarmServerAlarmGroup.Name;
            alarmServerAlarmGroup.Site = site;

            var alarmItem = new AlarmItem
            {
                Name = "AlarmItem0",
                DisplayName = "AlarmItem0",
                Value = 0
            };

            var alarmItemSite = Substitute.For<ISite>();
            alarmItemSite.Name = alarmItem.Name;
            alarmItem.Site = alarmItemSite;

            alarmServerAlarmGroup.AlarmItems.Add(alarmItem);
            ((IAlarmItem)alarmItem).EnableValueInput = true;
            return alarmItem;
        }

        private Button GetButton()
        {
            //add to container - GetFullName should skip it
            var canvas = new NeoElementCanvas
            {
                Name = "Canvas"
            };

            var editorControl = Substitute.For<EditorControl>();
            editorControl.Name = "Editor";
            editorControl.Content = canvas;
            canvas.ServiceProvider = m_ServiceProvider;

            var button = new Button {Name = ButtonName};
            canvas.Children.Add(button);

            return button;
        }

        private StringInterval GetStringInterval()
        {
            Button button = GetButton();
            var mapper = new StringIntervalMapper();
            var interval = new StringInterval();
            mapper.Intervals.Add(interval);
            //order of adding is important
            button.TextIntervalMapper = mapper;
            return interval;
        }

        private IFunctionKey GetFunctionKey(Keys keyCode, string screenName = "")
        {
            var functionKeyManager = new FunctionKeyManager
            {
                Name = FunctionKeyManager
            };

            var functionKey = new FunctionKey(keyCode);

            var site = Substitute.For<ISite>();
            site.Name = functionKey.Name;
            functionKey.Site = site;

            if (string.IsNullOrEmpty(screenName))
            {
                functionKeyManager.GlobalFunctionKeys.Add(functionKey);
            }
            else
            {
                var functionKeyContextScreen = new FunctionKeyContext(functionKeyManager)
                {
                    ScreenName = ScreenName,
                    Name = ScreenName
                };

                var screenSite = Substitute.For<ISite>();
                screenSite.Name = functionKeyContextScreen.Name;
                functionKeyContextScreen.Site = screenSite;

                functionKeyContextScreen.FunctionKeys.Add(functionKey);
                functionKeyManager.LocalFunctionKeys.Add(functionKeyContextScreen);
            }

            return functionKey;
        }
    }
}
#endif
