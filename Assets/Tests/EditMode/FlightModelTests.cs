using NUnit.Framework;

namespace FlappyDragon.Core.Tests
{
    public class FlightModelTests
    {
        static FlightModel Create(float height = 0f) => new FlightModel(new FlightSettings(), height);

        [Test]
        public void Flap_SetsTheSameUpwardSpeedWhateverTheCurrentFall()
        {
            var calm = Create();
            var falling = Create();
            for (int i = 0; i < 20; i++)
            {
                falling.Step(0.02f);
            }
            Assume.That(falling.Velocity, Is.LessThan(-5f));

            calm.Flap();
            falling.Flap();

            Assert.That(falling.Velocity, Is.EqualTo(calm.Velocity));
            Assert.That(calm.Velocity, Is.EqualTo(new FlightSettings().FlapSpeed));
        }

        [Test]
        public void Step_FallsUnderGravityUntilTheTerminalSpeed()
        {
            var model = Create(height: 5f);
            model.Step(0.1f);
            Assert.That(model.Velocity, Is.EqualTo(-3f).Within(1e-4));
            Assert.That(model.Height, Is.LessThan(5f));

            for (int i = 0; i < 10; i++)
            {
                model.Step(0.05f);
            }
            Assert.That(model.Velocity, Is.EqualTo(new FlightSettings().MaxFallSpeed));
        }

        [Test]
        public void Step_StopsAtTheCeilingWithoutGoingThrough()
        {
            var settings = new FlightSettings();
            var model = new FlightModel(settings, settings.Ceiling - 0.1f);
            model.Flap();
            model.Step(0.1f);
            Assert.That(model.Height, Is.EqualTo(settings.Ceiling));
            Assert.That(model.Velocity, Is.LessThanOrEqualTo(0f));
        }

        [Test]
        public void ReachingTheFloor_GroundsTheDragonAndFreezesIt()
        {
            var settings = new FlightSettings();
            var model = new FlightModel(settings, settings.Floor + 0.2f);
            for (int i = 0; i < 30; i++)
            {
                model.Step(0.02f);
            }
            Assert.That(model.IsGrounded, Is.True);
            Assert.That(model.Height, Is.EqualTo(settings.Floor));

            model.Flap();
            model.Step(0.02f);
            Assert.That(model.Height, Is.EqualTo(settings.Floor), "A grounded dragon cannot take off again.");
        }

        [Test]
        public void Pitch_PointsUpWhileRisingAndDivesWhileFalling()
        {
            var settings = new FlightSettings { Floor = -1000f };
            var model = new FlightModel(settings, 0f);
            model.Flap();
            Assert.That(model.Pitch, Is.EqualTo(settings.MaxPitchUp));

            for (int i = 0; i < 50; i++)
            {
                model.Step(0.02f);
            }
            Assert.That(model.Pitch, Is.EqualTo(settings.MaxPitchDown));
        }

        [Test]
        public void Step_IgnoresNonPositiveTime()
        {
            var model = Create(height: 1f);
            model.Step(0f);
            model.Step(-1f);
            Assert.That(model.Height, Is.EqualTo(1f));
            Assert.That(model.Velocity, Is.EqualTo(0f));
        }

        [Test]
        public void Reset_PlacesTheDragonAtRest()
        {
            var model = Create(height: 2f);
            model.Flap();
            model.Step(0.1f);
            model.Reset(1f);
            Assert.That(model.Height, Is.EqualTo(1f));
            Assert.That(model.Velocity, Is.EqualTo(0f));
        }
    }
}
