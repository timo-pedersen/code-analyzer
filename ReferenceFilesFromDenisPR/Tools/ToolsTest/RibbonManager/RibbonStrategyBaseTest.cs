using System;
using System.Collections.Generic;
using DevComponents.DotNetBar;
using Microsoft.Windows.Controls.Ribbon;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Tools.Ribbon;
using Neo.ApplicationFramework.Tools.Ribbon.Strategies;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.RibbonManager
{
    public class RibbonStrategyBaseTest
    {
        protected IRibbonContextManager m_RibbonContextManager;
        protected TestRibbonContextProviderService m_TestRibbonContextProviderService;
        protected RibbonBarMergeContainer m_RibbonBarMergeContainer;
        protected static List<IRibbonContextContainer> m_RibbonContextContainerList;
        protected static string m_DefaultRibbonContext;

        protected object m_PrimarySelection;
        protected List<Object> m_Selection;


        private IRibbonStrategy m_RibbonStrategy;

        protected IRibbonStrategy RibbonStrategy
        {
            get { return m_RibbonStrategy; }
            set { m_RibbonStrategy = value; }
        }

        protected void CommonSetup()
        {
            m_RibbonContextManager = Substitute.For<IRibbonContextManager>();
            m_TestRibbonContextProviderService = new TestRibbonContextProviderService();
            m_RibbonContextManager.RibbonContextProviderService.Returns(m_TestRibbonContextProviderService);

            m_PrimarySelection = new object();
            m_Selection = new List<Object>();
            m_Selection.Add(m_PrimarySelection);
        }

        protected virtual void SetupExpectationsForRibbonSelection(string ribbonToReturnFromGet, string ribbonToMerge, string ribbonToSelect)
        {
            m_RibbonContextManager.SelectedRibbonTab.Returns(new RibbonTab(){Name=ribbonToReturnFromGet});
            TestRibbonContextContainer ribbonContextContainer = new TestRibbonContextContainer(ribbonToMerge);

            m_RibbonContextContainerList = new List<IRibbonContextContainer>();
            m_RibbonContextContainerList.Add(ribbonContextContainer);

            m_DefaultRibbonContext = ribbonToMerge;
        }

        protected virtual void SetSelectionAndUpdateRibbon()
        {
            RibbonStrategyBaseTest.ListMatcher listMatcher = new ListMatcher();
            IList<IRibbonContextContainer> contextContainers = RibbonStrategy.Update(m_Selection, m_PrimarySelection);
            Assert.That(listMatcher.Matches(contextContainers), NUnit.Framework.Is.True);
        }

        #region Help classes

        public class TestRibbonContextContainer : IRibbonContextContainer
        {
            private readonly string m_TabTitle;

            public TestRibbonContextContainer(string tabName)
            {
                m_TabTitle = string.Empty;
                TabName = tabName;
            }

            #region IRibbonContextContainer Members


            public void UpdateContent()
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public string TabName { get; set; }

            public string ContextualTabGroupName { get; set; }

            public string TabTitle
            {
                get { return m_TabTitle; }
            }

            public string ContextualTabGroupTitle
            {
                get { return "Properties"; }
            }

            public string KeyTip
            {
                get { return string.Empty; }
            }

            #endregion
        }

        public class ListMatcher
        {
            private IRibbonContextContainer m_MissingContainer;

            public void DescribeTo(System.IO.TextWriter writer)
            {
                writer.WriteLine("List did not contain expected element: " + m_MissingContainer);
            }

            public bool Matches(object o)
            {
                ICollection<IRibbonContextContainer> ribbonContextContainers = o as ICollection<IRibbonContextContainer>;
                if (ribbonContextContainers != null)
                {
                    if (ribbonContextContainers.Count != m_RibbonContextContainerList.Count)
                        return false;

                    foreach (IRibbonContextContainer ribbonContextContainer in m_RibbonContextContainerList)
                    {
                        if (!ribbonContextContainers.Contains(ribbonContextContainer))
                        {
                            m_MissingContainer = ribbonContextContainer;
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }
        }



        public class TestRibbonContextProviderService : IRibbonContextProviderService
        {
            #region IRibbonContextProviderService Members

            public void AddProvider(IRibbonContextProvider ribbonContextProvider)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public void RemoveProvider(IRibbonContextProvider ribbonContextProvider)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public void AddFirst(IRibbonContextProvider ribbonContextProvider)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public List<IRibbonContextContainer> GetRibbonContextContainers(System.Collections.ICollection collection)
            {
                return m_RibbonContextContainerList;
            }

            public string GetDefaultRibbonContext(Object selectedObject)
            {
                return m_DefaultRibbonContext;
            }

            #endregion

        }
        #endregion
    }
}
