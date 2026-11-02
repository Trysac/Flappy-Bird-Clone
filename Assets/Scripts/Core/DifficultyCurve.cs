using System;

namespace FlappyDragon.Core
{
    /// <summary>How hard the run is at one moment.</summary>
    public readonly struct Difficulty
    {
        public Difficulty(float speed, float gapSize, float spacing, float batChance)
        {
            Speed = speed;
            GapSize = gapSize;
            Spacing = spacing;
            BatChance = batChance;
        }

        /// <summary>Scroll speed of the world, in units per second.</summary>
        public float Speed { get; }

        /// <summary>Vertical opening between the two rocks of a gate.</summary>
        public float GapSize { get; }

        /// <summary>Horizontal distance between consecutive gates.</summary>
        public float Spacing { get; }

        /// <summary>Probability (0..1) that a bat guards the next gate.</summary>
        public float BatChance { get; }
    }

    /// <summary>
    /// Eases from a forgiving start to a hard cap as gates are passed, so the first gates teach
    /// the rhythm and the curve then flattens instead of growing forever.
    /// </summary>
    public sealed class DifficultyCurve
    {
        public DifficultyCurve(Difficulty easy, Difficulty hard, int gatesToMax)
        {
            if (gatesToMax <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(gatesToMax), "Must be positive.");
            }
            Easy = easy;
            Hard = hard;
            GatesToMax = gatesToMax;
        }

        /// <summary>The tuning used by the game.</summary>
        public static DifficultyCurve Default { get; } = new DifficultyCurve(
            easy: new Difficulty(speed: 4.2f, gapSize: 4.4f, spacing: 7.5f, batChance: 0f),
            hard: new Difficulty(speed: 7.2f, gapSize: 3.0f, spacing: 6.0f, batChance: 0.45f),
            gatesToMax: 60);

        public Difficulty Easy { get; }

        public Difficulty Hard { get; }

        public int GatesToMax { get; }

        /// <summary>0 at the start, 1 once <see cref="GatesToMax"/> gates were passed (smoothstep in between).</summary>
        public float Progress(int gatesPassed)
        {
            float t = Math.Min(Math.Max(gatesPassed, 0) / (float)GatesToMax, 1f);
            return t * t * (3f - 2f * t);
        }

        public Difficulty At(int gatesPassed)
        {
            float t = Progress(gatesPassed);
            // Bats only appear after the first handful of gates.
            float batT = gatesPassed < 5 ? 0f : t;
            return new Difficulty(
                Lerp(Easy.Speed, Hard.Speed, t),
                Lerp(Easy.GapSize, Hard.GapSize, t),
                Lerp(Easy.Spacing, Hard.Spacing, t),
                Lerp(Easy.BatChance, Hard.BatChance, batT));
        }

        static float Lerp(float a, float b, float t) => a + (b - a) * t;
    }
}
