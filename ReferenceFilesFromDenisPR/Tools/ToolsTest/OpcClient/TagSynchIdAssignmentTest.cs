#if !VNEXT_TARGET
using System.Linq;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Common.Collections;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.OpcUaServer;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.OpcClient
{
    [TestFixture]
    public class TagSynchIdAssignmentTest
    {
        private ITagSynchIdAssignmentServiceIde m_TagSynchIdAssignmentServiceIde;
        private GlobalController m_Ctrl;

        [SetUp]
        public void Setup()
        {
            NeoDesignerProperties.IsInDesignMode = true;

            TestHelper.ClearServices();
            var projectmanager = Substitute.For<IProjectManager>();
            var opc = Substitute.For<IOpcClientServiceIde>();
            TestHelper.AddService<IOpcClientServiceCF>(opc);
            TestHelper.AddService<IOpcClientServiceIde>(opc);
            opc.Controllers.Returns(new ExtendedBindingList<IDataSourceContainer>());
            m_Ctrl = new GlobalController();
            opc.GlobalController.Returns(m_Ctrl);
            var tagChanged = Substitute.For<ITagChangedNotificationServiceCF>();
            var opcuaserverservice = Substitute.For<IOpcUaServerServiceIde>();
            m_TagSynchIdAssignmentServiceIde = new TagSynchIdAssignmentServiceIde(projectmanager, opc, tagChanged, opcuaserverservice);
        }

        [TearDown]
        public void TearDown()
        {
            m_Ctrl.Dispose();
            NeoDesignerProperties.IsInDesignMode = false;

            TestHelper.ClearServices();
        }

        [Test]
        public void MakeSureNoIdIsFixed()
        {
            Assert.IsTrue(GlobalDataItem.NoIdAssigned == int.MinValue);
        }

        [Test]
        public void MakeSureUintWhichDoesntSupportDefaultValuesIsUsed()
        {
            Assert.IsTrue(new GlobalDataItem().SynchId.GetType() != typeof(uint));
        }

        [Test]
        public void TestAllTagsHaveUniqueIdsWhenOneIsNotAssigned()
        {
            m_Ctrl.GlobalDataItems.Add(new GlobalDataItem() { Name = "a", SynchId = GlobalDataItem.NoIdAssigned});
            m_Ctrl.GlobalDataItems.Add(new GlobalDataItem() { Name = "b", SynchId = 1});
            Assert.IsFalse(m_TagSynchIdAssignmentServiceIde.AllTagsHaveUniqueIds());
        }
        
        [Test]
        public void TestAllTagsHaveUniqueIdsWhenTwoIdsAreTheSame()
        {
            m_Ctrl.GlobalDataItems.Add(new GlobalDataItem() { Name = "a", SynchId = 1 });
            m_Ctrl.GlobalDataItems.Add(new GlobalDataItem() { Name = "b", SynchId = 1 });
            Assert.IsFalse(m_TagSynchIdAssignmentServiceIde.AllTagsHaveUniqueIds());
        }


        [Test]
        public void TestAllTagsHaveUniqueIdsWhenTwoIdsDiffer()
        {
            m_Ctrl.GlobalDataItems.Add(new GlobalDataItem() { Name = "a", SynchId = 1 });
            m_Ctrl.GlobalDataItems.Add(new GlobalDataItem() { Name = "b", SynchId = 2 });
            bool result = m_TagSynchIdAssignmentServiceIde.AllTagsHaveUniqueIds();
            Assert.IsTrue(result);
        }

        [Test]
        public void TestRemoveIdsOnAllTags()
        {
            m_Ctrl.GlobalDataItems.Add(new GlobalDataItem() { Name = "a", SynchId = 1 });
            m_Ctrl.GlobalDataItems.Add(new GlobalDataItem() { Name = "b", SynchId = 2 });
            Assert.IsTrue(m_TagSynchIdAssignmentServiceIde.AllTagsHaveUniqueIds());
            
            
            m_TagSynchIdAssignmentServiceIde.RemoveIdsOnAllTags();

            Assert.IsFalse(m_Ctrl.GlobalDataItems.Any(item => item.SynchId != GlobalDataItem.NoIdAssigned));
        }

        [Test]
        public void TestEnsureUniqueIdsOnAllTagsWhenNeeded()
        {
            m_Ctrl.GlobalDataItems.Add(new GlobalDataItem() { Name = "a", SynchId = GlobalDataItem.NoIdAssigned });
            m_Ctrl.GlobalDataItems.Add(new GlobalDataItem() { Name = "b", SynchId = 2 });
            Assert.IsFalse(m_TagSynchIdAssignmentServiceIde.AllTagsHaveUniqueIds());
            
            
            m_TagSynchIdAssignmentServiceIde.EnsureUniqueIdsOnAllTags();

            Assert.IsTrue(m_TagSynchIdAssignmentServiceIde.AllTagsHaveUniqueIds());
        }


        [Test]
        public void TestEnsureUniqueIdsOnAllTagsWhenNotNeeded()
        {
            m_Ctrl.GlobalDataItems.Add(new GlobalDataItem() { Name = "a", SynchId = 16 });
            m_Ctrl.GlobalDataItems.Add(new GlobalDataItem() { Name = "b", SynchId = 2 });
            Assert.IsTrue(m_TagSynchIdAssignmentServiceIde.AllTagsHaveUniqueIds());


            m_TagSynchIdAssignmentServiceIde.EnsureUniqueIdsOnAllTags();
            
            Assert.IsTrue(m_Ctrl.GlobalDataItems[0].SynchId == 16);
            Assert.IsTrue(m_Ctrl.GlobalDataItems[1].SynchId == 2);
            Assert.IsTrue(m_TagSynchIdAssignmentServiceIde.AllTagsHaveUniqueIds());
        }


        [Test]
        public void TestGeneratedNewUniqueIdWhenElementsExists()
        {
            m_Ctrl.GlobalDataItems.Add(new GlobalDataItem() { Name = "a", SynchId = 16 });
            m_Ctrl.GlobalDataItems.Add(new GlobalDataItem() { Name = "b", SynchId = 2 });
            var id = m_TagSynchIdAssignmentServiceIde.GeneratedNewUniqueId();
            Assert.IsTrue(id > 16) ;
        }

        [Test]
        public void TestGeneratedNewUniqueIdWhenNoElementsExists()
        {
            var id = m_TagSynchIdAssignmentServiceIde.GeneratedNewUniqueId();
            Assert.IsTrue(id != GlobalDataItem.NoIdAssigned); // this should always be valid to check.
            Assert.IsTrue(id == 1); // this could change, but today this is true
        }

    }
}
#endif
