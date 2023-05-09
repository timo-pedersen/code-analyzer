using System;
using System.CodeDom;
using Core.Api.Platform;
using Core.Api.ProjectTarget;
using Core.Component.Api.CodeGeneration;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Design;
using Neo.ApplicationFramework.Tools.Design.CodeGeneration;
using Neo.ApplicationFramework.Tools.EventMapper;
using Neo.ApplicationFramework.Tools.OpcClient;
using NUnit.Framework;
using Rhino.Mocks;

namespace Neo.ApplicationFramework.Tools.Action
{
    [TestFixture]
    public class CodeGeneratedActionParameterInfoPositionTest : CodeDomTestHelper
    {
        private const string MethodName = "MethodName";
        private const string FirstParameterName = "First";
        private const string LastParameterName = "Last";
        private const string SecondParameterName = "Second";
        private readonly Action m_Action = new Action("ActionName");


        [SetUp]
        public void SetUp()
        {
            CodeTypeDeclaration = new CodeTypeDeclaration("CodeGeneratedActionParameterInfoPositionTest");
            CompileUnitGenerator = new CompileUnitGenerator(BeijerNamespace, CodeTypeDeclaration, new object());
            CodeGenerationHelper = CodeGenerationHelperFactory.Create(CompileUnitGenerator);

            var target = new Target(TargetPlatform.Windows, string.Empty, string.Empty);
            var targetService = MockRepository.GenerateMock<ITargetService>();
            targetService.Stub(x => x.CurrentTarget).Return(target);
            TestHelper.AddService(targetService);
            TestHelper.AddService<IEventMapperService>(new EventMapperService(targetService.ToILazy()));

            m_Action.ActionMethodInfo = new ActionMethodInfo("ValueChanged")
            {
                ObjectName = "SomeObjectName",
                EventName = "ValueChanged",
                ActionParameterInfo = new ActionParameterInfoList()
            };
        }

        [Test]
        public void OrderedByRandomPositions()
        {
            AddActionParametersString(
                    new ActionParameterInfo
                    {
                        ParameterName = LastParameterName,
                        Position = 2
                    },

                    new ActionParameterInfo
                    {
                        ParameterName = SecondParameterName,
                        Position = 1
                    },

                    new ActionParameterInfo
                    {
                        ParameterName = FirstParameterName,
                        Position = 0
                    }
                );

            CodeMethodInvokeExpression method  = FindMethodInvokeExpression(FindMethodStartsWith(MethodName).Statements, m_Action.ActionMethodInfo.Name);
            Assert.IsNotNull(method);
            Assert.IsNotNull(method.Parameters);

            Assert.AreEqual(FirstParameterName, GetParameter(method, 0));
            Assert.AreEqual(SecondParameterName, GetParameter(method, 1));
            Assert.AreEqual(LastParameterName, GetParameter(method, 2));

        }

        [Test]
        public void OrderedPositionEmpty()
        {
            AddActionParametersString(
                new ActionParameterInfo
                {
                    ParameterName = FirstParameterName,
                },

                new ActionParameterInfo
                {
                    ParameterName = SecondParameterName,
                },

                new ActionParameterInfo
                {
                    ParameterName = LastParameterName,
                }
            );

            CodeMethodInvokeExpression method = FindMethodInvokeExpression(FindMethodStartsWith(MethodName).Statements, m_Action.ActionMethodInfo.Name);
            Assert.IsNotNull(method);
            Assert.IsNotNull(method.Parameters);

            Assert.AreEqual(FirstParameterName, GetParameter(method, 0));
            Assert.AreEqual(SecondParameterName, GetParameter(method, 1));
            Assert.AreEqual(LastParameterName, GetParameter(method, 2));
        }

        [Test]
        public void OrderedPositionNull()
        {
            AddActionParametersString(
                new ActionParameterInfo
                {
                    ParameterName = FirstParameterName,
                    Position = null
                },

                new ActionParameterInfo
                {
                    ParameterName = SecondParameterName,
                    Position = null
                },

                new ActionParameterInfo
                {
                    ParameterName = LastParameterName,
                    Position = null
                }
            );

            CodeMethodInvokeExpression method = FindMethodInvokeExpression(FindMethodStartsWith(MethodName).Statements, m_Action.ActionMethodInfo.Name);
            Assert.IsNotNull(method);
            Assert.IsNotNull(method.Parameters);

            Assert.AreEqual(FirstParameterName, GetParameter(method, 0));
            Assert.AreEqual(SecondParameterName, GetParameter(method, 1));
            Assert.AreEqual(LastParameterName, GetParameter(method, 2));
        }

        [Test]
        public void OrderAllEqualPositions()
        {
            AddActionParametersString(
                new ActionParameterInfo
                {
                    ParameterName = SecondParameterName,
                    Position = 0
                },

                new ActionParameterInfo
                {
                    ParameterName = FirstParameterName,
                    Position = 0
                },

                new ActionParameterInfo
                {
                    ParameterName = LastParameterName,
                    Position = 0
                }
            );

            CodeMethodInvokeExpression method = FindMethodInvokeExpression(FindMethodStartsWith(MethodName).Statements, m_Action.ActionMethodInfo.Name);
            Assert.IsNotNull(method);
            Assert.IsNotNull(method.Parameters);

            Assert.AreEqual(SecondParameterName, GetParameter(method, 0));
            Assert.AreEqual(FirstParameterName, GetParameter(method, 1));
            Assert.AreEqual(LastParameterName, GetParameter(method, 2));
        }

        [Test]
        public void OrderFirstPositionSet()
        {
            AddActionParametersString(
                new ActionParameterInfo
                {
                    ParameterName = SecondParameterName
                },

                new ActionParameterInfo
                {
                    ParameterName = FirstParameterName,
                    Position = 0
                },

                new ActionParameterInfo
                {
                    ParameterName = LastParameterName
                }
            );

            CodeMethodInvokeExpression method = FindMethodInvokeExpression(FindMethodStartsWith(MethodName).Statements, m_Action.ActionMethodInfo.Name);
            Assert.IsNotNull(method);
            Assert.IsNotNull(method.Parameters);

            Assert.AreEqual(FirstParameterName, GetParameter(method, 0));
            Assert.AreEqual(SecondParameterName, GetParameter(method, 1));
            Assert.AreEqual(LastParameterName, GetParameter(method, 2));
        }

        [Test]
        public void OrderSecondPosition()
        {
            AddActionParametersString(
                new ActionParameterInfo
                {
                    ParameterName = SecondParameterName,
                    Position = 1
                },

                new ActionParameterInfo
                {
                    ParameterName = FirstParameterName
                },

                new ActionParameterInfo
                {
                    ParameterName = LastParameterName
                }
            );

            CodeMethodInvokeExpression method = FindMethodInvokeExpression(FindMethodStartsWith(MethodName).Statements, m_Action.ActionMethodInfo.Name);
            Assert.IsNotNull(method);
            Assert.IsNotNull(method.Parameters);

            Assert.AreEqual(FirstParameterName, GetParameter(method, 0));
            Assert.AreEqual(SecondParameterName, GetParameter(method, 1));
            Assert.AreEqual(LastParameterName, GetParameter(method, 2));
        }

        [Test]
        public void OrderLastPosition()
        {
            AddActionParametersString(
                new ActionParameterInfo
                {
                    ParameterName = FirstParameterName,
                },

                new ActionParameterInfo
                {
                    ParameterName = LastParameterName,
                    Position = 2
                },

                new ActionParameterInfo
                {
                    ParameterName = SecondParameterName,
                }
            );

            CodeMethodInvokeExpression method = FindMethodInvokeExpression(FindMethodStartsWith(MethodName).Statements, m_Action.ActionMethodInfo.Name);
            Assert.IsNotNull(method);
            Assert.IsNotNull(method.Parameters);

            Assert.AreEqual(FirstParameterName, GetParameter(method, 0));
            Assert.AreEqual(SecondParameterName, GetParameter(method, 1));
            Assert.AreEqual(LastParameterName, GetParameter(method, 2));
        }

        [Test]
        public void OrderFirstTwoPositionSet()
        {
            AddActionParametersString(
                new ActionParameterInfo
                {
                    ParameterName = SecondParameterName,
                    Position = 1
                },

                new ActionParameterInfo
                {
                    ParameterName = FirstParameterName,
                    Position = 0
                },

                new ActionParameterInfo
                {
                    ParameterName = LastParameterName,
                }
            );

            CodeMethodInvokeExpression method = FindMethodInvokeExpression(FindMethodStartsWith(MethodName).Statements, m_Action.ActionMethodInfo.Name);
            Assert.IsNotNull(method);
            Assert.IsNotNull(method.Parameters);

            Assert.AreEqual(FirstParameterName, GetParameter(method, 0));
            Assert.AreEqual(SecondParameterName, GetParameter(method, 1));
            Assert.AreEqual(LastParameterName, GetParameter(method, 2));
        }

        [Test]
        public void OrderLastTwoPositionSet()
        {
            AddActionParametersString(
                new ActionParameterInfo
                {
                    ParameterName = SecondParameterName,
                    Position = 1
                },

                new ActionParameterInfo
                {
                    ParameterName = FirstParameterName,
                },

                new ActionParameterInfo
                {
                    ParameterName = LastParameterName,
                    Position = 2
                }
            );

            CodeMethodInvokeExpression method = FindMethodInvokeExpression(FindMethodStartsWith(MethodName).Statements, m_Action.ActionMethodInfo.Name);
            Assert.IsNotNull(method);
            Assert.IsNotNull(method.Parameters);

            Assert.AreEqual(FirstParameterName, GetParameter(method, 0));
            Assert.AreEqual(SecondParameterName, GetParameter(method, 1));
            Assert.AreEqual(LastParameterName, GetParameter(method, 2));
        }

        [Test]
        public void OrderWithTwoEqualPositions()
        {
            AddActionParametersString(
                new ActionParameterInfo
                {
                    ParameterName = SecondParameterName,
                    Position = 0
                },

                new ActionParameterInfo
                {
                    ParameterName = FirstParameterName,
                    Position = 0
                },

                new ActionParameterInfo
                {
                    ParameterName = LastParameterName,
                }
            );

            CodeMethodInvokeExpression method = FindMethodInvokeExpression(FindMethodStartsWith(MethodName).Statements, m_Action.ActionMethodInfo.Name);
            Assert.IsNotNull(method);
            Assert.IsNotNull(method.Parameters);

            Assert.AreEqual(SecondParameterName, GetParameter(method, 0));
            Assert.AreEqual(FirstParameterName, GetParameter(method, 1));
            Assert.AreEqual(LastParameterName, GetParameter(method, 2));
        }

        [Test]
        public void OrderPositionNoBiggerThanListExceptionThrown()
        {
            Assert.Catch<ArgumentException>(
                () =>
                {
                    AddActionParametersString(
                        new ActionParameterInfo
                        {
                            ParameterName = LastParameterName,
                            Position = 3
                        },

                        new ActionParameterInfo
                        {
                            ParameterName = FirstParameterName
                        }
                    );
                });
        }


        [Test]
        public void OrderPositionIsNegativeDoublePositions()
        {
            Assert.Catch<ArgumentException>(
                () =>
                {
                    AddActionParametersString(
                        new ActionParameterInfo
                        {
                            ParameterName = LastParameterName,
                            Position = -5
                        },

                        new ActionParameterInfo
                        {
                            ParameterName = SecondParameterName,
                            Position = -5
                        },

                        new ActionParameterInfo
                        {
                            ParameterName = FirstParameterName
                        }
                    );
                });
        }

        [Test]
        public void InvalidActionNotAdded()
        {
            m_Action.ActionMethodInfo.ActionParameterInfo.Clear();

            m_Action.ActionMethodInfo.ActionParameterInfo.Add(new ActionParameterInfo
            {
                ParameterName = SecondParameterName,
                Value = null,
                Position = 0,
                ParameterType = typeof(System.Drawing.Point)
            });

            CodeGenerationHelper.AddActions(MethodName, new GlobalDataItem(), new ActionList { m_Action });

            CodeMethodInvokeExpression method = FindMethodInvokeExpression(FindMethodStartsWith(MethodName).Statements, m_Action.ActionMethodInfo.Name);
            Assert.IsNotNull(method);
            Assert.IsEmpty(method.Parameters);
        }

        [Test]
        public void InvalidActionValidAdded()
        {
            m_Action.ActionMethodInfo.ActionParameterInfo.Clear();

            m_Action.ActionMethodInfo.ActionParameterInfo.Add(new ActionParameterInfo
            {
                ParameterName = SecondParameterName,
                Value = null,
                Position = 0,
                ParameterType = typeof(System.Drawing.Point)
            });

            m_Action.ActionMethodInfo.ActionParameterInfo.Add(new ActionParameterInfo
            {
                ParameterName = FirstParameterName,
                Value = FirstParameterName,
                Position = 1,
                ParameterType = typeof(string)
            });

            CodeGenerationHelper.AddActions(MethodName, new GlobalDataItem(), new ActionList { m_Action });

            CodeMethodInvokeExpression method = FindMethodInvokeExpression(FindMethodStartsWith(MethodName).Statements, m_Action.ActionMethodInfo.Name);
            Assert.IsNotNull(method);
            Assert.IsNotNull(method.Parameters);

            Assert.AreEqual(FirstParameterName, GetParameter(method, 0));
        }


        #region Helper

        private void AddActionParametersString(params ActionParameterInfo[] infos)
        {
            m_Action.ActionMethodInfo.ActionParameterInfo.Clear();

            foreach (ActionParameterInfo actionParameterInfo in infos)
            {
                actionParameterInfo.ParameterType = typeof(string);
                actionParameterInfo.Value = actionParameterInfo.ParameterName;
                m_Action.ActionMethodInfo.ActionParameterInfo.Add(actionParameterInfo);
            }

            CodeGenerationHelper.AddActions(MethodName, new GlobalDataItem(), new ActionList { m_Action });
        }

        private string GetParameter(CodeMethodInvokeExpression method, int index)
        {
            return (method.Parameters[index] as CodePrimitiveExpression)?.Value as string;
        }

        #endregion
    }
}
