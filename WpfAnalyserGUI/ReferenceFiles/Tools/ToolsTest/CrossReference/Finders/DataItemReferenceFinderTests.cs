using System;
using System.Windows;
using System.Windows.Data;
using Core.Api.CrossReference;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls;
using Neo.ApplicationFramework.Controls.OpcClient.Bindings;
using Neo.ApplicationFramework.Controls.Screen.Bindings;
using Neo.ApplicationFramework.CrossReference;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.CrossReference.Finders;
using Neo.ApplicationFramework.Tools.Design.Bindings;
using Neo.ApplicationFramework.Tools.OpcClient.Bindings;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.CrossReference
{
    [TestFixture]
    public class DataItemReferenceFinderTests
    {
        private ICrossReferenceContainerSignature m_CrossReferenceContainer;
        private ICrossReferenceFinder m_CrossReferenceFinder;
        private IActionService m_ActionServiceMock;

        [SetUp]
        public void SetUp()
        {
            TestHelper.AddService<IPropertyBinderFactory>(new PropertyBinderFactory());
            ITarget targetStub = Substitute.For<ITarget>();

            var terminalStub = Substitute.For<ITerminal>();
            var targetInfoStub = Substitute.For<ITargetInfo>();
            targetInfoStub.TerminalDescription = terminalStub;
            ITargetService targetServiceStub = TestHelper.AddServiceStub<ITargetService>();
            targetServiceStub.CurrentTarget = targetStub;
            targetServiceStub.CurrentTargetInfo.Returns(targetInfoStub);

            m_CrossReferenceContainer = Substitute.For<ICrossReferenceContainerSignature>();
            TestHelper.Bindings.Wpf.RegisterSimpleDataItemBindingSourceProvider();

            m_ActionServiceMock = Substitute.For<IActionService>();
            m_CrossReferenceFinder = new DataItemCrossReferenceFinder(m_ActionServiceMock.ToILazy());
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
            TestHelper.Bindings.Wpf.ClearProviders();
        }

        [Test]
        public void AliasBindingsAreNotAddedToContainer()
        {
            // Arrange

            var bindingSourceDescription = new LocalPropertyBindingSourceDescription("MyAlias");

            var button = new Button() { Name = "MyButton" };
            var binding = new LocalPropertyBindingProvider().ProvideWpfBinding(bindingSourceDescription);
            BindingOperations.SetBinding(button, Button.ValueProperty, binding);

            // Act
            m_CrossReferenceFinder.FindReferences<DependencyObject, Binding>(m_CrossReferenceContainer, button, null);

            m_CrossReferenceContainer.DidNotReceiveWithAnyArgs().AddReference(Arg.Any<CrossReferenceItem>());
        }

        [Test]
        public void DataItemBindingsAreAddedToContainer()
        {
            // Arrange
            var bindingSourceDescription = DataItemBindingSourceDescription.Create(StringConstants.TagsRoot + "Tag1");

            var button = new Button() { Name = "MyButton" };
            var binding = new TagBindingProvider().ProvideWpfBinding(bindingSourceDescription);
            BindingOperations.SetBinding(button, Button.ValueProperty, binding);

            m_ActionServiceMock.GetActionList(Arg.Any<object>()).Returns(new ActionList { new Action.Action()});

            // Act
            m_CrossReferenceFinder.FindReferences<DependencyObject, Binding>(m_CrossReferenceContainer, button, null);

            m_CrossReferenceContainer.Received(1)
                .AddReference(Arg.Is<CrossReferenceItem>(y => y.TargetFullName == "MyButton" && y.TargetPropertyName == "Value" 
                    && y.SourceFullName == StringConstants.TagsRoot + "Tag1"));
        }

        [Test]
        public void OtherBindingsAreNotAddedToContainer()
        {
            var button = new Button() { Name = "MyButton" };
            var binding = new Binding("SomePath") { Source = new object() };
            BindingOperations.SetBinding(button, Button.ValueProperty, binding);

            m_CrossReferenceFinder.FindReferences<DependencyObject, Binding>(m_CrossReferenceContainer, button, null);

            m_CrossReferenceContainer.DidNotReceiveWithAnyArgs().AddReference(Arg.Any<CrossReferenceItem>());
        }
    }
}
