using Core.Api.GlobalReference;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;
using TestHelper = Neo.ApplicationFramework.TestUtilities.TestHelper;

namespace Neo.ApplicationFramework.Common.Data
{
    [TestFixture]
    public class DataItemProxyFactoryTest
    {
        [SetUp]
        public void SetUp()
        {
            TestHelper.UseTestWindowThreadHelper = true;
            TestHelper.CreateAndAddServiceStub<IGlobalReferenceService>();
        }

        [Test]
        public void GetProxyForNonExistingDataItemWhenValidating()
        {
            IDataItemProxy dataItemProxy = DataItemProxyFactory.Instance["Controller1.D0"];

            Assert.IsNull(dataItemProxy);
        }

        [Test]
        public void GetProxyForNonExistingDataItemWhenNotValidating()
        {
            NeoDesignerProperties.IsInDesignMode = true;

            IDataItemProxy dataItemProxy = DataItemProxyFactory.Instance["Controller1.D0"];

            Assert.IsNotNull(dataItemProxy);
        }
    }
}