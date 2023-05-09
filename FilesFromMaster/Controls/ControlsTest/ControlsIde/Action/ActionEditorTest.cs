using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.Action
{
    [TestFixture]
    public class ActionEditorTest
    {
        private TestActionEditor m_TestActionEditor;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            m_TestActionEditor = new TestActionEditor();
        }

        [Test]
        public void ActionName()
        {
            string actionName = "ActionName";
            m_TestActionEditor.CallActionName = actionName;
            Assert.AreEqual(actionName, m_TestActionEditor.CallActionName, "Failed to set ActionName.");
        }

        [Test]
        public void Control()
        {
            Assert.IsNull(m_TestActionEditor.CallControl, "This should be null");
        }
    }

}
