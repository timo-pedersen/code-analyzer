using NUnit.Framework;
using Rhino.Mocks;

namespace TestProject.Tests;

[TestFixture]
public class Test1
{
    private IClass1 Sut;

    [SetUp]
    public void SetUp()
    {
        var class2Mock = MockRepository.GenerateStub<Class2>();
        Sut = new Class1(class2Mock);
    }

    [Test]
    void ThisIsATest()
    {
        int a = Sut.AddTwoNumbers(2, 3);
        Assert.AreEqual(5, a);
    }
}

