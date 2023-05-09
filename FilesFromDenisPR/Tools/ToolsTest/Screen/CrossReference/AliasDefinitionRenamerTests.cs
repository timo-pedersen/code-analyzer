#if !VNEXT_TARGET
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Core.Api.CrossReference;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Core.Component.Engine.Design;
using Neo.ApplicationFramework.Common.CrossReference;
using Neo.ApplicationFramework.Common.Dynamics;
using Neo.ApplicationFramework.Controls.Controls;
using Neo.ApplicationFramework.Controls.Screen.Alias;
using Neo.ApplicationFramework.Controls.Screen.CrossReference;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.CrossReference;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Screen.Alias;
using NSubstitute;
using NUnit.Framework;
using Button = Neo.ApplicationFramework.Controls.Button;

namespace Neo.ApplicationFramework.Tools.Screen.CrossReference
{
    [TestFixture]
    public class AliasDefinitionRenamerTests
    {
        private const string OldName = "OldName";
        private const string NewName = "NewName";

        private AliasDefinition m_Alias;
        private IList<FrameworkElement> m_Elements;
        private AliasInstanceList m_AliasInstances;
        private IAliasConfiguration m_AliasConfiguration;
        private ScreenDesign.Screen m_Screen;
        private IScreenRootDesigner m_RootDesigner;
        private INeoDesignerHost m_DesignerHost;
        private IFindAliasWpfBindings m_AliasWpfBindingFinder;
        private IUpdateAliasBindings m_AliasBindingUpdater;
        private ICrossReferenceRenameService m_RenamerByCrossReferenceService;
        private ICrossReferenceQueryService m_CrossReferenceQueryService;

        // Service under test
        private IRenameAliases m_AliasRenamer;


        [SetUp]
        public void SetUp()
        {
            m_Alias = new AliasDefinition() { Name = OldName };

            m_Elements = new List<FrameworkElement>();

            m_AliasInstances = new AliasInstanceList(null);

            m_AliasConfiguration = Substitute.For<IAliasConfiguration>();
            m_AliasConfiguration.Instances = m_AliasInstances;

            m_Screen = Substitute.For<ScreenDesign.Screen>();
            m_Screen.AliasConfiguration = m_AliasConfiguration;

            m_RootDesigner = Substitute.For<IScreenRootDesigner>();
            m_RootDesigner.Component.Returns(m_Screen);
            m_RootDesigner.Elements.Returns(m_Elements);

            m_DesignerHost = Substitute.For<INeoDesignerHost>();
            m_DesignerHost.RootDesigner.Returns(m_RootDesigner);

            m_AliasWpfBindingFinder = Substitute.For<IFindAliasWpfBindings>();
            m_AliasBindingUpdater = Substitute.For<IUpdateAliasBindings>();
            m_RenamerByCrossReferenceService = Substitute.For<ICrossReferenceRenameService>();
            m_CrossReferenceQueryService = Substitute.For<ICrossReferenceQueryService>();

            m_AliasRenamer = new AliasDefinitionRenamer(
                m_AliasWpfBindingFinder,
                m_AliasBindingUpdater,
                m_RenamerByCrossReferenceService.ToILazy(),
                m_CrossReferenceQueryService.ToILazy());

            var targetService = Substitute.For<ITargetService>();
            var target = Substitute.For<ITarget>();
            target.Id.Returns(TargetPlatform.Windows);
            targetService.CurrentTarget.Returns(target);
            targetService.CurrentTargetInfo.Returns(Substitute.For<ITargetInfo>());
            TestHelper.AddService<ITargetService>(targetService);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        public void RenameAliasNameIsUpdated()
        {
            // Arrange
            m_RenamerByCrossReferenceService.NameShouldBeUpdated<IActionCrossReferenceItem>(Arg.Any<string>(), 
                Arg.Any<Func<string, IEnumerable<ICrossReferenceItem>>>(), Arg.Any<string>()).Returns(true);
            m_AliasWpfBindingFinder.FindAliasBindings(m_Elements, OldName)
                .Returns(Enumerable.Empty<WpfBindingInfo>());

            // Act
            m_AliasRenamer.Rename(m_DesignerHost, m_Alias, OldName, NewName);

            // Assert
            Assert.That(m_Alias.Name, Is.EqualTo(NewName));
        }

        [Test]
        public void RenameAliasInstancesAreUpdated()
        {
            // Arrange
            m_RenamerByCrossReferenceService.NameShouldBeUpdated<IActionCrossReferenceItem>(Arg.Any<string>(), 
                Arg.Any<Func<string, IEnumerable<ICrossReferenceItem>>>(), Arg.Any<string>()).Returns(true);
            m_AliasWpfBindingFinder.FindAliasBindings(m_Elements, OldName)
                .Returns(Enumerable.Empty<WpfBindingInfo>());

            m_AliasInstances.Add(new AliasInstance() { Name = "inst1", Values = new AliasValueList() { new AliasValue() { Name = OldName } } });
            m_AliasInstances.Add(new AliasInstance() { Name = "inst2", Values = new AliasValueList() { new AliasValue() { Name = OldName } } });
            m_AliasInstances.Add(new AliasInstance() { Name = "inst3", Values = new AliasValueList() { new AliasValue() { Name = OldName } } });

            // Act
            m_AliasRenamer.Rename(m_DesignerHost, m_Alias, OldName, NewName);

            // Assert
            m_AliasInstances.Each(instance => Assert.That(instance.Values[0].Name, Is.EqualTo(NewName)));
        }

        [Test]
        public void RenameInstancesAreNotUpdatedIfAliasRenameFails()
        {
            // Arrange
            m_RenamerByCrossReferenceService.NameShouldBeUpdated<IActionCrossReferenceItem>(Arg.Any<string>(), Arg.Any<Func<string, IEnumerable<ICrossReferenceItem>>>(), Arg.Any<string>()).Returns(true);
            m_Alias.Site = new FakeSite(OldName);

            m_AliasInstances.Add(new AliasInstance() { Name = "inst1", Values = new AliasValueList() { new AliasValue() { Name = OldName } } });
            m_AliasInstances.Add(new AliasInstance() { Name = "inst2", Values = new AliasValueList() { new AliasValue() { Name = OldName } } });
            m_AliasInstances.Add(new AliasInstance() { Name = "inst3", Values = new AliasValueList() { new AliasValue() { Name = OldName } } });

            // Act
            try
            {
                m_AliasRenamer.Rename(m_DesignerHost, m_Alias, OldName, NewName);
                Assert.Fail("Expected NotImplementedException thrown from FakeSite.Name");
            }
            catch (NotImplementedException) { }

            // Assert
            m_AliasInstances.Each(instance => Assert.That(instance.Values[0].Name, Is.EqualTo(OldName)));
        }

        [Test]
        public void RenameWhenNameShouldNotBeUpdatedVerifyNoUpdate()
        {
            // Arrange
            m_RenamerByCrossReferenceService.NameShouldBeUpdated<IActionCrossReferenceItem>(Arg.Any<string>(), Arg.Any<Func<string, IEnumerable<ICrossReferenceItem>>>(), Arg.Any<string>()).Returns(false);

            m_AliasInstances.Add(new AliasInstance() { Name = "inst1", Values = new AliasValueList() { new AliasValue() { Name = OldName } } });
            m_AliasInstances.Add(new AliasInstance() { Name = "inst2", Values = new AliasValueList() { new AliasValue() { Name = OldName } } });
            m_AliasInstances.Add(new AliasInstance() { Name = "inst3", Values = new AliasValueList() { new AliasValue() { Name = OldName } } });

            // Act
            m_AliasRenamer.Rename(m_DesignerHost, m_Alias, OldName, NewName);

            // Assert
            m_AliasInstances.Each(instance => Assert.That(instance.Values[0].Name, Is.EqualTo(OldName)));
        }

        [Test]
        public void RenameElementBindingsAreUpdated()
        {
            // Arrange
            var button = new Button();
            Binding buttonBinding = Bind(button, Button.ValueProperty, OldName);
            m_Elements.Add(button);

            var meter = new LinearMeter();
            Binding meterBinding = Bind(meter, Meter.ValueProperty, OldName);
            m_Elements.Add(meter);

            m_RenamerByCrossReferenceService.NameShouldBeUpdated<IActionCrossReferenceItem>(Arg.Any<string>(), 
                Arg.Any<Func<string, IEnumerable<ICrossReferenceItem>>>(), Arg.Any<string>()).Returns(true);
            m_AliasWpfBindingFinder.FindAliasBindings(m_Elements, OldName)
                .Returns(new[]
                            {
                                new WpfBindingInfo(button, Button.ValueProperty, buttonBinding),
                                new WpfBindingInfo(meter, Meter.ValueProperty, meterBinding),
                            });

            // Act
            m_AliasRenamer.Rename(m_DesignerHost, m_Alias, OldName, NewName);

            // Assert
            m_AliasBindingUpdater.Received().UpdateBindingWithNewAliasName(buttonBinding, OldName, NewName);
            m_AliasBindingUpdater.Received().UpdateBindingWithNewAliasName(meterBinding, OldName, NewName);
        }

        [Test]
        public void RenameStringIntervalElementBindingsAreUpdated()
        {
            // Arrange
            var button = new Button();
            m_Elements.Add(button);

            button.TextIntervalMapper = new StringIntervalMapper();
            button.TextIntervalMapper.Intervals.Add(new StringInterval());
            button.TextIntervalMapper.Intervals.Add(new StringInterval());
            button.TextIntervalMapper.Intervals.Add(new StringInterval());
            Binding intervalBinding = Bind(button.TextIntervalMapper.Intervals[0], StringInterval.ValueProperty, OldName);
            Binding intervalBinding3rdLine = Bind(button.TextIntervalMapper.Intervals[2], StringInterval.ValueProperty, OldName);

            m_RenamerByCrossReferenceService.NameShouldBeUpdated<IActionCrossReferenceItem>(Arg.Any<string>(), 
                Arg.Any<Func<string, IEnumerable<ICrossReferenceItem>>>(), Arg.Any<string>()).Returns(true);
            m_AliasWpfBindingFinder
                .FindAliasBindings(m_Elements, OldName)
                .Returns(new[]
                            {
                                new WpfBindingInfo(button.TextIntervalMapper.Intervals[0], StringInterval.ValueProperty, intervalBinding),
                                new WpfBindingInfo(button.TextIntervalMapper.Intervals[2], StringInterval.ValueProperty, intervalBinding3rdLine),
                            });


            // Act
            m_AliasRenamer.Rename(m_DesignerHost, m_Alias, OldName, NewName);

            // Assert
            m_AliasBindingUpdater.Received().UpdateBindingWithNewAliasName(intervalBinding, OldName, NewName);
            m_AliasBindingUpdater.Received().UpdateBindingWithNewAliasName(intervalBinding3rdLine, OldName, NewName);
        }


        [Test]
        public void RenameElementBindingsAreNotUpdatedIfAliasRenameFails()
        {
            // Arrange
            m_Alias.Site = new FakeSite(OldName);

            var button = new Button();
            Binding buttonBinding = Bind(button, Button.ValueProperty, OldName);
            m_Elements.Add(button);

            var meter = new LinearMeter();
            Binding meterBinding = Bind(meter, Meter.ValueProperty, OldName);
            m_Elements.Add(meter);

            m_RenamerByCrossReferenceService.NameShouldBeUpdated<IActionCrossReferenceItem>(Arg.Any<string>(), Arg.Any<Func<string, IEnumerable<ICrossReferenceItem>>>(), Arg.Any<string>()).Returns(true);

            // Act
            try
            {
                m_AliasRenamer.Rename(m_DesignerHost, m_Alias, OldName, NewName);
                Assert.Fail("Expected an exception to be thrown here...");
            }
            catch (NotImplementedException) { }

            // Assert
            m_AliasBindingUpdater.DidNotReceive().UpdateBindingWithNewAliasName(buttonBinding, OldName, NewName);
            m_AliasBindingUpdater.DidNotReceive().UpdateBindingWithNewAliasName(meterBinding, OldName, NewName);
        }

        private static Binding Bind(DependencyObject dependencyObject, DependencyProperty dependencyProperty, string aliasName)
        {
            Binding binding = new Binding(aliasName);
            binding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(IScreenWindow), 1);
            BindingOperations.SetBinding(dependencyObject, dependencyProperty, binding);

            return binding;
        }

        private class FakeSite : ISite
        {
            private readonly string m_Name;

            public FakeSite()
            { }

            public FakeSite(string name)
            {
                m_Name = name;
            }
            #region Implementation of IServiceProvider

            public object GetService(Type serviceType)
            {
                throw new NotImplementedException();
            }

            #endregion

            #region Implementation of ISite

            public IComponent Component
            {
                get { throw new NotImplementedException(); }
            }

            public IContainer Container
            {
                get { throw new NotImplementedException(); }
            }

            public bool DesignMode
            {
                get { throw new NotImplementedException(); }
            }

            public string Name
            {
                get
                {
                    if (string.IsNullOrEmpty(m_Name))
                        throw new NotImplementedException();

                    return m_Name;
                }
                set { throw new NotImplementedException(); }
            }

            #endregion
        }
    }
}
#endif
