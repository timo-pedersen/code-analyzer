using System;
using System.Drawing;
using System.Linq;
using Core.Api.GlobalReference;
using Core.Api.Service;
using Neo.ApplicationFramework.Interfaces;
using Neo.ApplicationFramework.TestUtilities;
using Neo.ApplicationFramework.Tools.Messagebox;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Tools.MessageBox
{
    [TestFixture]
    public class MessageBoxLayoutHelperTest
    {
        private const string m_VeryLongTextWithNoLineBreaks = "RecipeTitleRecipeTitleRecipeTitleRecipeTitleRecipeTitleRecipeTitleRecipeTitleRecipeTitleRecipeTitleRecipeTitleRecipeTitleRecipeTitleRecipeTitleRecipeTitleRecipeTitleRecipeTitleRecipeTitleRecipeTitleRecipeTitleRecipeTitle";
        private const string m_LorumIpsumLong = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris tempus dictum lacus et elementum. Curabitur sit amet porta quam. Sed quis purus non elit pellentesque feugiat. Integer id magna at justo feugiat sagittis dignissim id enim. In hac habitasse platea dictumst. Nunc gravida, justo ut laoreet aliquam, ante lacus vulputate erat, eget suscipit justo elit vel dui. Curabitur vitae volutpat ante. Duis dictum mollis tempus. Aliquam mauris ligula, egestas quis pellentesque ut, congue eu tellus. Nunc ut est ac elit viverra suscipit. Quisque vehicula sapien sed enim cursus vulputate commodo diam bibendum. In ligula mauris, fermentum quis tempus nec, dignissim at lorem. Mauris vestibulum fringilla dui iaculis malesuada. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Phasellus iaculis ullamcorper dui, vel viverra tortor auctor non.

Suspendisse fermentum dapibus adipiscing. Aliquam aliquam nibh sit amet turpis condimentum mattis hendrerit nibh iaculis. Phasellus hendrerit tincidunt placerat. Phasellus metus nisl, varius id venenatis id, dignissim eu tortor. Nam varius porttitor convallis. Suspendisse egestas, odio sed consectetur mollis, ante tortor mollis magna, in fringilla elit lorem eu ipsum. Donec a ligula lacus. Quisque sapien justo, consequat nec venenatis id, rutrum ut urna. Aenean sem lorem, malesuada nec aliquam eu, malesuada sed lacus. Proin gravida, felis hendrerit viverra lacinia, ante sapien porttitor nisi, varius volutpat sapien velit eu massa. Suspendisse potenti.

Mauris venenatis, nisl et pretium consectetur, risus ipsum euismod magna, vitae tempor magna dui non massa. In lobortis dapibus ante vitae condimentum. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Cras fringilla lacinia nunc, id imperdiet nunc pulvinar luctus. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce sed ante non nunc euismod iaculis placerat ac urna. Sed risus nibh, vehicula ac varius in, rhoncus ac justo. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae;

Ut ultrices magna quis nisi scelerisque ornare. Sed interdum, sapien ut varius pretium, nibh nunc accumsan justo, sit amet mattis elit neque faucibus ante. Nam eget ipsum sit amet lectus sodales euismod sed vehicula libero. Curabitur eu nisl non enim euismod pretium sed sed ante. Nam elementum placerat mi in euismod. Phasellus vel dolor arcu, sit amet blandit tellus. Donec volutpat leo vel lacus lobortis sagittis.

Etiam vel nisl vel neque semper mattis et dapibus quam. Ut accumsan ullamcorper metus, quis tincidunt justo consectetur vitae. Pellentesque mollis euismod ultrices. Aliquam a mauris et magna suscipit varius. Phasellus arcu quam, feugiat id ultricies a, ultrices non leo. Nullam volutpat, arcu nec tincidunt egestas, urna leo cursus ligula, tincidunt vulputate sem lectus in ipsum. Pellentesque eget molestie magna. Etiam felis augue, interdum vel sollicitudin quis, interdum eu neque. Duis sit amet elit velit, at feugiat turpis. Sed a lorem at nisl placerat convallis. Pellentesque at lorem ut eros auctor pellentesque. Duis tempus urna eu odio dignissim elementum. In ante tortor, vestibulum eget feugiat in, bibendum placerat mauris.

Sed feugiat mi a ligula bibendum non consectetur arcu viverra. Morbi sed mollis dui. Aenean porttitor ultrices risus pellentesque tincidunt. In pharetra nisl vel libero sollicitudin volutpat. Etiam blandit neque nec odio mollis molestie. Quisque sit amet posuere nulla. Nullam ut lacus ac nisi pulvinar interdum quis et nulla. Vestibulum pulvinar mauris sed dui porttitor laoreet. Nunc porttitor vehicula nisl nec interdum.

Sed sapien purus, bibendum eget porttitor eu, euismod ac velit. Proin pharetra tortor in ipsum luctus ornare. Duis facilisis tincidunt dignissim. Integer eu arcu eros. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Nunc imperdiet hendrerit augue in fringilla. Phasellus dictum fringilla urna at commodo. Praesent congue, odio sit amet tincidunt fermentum, erat. ";
        
        [SetUp]
        public void Setup()
        {
            ServiceContainerCF.Instance.AddService<INativeAPI>(new Neo.ApplicationFramework.Common.Utilities.NativeAPI()); 
            
            IGlobalReferenceService referenceServiceStub = TestHelper.CreateAndAddServiceStub<IGlobalReferenceService>();
            referenceServiceStub.GetObjects<IProjectConfiguration>().Returns(new IProjectConfiguration[0]);
        }

        [Test]
        public void TextWidthIsTruncatedWhenTextIsToLong()
        {
            MessageBoxLayoutHelper messageBoxLayoutHelper = new MessageBoxLayoutHelper();

            Size textSize = messageBoxLayoutHelper.MeasureMessageText(m_VeryLongTextWithNoLineBreaks, 350);

            Assert.LessOrEqual(textSize.Width, 350);
        }

        [Test]
        public void TextIsNotTruncatedWhenTextIsNotToLong()
        {
            MessageBoxLayoutHelper messageBoxLayoutHelper = new MessageBoxLayoutHelper();

            string modifiedText;
            string originalText = "RecipeTitle";
            Size textSize = messageBoxLayoutHelper.FitMessageTextToAvailableWidth(originalText, 350, "Tahoma", 14, out modifiedText);

            Assert.AreEqual(originalText, modifiedText);
        }

        [Test]
        public void TextIsNotTruncatedWhenTextContainsLineBreakAndFits()
        {
            MessageBoxLayoutHelper messageBoxLayoutHelper = new MessageBoxLayoutHelper();

            string modifiedText;
            string originalText = "RecipeTitle\nRecipeTitle\nRecipeTitle\n";
            Size textSize = messageBoxLayoutHelper.FitMessageTextToAvailableWidth(originalText, 140, "Tahoma", 14, out modifiedText);

            Assert.AreEqual(originalText, modifiedText);
        }

        [Test]
        public void TextIsNotDeletedWhenTextDoesNotFit()
        {
            MessageBoxLayoutHelper messageBoxLayoutHelper = new MessageBoxLayoutHelper();

            string modifiedText;
            string originalText = "RecipeTitleRecipeTitleRecipeTitle";
            Size textSize = messageBoxLayoutHelper.FitMessageTextToAvailableWidth(originalText, 10, "Tahoma", 14, out modifiedText);

            Assert.AreEqual(originalText, modifiedText);
        }

        [Test]
        public void CalculatedWindowsWidthIsNotBiggerThanScreenSize()
        {
            MessageBoxLayoutHelper messageBoxLayoutHelper = new MessageBoxLayoutHelper { MainScreenSize = new Size(640, 480), NumberOfButtons = 2 };
            Size calculatedSize = messageBoxLayoutHelper.CalculateWindowSize(m_VeryLongTextWithNoLineBreaks);

            Assert.LessOrEqual(calculatedSize.Width, 640);
        }


        [Test]
        public void CalculatedWindowsWidthIsNotBiggerThanScreenSizeWhenLongTitle()
        {
            MessageBoxLayoutHelper messageBoxLayoutHelper = new MessageBoxLayoutHelper { MainScreenSize = new Size(640, 480), NumberOfButtons = 2, Caption = m_VeryLongTextWithNoLineBreaks };
            Size calculatedSize = messageBoxLayoutHelper.CalculateWindowSize(m_VeryLongTextWithNoLineBreaks);

            Assert.LessOrEqual(calculatedSize.Width, 640);
        }

        [Test]
        public void CalculatedWindowsHeightIsNotBiggerThanScreenSize()
        {
            MessageBoxLayoutHelper messageBoxLayoutHelper = new MessageBoxLayoutHelper { MainScreenSize = new Size(640, 480), NumberOfButtons = 2, Caption = "Lorum Ipsum" };
            Size calculatedSize = messageBoxLayoutHelper.CalculateWindowSize(m_LorumIpsumLong);

            Assert.LessOrEqual(calculatedSize.Height, 480);

        }

        [Test]
        public void IfPossibleDoNotLineBreakWords()
        {
            MessageBoxLayoutHelper messageBoxLayoutHelper = new MessageBoxLayoutHelper { MainScreenSize = new Size(640, 480), NumberOfButtons = 2, Caption = "Lorum Ipsum" };
            Size calculatedSize = messageBoxLayoutHelper.CalculateWindowSize(m_LorumIpsumLong);

            char[] splitCharacters = new char[] { '\n', ' ', '\r' };
            string[] originalString = m_LorumIpsumLong.Split(splitCharacters, StringSplitOptions.RemoveEmptyEntries);
            string[] modifiedString = messageBoxLayoutHelper.ModifiedText.Split(splitCharacters, StringSplitOptions.RemoveEmptyEntries);

            Assert.AreEqual(originalString.Count(), modifiedString.Count());
        }
    }
}
