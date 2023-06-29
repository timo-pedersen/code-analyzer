#if !VNEXT_TARGET
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Neo.ApplicationFramework.Controls.TreeViews;
using Neo.ApplicationFramework.Tools.OpcUa.ViewModels;

namespace Neo.ApplicationFramework.Tools.OpcUa
{
    [TestFixture]
    public class OpcUaNodeTreeViewItemsViewModelTest
    {
        private const int m_NumberOfNodes = 10;
        private ISimpleTreeViewItemsViewModel m_Root;

        [SetUp]
        public void SetUp()
        {
            m_Root = CreateTreeViewModel();
        }

        [TestCase("", "1", "", "1,2,3,4,5,6")]
        [TestCase("", "7,8,9,10", "", "1,2,3,4,5,6")]
        [TestCase("", "4", "", "1,2,4")]
        [TestCase("", "4,5", "", "1,2,3,4,5")]
        [TestCase("", "5,6", "", "1,3,5,6")]
        [TestCase("", "7", "", "1,2,4")]
        [TestCase("", "9", "", "1,3,5")]
        [TestCase("", "2", "", "1,2,4")]
        [TestCase("1", "", "5", "1,2,3,4,6")]
        [TestCase("1", "", "1", "")]
        [TestCase("4", "", "4", "")]
        [TestCase("1", "", "7,8,9,10", "1,2,3")]
        [TestCase("1", "", "2", "1,3,5,6")]
        [TestCase("4,6", "5", "6", "1,2,3,4,5")]
        [TestCase("4,6", "5", "4,6", "1,3,5")]
        [TestCase("8,9,10", "7", "8,9", "1,2,3,4,6")]
        [TestCase("", "", "", "")]
        public void GivenTreeStructureWithPreselectedNodes_WhenCheckingOrUnCheckingSpecifiedNodes_ThenSetsHasSelectedItemsToTrueForAppropriateNodes(string nodesAlreadySelected, string nodesToBeChecked, string nodesToBeUnchecked, string nodesThatShouldHaveHasSelectedItems)
        {
            //ARRANGE            
            int[] selectedNodes = nodesAlreadySelected.Split(',').Where(x => !string.IsNullOrEmpty(x)).Select(int.Parse).ToArray();
            int[] nodesToCheck = nodesToBeChecked.Split(',').Where(x => !string.IsNullOrEmpty(x)).Select(int.Parse).ToArray();
            int[] nodesToUnCheck = nodesToBeUnchecked.Split(',').Where(x => !string.IsNullOrEmpty(x)).Select(int.Parse).ToArray();
            int[] nodesWithHasSelectedItems = nodesThatShouldHaveHasSelectedItems.Split(',').Where(x => !string.IsNullOrEmpty(x)).Select(int.Parse).ToArray();

            //ACT
            selectedNodes.Each(x => UpdateCheckNode(x, true));
            nodesToCheck.Each(x => UpdateCheckNode(x, true));
            nodesToUnCheck.Each(x => UpdateCheckNode(x, false));

            //ASSERT
            for (int i = 1; i <= m_NumberOfNodes; i++)
            {
                Assert.AreEqual(nodesWithHasSelectedItems.Contains(i), GetNode(i, m_Root).HasSelectedItems);
            }
        }

        private ISimpleTreeViewItemsViewModel CreateTreeViewModel()
        {
            /*
             7   8     9    10
              \   \   /    /
               4    5     6 
                \    \   /
                 2     3
                  \   /
                    1
            */
            ISimpleTreeViewItemsViewModel treeViewModel = new OpcUaNodeTreeViewItemsViewModel(CreateNode(1, CreateChildren()));
            return treeViewModel;
        }

        private List<LazyBrowseOpcUaNode> CreateChildren()
        {
            var node10 = CreateNode(10);
            var node9 = CreateNode(9);
            var node8 = CreateNode(8);
            var node7 = CreateNode(7);
            var node6 = CreateNode(6, new List<LazyBrowseOpcUaNode>()
            {
                node10
            });
            var node5 = CreateNode(5, new List<LazyBrowseOpcUaNode>()
            {
                node8,
                node9
            });
            var node4 = CreateNode(4, new List<LazyBrowseOpcUaNode>()
            {
                node7
            });
            var node3 = CreateNode(3, new List<LazyBrowseOpcUaNode>()
            {
                node5,
                node6
            });
            var node2 = CreateNode(2, new List<LazyBrowseOpcUaNode>()
            {
                node4
            });
            return new List<LazyBrowseOpcUaNode>()
            {
                node2,
                node3
            };
        }

        private LazyBrowseOpcUaNode CreateNode(object identifier, List<LazyBrowseOpcUaNode> children = null)
        {
            return new LazyBrowseOpcUaNode(() => children ?? new List<LazyBrowseOpcUaNode>(), new OpcUaNodeBrowseInfo(identifier, identifier.ToString(), 1, "Numeric", identifier.ToString()));
        }

        private ISimpleTreeViewItemsViewModel GetNode(int identifier, ISimpleTreeViewItemsViewModel node)
        {
            if (node.Name.Equals(identifier.ToString()))
                return node;

            foreach (var child in node.Children)
            {
                var foundNode = GetNode(identifier, child);
                if (foundNode != null)
                    return foundNode;
            }

            return null;
        }

        private void UpdateCheckNode(int identifier, bool isChecked)
        {
            var existingNode = GetNode(identifier, m_Root);
            if (existingNode != null)
                existingNode.IsChecked = isChecked;
        }
    }
}
#endif
