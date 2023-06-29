using System.IO;
using Core.Api.Application;
using Neo.ApplicationFramework.TestUtilities;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Terminal
{
    [TestFixture]
    public class TerminalTest
    {
        private const string TestDirectory = "TerminalTestDirectory";
        private const string SchemaFileName = "Terminal.xsd";
#if VNEXT_TARGET
        private const string TerminalFileName = "X2plusPro7.xml";
#else
        private const string TerminalFileName = "X2marine15HB.xml"; 
#endif

        private Terminal m_Terminal;
        private string m_TerminalTestFolder;

        [SetUp]
        public void SetUp()
        {
            ICoreApplication coreApplication = Substitute.For<ICoreApplication>();

            const string startupPath = "./";
            Directory.CreateDirectory(startupPath);
            coreApplication.StartupPath.Returns(startupPath);
            TestHelper.AddService<ICoreApplication>(coreApplication);

            m_Terminal = CreateTerminal(TerminalFileName);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            m_TerminalTestFolder = CreateTerminalTestFolder();
            CopyTerminalFiles();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            DeleteTerminalTestFolder();
        }

#if VNEXT_TARGET
        [Test]
        public void PropertyTests()
        {
            Assert.True(m_Terminal.Name.Equals("X2+ pro 7"));
            Assert.True(m_Terminal.PanelBaseName.Equals("X2pro7"));
            Assert.True(m_Terminal.Platform.Equals(Core.Api.Platform.TargetPlatform.WindowsCE));
            Assert.True(m_Terminal.PlatformVersion.Equals(Core.Api.Platform.TargetPlatformVersion.CE8));
            Assert.True(m_Terminal.CpuArchitecture.Equals(Core.Api.DI.PlatformFactory.CpuArchitecture.Arm64));
            Assert.True(m_Terminal.SupportRotate);
            Assert.False(m_Terminal.SupportDipSwitch);
            Assert.True(m_Terminal.SupportSerialNumber);
            Assert.True(m_Terminal.SupportFlashLifeTimeInfo);
            Assert.True(m_Terminal.TimeoutForTransfer.Equals(100));
            Assert.False(m_Terminal.IsKeyPanel);
            Assert.True(m_Terminal.IsPanel);
            Assert.False(m_Terminal.IsPC);
            Assert.False(m_Terminal.IsDeprecated);
            Assert.True(m_Terminal.ItemHeightScaleFactor.Equals(1));
            Assert.True(m_Terminal.ItemWidthScaleFactor.Equals(1));
            Assert.True(m_Terminal.ImageHeight.Equals(776));
            Assert.True(m_Terminal.ImageWidth.Equals(1111));
            Assert.True(m_Terminal.DefaultKeyboardLayout.Equals("US"));

            string ethernetAdapterName;
            m_Terminal.EthernetAdapterNames.TryGetValue(1, out ethernetAdapterName);
            Assert.True(ethernetAdapterName != null && ethernetAdapterName.Equals("ENET1"));

            Assert.True(m_Terminal.NumberOfCpuCores.Equals(1));
        }
#else
        [Test]
        public void PropertyTests()
        {
            Assert.True(m_Terminal.Name.Equals("X2 marine 15 - HB"));
            Assert.True(m_Terminal.PanelBaseName.Equals("X2marine15HB"));
            Assert.True(m_Terminal.Platform.Equals(Core.Api.Platform.TargetPlatform.WindowsCE));
            Assert.True(m_Terminal.PlatformVersion.Equals(Core.Api.Platform.TargetPlatformVersion.CE8));
            Assert.True(m_Terminal.CpuArchitecture.Equals(Core.Api.DI.PlatformFactory.CpuArchitecture.ArmV7));
            Assert.True(m_Terminal.SupportRotate);
            Assert.False(m_Terminal.SupportDipSwitch);
            Assert.True(m_Terminal.SupportSerialNumber);
            Assert.True(m_Terminal.SupportFlashLifeTimeInfo);
            Assert.True(m_Terminal.TimeoutForTransfer.Equals(100));
            Assert.False(m_Terminal.IsKeyPanel);
            Assert.True(m_Terminal.IsPanel);
            Assert.False(m_Terminal.IsPC);
            Assert.False(m_Terminal.IsDeprecated);
            Assert.True(m_Terminal.ItemHeightScaleFactor.Equals(1.6));
            Assert.True(m_Terminal.ItemWidthScaleFactor.Equals(1.6));
            Assert.True(m_Terminal.ImageHeight.Equals(1156));
            Assert.True(m_Terminal.ImageWidth.Equals(1657));
            Assert.True(m_Terminal.DefaultKeyboardLayout.Equals("US"));

            string ethernetAdapterName;
            m_Terminal.EthernetAdapterNames.TryGetValue(1, out ethernetAdapterName);
            Assert.True(ethernetAdapterName != null && ethernetAdapterName.Equals("ENET1"));

            m_Terminal.EthernetAdapterNames.TryGetValue(2, out ethernetAdapterName);
            Assert.True(ethernetAdapterName != null && ethernetAdapterName.Equals("SMSC95001"));

            Assert.True(m_Terminal.NumberOfCpuCores.Equals(4));
        }
#endif

        [Test]
        public void GetParameterTest()
        {
            Assert.True(m_Terminal.GetParameter("PlatformVersion").Equals(Core.Api.Platform.TargetPlatformVersion.CE8));
        }

        private Terminal CreateTerminal(string terminalFilename)
        {
            var terminal = Substitute.For<Terminal>();
            terminal.DirectoryPath.Returns(m_TerminalTestFolder);
            terminal.FilePath = terminalFilename;
            return terminal;
        }

        private static string CreateTerminalTestFolder()
        {
            string directoryPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd(@"\".ToCharArray()), TestDirectory);
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            return directoryPath;
        }

        private void DeleteTerminalTestFolder()
        {
            if (!Directory.Exists(m_TerminalTestFolder))
                return;

            foreach (var filePath in Directory.GetFiles(m_TerminalTestFolder))
            {
                var file = new FileInfo(filePath);
                file.IsReadOnly = false;
            }

            Directory.Delete(m_TerminalTestFolder, true);
        }

        private void CopyTerminalFiles()
        {
            // copy the terminal file
#if VNEXT_TARGET
            string terminalFolder = System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd(@"\".ToCharArray()) + @"..\..\..\..\..\..\..\Brands\vNext\Terminals";
#else
            string terminalFolder = System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd(@"\".ToCharArray()) + @"..\..\..\..\..\Brands\iX\Terminals";
#endif
            string terminalFile = Path.Combine(terminalFolder, TerminalFileName);
            File.Copy(terminalFile, Path.Combine(m_TerminalTestFolder, TerminalFileName), true);


            // copy the terminal schema file
#if DEBUG
            const string solutionConfig = "Debug";
#else
            const string solutionConfig = "Release";
#endif

#if VNEXT_TARGET
            string schemaFolder = System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd(@"\".ToCharArray()) + "..\\..\\..\\..\\..\\..\\..\\Output\\" + solutionConfig +
                                  "\\Resources\\Terminals";
#else
            string schemaFolder = System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd(@"\".ToCharArray()) + "..\\..\\..\\..\\..\\Output\\" + solutionConfig +
                                  "\\Resources\\Terminals";
#endif
            string schemaFile = Path.Combine(schemaFolder, SchemaFileName);
            File.Copy(schemaFile, Path.Combine(m_TerminalTestFolder, SchemaFileName));
        }
    }
}
