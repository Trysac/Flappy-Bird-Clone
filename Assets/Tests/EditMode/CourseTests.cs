using System;
using NUnit.Framework;

namespace FlappyDragon.Core.Tests
{
    public class DifficultyCurveTests
    {
        readonly DifficultyCurve curve = DifficultyCurve.Default;

        [Test]
        public void StartsEasy()
        {
            Difficulty start = curve.At(0);
            Assert.That(start.Speed, Is.EqualTo(curve.Easy.Speed));
            Assert.That(start.GapSize, Is.EqualTo(curve.Easy.GapSize));
            Assert.That(start.BatChance, Is.EqualTo(0f));
        }

        [Test]
        public void GetsHarderMonotonicallyAndStopsAtTheCap()
        {
            Difficulty previous = curve.At(0);
            for (int gates = 1; gates <= curve.GatesToMax + 40; gates++)
            {
                Difficulty next = curve.At(gates);
                Assert.That(next.Speed, Is.GreaterThanOrEqualTo(previous.Speed), $"speed at {gates}");
                Assert.That(next.GapSize, Is.LessThanOrEqualTo(previous.GapSize), $"gap at {gates}");
                Assert.That(next.Spacing, Is.LessThanOrEqualTo(previous.Spacing), $"spacing at {gates}");
                previous = next;
            }
            Assert.That(previous.Speed, Is.EqualTo(curve.Hard.Speed));
            Assert.That(previous.GapSize, Is.EqualTo(curve.Hard.GapSize));
        }

        [Test]
        public void KeepsBatsAwayFromTheFirstGates()
        {
            for (int gates = 0; gates < 5; gates++)
            {
                Assert.That(curve.At(gates).BatChance, Is.EqualTo(0f));
            }
            Assert.That(curve.At(30).BatChance, Is.GreaterThan(0f));
        }

        [Test]
        public void RejectsANonPositiveLength()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DifficultyCurve(curve.Easy, curve.Hard, 0));
        }
    }

    public class GatePlannerTests
    {
        const float Lowest = -3.6f;
        const float Highest = 4.8f;
        const float MaxStep = 2.5f;

        [Test]
        public void EveryOpeningFitsBetweenGroundAndSkyAndCanBeReached()
        {
            var curve = DifficultyCurve.Default;
            var planner = new GatePlanner(seed: 7, Lowest, Highest, MaxStep);
            float? previous = null;
            for (int gate = 0; gate < 2000; gate++)
            {
                GatePlan plan = planner.Next(curve.At(gate % 120));
                Assert.That(plan.GapBottom, Is.GreaterThanOrEqualTo(Lowest - 1e-4f));
                Assert.That(plan.GapTop, Is.LessThanOrEqualTo(Highest + 1e-4f));
                if (previous.HasValue)
                {
                    Assert.That(Math.Abs(plan.GapCenter - previous.Value), Is.LessThanOrEqualTo(MaxStep + 1e-4f));
                }
                previous = plan.GapCenter;
            }
        }

        [Test]
        public void TheSameSeedPlansTheSameCourse()
        {
            var a = new GatePlanner(42, Lowest, Highest, MaxStep);
            var b = new GatePlanner(42, Lowest, Highest, MaxStep);
            Difficulty d = DifficultyCurve.Default.At(20);
            for (int i = 0; i < 50; i++)
            {
                GatePlan x = a.Next(d);
                GatePlan y = b.Next(d);
                Assert.That(x.GapCenter, Is.EqualTo(y.GapCenter));
                Assert.That(x.Coin, Is.EqualTo(y.Coin));
                Assert.That(x.HasBat, Is.EqualTo(y.HasBat));
            }
        }

        [Test]
        public void NoBatsWhenTheDifficultyHasNone()
        {
            var planner = new GatePlanner(3, Lowest, Highest, MaxStep);
            for (int i = 0; i < 200; i++)
            {
                Assert.That(planner.Next(DifficultyCurve.Default.At(0)).HasBat, Is.False);
            }
        }

        [Test]
        public void AnOpeningTallerThanTheSkyIsCentred()
        {
            var planner = new GatePlanner(1, Lowest, Highest, MaxStep);
            GatePlan plan = planner.Next(new Difficulty(5f, 20f, 7f, 0f));
            Assert.That(plan.GapCenter, Is.EqualTo((Lowest + Highest) / 2f).Within(1e-4));
        }

        [Test]
        public void CoinValuesGrowWithRarity()
        {
            Assert.That(GatePlanner.ValueOf(CoinKind.None), Is.EqualTo(0));
            Assert.That(GatePlanner.ValueOf(CoinKind.Bronze), Is.EqualTo(1));
            Assert.That(GatePlanner.ValueOf(CoinKind.Silver), Is.EqualTo(3));
            Assert.That(GatePlanner.ValueOf(CoinKind.Gold), Is.EqualTo(5));
        }

        [Test]
        public void RejectsAnUpsideDownSky()
        {
            Assert.Throws<ArgumentException>(() => new GatePlanner(1, 2f, 1f, MaxStep));
        }
    }

    public class ShapesTests
    {
        [TestCase(0f, 0f, true, TestName = "Centre inside")]
        [TestCase(-1.4f, 0f, true, TestName = "Touching the left side")]
        [TestCase(-1.6f, 0f, false, TestName = "Just left of the rectangle")]
        [TestCase(1.4f, 1.4f, false, TestName = "Outside a rounded corner")]
        [TestCase(1.3f, 1.3f, true, TestName = "Inside a rounded corner")]
        public void CircleOverlapsRect(float x, float y, bool expected)
        {
            // Unit square from (-1,-1) to (1,1), circle of radius 0.5.
            Assert.That(Shapes.CircleOverlapsRect(x, y, 0.5f, -1f, -1f, 1f, 1f), Is.EqualTo(expected));
        }

        [Test]
        public void CirclesOverlapOnlyWhenCloserThanTheirRadii()
        {
            Assert.That(Shapes.CirclesOverlap(0f, 0f, 1f, 1.9f, 0f, 1f), Is.True);
            Assert.That(Shapes.CirclesOverlap(0f, 0f, 1f, 2.1f, 0f, 1f), Is.False);
        }
    }
}
