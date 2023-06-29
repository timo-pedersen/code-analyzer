using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Interfaces;
using NSubstitute;
using NUnit.Framework;

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

        [SetUp]
        public void Setup()
        {
            m_ProjectFiles = new List<string>();

            ITargetInfo targetInfoStub = Substitute.For<ITargetInfo>();
            targetInfoStub.ProjectPath = ProjectFolderPath;
            targetInfoStub.TempPath = Path.Combine(ProjectFolderPath, TempFilesFolderName);
            targetInfoStub.BuildFilesPath = Path.Combine(ProjectFolderPath, BuildFilesFolderName);

            FileHelper fileHelperStub = Substitute.For<FileHelper>();
            fileHelperStub.Exists(ZipFilePath).Returns(false);

            DirectoryHelper directoryHelper = Substitute.For<DirectoryHelper>();
            directoryHelper.GetFiles(ProjectFolderPath, true)
                .Returns(m_ProjectFiles.ToArray());
            directoryHelper.GetFiles(ProjectFolderPath, false)
                .Returns(GetProjectFilesOnRootLevel().ToArray());
            directoryHelper.GetDirectories(ProjectFolderPath)
                .Returns(GetProjectFoldersOnRootLevel().ToArray());
            directoryHelper.Exists(Arg.Any<string>()).Returns(true);

            m_ArchiveMock = Substitute.For<IZipFileArchive>();
            NeoZipFileFactory factory = Substitute.For<NeoZipFileFactory>();
            factory.CreateZipFile(ZipFilePath, Arg.Any<string>()).Returns(m_ArchiveMock);

            m_Generator = new CompressedProjectGenerator(targetInfoStub);
            m_Generator.NeoZipFileFactory = factory;
            m_Generator.FileHelper = fileHelperStub;
            m_Generator.DirectoryHelper = directoryHelper;
        }

        [Test]
        public void FilesInOptedOutFoldersAreNotIncluded()
        {
            m_ProjectFiles.Add(Path.Combine(ProjectFolderPath, "afile.ext"));
            m_ProjectFiles.Add(Path.Combine(ProjectFolderPath, "afile.txt"));
            m_ProjectFiles.Add(Path.Combine(ProjectFolderPath, TempFilesFolderName, "afile.ext"));
            m_ProjectFiles.Add(Path.Combine(ProjectFolderPath, TempFilesFolderName, "output", "afile.ext"));
            m_ProjectFiles.Add(Path.Combine(ProjectFolderPath, BuildFilesFolderName, "anotherfile.txt"));

            ExecuteCompress();

            m_ArchiveMock.Received(1).AddAllFilesInRoot(ProjectFolderPath);
            //After the calls above, no more calls
            m_ArchiveMock.DidNotReceiveWithAnyArgs().AddFiles(Arg.Any<string>());
            m_ArchiveMock.DidNotReceiveWithAnyArgs().AddFile(Arg.Any<string>());
            m_ArchiveMock.DidNotReceiveWithAnyArgs().AddAllFilesInRoot(Arg.Any<string>());
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

            ExecuteCompress();

            m_ArchiveMock.Received().AddFiles(subFolderPath1);
            m_ArchiveMock.Received().AddFiles(subFolderPath2);
            //After the call above, no more calls
            m_ArchiveMock.DidNotReceiveWithAnyArgs().AddFiles(Arg.Any<string>());
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
