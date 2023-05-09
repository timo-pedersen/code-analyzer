using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Neo.ApplicationFramework.Common.Constants;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.ProjectManager;
using Neo.ApplicationFramework.Tools.ProjectManager.Validation;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Build.BuildManager.Validators
{
    [TestFixture]
    class ReferencedAssemblyValidatorTest
    {
        private IErrorListService m_ErrorListService;
        private IProjectManager m_ProjectManager;
        private ReferencedAssemblyValidator m_ReferencedAssemblyValidator;
        private string m_ReferencedAssemblyFolder;

        private const string TestFolder = @"..\..\..\..\Tools\ToolsTest\Build\BuildManager\Validators\TestFiles";
        private const string ApplicationStartupPath = @".";

        [SetUp]
        public void SetUp()
        {
            TestHelper.ClearServices();
            m_ErrorListService = TestHelper.CreateAndAddServiceMock<IErrorListService>();
            m_ProjectManager = TestHelper.CreateAndAddServiceStub<IProjectManager>();
            m_ProjectManager.Project = TestHelper.CreateAndAddServiceStub<IProject>();
            m_ProjectManager.Project.FolderPath = TestFolder;

            m_ReferencedAssemblyValidator = new ReferencedAssemblyValidator(m_ProjectManager.ToILazy(), ApplicationStartupPath);
            m_ReferencedAssemblyFolder = Path.Combine(TestFolder, DirectoryConstants.ReferencedAssemblyFolderName);

            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;

            DeleteReferencedAssemblyDirectory();

            Directory.CreateDirectory(m_ReferencedAssemblyFolder);
        }

        private void DeleteReferencedAssemblyDirectory()
        {
            if (!Directory.Exists(m_ReferencedAssemblyFolder))
                return;

            foreach (var fileToDelete in Directory.GetFiles(m_ReferencedAssemblyFolder))
            {
                DeleteFile(fileToDelete);
            }

            Directory.Delete(m_ReferencedAssemblyFolder);
        }

        private void DeleteFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.SetAttributes(fileName, FileAttributes.Normal);
                File.Delete(fileName);
            }
        }

        private void AddReferencedAssemblies(IEnumerable<string> assemblyNames = null, string assemblyName = null,
            string srcFolder = null)
        {
            var referencedAssemblies = new BindingList<IReferencedAssemblyInfo>();
            if (string.IsNullOrEmpty(srcFolder))
                srcFolder = TestFolder;

            Directory.CreateDirectory(m_ReferencedAssemblyFolder);
            if (assemblyNames != null)
            {
                foreach (var assemblyFileName in assemblyNames)
                    AddToCollection(assemblyFileName, referencedAssemblies, srcFolder);
            }

            if (!string.IsNullOrEmpty(assemblyName))
                AddToCollection(assemblyName, referencedAssemblies, srcFolder);

            m_ProjectManager.Project.Stub(p => p.ReferencedAssemblies).Return(referencedAssemblies);
        }

        private void AddToCollection(string assemblyName, ICollection<IReferencedAssemblyInfo> referencedAssemblies,
            string srcFolder)
        {

            File.Copy(Path.Combine(srcFolder, assemblyName), Path.Combine(m_ReferencedAssemblyFolder, assemblyName));
            referencedAssemblies.Add(new ReferencedAssemblyInfo(assemblyName));
        }

        //file not found
        [Test]
        public void FileNotExist()
        {
            //ARRANGE
            var toBeDeletedFile = "ABCD.dll";
            AddReferencedAssemblies(assemblyName: toBeDeletedFile);
            DeleteFile(Path.Combine(m_ReferencedAssemblyFolder, toBeDeletedFile));

            m_ErrorListService.Expect(x => x.AddNewCompilerError(Arg<string>.Is.Anything, Arg<bool>.Is.Anything)).Repeat
                .Once();

            //ACT
            var isValid = m_ReferencedAssemblyValidator.Validate();

            //ASSERT
            m_ErrorListService.VerifyAllExpectations();
            Assert.That(isValid, Is.EqualTo(false));
        }

        //no referenced assemblies => no error
        [Test]
        public void NoReferencedAssemblies()
        {
            //ARRANGE
            var referencedAssemblies = new BindingList<IReferencedAssemblyInfo>();
            m_ProjectManager.Project.Stub(p => p.ReferencedAssemblies).Return(referencedAssemblies);

            m_ErrorListService.Expect(x => x.AddNewCompilerError(Arg<string>.Is.Anything, Arg<bool>.Is.Anything)).Repeat.Never();

            //ACT
            var isValid = m_ReferencedAssemblyValidator.Validate();

            //ASSERT
            m_ErrorListService.VerifyAllExpectations();
            Assert.That(isValid, Is.EqualTo(true));
        }

        //reference assemblies with same display name but with case difference
        [Test]
        public void AssemblyWithSameDisplayNameButCaseDifferent()
        {
            //ARRANGE
            AddReferencedAssemblies(assemblyName: "controls.dll");

            m_ErrorListService.Expect(x => x.AddNewCompilerError(Arg<string>.Is.Anything, Arg<bool>.Is.Anything)).Repeat
                .Once();

            //ACT
            var isValid = m_ReferencedAssemblyValidator.Validate();

            //ASSERT
            m_ErrorListService.VerifyAllExpectations();
            Assert.That(isValid, Is.EqualTo(false));
        }

        //reference assemblies with same simple name but with case difference
        [Test]
        public void AssemblyWithSameSimpleNameButCaseDifferent()
        {
            //ARRANGE
            AddReferencedAssemblies(assemblyName: "PQR.dll");

            m_ErrorListService.Expect(x => x.AddNewCompilerError(Arg<string>.Is.Anything, Arg<bool>.Is.Anything)).Repeat
                .Once();

            //ACT
            var isValid = m_ReferencedAssemblyValidator.Validate();

            //ASSERT
            m_ErrorListService.VerifyAllExpectations();
            Assert.That(isValid, Is.EqualTo(false));
        }

        //valid referenced assemblies (different in simple name and display name) => no error
        [Test]
        public void ValidReferencedAssemblies()
        {
            //ARRANGE
            AddReferencedAssemblies(assemblyName: "ABCD.dll");

            m_ErrorListService.Expect(x => x.AddNewCompilerError(Arg<string>.Is.Anything, Arg<bool>.Is.Anything)).Repeat.Never();

            //ACT
            var isValid = m_ReferencedAssemblyValidator.Validate();

            //ASSERT
            m_ErrorListService.VerifyAllExpectations();
            Assert.That(isValid, Is.EqualTo(true));
        }

        //reference assemblies with different simple name but with same display names
        [Test]
        public void AssemblyWithSameDisplayNameButDifferentSimpleName()
        {
            //ARRANGE
            AddReferencedAssemblies(assemblyName: "ToolsCF.dll");

            m_ErrorListService.Expect(x => x.AddNewCompilerError(Arg<string>.Is.Anything, Arg<bool>.Is.Anything)).Repeat
                .Once();

            //ACT
            var isValid = m_ReferencedAssemblyValidator.Validate();

            //ASSERT
            m_ErrorListService.VerifyAllExpectations();
            Assert.That(isValid, Is.EqualTo(false));
        }

        //reference assemblies with same simple name but with different display names
        [Test]
        public void AssemblyWithSameSimpleNameButDifferentDisplayName()
        {
            //ARRANGE
            AddReferencedAssemblies(assemblyName: "Attributes - Copy.dll");

            m_ErrorListService.Expect(x => x.AddNewCompilerError(Arg<string>.Is.Anything, Arg<bool>.Is.Anything)).Repeat
                .Once();

            //ACT
            var isValid = m_ReferencedAssemblyValidator.Validate();

            //ASSERT
            m_ErrorListService.VerifyAllExpectations();
            Assert.That(isValid, Is.EqualTo(false));
        }

        //reference assemblies with same simple name and same display name but different versions
        [Test]
        public void AssemblyWithSameSimpleNameAndSameDisplayNameButDifferentVersion()
        {
            //ARRANGE
            AddReferencedAssemblies(assemblyName: "Attributes.dll");

            m_ErrorListService.Expect(x => x.AddNewCompilerError(Arg<string>.Is.Anything, Arg<bool>.Is.Anything)).Repeat
                .Once();

            //ACT
            var isValid = m_ReferencedAssemblyValidator.Validate();

            //ASSERT
            m_ErrorListService.VerifyAllExpectations();
            Assert.That(isValid, Is.EqualTo(false));
        }

        [Test]
        public void TwoExactlySameAssemblies()
        {
            //ARRANGE
            AddReferencedAssemblies(assemblyName: "Resources.dll", srcFolder: ApplicationStartupPath);

            m_ErrorListService.Expect(x => x.AddNewCompilerError(Arg<string>.Is.Anything, Arg<bool>.Is.Anything)).Repeat
                .Never();

            //ACT
            var isValid = m_ReferencedAssemblyValidator.Validate();

            //ASSERT
            m_ErrorListService.VerifyAllExpectations();
            Assert.That(isValid, Is.EqualTo(true));
        }

        //reference assemblies with both valid and invalid assemblies
        [Test]
        public void AssemblyWithMultiplieInvalidFiles()
        {
            //ARRANGE
            AddReferencedAssemblies(new[]
            {
                "ABCD.dll",
                "Attributes - Copy.dll",
                "Attributes.dll",
                "controls.dll",
                "PQR.dll",
                "ToolsCF.dll"
            });
            DeleteFile(Path.Combine(m_ReferencedAssemblyFolder, "ABCD.dll"));

            m_ErrorListService.Expect(x => x.AddNewCompilerError(Arg<string>.Is.Anything, Arg<bool>.Is.Anything)).Repeat
                .Times(6);

            //ACT
            var isValid = m_ReferencedAssemblyValidator.Validate();

            //ASSERT
            m_ErrorListService.VerifyAllExpectations();
            Assert.That(isValid, Is.EqualTo(false));
        }

    }
}
