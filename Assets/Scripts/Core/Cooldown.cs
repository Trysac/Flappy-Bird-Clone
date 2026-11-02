using System;

namespace FlappyDragon.Core
{
    /// <summary>A recharging ability: usable once, then again after <see cref="Duration"/> seconds.</summary>
    public sealed class Cooldown
    {
        public Cooldown(float duration)
        {
            if (duration <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "Must be positive.");
            }
            Duration = duration;
        }

        public float Duration { get; }

        public float Remaining { get; private set; }

        public bool IsReady => Remaining <= 0f;

        /// <summary>0 right after use, 1 when ready again.</summary>
        public float Charge => 1f - Remaining / Duration;

        /// <summary>Uses the ability if it is ready; returns whether it was used.</summary>
        public bool TryUse()
        {
            if (!IsReady)
            {
                return false;
            }
            Remaining = Duration;
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime > 0f)
            {
                Remaining = Math.Max(Remaining - deltaTime, 0f);
            }
        }

        public void Reset() => Remaining = 0f;
    }
}
