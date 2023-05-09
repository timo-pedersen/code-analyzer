using System.Collections.Generic;
using System.Linq;
using Neo.ApplicationFramework.Common.Printer.Document;
using Neo.ApplicationFramework.Common.Printer.GdiPrinter.NativeWrappers;
using Neo.ApplicationFramework.Interfaces.Printer;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Common.Printer.GdiPrinter
{
    [TestFixture]
    public class ColumnWidthCalculatorTest
    {
        private IGdiWrapper m_GdiWrapperMock;

        private List<TableColumn> m_TableColumns;

        [SetUp]
        public void Setup()
        {
            m_GdiWrapperMock = MockRepository.GenerateMock<IGdiWrapper>();

            // Use a dpi of 72 since this will translate 1:1 between points (defined as 1/72 of an inch)
            // and pixels.
            m_GdiWrapperMock.Stub(m => m.GetDeviceCaps(GdiConstants.LOGPIXELSY)).Return(72);
        }

        [Test]
        public void Should_distribute_space_equally_between_auto_columns()
        {
            var columns = CreateTableColumns(3);
            columns[0].Width = TableColumn.SizeToAvailable;
            columns[1].Width = TableColumn.SizeToAvailable;
            columns[2].Width = TableColumn.SizeToAvailable;

            IList<int> actualWidths = CalculateWidthsForColumnsAndWidth(columns, 600);

            Assert.That(actualWidths[0], Is.EqualTo(200));
            Assert.That(actualWidths[1], Is.EqualTo(200));
            Assert.That(actualWidths[2], Is.EqualTo(200));
        }

        [Test]
        public void Should_distribute_remaining_space_euqally_between_auto_columns_when_used_with_fixed_witdh()
        {
            var columns = CreateTableColumns(4);
            columns[0].Width = TableColumn.SizeToAvailable;
            columns[1].Width = 100;
            columns[2].Width = TableColumn.SizeToAvailable;
            columns[3].Width = 50;

            IList<int> actualWidths = CalculateWidthsForColumnsAndWidth(columns, 600);

            Assert.That(actualWidths[0], Is.EqualTo(225));
            Assert.That(actualWidths[2], Is.EqualTo(225));
        }

        [Test]
        public void Should_give_overflowing_columns_zero_width()
        {
            var columns = CreateTableColumns(5);
            columns[0].Width = 200;
            columns[1].Width = 200;
            columns[2].Width = 200;
            columns[3].Width = 200;
            columns[4].Width = 200;

            IList<int> actualWidths = CalculateWidthsForColumnsAndWidth(columns, 600);

            Assert.That(actualWidths[3], Is.EqualTo(0));
            Assert.That(actualWidths[4], Is.EqualTo(0));
        }
        
        [Test]
        public void Should_give_last_space_to_cell_that_exceed_the_bounds()
        {
            var columns = CreateTableColumns(2);
            columns[0].Width = 500;
            columns[1].Width = 200;

            IList<int> actualWidths = CalculateWidthsForColumnsAndWidth(columns, 600);

            Assert.That(actualWidths[1], Is.EqualTo(100));
        }

        [Test]
        public void Should_translate_device_independent_column_widths_to_pixels()
        {
            // multiple of 72 (points are defined as 1/72 inches). A dpi of 144 means each points translates
            // to two pixels
            int dpi = 144; 
            m_GdiWrapperMock.Stub(m => m.GetDeviceCaps(GdiConstants.LOGPIXELSY)).Return(dpi).Repeat.Any();
            var columns = CreateTableColumns(2);
            columns[0].Width = 200;

            IList<int> actualWidths = CalculateWidthsForColumnsAndWidth(columns, 600);

            Assert.That(actualWidths[0], Is.EqualTo(400));
        }

        private IList<int> CalculateWidthsForColumnsAndWidth(List<TableColumn> columns, int availableWidth)
        {
            ColumnWidthCalculator columnWidthCalculator = new ColumnWidthCalculator(m_GdiWrapperMock, columns);
            return columnWidthCalculator.GetColumnWidths(availableWidth);
        }

        private List<TableColumn> CreateTableColumns(int numColumns)
        {
            m_TableColumns = new List<TableColumn>();

            foreach(var col in Enumerable.Range(0, numColumns))
            {
                m_TableColumns.Add(new TableColumn());
            }

            return m_TableColumns;
        }
    }
}