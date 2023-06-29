using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Core.Api.Utilities;
using Neo.ApplicationFramework.Common.Graphics.Logic;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;
using Resco.Controls.SmartGrid;

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
            var auditTrailMock = Substitute.For<ILazy<IAuditTrailService>>();
            var alarmServer = Substitute.For<IAlarmServer>();
            var alarmItemRepository = Substitute.For<IAlarmItemRepository>();
            alarmServer.AlarmItemRepository.Returns(alarmItemRepository);
            var viewModel = new AlarmItemsViewModel(alarmViewer, null, new List<AlarmItemWrapper>
            {
                TestAlarmsItemViewModel.SetupStubItem(false, "Text #1", "Alarm1", "Group1"),
                TestAlarmsItemViewModel.SetupStubItem(true, "HaHa", "Alarm2", "Group1a"),
                TestAlarmsItemViewModel.SetupStubItem(true, "Woe is me", "Alarm1a", "Group2"),
            }, alarmServer);

            var nativeApi = Substitute.For<INativeAPI>();
            nativeApi.GetWindowLong(Arg.Any<IntPtr>(), Arg.Any<GWL_STYLE>()).Returns((IntPtr)WS_STYLE.WS_VISIBLE);
            return new AlarmItemViewerPopup(viewModel, auditTrailMock, nativeApi.ToILazy(), true);
        }

        private AlarmViewer SetupAlarmViewer()
        {
            AlarmViewer alarmViewer = Substitute.For<AlarmViewer>();
            alarmViewer.Background = new BrushCF(Color.AliceBlue);
            alarmViewer.Foreground = new BrushCF(Color.BlanchedAlmond);
            alarmViewer.Width = 111;
            alarmViewer.Height = 222;
            alarmViewer.ForeColor = Color.Aqua;
            alarmViewer.BackColor = Color.Red;
            alarmViewer.PointToScreen(Arg.Any<Point>()).Returns(new Point(8, 8));
            var smartGrid = Substitute.For<SmartGrid>();
            alarmViewer.AlarmGrid.Returns(smartGrid);
            alarmViewer.Font = new System.Drawing.Font("Arial", 25.0f);
            var buttonControl = Substitute.For<ButtonControl>();
            buttonControl.Dock = DockStyle.Left;
            buttonControl.Font = alarmViewer.Font;
            alarmViewer.ButtonControl.Returns(buttonControl);
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
