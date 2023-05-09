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
using NUnit.Framework;
using Rhino.Mocks;

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
            m_ActionServiceMock = MockRepository.GenerateMock<IActionService>();
            m_CrossReferenceFinder = new DatabaseServiceActionCrossReferenceFinder(m_ActionServiceMock.ToILazy());
            m_CrossReferenceContainer = MockRepository.GenerateMock<ICrossReferenceContainer>();
        }

        [TearDown]
        public void TearDown()
        {
            m_CrossReferenceContainer.VerifyAllExpectations();
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
            m_ActionServiceMock.Stub(x => x.GetActionList(Arg<object>.Is.Anything)).Return(new ActionList { action });
            m_ActionServiceMock.Stub(x => x.GetActionInfo(Arg<string>.Is.Anything)).Return(new ActionInfo(typeof(IDatabaseImportExportService)));
            m_CrossReferenceContainer.Expect(x => x.AddReference(Arg<IActionCrossReferenceItem>.Is.Anything)).Repeat.Once();

            // Act
            m_CrossReferenceFinder.FindReferences<DependencyObject, Binding>(m_CrossReferenceContainer, dependencyObject, null);

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
