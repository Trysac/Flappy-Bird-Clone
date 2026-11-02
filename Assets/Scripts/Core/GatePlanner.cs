using System;

namespace FlappyDragon.Core
{
    public enum CoinKind
    {
        None,
        Bronze,
        Silver,
        Gold,
    }

    /// <summary>One gate to spawn: an opening between two rocks, plus what waits inside it.</summary>
    public readonly struct GatePlan
    {
        public GatePlan(float gapCenter, float gapSize, CoinKind coin, bool hasBat)
        {
            GapCenter = gapCenter;
            GapSize = gapSize;
            Coin = coin;
            HasBat = hasBat;
        }

        public float GapCenter { get; }

        public float GapSize { get; }

        public float GapBottom => GapCenter - GapSize / 2f;

        public float GapTop => GapCenter + GapSize / 2f;

        public CoinKind Coin { get; }

        public bool HasBat { get; }
    }

    /// <summary>
    /// Chooses where the next opening goes. The opening always fits between the ground and the
    /// sky, and never jumps further from the previous one than a dragon can climb or dive in
    /// the time between gates, so every generated course can be flown.
    /// </summary>
    public sealed class GatePlanner
    {
        readonly Random random;
        float? previousCenter;

        public GatePlanner(int seed, float lowest, float highest, float maxStep)
        {
            if (highest <= lowest)
            {
                throw new ArgumentException("The highest point must be above the lowest one.");
            }
            random = new Random(seed);
            Lowest = lowest;
            Highest = highest;
            MaxStep = maxStep;
        }

        /// <summary>Lowest point an opening may reach (just above the ground).</summary>
        public float Lowest { get; }

        /// <summary>Highest point an opening may reach (just below the sky).</summary>
        public float Highest { get; }

        /// <summary>Largest change of the opening's centre between two gates.</summary>
        public float MaxStep { get; }

        public GatePlan Next(Difficulty difficulty)
        {
            float half = difficulty.GapSize / 2f;
            float min = Lowest + half;
            float max = Highest - half;
            if (min > max)
            {
                min = max = (Lowest + Highest) / 2f;
            }
            float center = Lerp(min, max, (float)random.NextDouble());
            if (previousCenter.HasValue)
            {
                float from = previousCenter.Value;
                center = Math.Min(Math.Max(center, from - MaxStep), from + MaxStep);
                center = Math.Min(Math.Max(center, min), max);
            }
            previousCenter = center;

            bool hasBat = random.NextDouble() < difficulty.BatChance;
            return new GatePlan(center, difficulty.GapSize, RollCoin(), hasBat);
        }

        /// <summary>Forgets the previous opening (a new run starts anywhere).</summary>
        public void Reset() => previousCenter = null;

        CoinKind RollCoin()
        {
            double roll = random.NextDouble();
            if (roll < 0.06)
            {
                return CoinKind.Gold;
            }
            if (roll < 0.20)
            {
                return CoinKind.Silver;
            }
            return roll < 0.55 ? CoinKind.Bronze : CoinKind.None;
        }

        /// <summary>Points a coin is worth.</summary>
        public static int ValueOf(CoinKind coin)
        {
            switch (coin)
            {
                case CoinKind.Bronze: return 1;
                case CoinKind.Silver: return 3;
                case CoinKind.Gold: return 5;
                default: return 0;
            }
        }

        static float Lerp(float a, float b, float t) => a + (b - a) * t;
    }
}
