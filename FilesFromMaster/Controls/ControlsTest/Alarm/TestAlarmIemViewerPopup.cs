using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Core.Api.Utilities;
using Neo.ApplicationFramework.Common.Graphics.Logic;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Resco.Controls.SmartGrid;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Controls.Alarm
{
    [TestFixture]
    public class TestAlarmIemViewerPopup
    {

        private readonly Func<string, int> m_MeasureItemsFunc = text => text.Length;


        [Test]
        public void NoExceptionsDuringCreation()
        {
            using (AlarmItemViewerPopup testItem = SetupAlarmItemPopup())
            {
                testItem.Width = testItem.Width;
            }
        }

        private AlarmItemViewerPopup SetupAlarmItemPopup()
        {
            AlarmViewer alarmViewer = SetupAlarmViewer();
            var auditTrailMock = MockRepository.GenerateStub<ILazy<IAuditTrailService>>();
            var alarmServer = MockRepository.GenerateStub<IAlarmServer>();
            var alarmItemRepository = MockRepository.GenerateStub<IAlarmItemRepository>();
            alarmServer.Stub(x => x.AlarmItemRepository).Return(alarmItemRepository);
            var viewModel = new AlarmItemsViewModel(alarmViewer, null, new List<AlarmItemWrapper>
            {
                TestAlarmsItemViewModel.SetupStubItem(false, "Text #1", "Alarm1", "Group1"),
                TestAlarmsItemViewModel.SetupStubItem(true, "HaHa", "Alarm2", "Group1a"),
                TestAlarmsItemViewModel.SetupStubItem(true, "Woe is me", "Alarm1a", "Group2"),
            }, alarmServer);

            var nativeApi = MockRepository.GenerateMock<INativeAPI>();
            nativeApi.Stub(api => api.GetWindowLong(Arg<IntPtr>.Is.Anything, Arg<GWL_STYLE>.Is.Anything)).Return((IntPtr)WS_STYLE.WS_VISIBLE);
            return new AlarmItemViewerPopup(viewModel, auditTrailMock, nativeApi.ToILazy(), true);
        }

        private AlarmViewer SetupAlarmViewer()
        {
            AlarmViewer alarmViewer = MockRepository.GenerateStub<AlarmViewer>();
            alarmViewer.Background = new BrushCF(Color.AliceBlue);
            alarmViewer.Foreground = new BrushCF(Color.BlanchedAlmond);
            alarmViewer.Width = 111;
            alarmViewer.Height = 222;
            alarmViewer.ForeColor = Color.Aqua;
            alarmViewer.BackColor = Color.Red;
            alarmViewer.Stub(av => av.PointToScreen(Arg<Point>.Is.Anything)).Return(new Point(8, 8));
            var smartGrid = MockRepository.GenerateStub<SmartGrid>();
            alarmViewer.Stub(av => av.AlarmGrid).Return(smartGrid);
            alarmViewer.Font = new System.Drawing.Font("Arial", 25.0f);
            var buttonControl = MockRepository.GenerateStub<ButtonControl>();
            buttonControl.Dock = DockStyle.Left;
            buttonControl.Font = alarmViewer.Font;
            alarmViewer.Stub(av => av.ButtonControl).Return(buttonControl);
            alarmViewer.ButtonHeight = 20;
            alarmViewer.ButtonWidth = 100;
            alarmViewer.ButtonMargin = 5;
            alarmViewer.ButtonBackground = new BrushCF(Color.Azure);
            return alarmViewer;
        }

        [Test]
        public void CalculateGeneralColumnWidth()
        {
            Func<string, string> extractorFunc = text => text;
            string[] columnItems = new[] { "One", "Two", "Three", "Four" };

            int longHeaderWidth = AlarmItemViewerPopup.CalculateGeneralColumnWidth(m_MeasureItemsFunc, "Long Header", columnItems, extractorFunc);
            int shortHeaderWidth = AlarmItemViewerPopup.CalculateGeneralColumnWidth(m_MeasureItemsFunc, "", columnItems, extractorFunc);

            Assert.IsTrue(longHeaderWidth == 11);
            Assert.IsTrue(shortHeaderWidth == 5);
        }

        [Test]
        public void CalculateDateColumnWidth()
        {
            Func<DateTime, string> dateToStringConverter = date => "HelloWorld";
            int longHeaderWidth = AlarmItemViewerPopup.CalculateDateColumnWidth(m_MeasureItemsFunc, "Long Header", dateToStringConverter);
            int shortHeaderWidth = AlarmItemViewerPopup.CalculateDateColumnWidth(m_MeasureItemsFunc, "", dateToStringConverter);

            Assert.IsTrue(longHeaderWidth == 11);
            Assert.IsTrue(shortHeaderWidth == 10);
        }
    }
}
