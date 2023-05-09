using System;
using System.Linq;
using System.Reflection;
using Core.Api.GlobalReference;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Reporting;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Reporting;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Action.ActionEditors.ViewModels
{
    [TestFixture]
    public class GenerateReportViewModelTest
    {
        private Reports m_Reports;
        private IReport m_FirstReport;
        private IReport m_SecondReport;
        private IAction m_Action;
        private GenerateReportViewModelDouble m_SystemUnderTest;

        private const int ReportActionParameterIndex = 0;
        private const int ReportNameParameterIndex = 1;
        private const int FileDirectoryParameterIndex = 2;
        private const int DestinationPathParameterIndex = 3;

        [SetUp]
        public virtual void SetUp()
        {
            TestHelper.ClearServices();
            TestHelper.CreateAndAddServiceMock<IObjectNotificationService>();
            TestHelper.CreateAndAddServiceMock<ICommandManagerService>();

            m_Reports = new Reports();
            m_FirstReport = MockRepository.GenerateStub<IReport>().With(rep => rep.Name = "Report 1");
            m_SecondReport = MockRepository.GenerateStub<IReport>().With(rep => rep.Name = "Report 2");
            m_Reports.ReportItems.Add(m_FirstReport);
            m_Reports.ReportItems.Add(m_SecondReport);

            IGlobalReferenceService referenceService = TestHelper.CreateAndAddServiceStub<IGlobalReferenceService>();
            referenceService.Stub(refService => refService.GetObjects<Reports>(false)).Return(new[] { m_Reports });
        }

        [Test]
        public void WhenActionIsNotSetViewModelShouldGetDefaultValues()
        {
            // Arrange
            SetupAction();

            // Act
            m_SystemUnderTest = new GenerateReportViewModelDouble(m_Action);

            // Assert
            Assert.AreEqual(GenerateReportAction.Print, m_SystemUnderTest.SelectedGenerateReportAction);
            Assert.AreEqual(m_FirstReport, m_SystemUnderTest.SelectedReport);
            Assert.AreEqual(FileDirectory.ProjectFiles, m_SystemUnderTest.FileDirectory);
            Assert.AreEqual(string.Empty, m_SystemUnderTest.DestinationPath);

            AssertActionParameterInfo(m_SystemUnderTest.ActionParameterInfo[ReportActionParameterIndex], GenerateReportViewModel.GenerateReportActionParameterName, GenerateReportAction.Print.ToString());
            AssertActionParameterInfo(m_SystemUnderTest.ActionParameterInfo[ReportNameParameterIndex], GenerateReportViewModel.ReportNameParameterName, m_FirstReport.Name);
            AssertActionParameterInfo(m_SystemUnderTest.ActionParameterInfo[FileDirectoryParameterIndex], GenerateReportViewModel.FileDirectoryParameterName, FileDirectory.ProjectFiles.ToString());
            AssertActionParameterInfo(m_SystemUnderTest.ActionParameterInfo[DestinationPathParameterIndex], GenerateReportViewModel.DestinationPathParameterName, "");
        }

        [Test]
        public void WhenActionParametersAreSetViewModelValuesAreSetToThoseValues()
        {
            // Arrange
            var path = "C:\\";

            SetupAction();
            SetActionValue(ReportActionParameterIndex, GenerateReportAction.Save.ToString());
            SetActionValue(ReportNameParameterIndex, m_SecondReport.Name);
            SetActionValue(FileDirectoryParameterIndex, FileDirectory.FilePath.ToString());
            SetActionValue(DestinationPathParameterIndex, path);

            // Act
            m_SystemUnderTest = new GenerateReportViewModelDouble(m_Action);

            // Assert
            Assert.AreEqual(m_SystemUnderTest.SelectedGenerateReportAction, GenerateReportAction.Save);
            Assert.AreEqual(m_SystemUnderTest.SelectedReport, m_SecondReport);
            Assert.AreEqual(m_SystemUnderTest.FileDirectory, FileDirectory.FilePath);
            Assert.AreEqual(m_SystemUnderTest.DestinationPath, path);

            AssertActionParameterInfo(m_SystemUnderTest.ActionParameterInfo[ReportActionParameterIndex], GenerateReportViewModel.GenerateReportActionParameterName, GenerateReportAction.Save.ToString());
            AssertActionParameterInfo(m_SystemUnderTest.ActionParameterInfo[ReportNameParameterIndex], GenerateReportViewModel.ReportNameParameterName, m_SecondReport.Name);
            AssertActionParameterInfo(m_SystemUnderTest.ActionParameterInfo[FileDirectoryParameterIndex], GenerateReportViewModel.FileDirectoryParameterName, FileDirectory.FilePath.ToString());
            AssertActionParameterInfo(m_SystemUnderTest.ActionParameterInfo[DestinationPathParameterIndex], GenerateReportViewModel.DestinationPathParameterName, path);
        }

        [Test]
        public void WhenViewModelPropertiesAreSetActionParametersAreChanged()
        {
            // Arrange
            var path = "C:\\";
            SetupAction();

            // Act
            m_SystemUnderTest = new GenerateReportViewModelDouble(m_Action);
            m_SystemUnderTest.SelectedGenerateReportAction = GenerateReportAction.Save;
            m_SystemUnderTest.SelectedReport = m_SecondReport;
            m_SystemUnderTest.FileDirectory = FileDirectory.FilePath;
            m_SystemUnderTest.DestinationPath = path;

            // Assert
            Assert.AreEqual(m_SystemUnderTest.SelectedGenerateReportAction, GenerateReportAction.Save);
            Assert.AreEqual(m_SystemUnderTest.SelectedReport, m_SecondReport);
            Assert.AreEqual(m_SystemUnderTest.FileDirectory, FileDirectory.FilePath);
            Assert.AreEqual(m_SystemUnderTest.DestinationPath, path);

            AssertActionParameterInfo(m_SystemUnderTest.ActionParameterInfo[ReportActionParameterIndex], GenerateReportViewModel.GenerateReportActionParameterName, GenerateReportAction.Save.ToString());
            AssertActionParameterInfo(m_SystemUnderTest.ActionParameterInfo[ReportNameParameterIndex], GenerateReportViewModel.ReportNameParameterName, m_SecondReport.Name);
            AssertActionParameterInfo(m_SystemUnderTest.ActionParameterInfo[FileDirectoryParameterIndex], GenerateReportViewModel.FileDirectoryParameterName, FileDirectory.FilePath.ToString());
            AssertActionParameterInfo(m_SystemUnderTest.ActionParameterInfo[DestinationPathParameterIndex], GenerateReportViewModel.DestinationPathParameterName, path);
        }

        [Test]
        public void ReportsObjectHasTheCorrectMethodForPrinting()
        {
            string expectedMethodName = GenerateReportViewModel.MethodNameForPrintReport;
            string expectedParameterReportName = GenerateReportViewModel.ReportNameParameterName;

            MethodInfo methodInfoForPrinting = typeof(Reports).GetMethods().SingleOrDefault(
                x => x.Name.Equals(expectedMethodName) && x.GetParameters().Count() == 1);

            Assert.That(
                methodInfoForPrinting,
                Is.Not.Null,
                String.Format("The action editor depends on a method with name {0} on {1} object. ", expectedMethodName, typeof(Reports)));

            AssertParameter(methodInfoForPrinting.GetParameters()[0], expectedMethodName, expectedParameterReportName, typeof(string));
        }

        [Test]
        public void ReportsObjectHasTheCorrectMethodForSavingToFileDirectory()
        {
            string expectedMethodName = GenerateReportViewModel.MethodNameForSaveReport;
            string expectedParameterReportName = GenerateReportViewModel.ReportNameParameterName;
            string expectedParameterFileDirectory = GenerateReportViewModel.FileDirectoryParameterName;

            MethodInfo methodInfoForSaving = typeof(Reports).GetMethods().SingleOrDefault(
                x => x.Name.Equals(expectedMethodName) && x.GetParameters().Count() == 2
                     && x.GetParameters().Last().ParameterType.Equals(typeof(FileDirectory)));

            Assert.That(
                methodInfoForSaving,
                Is.Not.Null,
                String.Format("The action editor depends on a method with name {0} on {1} object. ", expectedMethodName, typeof(Reporting.Reports)));

            AssertParameter(methodInfoForSaving.GetParameters()[0], expectedMethodName, expectedParameterReportName, typeof(String));
            AssertParameter(methodInfoForSaving.GetParameters()[1], expectedMethodName, expectedParameterFileDirectory, typeof(FileDirectory));
        }

        [Test]
        public void ReportsObjectHasTheCorrectMethodForSavingToPath()
        {
            string expectedMethodName = GenerateReportViewModel.MethodNameForSaveReport;
            string expectedParameterReportName = GenerateReportViewModel.ReportNameParameterName;
            string expectedParameterPath = GenerateReportViewModel.DestinationPathParameterName;

            MethodInfo methodInfoForSaving = typeof(Reports).GetMethods().SingleOrDefault(
                x => x.Name.Equals(expectedMethodName) && x.GetParameters().Count() == 2
                     && x.GetParameters().Last().ParameterType.Equals(typeof(string)));

            Assert.That(
                methodInfoForSaving,
                Is.Not.Null,
                String.Format("The action editor depends on a method with name {0} on {1} object. ", expectedMethodName, typeof(Reporting.Reports)));

            AssertParameter(methodInfoForSaving.GetParameters()[0], expectedMethodName, expectedParameterReportName, typeof(String));
            AssertParameter(methodInfoForSaving.GetParameters()[1], expectedMethodName, expectedParameterPath, typeof(String));
        }

        private void SetActionValue(int index, object value)
        {
            m_Action.ActionMethodInfo.ActionParameterInfo[index].Value = value;
        }

        private void AssertParameter(ParameterInfo parameterInfo, string methodName, string expectedName, Type expectedType)
        {
            Assert.That(
                parameterInfo.Name,
                Is.EqualTo(expectedName),
                String.Format("The action editor depends on a method with name {0} and first parameter named {1}!", methodName, expectedName));

            Assert.That(
                parameterInfo.ParameterType,
                Is.EqualTo(expectedType),
                String.Format("The action editor depends on a method with name {0} and first parameter of type {1}!", methodName, expectedType));
        }

        private void AssertActionParameterInfo(IActionParameterInfo info, string name, object value)
        {
            Assert.That(info.ParameterName, Is.EqualTo(name));
            Assert.That(info.Value, Is.EqualTo(value));
        }

        private void SetupAction()
        {
            m_Action = new Action("GenerateReport");
            m_Action.ActionMethodInfo = new ActionMethodInfo("GenerateReport");
            ActionParameterInfoList actionParameterInfoList = new ActionParameterInfoList
            {
                new ActionParameterInfo()
                {
                    ParameterName = "generateReportAction",
                    ParameterType = typeof(GenerateReportAction),
                    Position = ReportActionParameterIndex
                },
                new ActionParameterInfo()
                {
                    ParameterName = "reportName",
                    ParameterType = typeof(string),
                    Position = ReportNameParameterIndex
                },
                new ActionParameterInfo()
                {
                    ParameterName = "fileDirectory",
                    ParameterType = typeof(FileDirectory),
                    Position = FileDirectoryParameterIndex
                },
                new ActionParameterInfo()
                {
                    ParameterName = "destinationPath",
                    ParameterType = typeof(string),
                    Position = DestinationPathParameterIndex
                }
            };

            m_Action.ActionMethodInfo.ActionParameterInfo = actionParameterInfoList;
        }

        internal class GenerateReportViewModelDouble : GenerateReportViewModel
        {
            public IAction ReportAction
            {
                get { return Action; }
            }

            public ActionParameterInfoList ActionParameterInfo
            {
                get { return ReportAction.ActionMethodInfo.ActionParameterInfo; }
            }

            public GenerateReportViewModelDouble(IAction action)
            {
                base.Action = action;
            }
        }
    }
}