using System;
using Neo.ApplicationFramework.Common.Runtime.Screen;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.Screen
{
    [TestFixture]
    class ScreenIndexerNamespaceTest
    {
        [Test]
        public void HasSameNameSpace()
        {
            Assert.IsTrue(string.Equals(typeof(ScreenIndexer).Namespace, typeof (ScreenIndexer<ScreenWindow>).Namespace,StringComparison.CurrentCulture));
        }

        [Test]
        public void HasSameName()
        {
            Assert.IsTrue(typeof(ScreenIndexer<ScreenWindow>).Name.StartsWith(typeof(ScreenIndexer).Name, StringComparison.CurrentCulture) );
        }
    }
}
