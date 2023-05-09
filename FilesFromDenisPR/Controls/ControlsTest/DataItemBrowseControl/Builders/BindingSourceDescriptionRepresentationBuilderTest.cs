#if !VNEXT_TARGET
using System.Collections.Generic;
using System.Linq;
using Core.Controls.Api.Bindings.DataSources;
using Neo.ApplicationFramework.Interfaces;
using NUnit.Framework;

namespace Neo.ApplicationFramework.Controls.DataItemBrowseControl.Builders
{
    [TestFixture]
    public class BindingSourceDescriptionRepresentationBuilderTest
    {
        [Test]
        public void TagsAsList()
        {
            // ARRANGE
            BindingSourceDescription tag1 = CreateBindingSourceDescription("Tag3", "Tag3", StringConstants.TagsRoot + "Tag3");
            BindingSourceDescription tag2 = CreateBindingSourceDescription("Tag1", "Tag1", StringConstants.TagsRoot + "Tag1");
            BindingSourceDescription tag3 = CreateBindingSourceDescription("Tag2", "Tag2", StringConstants.TagsRoot + "Tag2");

            var builder = new BindingSourceDescriptionRepresentationBuilder();

            // ACT
            List<BindingSourceDescriptionViewModelBase> result = builder
                .BuildListAsync(new[] { tag1, tag2, tag3 }).Result
                .ToList();

            // ASSERT, should be a flat list of tags
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.ElementAt(0), Is.TypeOf<BindingSourceDescriptionViewModel>());
            Assert.That(result.ElementAt(0).Name, Is.EqualTo("Tag1"));
            Assert.That(result.ElementAt(1), Is.TypeOf<BindingSourceDescriptionViewModel>());
            Assert.That(result.ElementAt(1).Name, Is.EqualTo("Tag2"));
            Assert.That(result.ElementAt(2), Is.TypeOf<BindingSourceDescriptionViewModel>());
            Assert.That(result.ElementAt(2).Name, Is.EqualTo("Tag3"));
        }

        [Test]
        public void TagsAsTree()
        {
            // ARRANGE
            BindingSourceDescription tag1 = CreateBindingSourceDescription("Tag3", "Tag3", StringConstants.TagsRoot + "Tag3");
            BindingSourceDescription tag2 = CreateBindingSourceDescription("Tag1", "Tag1", StringConstants.TagsRoot + "Tag1");
            BindingSourceDescription tag3 = CreateBindingSourceDescription("Tag2", "Tag2", StringConstants.TagsRoot + "Tag2");

            var builder = new BindingSourceDescriptionRepresentationBuilder();

            // ACT
            List<BindingSourceDescriptionViewModelBase> result = builder
                .BuildTreeAsync(new[] { tag1, tag2, tag3 }).Result
                .ToList();

            // ASSERT, should be a flat list of tags
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.ElementAt(0), Is.TypeOf<HierarchicalBindingSourceDescriptionViewModel>());
            Assert.That(result.ElementAt(0).Name, Is.EqualTo("Tag1"));
            Assert.That(result.ElementAt(1), Is.TypeOf<HierarchicalBindingSourceDescriptionViewModel>());
            Assert.That(result.ElementAt(1).Name, Is.EqualTo("Tag2"));
            Assert.That(result.ElementAt(2), Is.TypeOf<HierarchicalBindingSourceDescriptionViewModel>());
            Assert.That(result.ElementAt(2).Name, Is.EqualTo("Tag3"));
        }

        [Test]
        public void StructuredTagsAsList()
        {
            // ARRANGE
            BindingSourceDescription saw1BeepNumber = CreateBindingSourceDescription("Saw1.BeepNumber", "Saw1.BeepNumber", StringConstants.TagsRoot + "Saw1.BeepNumber");
            BindingSourceDescription saw1MotorSpeed = CreateBindingSourceDescription("Saw1.Motor.Speed", "Saw1.Motor.Speed", StringConstants.TagsRoot + "Saw1.Motor.Speed");
            BindingSourceDescription saw1MotorAcceleration = CreateBindingSourceDescription("Saw1.Motor.Acceleration", "Saw1.Motor.Acceleration", StringConstants.TagsRoot + "Saw1.Motor.Acceleration");
            BindingSourceDescription saw2BeepNumber = CreateBindingSourceDescription("Saw2.BeepNumber", "Saw2.BeepNumber", StringConstants.TagsRoot + "Saw2.BeepNumber");
            BindingSourceDescription saw2MotorSpeed = CreateBindingSourceDescription("Saw2.Motor.Speed", "Saw2.Motor.Speed", StringConstants.TagsRoot + "Saw2.Motor.Speed");
            BindingSourceDescription saw2MotorAcceleration = CreateBindingSourceDescription("Saw2.Motor.Acceleration", "Saw2.Motor.Acceleration", StringConstants.TagsRoot + "Saw2.Motor.Acceleration");

            var builder = new BindingSourceDescriptionRepresentationBuilder();

            // ACT
            List<BindingSourceDescriptionViewModelBase> result = builder
                .BuildListAsync(new[] { saw1BeepNumber, saw1MotorSpeed, saw1MotorAcceleration, saw2BeepNumber, saw2MotorSpeed, saw2MotorAcceleration }).Result
                .ToList();

            // ASSERT, should be a flat list of structured tags
            Assert.That(result.Count, Is.EqualTo(6));
            Assert.That(result.ElementAt(0), Is.TypeOf<BindingSourceDescriptionViewModel>());
            Assert.That(result.ElementAt(0).Name, Is.EqualTo("Saw1.BeepNumber"));
            Assert.That(result.ElementAt(1), Is.TypeOf<BindingSourceDescriptionViewModel>());
            Assert.That(result.ElementAt(1).Name, Is.EqualTo("Saw1.Motor.Acceleration"));
            Assert.That(result.ElementAt(2), Is.TypeOf<BindingSourceDescriptionViewModel>());
            Assert.That(result.ElementAt(2).Name, Is.EqualTo("Saw1.Motor.Speed"));
            Assert.That(result.ElementAt(3), Is.TypeOf<BindingSourceDescriptionViewModel>());
            Assert.That(result.ElementAt(3).Name, Is.EqualTo("Saw2.BeepNumber"));
            Assert.That(result.ElementAt(4), Is.TypeOf<BindingSourceDescriptionViewModel>());
            Assert.That(result.ElementAt(4).Name, Is.EqualTo("Saw2.Motor.Acceleration"));
            Assert.That(result.ElementAt(5), Is.TypeOf<BindingSourceDescriptionViewModel>());
            Assert.That(result.ElementAt(5).Name, Is.EqualTo("Saw2.Motor.Speed"));
        }

        [Test]
        public void StructuredTagsAsTree()
        {
            // ARRANGE
            BindingSourceDescription saw1 = CreateBindingSourceDescription("Saw1", "Saw1", StringConstants.TagsRoot + "Saw1");
            BindingSourceDescription saw1BeepNumber = CreateBindingSourceDescription("Saw1.BeepNumber", "Saw1.BeepNumber", StringConstants.TagsRoot + "Saw1.BeepNumber");
            BindingSourceDescription saw1Motor = CreateBindingSourceDescription("Saw1.Motor", "Saw1.Motor", StringConstants.TagsRoot + "Saw1.Motor");
            BindingSourceDescription saw1MotorAcceleration = CreateBindingSourceDescription("Saw1.Motor.Acceleration", "Saw1.Motor.Acceleration", StringConstants.TagsRoot + "Saw1.Motor.Acceleration");
            BindingSourceDescription saw1MotorSpeed = CreateBindingSourceDescription("Saw1.Motor.Speed", "Saw1.Motor.Speed", StringConstants.TagsRoot + "Saw1.Motor.Speed");
            BindingSourceDescription saw2 = CreateBindingSourceDescription("Saw2", "Saw2", StringConstants.TagsRoot + "Saw2");
            BindingSourceDescription saw2BeepNumber = CreateBindingSourceDescription("Saw2.BeepNumber", "Saw2.BeepNumber", StringConstants.TagsRoot + "Saw2.BeepNumber");
            BindingSourceDescription saw2Motor = CreateBindingSourceDescription("Saw2.Motor", "Saw2.Motor", StringConstants.TagsRoot + "Saw2.Motor");
            BindingSourceDescription saw2MotorAcceleration = CreateBindingSourceDescription("Saw2.Motor.Acceleration", "Saw2.Motor.Acceleration", StringConstants.TagsRoot + "Saw2.Motor.Acceleration");
            BindingSourceDescription saw2MotorSpeed = CreateBindingSourceDescription("Saw2.Motor.Speed", "Saw2.Motor.Speed", StringConstants.TagsRoot + "Saw2.Motor.Speed");

            var builder = new BindingSourceDescriptionRepresentationBuilder();

            // ACT
            List<BindingSourceDescriptionViewModelBase> result = builder
                .BuildTreeAsync(new[]
                {
                    saw1, saw1BeepNumber, saw1Motor, saw1MotorAcceleration, saw1MotorSpeed,
                    saw2, saw2BeepNumber, saw2Motor, saw2MotorAcceleration, saw2MotorSpeed
                })
                .Result
                .ToList();

            // ASSERT, structure should be like this:
            //
            //         + Saw1
            //            - BeepNumber
            //            + Motor
            //               - Acceleration
            //               - Speed
            //         + Saw2
            //            - BeepNumber
            //            + Motor
            //               - Acceleration
            //               - Speed
            Assert.That(result.Count, Is.EqualTo(2));

            // Saw1
            var expectedSaw1 = result.ElementAt(0) as HierarchicalBindingSourceDescriptionViewModel;
            Assert.That(expectedSaw1, Is.Not.Null);
            Assert.That(expectedSaw1.Name, Is.EqualTo("Saw1"));
            Assert.That(expectedSaw1.Children.Count, Is.EqualTo(2));

            // Saw1.BeepNumber
            var expectedSaw1BeepNumber = expectedSaw1.Children.ElementAt(0);
            Assert.That(expectedSaw1BeepNumber, Is.Not.Null);
            Assert.That(expectedSaw1BeepNumber.Name, Is.EqualTo("BeepNumber"));

            // Saw1.Motor
            var expectedSaw1Motor = expectedSaw1.Children.ElementAt(1);
            Assert.That(expectedSaw1Motor, Is.Not.Null);
            Assert.That(expectedSaw1Motor.Name, Is.EqualTo("Motor"));
            Assert.That(expectedSaw1Motor.Children.Count, Is.EqualTo(2));

            // Saw1.Motor.Acceleration
            var expectedSaw1MotorAcceleration = expectedSaw1Motor.Children.ElementAt(0);
            Assert.That(expectedSaw1MotorAcceleration, Is.Not.Null);
            Assert.That(expectedSaw1MotorAcceleration.Name, Is.EqualTo("Acceleration"));

            // Saw1.Motor.Speed
            var expectedSaw1MotorSpeed = expectedSaw1Motor.Children.ElementAt(1);
            Assert.That(expectedSaw1MotorSpeed, Is.Not.Null);
            Assert.That(expectedSaw1MotorSpeed.Name, Is.EqualTo("Speed"));

            // Saw2
            var expectedSaw2 = result.ElementAt(1) as HierarchicalBindingSourceDescriptionViewModel;
            Assert.That(expectedSaw2, Is.Not.Null);
            Assert.That(expectedSaw2.Name, Is.EqualTo("Saw2"));
            Assert.That(expectedSaw2.Children.Count, Is.EqualTo(2));

            // Saw2.BeepNumber
            var expectedSaw2BeepNumber = expectedSaw2.Children.ElementAt(0);
            Assert.That(expectedSaw2BeepNumber, Is.Not.Null);
            Assert.That(expectedSaw2BeepNumber.Name, Is.EqualTo("BeepNumber"));

            // Saw2.Motor
            var expectedSaw2Motor = expectedSaw2.Children.ElementAt(1);
            Assert.That(expectedSaw2Motor, Is.Not.Null);
            Assert.That(expectedSaw2Motor.Name, Is.EqualTo("Motor"));
            Assert.That(expectedSaw2Motor.Children.Count, Is.EqualTo(2));

            // Saw2.Motor.Acceleration
            var expectedSaw2MotorAcceleration = expectedSaw2Motor.Children.ElementAt(0);
            Assert.That(expectedSaw2MotorAcceleration, Is.Not.Null);
            Assert.That(expectedSaw2MotorAcceleration.Name, Is.EqualTo("Acceleration"));

            // Saw2.Motor.Speed
            var expectedSaw2MotorSpeed = expectedSaw2Motor.Children.ElementAt(1);
            Assert.That(expectedSaw2MotorSpeed, Is.Not.Null);
            Assert.That(expectedSaw2MotorSpeed.Name, Is.EqualTo("Speed"));
        }

        [Test]
        public void MixtureOfTagsAndStructuredTagsAsList()
        {
            BindingSourceDescription tag2 = CreateBindingSourceDescription("Tag2", "Tag2", StringConstants.TagsRoot + "Tag2");
            BindingSourceDescription tag1 = CreateBindingSourceDescription("Tag1", "Tag1", StringConstants.TagsRoot + "Tag1");
            BindingSourceDescription saw1BeepNumber = CreateBindingSourceDescription("Saw1.BeepNumber", "Saw1.BeepNumber", StringConstants.TagsRoot + "Saw1.BeepNumber");

            var builder = new BindingSourceDescriptionRepresentationBuilder();

            // ACT
            List<BindingSourceDescriptionViewModelBase> result = builder
                .BuildListAsync(new[] { tag2, tag1, saw1BeepNumber }).Result
                .ToList();

            // ASSERT, should be a flat list of tags and structured tags
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.ElementAt(0), Is.TypeOf<BindingSourceDescriptionViewModel>());
            Assert.That(result.ElementAt(0).Name, Is.EqualTo("Saw1.BeepNumber"));
            Assert.That(result.ElementAt(1), Is.TypeOf<BindingSourceDescriptionViewModel>());
            Assert.That(result.ElementAt(1).Name, Is.EqualTo("Tag1"));
            Assert.That(result.ElementAt(2), Is.TypeOf<BindingSourceDescriptionViewModel>());
            Assert.That(result.ElementAt(2).Name, Is.EqualTo("Tag2"));
        }

        [Test]
        public void MixtureOfTagsAndStructuredTagsAsTree()
        {
            BindingSourceDescription tag2 = CreateBindingSourceDescription("Tag2", "Tag2", StringConstants.TagsRoot + "Tag2");
            BindingSourceDescription tag1 = CreateBindingSourceDescription("Tag1", "Tag1", StringConstants.TagsRoot + "Tag1");
            BindingSourceDescription saw1 = CreateBindingSourceDescription("Saw1", "Saw1", StringConstants.TagsRoot + "Saw1");
            BindingSourceDescription saw1BeepNumber = CreateBindingSourceDescription("Saw1.BeepNumber", "Saw1.BeepNumber", StringConstants.TagsRoot + "Saw1.BeepNumber");

            var builder = new BindingSourceDescriptionRepresentationBuilder();

            // ACT
            List<BindingSourceDescriptionViewModelBase> result = builder
                .BuildTreeAsync(new[] { tag2, tag1, saw1, saw1BeepNumber }).Result
                .ToList();

            // ASSERT, structure should be like this:
            //
            //         + Saw1
            //            - BeepNumber
            //         Tag1
            //         Tag2
            Assert.That(result.Count, Is.EqualTo(3));

            // Saw1
            var expectedSaw1 = result.ElementAt(0) as HierarchicalBindingSourceDescriptionViewModel;
            Assert.That(expectedSaw1, Is.Not.Null);
            Assert.That(expectedSaw1.Name, Is.EqualTo("Saw1"));
            Assert.That(expectedSaw1.Children.Count, Is.EqualTo(1));

            // Saw1.BeepNumber
            var expectedSaw1BeepNumber = expectedSaw1.Children.ElementAt(0);
            Assert.That(expectedSaw1BeepNumber, Is.Not.Null);
            Assert.That(expectedSaw1BeepNumber.Name, Is.EqualTo("BeepNumber"));

            // Tag1
            var expectedTag1 = result.ElementAt(1) as HierarchicalBindingSourceDescriptionViewModel;
            Assert.That(expectedTag1, Is.Not.Null);
            Assert.That(expectedTag1.Name, Is.EqualTo("Tag1"));

            // Tag2
            var expectedTag2 = result.ElementAt(2) as HierarchicalBindingSourceDescriptionViewModel;
            Assert.That(expectedTag2, Is.Not.Null);
            Assert.That(expectedTag2.Name, Is.EqualTo("Tag2"));
        }

        [Test]
        public void AliasAsList()
        {
            BindingSourceDescription alias3 = CreateBindingSourceDescription("Alias3", "#Alias3", "Alias3");
            BindingSourceDescription alias1 = CreateBindingSourceDescription("Alias1", "#Alias1", "Alias1");
            BindingSourceDescription alias2 = CreateBindingSourceDescription("Alias2", "#Alias2", "Alias2");

            var builder = new BindingSourceDescriptionRepresentationBuilder();

            // ACT
            List<BindingSourceDescriptionViewModelBase> result = builder
                .BuildListAsync(new[] { alias3, alias1, alias2 }).Result
                .ToList();

            // ASSERT, should be a flat list of aliases
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.ElementAt(0), Is.TypeOf<BindingSourceDescriptionViewModel>());
            Assert.That(result.ElementAt(0).Name, Is.EqualTo("Alias1"));
            Assert.That(result.ElementAt(1), Is.TypeOf<BindingSourceDescriptionViewModel>());
            Assert.That(result.ElementAt(1).Name, Is.EqualTo("Alias2"));
            Assert.That(result.ElementAt(2), Is.TypeOf<BindingSourceDescriptionViewModel>());
            Assert.That(result.ElementAt(2).Name, Is.EqualTo("Alias3"));
        }

        [Test]
        public void AliasAsTree()
        {
            BindingSourceDescription alias3 = CreateBindingSourceDescription("Alias3", "#Alias3", "Alias3");
            BindingSourceDescription alias1 = CreateBindingSourceDescription("Alias1", "#Alias1", "Alias1");
            BindingSourceDescription alias2 = CreateBindingSourceDescription("Alias2", "#Alias2", "Alias2");

            var builder = new BindingSourceDescriptionRepresentationBuilder();

            // ACT
            List<BindingSourceDescriptionViewModelBase> result = builder
                .BuildTreeAsync(new[] { alias3, alias1, alias2 }).Result
                .ToList();

            // ASSERT, should be a flat list of aliases
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.ElementAt(0), Is.TypeOf<HierarchicalBindingSourceDescriptionViewModel>());
            Assert.That(result.ElementAt(0).Name, Is.EqualTo("Alias1"));
            Assert.That(result.ElementAt(1), Is.TypeOf<HierarchicalBindingSourceDescriptionViewModel>());
            Assert.That(result.ElementAt(1).Name, Is.EqualTo("Alias2"));
            Assert.That(result.ElementAt(2), Is.TypeOf<HierarchicalBindingSourceDescriptionViewModel>());
            Assert.That(result.ElementAt(2).Name, Is.EqualTo("Alias3"));
        }

        [Test]
        public void MultipleGenerations()
        {
            // ARRANGE
            var east = CreateBindingSourceDescription("East", "East", StringConstants.TagsRoot + "East");
            var boiler1 = CreateBindingSourceDescription("East.Boiler1", "East.Boiler1", StringConstants.TagsRoot + "East.Boiler1");
            var drum1002 = CreateBindingSourceDescription("East.Boiler1.Drum1002", "East.Boiler1.Drum1002", StringConstants.TagsRoot + "East.Boiler1.Drum1002");
            var drum1002_Measurement = CreateBindingSourceDescription("East.Boiler1.Drum1002.Measurement", "East.Boiler1.Drum1002.Measurement", StringConstants.TagsRoot + "East.Boiler1.Drum1002.Measurement");
            var drum1002_Online = CreateBindingSourceDescription("East.Boiler1.Drum1002.Online", "East.Boiler1.Drum1002.Online", StringConstants.TagsRoot + "East.Boiler1.Drum1002.Online");
            var drum1002_Online_False = CreateBindingSourceDescription("East.Boiler1.Drum1002.Online.FalseState", "East.Boiler1.Drum1002.Online.FalseState", StringConstants.TagsRoot + "East.Boiler1.Drum1002.Online.FalseState");
            var drum1002_Online_True = CreateBindingSourceDescription("East.Boiler1.Drum1002.Online.TrueState", "East.Boiler1.Drum1002.Online.TrueState", StringConstants.TagsRoot + "East.Boiler1.Drum1002.Online.TrueState");
            var pipe1001 = CreateBindingSourceDescription("East.Boiler1.Pipe1001", "East.Boiler1.Pipe1001", StringConstants.TagsRoot + "East.Boiler1.Pipe1001");
            var pipe1001_Measurement = CreateBindingSourceDescription("East.Boiler1.Pipe1001.Measurement", "East.Boiler1.Pipe1001.Measurement", StringConstants.TagsRoot + "East.Boiler1.Pipe1001.Measurement");
            var pipe1001_Online = CreateBindingSourceDescription("East.Boiler1.Pipe1001.Online", "East.Boiler1.Pipe1001.Online", StringConstants.TagsRoot + "East.Boiler1.Pipe1001.Online");
            var pipe1001_Online_False = CreateBindingSourceDescription("East.Boiler1.Pipe1001.Online.FalseState", "East.Boiler1.Pipe1001.Online.FalseState", StringConstants.TagsRoot + "East.Boiler1.Pipe1001.Online.FalseState");
            var pipe1001_Online_True = CreateBindingSourceDescription("East.Boiler1.Pipe1001.Online.TrueState", "East.Boiler1.Pipe1001.Online.TrueState", StringConstants.TagsRoot + "East.Boiler1.Pipe1001.Online.TrueState");

            var builder = new BindingSourceDescriptionRepresentationBuilder();

            // ACT
            List<BindingSourceDescriptionViewModelBase> result = builder
                .BuildTreeAsync(new[]
                {
                    east,
                    boiler1,
                    drum1002,
                    drum1002_Measurement,
                    drum1002_Online,
                    drum1002_Online_False,
                    drum1002_Online_True,
                    pipe1001,
                    pipe1001_Measurement,
                    pipe1001_Online,
                    pipe1001_Online_False,
                    pipe1001_Online_True
                })
                .Result
                .ToList();

            // ASSERT, structure should be like this
            //
            //         + East
            //           + Boiler1
            //             + Pipe1001
            //               - Measurement
            //               + Online
            //                 - FalseState
            //                 - TrueState
            //             + Drum1002
            //               - Measurement
            //               + Online
            //                 - FalseState
            //                 - TrueState

            Assert.That(result.Count, Is.EqualTo(1));

            // East
            var expectedEast = result.ElementAt(0) as HierarchicalBindingSourceDescriptionViewModel;
            Assert.That(expectedEast, Is.Not.Null);
            Assert.That(expectedEast.Name, Is.EqualTo("East"));
            Assert.That(expectedEast.Children.Count, Is.EqualTo(1));

            // Boiler1
            var expectedBoiler1 = expectedEast.Children.ElementAt(0);
            Assert.That(expectedBoiler1.Name, Is.EqualTo("Boiler1"));
            Assert.That(expectedBoiler1.Children.Count, Is.EqualTo(2));

            // Drum1002
            var expectedDrum1002 = expectedBoiler1.Children.ElementAt(0);
            Assert.That(expectedDrum1002.Name, Is.EqualTo("Drum1002"));
            Assert.That(expectedDrum1002.Children.Count, Is.EqualTo(2));

            // Drum1002.Measurement
            var expectedDrum1002_Measurement = expectedDrum1002.Children.ElementAt(0);
            Assert.That(expectedDrum1002_Measurement.Name, Is.EqualTo("Measurement"));
            Assert.That(expectedDrum1002_Measurement.Children.Count, Is.EqualTo(0));

            // Drum1002.Online
            var expectedDrum1002_Online = expectedDrum1002.Children.ElementAt(1);
            Assert.That(expectedDrum1002_Online.Name, Is.EqualTo("Online"));
            Assert.That(expectedDrum1002_Online.Children.Count, Is.EqualTo(2));

            // Drum1002.Online.FalseState
            var expectedDrum1002_Online_False = expectedDrum1002_Online.Children.ElementAt(0);
            Assert.That(expectedDrum1002_Online_False.Name, Is.EqualTo("FalseState"));
            Assert.That(expectedDrum1002_Online_False.Children.Count, Is.EqualTo(0));

            // Drum1002.Online.TrueState
            var expectedDrum1002_Online_True = expectedDrum1002_Online.Children.ElementAt(1);
            Assert.That(expectedDrum1002_Online_True.Name, Is.EqualTo("TrueState"));
            Assert.That(expectedDrum1002_Online_True.Children.Count, Is.EqualTo(0));

            // Pipe1001
            var expectedPipe1001 = expectedBoiler1.Children.ElementAt(1);
            Assert.That(expectedPipe1001.Name, Is.EqualTo("Pipe1001"));
            Assert.That(expectedPipe1001.Children.Count, Is.EqualTo(2));

            // Pipe1001.Measurement
            var expectedPipe1001_Measurement = expectedPipe1001.Children.ElementAt(0);
            Assert.That(expectedPipe1001_Measurement.Name, Is.EqualTo("Measurement"));
            Assert.That(expectedPipe1001_Measurement.Children.Count, Is.EqualTo(0));

            // Pipe1001.Online
            var expectedPipe1001_Online = expectedPipe1001.Children.ElementAt(1);
            Assert.That(expectedPipe1001_Online.Name, Is.EqualTo("Online"));
            Assert.That(expectedPipe1001_Online.Children.Count, Is.EqualTo(2));

            // Pipe1001.Online.FalseState
            var expectedPipe1001_Online_False = expectedPipe1001_Online.Children.ElementAt(0);
            Assert.That(expectedPipe1001_Online_False.Name, Is.EqualTo("FalseState"));
            Assert.That(expectedPipe1001_Online_False.Children.Count, Is.EqualTo(0));

            // Pipe1001.Online.TrueState
            var expectedPipe1001_Online_True = expectedPipe1001_Online.Children.ElementAt(1);
            Assert.That(expectedPipe1001_Online_True.Name, Is.EqualTo("TrueState"));
            Assert.That(expectedPipe1001_Online_True.Children.Count, Is.EqualTo(0));
        }

        [Test]
        public void DataIsNotHierarchical()
        {
            // ARRANGE
            BindingSourceDescription tag3 = CreateBindingSourceDescription("Tag3", "Tag3", StringConstants.TagsRoot + "Tag3");
            BindingSourceDescription tag1 = CreateBindingSourceDescription("Tag1", "Tag1", StringConstants.TagsRoot + "Tag1");
            BindingSourceDescription tag2 = CreateBindingSourceDescription("Tag2", "Tag2", StringConstants.TagsRoot + "Tag2");

            var builder = new BindingSourceDescriptionRepresentationBuilder();

            // ACT
            bool isHierarchical = builder.IsHierarchical(new[] { tag3, tag1, tag2 });

            // ASSERT
            Assert.That(isHierarchical, Is.False);
        }

        [Test]
        public void DataIsHierarchical()
        {
            // ARRANGE
            BindingSourceDescription saw1BeepNumber = CreateBindingSourceDescription("Saw1.BeepNumber", "Saw1.BeepNumber", StringConstants.TagsRoot + "Saw1.BeepNumber");
            BindingSourceDescription tag1 = CreateBindingSourceDescription("Tag1", "Tag1", StringConstants.TagsRoot + "Tag1");
            BindingSourceDescription tag2 = CreateBindingSourceDescription("Tag2", "Tag2", StringConstants.TagsRoot + "Tag2");

            var builder = new BindingSourceDescriptionRepresentationBuilder();

            // ACT
            bool isHierarchical = builder.IsHierarchical(new[] { saw1BeepNumber, tag1, tag2 });

            // ASSERT
            Assert.That(isHierarchical, Is.True);
        }

#region Helper methods

        private static BindingSourceDescription CreateBindingSourceDescription(string name, string displayName, string fullName)
        {
            return new BindingSourceDescription(name, displayName, fullName, false);
        }

#endregion
    }
}
#endif
