using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.vNext.Gaps;
using Neo.ApplicationFramework.TestUtilities;
using NUnit.Framework;
using Rhino.Mocks;
using System.Text.RegularExpressions;

namespace Neo.ApplicationFramework.Tools.ProjectManager
{
    [TestFixture]
    public class CompressedProjectGeneratorTest
    {
        private const string BuildFilesFolderName = "Bild Fils Misspelled";
        private const string TempFilesFolderName = "Temp Fils Misspelled";
        private const string ZipFilePath = @"z:\somefolder\project.zip";
        private const string ProjectFolderPath = @"z:\project folder path";
        private const string Pattern = "\\\\";
        private CompressedProjectGenerator m_Generator;
        private IZipFileArchive m_ArchiveMock;
        private IList<string> m_ProjectFiles;
        private IGapService m_GapServiceStub;

        [SetUp]
        public void Setup()
        {
            m_ProjectFiles = new List<string>();

            ITargetInfo targetInfoStub = MockRepository.GenerateStub<ITargetInfo>();
            targetInfoStub.ProjectPath = ProjectFolderPath;
            targetInfoStub.TempPath = Path.Combine(ProjectFolderPath, TempFilesFolderName);
            targetInfoStub.BuildFilesPath = Path.Combine(ProjectFolderPath, BuildFilesFolderName);

            FileHelper fileHelperStub = MockRepository.GenerateStub<FileHelper>();
            fileHelperStub.Stub(x => x.Exists(Arg<string>.Is.Equal(ZipFilePath))).Return(false);

            DirectoryHelper directoryHelper = MockRepository.GenerateStub<DirectoryHelper>();
            directoryHelper.Stub(x => x.GetFiles(Arg<string>.Is.Equal(ProjectFolderPath), Arg<bool>.Is.Equal(true))).Do(new Func<string, bool, string[]>((folderPath, recursive) => m_ProjectFiles.ToArray()));
            directoryHelper.Stub(x => x.GetFiles(Arg<string>.Is.Equal(ProjectFolderPath), Arg<bool>.Is.Equal(false))).Do(new Func<string, bool, string[]>((folderPath, recursive) => GetProjectFilesOnRootLevel().ToArray()));
            directoryHelper.Stub(x => x.GetDirectories(Arg<string>.Is.Equal(ProjectFolderPath))).Do(new Func<string, string[]>((folderPath) => GetProjectFoldersOnRootLevel().ToArray()));
            directoryHelper.Stub(x => x.Exists(Arg<string>.Is.Anything)).Return(true);

            m_ArchiveMock = MockRepository.GenerateMock<IZipFileArchive>();
            NeoZipFileFactory factory = MockRepository.GenerateStub<NeoZipFileFactory>();
            factory.Stub(x => x.CreateZipFile(Arg<string>.Is.Equal(ZipFilePath), Arg<string>.Is.Anything)).Return(m_ArchiveMock);

            m_Generator = new CompressedProjectGenerator(targetInfoStub);
            m_Generator.NeoZipFileFactory = factory;
            m_Generator.FileHelper = fileHelperStub;
            m_Generator.DirectoryHelper = directoryHelper;

            m_GapServiceStub = TestHelper.AddServiceStub<IGapService>();
        }

        [Test]
        public void FilesInOptedOutFoldersAreNotIncluded()
        {
            m_ProjectFiles.Add(Path.Combine(ProjectFolderPath, "afile.ext"));
            m_ProjectFiles.Add(Path.Combine(ProjectFolderPath, "afile.txt"));
            m_ProjectFiles.Add(Path.Combine(ProjectFolderPath, TempFilesFolderName, "afile.ext"));
            m_ProjectFiles.Add(Path.Combine(ProjectFolderPath, TempFilesFolderName, "output", "afile.ext"));
            m_ProjectFiles.Add(Path.Combine(ProjectFolderPath, BuildFilesFolderName, "anotherfile.txt"));

            m_ArchiveMock.Expect(x => x.AddAllFilesInRoot(ProjectFolderPath)).Repeat.Once();
            //After the calls above, no more calls
            m_ArchiveMock.Expect(x => x.AddFiles(Arg<string>.Is.Anything)).Repeat.Times(0);
            m_ArchiveMock.Expect(x => x.AddFile(Arg<string>.Is.Anything)).Repeat.Times(0);
            m_ArchiveMock.Expect(x => x.AddAllFilesInRoot(Arg<string>.Is.Anything)).Repeat.Times(0);

            ExecuteCompress();

            m_ArchiveMock.VerifyAllExpectations();
        }

        [Test]
        public void FoldersInRootLevelThatAreNotOptedOutAreIncluded()
        {
            string subFolderPath1 = Path.Combine(ProjectFolderPath, "subfolder1");
            string subFolderPath2 = Path.Combine(ProjectFolderPath, "subfolder2");

            string file1 = Path.Combine(subFolderPath1, "afile.ext");
            string file2 = Path.Combine(subFolderPath1, "afile.txt");
            string file3 = Path.Combine(subFolderPath1, "sub2", "afile.ext");
            string file4 = Path.Combine(subFolderPath2, "afile.txt");

            m_ProjectFiles.Add(file1);
            m_ProjectFiles.Add(file2);
            m_ProjectFiles.Add(file3);
            m_ProjectFiles.Add(file4);

            m_ArchiveMock.Expect(x => x.AddFiles(subFolderPath1)).Repeat.Once();
            m_ArchiveMock.Expect(x => x.AddFiles(subFolderPath2)).Repeat.Once();
            //After the call above, no more calls
            m_ArchiveMock.Expect(x => x.AddFiles(Arg<string>.Is.Anything)).Repeat.Times(0);

            ExecuteCompress();

            m_ArchiveMock.VerifyAllExpectations();
        }

        private void ExecuteCompress()
        {
            m_Generator.CompressTo(ZipFilePath, string.Empty);
        }

        private IEnumerable<string> GetProjectFilesOnRootLevel()
        {
            return m_ProjectFiles.Where(x => Regex.Matches(x, Pattern).Count < 3);
        }

        private IEnumerable<string> GetProjectFoldersOnRootLevel()
        {
            IList<string> rootDirectories = new List<string>();

            foreach(var path in m_ProjectFiles)
            {
                var relativePath = Path.GetDirectoryName(path).Replace(ProjectFolderPath, "").Trim(new Char[] { '\\' });
                if (!relativePath.Contains("\\") && !rootDirectories.Contains(Path.GetDirectoryName(path)) && !relativePath.IsNullOrEmpty())
                    rootDirectories.Add(Path.GetDirectoryName(path));
            }

            return rootDirectories;
        }
    }
}
