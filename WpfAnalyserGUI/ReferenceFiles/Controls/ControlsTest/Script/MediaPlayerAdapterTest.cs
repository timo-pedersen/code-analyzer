using Neo.ApplicationFramework.Controls.Media;
using NSubstitute;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.Script
{
    [TestFixture]
    public class MediaPlayerAdapterTest
    {
        [Test]
        public void SourceIsNullAfterSetFirstTime()
        {
            MediaPlayer mediaPlayer = new MediaPlayer();
            MediaPlayerAdapter mediaPlayerAdapter = new MediaPlayerAdapter();

            mediaPlayerAdapter.AdaptedObject = mediaPlayer;

            mediaPlayerAdapter.Source = null;

            Assert.IsNull(mediaPlayerAdapter.Source);
        }

        // This is not a valid test nowadays. MediaPlayerAdapter is only used in runtime. 
        // Therefore is a source only set if it can be found on the computer, CE panel or EPC.
        // Either the correct path has to be found,
        // or must the file be possible to find in the project files folder. 
        // If the file can't be found, no source is set.
        // The functionality is a lot harder to test this way, though ...

        //[Test]
        //public void SourceIsCorrectAfterSetFirstTime()
        //{
        //    string source = @"c:\SomeMovie.avi";
        //    MediaPlayer mediaPlayer = new MediaPlayer();
        //    MediaPlayerAdapter mediaPlayerAdapter = new MediaPlayerAdapter();

        //    mediaPlayerAdapter.AdaptedObject = mediaPlayer;

        //    mediaPlayerAdapter.Source = source;

        //    Assert.AreEqual(source, mediaPlayerAdapter.Source);
        //}

        [Test]
        public void SourceIsNotSetWhenSettingSameAsAlreadySet()
        {
            string source = @"c:\SomeMovie.avi";
            MediaPlayer mediaPlayer = Substitute.For<MediaPlayer>();
            MediaPlayerAdapter mediaPlayerAdapter = new MediaPlayerAdapter();

            mediaPlayerAdapter.AdaptedObject = mediaPlayer;

            //Set up getter to return same value as we are setting later
            mediaPlayer.Source.Returns(source);

            mediaPlayerAdapter.Source = source;

            Assert.IsNotNull(mediaPlayer.Source);
        }
    }
}
