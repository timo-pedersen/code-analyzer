using System.Collections.Generic;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.ProjectManager.LoadOnDemand
{
    [TestFixture]
    public class CombinationUnloadStrategyTest
    {
        [Test]
        public void ShouldUnloadReturnsTrueIfStrategy1Does()
        {
            IUnloadStrategy unloadStrategy1 = Substitute.For<IUnloadStrategy>();
            IUnloadStrategy unloadStrategy2 = Substitute.For<IUnloadStrategy>();

            CombinationUnloadStrategy combUnload = new CombinationUnloadStrategy(unloadStrategy1, unloadStrategy2);

            unloadStrategy1.ShouldUnload().Returns(true);
            unloadStrategy2.ShouldUnload().Returns(false);

            bool shouldUnload = combUnload.ShouldUnload();
            Assert.IsTrue(shouldUnload);

            unloadStrategy1.Received(1).ShouldUnload();
            unloadStrategy2.Received().ShouldUnload();
        }

        [Test]
        public void ShouldUnloadReturnsTrueIfStrategy2Does()
        {
            IUnloadStrategy unloadStrategy1 = Substitute.For<IUnloadStrategy>();
            IUnloadStrategy unloadStrategy2 = Substitute.For<IUnloadStrategy>();

            CombinationUnloadStrategy combUnload = new CombinationUnloadStrategy(unloadStrategy1, unloadStrategy2);

            unloadStrategy1.ShouldUnload().Returns(false);
            unloadStrategy2.ShouldUnload().Returns(true);
            
            bool shouldUnload = combUnload.ShouldUnload();
            Assert.IsTrue(shouldUnload);

            unloadStrategy1.Received().ShouldUnload();
            unloadStrategy2.Received(1).ShouldUnload();
        }

        [Test]
        public void ShouldUnloadReturnsTrueIfBothStrategiesDo()
        {
            IUnloadStrategy unloadStrategy1 = Substitute.For<IUnloadStrategy>();
            IUnloadStrategy unloadStrategy2 = Substitute.For<IUnloadStrategy>();

            CombinationUnloadStrategy combUnload = new CombinationUnloadStrategy(unloadStrategy1, unloadStrategy2);

            unloadStrategy1.ShouldUnload().Returns(true);
            unloadStrategy2.ShouldUnload().Returns(true);

            bool shouldUnload = combUnload.ShouldUnload();
            Assert.IsTrue(shouldUnload);

            unloadStrategy1.Received().ShouldUnload();
            unloadStrategy2.Received().ShouldUnload();
        }

        [Test]
        public void MinimumLoadedDesignersReturnsFromStrategy1()
        {
            IUnloadStrategy unloadStrategy1 = Substitute.For<IUnloadStrategy>();
            IUnloadStrategy unloadStrategy2 = Substitute.For<IUnloadStrategy>();
            int minimumLoaded = 4123; //Bogus value

            CombinationUnloadStrategy combUnload = new CombinationUnloadStrategy(unloadStrategy1, unloadStrategy2);

            unloadStrategy1.MinimumLoadedDesigners.Returns(minimumLoaded);

            int combMinLoaded = combUnload.MinimumLoadedDesigners;
            Assert.AreEqual(minimumLoaded, combMinLoaded);
        }

        [Test]
        public void LoadedDesignersListReturnsFromStrategy1()
        {
            IUnloadStrategy unloadStrategy1 = Substitute.For<IUnloadStrategy>();
            IUnloadStrategy unloadStrategy2 = Substitute.For<IUnloadStrategy>();
            
            List<IDesignerProjectItem> bogusList = new List<IDesignerProjectItem>();

            CombinationUnloadStrategy combUnload = new CombinationUnloadStrategy(unloadStrategy1, unloadStrategy2);

            unloadStrategy1.LoadedDesignersList.Returns(bogusList);

            IList<IDesignerProjectItem> list = combUnload.LoadedDesignersList;
            Assert.AreSame(bogusList, list);
        }

        [Test]
        public void LoadedDesignersListSetsToBothSubStrategies()
        {
            IUnloadStrategy unloadStrategy1 = Substitute.For<IUnloadStrategy>();
            IUnloadStrategy unloadStrategy2 = Substitute.For<IUnloadStrategy>();

            List<IDesignerProjectItem> bogusList = new List<IDesignerProjectItem>();

            CombinationUnloadStrategy combUnload = new CombinationUnloadStrategy(unloadStrategy1, unloadStrategy2);

            unloadStrategy1.LoadedDesignersList = bogusList;
            unloadStrategy2.LoadedDesignersList = bogusList;
            
            combUnload.LoadedDesignersList = bogusList;
        }
    }
}
