using System.Collections.Generic;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.ProjectManager.LoadOnDemand
{
    [TestFixture]
    public class CombinationUnloadStrategyTest
    {
        private readonly MockRepository m_Mocks = new MockRepository();
        
        [Test]
        public void ShouldUnloadReturnsTrueIfStrategy1Does()
        {
            IUnloadStrategy unloadStrategy1 = m_Mocks.StrictMock<IUnloadStrategy>();
            IUnloadStrategy unloadStrategy2 = m_Mocks.StrictMock<IUnloadStrategy>();

            CombinationUnloadStrategy combUnload = new CombinationUnloadStrategy(unloadStrategy1, unloadStrategy2);

            using (m_Mocks.Record())
            {
                Expect.Call(unloadStrategy1.ShouldUnload()).Repeat.Once().Return(true);
                Expect.Call(unloadStrategy2.ShouldUnload()).Repeat.Any().Return(false);
            }

            using (m_Mocks.Playback())
            {
                bool shouldUnload = combUnload.ShouldUnload();
                Assert.IsTrue(shouldUnload);
            }
            
        }

        [Test]
        public void ShouldUnloadReturnsTrueIfStrategy2Does()
        {
            IUnloadStrategy unloadStrategy1 = m_Mocks.StrictMock<IUnloadStrategy>();
            IUnloadStrategy unloadStrategy2 = m_Mocks.StrictMock<IUnloadStrategy>();

            CombinationUnloadStrategy combUnload = new CombinationUnloadStrategy(unloadStrategy1, unloadStrategy2);

            using (m_Mocks.Record())
            {
                Expect.Call(unloadStrategy1.ShouldUnload()).Repeat.Any().Return(false);
                Expect.Call(unloadStrategy2.ShouldUnload()).Repeat.Once().Return(true);
            }

            using (m_Mocks.Playback())
            {
                bool shouldUnload = combUnload.ShouldUnload();
                Assert.IsTrue(shouldUnload);
            }

        }

        [Test]
        public void ShouldUnloadReturnsTrueIfBothStrategiesDo()
        {
            IUnloadStrategy unloadStrategy1 = m_Mocks.StrictMock<IUnloadStrategy>();
            IUnloadStrategy unloadStrategy2 = m_Mocks.StrictMock<IUnloadStrategy>();

            CombinationUnloadStrategy combUnload = new CombinationUnloadStrategy(unloadStrategy1, unloadStrategy2);

            using (m_Mocks.Record())
            {
                Expect.Call(unloadStrategy1.ShouldUnload()).Repeat.Any().Return(true);
                Expect.Call(unloadStrategy2.ShouldUnload()).Repeat.Any().Return(true);
            }

            using (m_Mocks.Playback())
            {
                bool shouldUnload = combUnload.ShouldUnload();
                Assert.IsTrue(shouldUnload);
            }

        }

        [Test]
        public void MinimumLoadedDesignersReturnsFromStrategy1()
        {
            IUnloadStrategy unloadStrategy1 = m_Mocks.StrictMock<IUnloadStrategy>();
            IUnloadStrategy unloadStrategy2 = m_Mocks.StrictMock<IUnloadStrategy>();
            int minimumLoaded = 4123; //Bogus value

            CombinationUnloadStrategy combUnload = new CombinationUnloadStrategy(unloadStrategy1, unloadStrategy2);

            using (m_Mocks.Record())
            {
                Expect.Call(unloadStrategy1.MinimumLoadedDesigners).Repeat.Once().Return(minimumLoaded);
                Expect.Call(unloadStrategy2.MinimumLoadedDesigners).Repeat.Never();
            }

            using (m_Mocks.Playback())
            {
                int combMinLoaded = combUnload.MinimumLoadedDesigners;
                Assert.AreEqual(minimumLoaded, combMinLoaded);
            }

        }

        [Test]
        public void LoadedDesignersListReturnsFromStrategy1()
        {
            IUnloadStrategy unloadStrategy1 = m_Mocks.StrictMock<IUnloadStrategy>();
            IUnloadStrategy unloadStrategy2 = m_Mocks.StrictMock<IUnloadStrategy>();
            
            List<IDesignerProjectItem> bogusList = new List<IDesignerProjectItem>();

            CombinationUnloadStrategy combUnload = new CombinationUnloadStrategy(unloadStrategy1, unloadStrategy2);

            using (m_Mocks.Record())
            {
                Expect.Call(unloadStrategy1.LoadedDesignersList).Repeat.Once().Return(bogusList);
                Expect.Call(unloadStrategy2.LoadedDesignersList).Repeat.Never();
            }

            using (m_Mocks.Playback())
            {
                IList<IDesignerProjectItem> list = combUnload.LoadedDesignersList;
                Assert.AreSame(bogusList, list);
            }
        }

        [Test]
        public void LoadedDesignersListSetsToBothSubStrategies()
        {
            IUnloadStrategy unloadStrategy1 = m_Mocks.StrictMock<IUnloadStrategy>();
            IUnloadStrategy unloadStrategy2 = m_Mocks.StrictMock<IUnloadStrategy>();

            List<IDesignerProjectItem> bogusList = new List<IDesignerProjectItem>();

            CombinationUnloadStrategy combUnload = new CombinationUnloadStrategy(unloadStrategy1, unloadStrategy2);

            using (m_Mocks.Record())
            {
                unloadStrategy1.LoadedDesignersList = bogusList;
                unloadStrategy2.LoadedDesignersList = bogusList;
            }

            using (m_Mocks.Playback())
            {
                combUnload.LoadedDesignersList = bogusList;
            }
        }
    }
}
