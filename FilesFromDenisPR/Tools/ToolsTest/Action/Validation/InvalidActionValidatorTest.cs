using System;
using System.Collections.Generic;
using Core.Api.CrossReference;
using Neo.ApplicationFramework.Common.Constants;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.Interfaces.Constants;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.CrossReference;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.Action.Validation
{
    [TestFixture]
    public class InvalidActionValidatorTest
    {
        private ICrossReferenceService m_CrossReferenceService;
        private InvalidActionValidator m_InvalidActionValidator;
        private IErrorListService m_ErrorListService;

        [SetUp]
        public void Setup()
        {
            m_ErrorListService = TestHelper.CreateAndAddServiceStub<IErrorListService>();
            m_CrossReferenceService = TestHelper.CreateAndAddServiceStub<ICrossReferenceService>();

            m_InvalidActionValidator = new InvalidActionValidator(m_CrossReferenceService.ToILazy());
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.ClearServices();
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void ValidateSomeInValidActions(int noOfInvalidActions)
        {
            //ARRANGE
            var invalidActions = GetInvalidActions(noOfInvalidActions);
            m_CrossReferenceService.GetReferences<IActionCrossReferenceItem>(ApplicationConstantsCF.InvalidActionCategoryName).Returns(invalidActions);
            

            //ACT
            bool isValid = m_InvalidActionValidator.Validate();

            //ASSERT
            Assert.That(isValid, Is.EqualTo((noOfInvalidActions <= 0)));
            m_ErrorListService.ReceivedWithAnyArgs(noOfInvalidActions).AddNewCompilerError(Arg.Any<string>(), Arg.Any<bool>());
        }

        [Test]
        public void ValidateOnlyUniqueErrorMessageIsShown()
        {
            //ARRANGE
            var invalidActions = GetInvalidActions(6, true);
            m_CrossReferenceService.GetReferences<IActionCrossReferenceItem>(ApplicationConstantsCF.InvalidActionCategoryName).Returns(invalidActions);

            //ACT
            bool isValid = m_InvalidActionValidator.Validate();

            //ASSERT
            Assert.IsFalse(isValid);
            m_ErrorListService.Received(3).AddNewCompilerError(Arg.Any<string>(), Arg.Any<bool>());
        }

        private IEnumerable<IActionCrossReferenceItem> GetInvalidActions(int count, bool includeDuplicates = false)
        {
            var actions = new List<IActionCrossReferenceItem>();

            for (int i = 0; i < count; i++)
            {
                var action = new ActionCrossReferenceItem
                {
                    ActionName = "MultiAction",
                    EventName = ActionConstants.ClickEvent,
                    TargetFullName = string.Format("Screen{0}.Control{0}", includeDuplicates ? i / 2 : i)
                };
                actions.Add(action);
            }

            return actions;
        }
    }
}
