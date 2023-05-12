using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Rhino.Mocks;

namespace TestProject.Tests
{
    [TestFixture]
    public class Test1
    {
        [SetUp]
        public void SetUp()
        {
            var Class2Mock = MockRepository.GenerateStub<Class2>();
        }
    }
}
