using System;
using System.Windows;
using System.Windows.Data;
using Core.Api.CrossReference;
using Core.Api.ProjectTarget;
using Neo.ApplicationFramework.Common.Utilities;
using Neo.ApplicationFramework.Controls;
using Neo.ApplicationFramework.Controls.OpcClient.Bindings;
using Neo.ApplicationFramework.Controls.Screen.Bindings;
using Neo.ApplicationFramework.Common.CrossReference;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.CrossReference.Finders;
using Neo.ApplicationFramework.Tools.Design.Bindings;
using Neo.ApplicationFramework.Tools.OpcClient.Bindings;
using NUnit.Framework;
using Rhino.Mocks;

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
            ITarget targetStub = MockRepository.GenerateStub<ITarget>();

            var terminalStub = MockRepository.GenerateStub<ITerminal>();
            var targetInfoStub = MockRepository.GenerateStub<ITargetInfo>();
            targetInfoStub.TerminalDescription = terminalStub;
            ITargetService targetServiceStub = TestHelper.AddServiceStub<ITargetService>();
            targetServiceStub.CurrentTarget = targetStub;
            targetServiceStub.Stub(x => x.CurrentTargetInfo).Return(targetInfoStub);

            m_CrossReferenceContainer = MockRepository.GenerateMock<ICrossReferenceContainerSignature>();
            TestHelper.Bindings.Wpf.RegisterSimpleDataItemBindingSourceProvider();

            m_ActionServiceMock = MockRepository.GenerateMock<IActionService>();
            m_CrossReferenceFinder = new DataItemCrossReferenceFinder(m_ActionServiceMock.ToILazy());
        }

        [TearDown]
        public void TearDown()
        {
            m_CrossReferenceContainer.VerifyAllExpectations();
            TestHelper.ClearServices();
            TestHelper.Bindings.Wpf.ClearProviders();
        }

        [Test]
        public void AliasBindingsAreNotAddedToContainer()
        {
            // Arrange

            var bindingSourceDescription = new LocalPropertyBindingSourceDescription("MyAlias");

            m_CrossReferenceContainer
                .Expect(x => x.AddReference(new CrossReferenceItem(null, null, null)))
                .IgnoreArguments()
                .Repeat.Never();

            var button = new Button() { Name = "MyButton" };
            var binding = new LocalPropertyBindingProvider().ProvideWpfBinding(bindingSourceDescription);
            BindingOperations.SetBinding(button, Button.ValueProperty, binding);

            // Act
            m_CrossReferenceFinder.FindReferences<DependencyObject, Binding>(m_CrossReferenceContainer, button, null);
        }

        [Test]
        public void DataItemBindingsAreAddedToContainer()
        {
            // Arrange
            var bindingSourceDescription = DataItemBindingSourceDescription.Create(StringConstants.TagsRoot + "Tag1");

            m_CrossReferenceContainer.Expect(
                x => x.AddReference(
                    Arg<CrossReferenceItem>.Matches(
                        y => y.TargetFullName == "MyButton" && y.TargetPropertyName == "Value" && y.SourceFullName == StringConstants.TagsRoot + "Tag1"
                    )
                )
            ).Repeat.Once();

            var button = new Button() { Name = "MyButton" };
            var binding = new TagBindingProvider().ProvideWpfBinding(bindingSourceDescription);
            BindingOperations.SetBinding(button, Button.ValueProperty, binding);

            m_ActionServiceMock.Stub(x => x.GetActionList(Arg<object>.Is.Anything)).Return(new ActionList { new Action.Action()});

            // Act
            m_CrossReferenceFinder.FindReferences<DependencyObject, Binding>(m_CrossReferenceContainer, button, null);
        }

        [Test]
        public void OtherBindingsAreNotAddedToContainer()
        {
            m_CrossReferenceContainer
                .Expect(x => x.AddReference(new CrossReferenceItem(null, null, null)))
                .IgnoreArguments()
                .Repeat.Never();

            var button = new Button() { Name = "MyButton" };
            var binding = new Binding("SomePath") { Source = new object() };
            BindingOperations.SetBinding(button, Button.ValueProperty, binding);

            m_CrossReferenceFinder.FindReferences<DependencyObject, Binding>(m_CrossReferenceContainer, button, null);
        }
    }
}
