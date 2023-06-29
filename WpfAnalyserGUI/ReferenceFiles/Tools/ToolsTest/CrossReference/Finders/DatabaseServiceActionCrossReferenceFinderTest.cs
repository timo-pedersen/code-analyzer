using System;
using System.Windows;
using System.Windows.Data;
using Core.Api.CrossReference;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Constants;
using Neo.ApplicationFramework.Interfaces.Storage;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Action;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.CrossReference.Finders
{
    [TestFixture]
    public class DatabaseServiceActionCrossReferenceFinderTest
    {
        private ICrossReferenceContainer m_CrossReferenceContainer;
        private IActionService m_ActionServiceMock;

        private DatabaseServiceActionCrossReferenceFinder m_CrossReferenceFinder;

        [SetUp]
        public void SetUp()
        {
            TestHelper.AddServiceStub<ITargetService>();
            m_ActionServiceMock = Substitute.For<IActionService>();
            m_CrossReferenceFinder = new DatabaseServiceActionCrossReferenceFinder(m_ActionServiceMock.ToILazy());
            m_CrossReferenceContainer = Substitute.For<ICrossReferenceContainer>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void FindReferencesVerifyActionFoundTest()
        {
            // Arrange
            DependencyObject dependencyObject = new DependencyObject();
            const string tableName = "DataLogger55";
            const string fileName = "fileName";
            var action = CreateDatabaseExportAction(tableName, fileName);
            m_ActionServiceMock.GetActionList(Arg.Any<object>()).Returns(new ActionList { action });
            m_ActionServiceMock.GetActionInfo(Arg.Any<string>()).Returns(new ActionInfo(typeof(IDatabaseImportExportService)));

            // Act
            m_CrossReferenceFinder.FindReferences<DependencyObject, Binding>(m_CrossReferenceContainer, dependencyObject, null);

            m_CrossReferenceContainer.Received(1).AddReference(Arg.Any<IActionCrossReferenceItem>());
        }

        private IAction CreateDatabaseExportAction(string tableName, string fileName)
        {
            var actionParameterInfoList = new ActionParameterInfoList
            {
                new ActionParameterInfo { ParameterName = ActionConstants.DatabaseExportActionName },
                new ActionParameterInfo { ParameterName = ActionConstants.DatabaseActionTableName, Value = tableName },
                new ActionParameterInfo { ParameterName = ActionConstants.DatabaseActionFileName, Value = fileName },
            };

            return new Action.Action
            {
                ActionMethodInfo = new ActionMethodInfo() { ActionParameterInfo = actionParameterInfoList, EventName = ActionConstants.DatabaseExportActionName, ReferenceType = ReferenceType.Service }
            };
        }
    }
}
