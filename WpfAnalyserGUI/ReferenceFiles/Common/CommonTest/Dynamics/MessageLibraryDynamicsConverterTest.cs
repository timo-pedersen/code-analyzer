using System;
using Core.Controls.Api.Bindings.PropertyBinders;
using Core.Controls.Api.Designer;
using Neo.ApplicationFramework.Tools.MessageLibrary;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Common.Dynamics
{
    [TestFixture]
    public class MessageLibraryDynamicsConverterTest
    {
        private const string Text1800To1899 = "19th Century";
        private const string Text1900To1999 = "20th Century";
        private const string Text2000To2099 = "21st Century";

        private MessageLibraryDynamicsConverterCF m_MessageLibraryDynamicsConverter;
        private IValueConverterCF m_ValueConverter;
        private MessageGroup m_MessageGroup;

        [SetUp]
        public void SetUp()
        {
            NeoDesignerProperties.IsInDesignMode = true;

            m_MessageGroup = new MessageGroup();
            m_MessageGroup.Name = "Group name";
            m_MessageGroup.Messages.Add(new MessageItem() { Message = Text1800To1899, StartValue = 1800, EndValue = 1899 });
            m_MessageGroup.Messages.Add(new MessageItem() { Message = Text1900To1999, StartValue = 1900, EndValue = 1999 });
            m_MessageGroup.Messages.Add(new MessageItem() { Message = Text2000To2099, StartValue = 2000, EndValue = 2099 });

            var groupResolver = Substitute.For<MessageLibraryGroupResolver>();
            groupResolver.GetGroup(Arg.Any<string>()).ReturnsForAnyArgs(m_MessageGroup);

            m_MessageLibraryDynamicsConverter = new MessageLibraryDynamicsConverterCF(m_MessageGroup.Name);
            m_MessageLibraryDynamicsConverter.MessageLibraryGroupResolver = groupResolver;
            m_ValueConverter = m_MessageLibraryDynamicsConverter;
        }

        [TearDown]
        public void TearDown()
        {
            NeoDesignerProperties.IsInDesignMode = false;
        }

        [Test]
        public void ConvertingValueInIntervalReturnsThatIntervalsString()
        {
            var result = m_ValueConverter.Convert(1977, typeof(string), null, null);

            Assert.IsInstanceOf<string>(result);
            Assert.AreEqual(Text1900To1999, result);
        }


        [Test]
        public void ConvertingValueWhereValueIsBelowRangeReturnsEmptyString()
        {
            var result = m_ValueConverter.Convert(1492, typeof(string), null, null);

            Assert.IsInstanceOf<string>(result);
            Assert.AreEqual(Text1800To1899, result);
        }

        [Test]
        public void ConvertingValueWhereValueExceedsRangeReturnsEmptyString()
        {
            var result = m_ValueConverter.Convert(2121, typeof(string), null, null);

            Assert.IsInstanceOf<string>(result);
            Assert.AreEqual(Text1800To1899, result);
        }

        [Test]
        public void ConvertingANonDoubleThrowsArgumentException()
        {
            IValueConverterCF valueConverter = m_MessageLibraryDynamicsConverter;

            Assert.Throws<ArgumentException>(() => valueConverter.Convert("Moi mukkulat!", typeof(string), null, null));
        }

        [Test]
        public void ConvertingANullReturnsNull()
        {
            IValueConverterCF valueConverter = m_MessageLibraryDynamicsConverter;

            object returnValue = valueConverter.Convert(null, typeof(string), null, null);
            Assert.IsNull(returnValue);
        }

        [Test]
        public void ConvertingToNonStringThrowsNotSupportedException()
        {
            IValueConverterCF valueConverter = m_MessageLibraryDynamicsConverter;

            Assert.Throws<NotSupportedException>(() => valueConverter.Convert(0, typeof(DateTime), null, null));
        }

    }
}
