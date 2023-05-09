using System.Windows;
using Neo.ApplicationFramework.Controls.DataGrids;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.TextIdBrowser
{
    [TestFixture]
    public class SelectFirstIfNoSelectedBehaviorTest
    {
        [Test]
        public void LoadedAndNoItemSelected()
        {
            // ARRANGE
            var dataGrid = new DynamicDataGrid();
            dataGrid.Items.Add(new object());
            dataGrid.Items.Add(new object());
            dataGrid.Items.Add(new object());

            var behavior = new SelectFirstIfNoSelectedBehavior();
            behavior.Attach(dataGrid);

            // ACT
            var args = new RoutedEventArgs(FrameworkElement.LoadedEvent, dataGrid);
            dataGrid.RaiseEvent(args);

            // ASSERT
            Assert.That(dataGrid.SelectedItem, Is.EqualTo(dataGrid.Items.GetItemAt(0)));
        }


        [Test]
        public void LoadedAndItemSelected()
        {
            // ARRANGE
            var dataGrid = new DynamicDataGrid();
            dataGrid.Items.Add(new object());
            dataGrid.Items.Add(new object());
            dataGrid.Items.Add(new object());

            // Set selected item
            dataGrid.SelectedItem = dataGrid.Items.GetItemAt(2);

            var behavior = new SelectFirstIfNoSelectedBehavior();
            behavior.Attach(dataGrid);

            // ACT
            var args = new RoutedEventArgs(FrameworkElement.LoadedEvent, dataGrid);
            dataGrid.RaiseEvent(args);
            
            // ASSERT
            Assert.That(dataGrid.SelectedItem, Is.EqualTo(dataGrid.Items.GetItemAt(2)));
        }


        [Test]
        public void LoadedAndNoItems()
        {
            // ARRANGE
            var dataGrid = new DynamicDataGrid();

            var behavior = new SelectFirstIfNoSelectedBehavior();
            behavior.Attach(dataGrid);

            // ACT
            var args = new RoutedEventArgs(FrameworkElement.LoadedEvent, dataGrid);
            dataGrid.RaiseEvent(args);
            
            // ASSERT
            Assert.That(dataGrid.SelectedItem, Is.Null);
        }


        [Test]
        public void CollectionChangedAndNoItemSelected()
        {
            // ARRANGE
            var dataGrid = new DynamicDataGrid();

            var behavior = new SelectFirstIfNoSelectedBehavior();
            behavior.Attach(dataGrid);

            // ACT
            dataGrid.Items.Add(new object());

            // ASSERT
            Assert.That(dataGrid.SelectedItem, Is.EqualTo(dataGrid.Items.GetItemAt(0)));
        }


        [Test]
        public void CollectionChangedAndItemSelected()
        {
            // ARRANGE
            var dataGrid = new DynamicDataGrid();
            dataGrid.Items.Add(new object());
            dataGrid.Items.Add(new object());
            dataGrid.Items.Add(new object());

            // Set selected item
            dataGrid.SelectedItem = dataGrid.Items.GetItemAt(2);

            var behavior = new SelectFirstIfNoSelectedBehavior();
            behavior.Attach(dataGrid);

            // ACT
            dataGrid.Items.Add(new object());

            // ASSERT
            Assert.That(dataGrid.SelectedItem, Is.EqualTo(dataGrid.Items.GetItemAt(2)));
        }


        [Test]
        public void CollectionChangedAndNoItems()
        {
            // ARRANGE
            var dataGrid = new DynamicDataGrid();
            dataGrid.Items.Add(new object());

            // Set selected item
            dataGrid.SelectedItem = dataGrid.Items.GetItemAt(0);

            // ACT
            dataGrid.Items.Remove(dataGrid.Items.GetItemAt(0));

            // ASSERT
            Assert.That(dataGrid.SelectedItem, Is.Null);
        }
    }
}