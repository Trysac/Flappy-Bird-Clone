namespace FlappyDragon.Core
{
    public enum Medal
    {
        None,
        Bronze,
        Silver,
        Gold,
        Platinum,
    }

    /// <summary>Medal awarded on the game-over screen for a final score.</summary>
    public static class Medals
    {
        public const int Bronze = 10;
        public const int Silver = 25;
        public const int Gold = 50;
        public const int Platinum = 100;

        public static Medal For(int score)
        {
            if (score >= Platinum)
            {
                return Medal.Platinum;
            }
            if (score >= Gold)
            {
                return Medal.Gold;
            }
            if (score >= Silver)
            {
                return Medal.Silver;
            }
            return score >= Bronze ? Medal.Bronze : Medal.None;
        }

        /// <summary>Score needed for the next medal, or null when the best one is already earned.</summary>
        public static int? NextThreshold(int score)
        {
            switch (For(score))
            {
                case Medal.None: return Bronze;
                case Medal.Bronze: return Silver;
                case Medal.Silver: return Gold;
                case Medal.Gold: return Platinum;
                default: return null;
            }
        }
    }
}
