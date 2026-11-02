using System;

namespace FlappyDragon.Core
{
    /// <summary>Tuning of the dragon's vertical flight, in world units and seconds.</summary>
    public sealed class FlightSettings
    {
        /// <summary>Downward acceleration (negative).</summary>
        public float Gravity { get; set; } = -30f;

        /// <summary>Upward speed set by every flap; flapping replaces the speed instead of adding to it.</summary>
        public float FlapSpeed { get; set; } = 9.2f;

        /// <summary>Terminal fall speed (negative).</summary>
        public float MaxFallSpeed { get; set; } = -15f;

        /// <summary>Highest point the dragon can reach; it stops rising there instead of dying.</summary>
        public float Ceiling { get; set; } = 5.2f;

        /// <summary>Height of the ground surface; touching it ends the run.</summary>
        public float Floor { get; set; } = -4.2f;

        /// <summary>Degrees of nose pitch per unit of vertical speed.</summary>
        public float PitchPerSpeed { get; set; } = 5f;

        public float MaxPitchUp { get; set; } = 28f;

        public float MaxPitchDown { get; set; } = -75f;
    }

    /// <summary>
    /// Flappy-style vertical motion integrated by hand instead of by the physics engine, so a
    /// flap always feels the same whatever the frame rate or the current fall speed.
    /// </summary>
    public sealed class FlightModel
    {
        readonly FlightSettings settings;

        public FlightModel(FlightSettings settings, float startHeight)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Height = startHeight;
        }

        public float Height { get; private set; }

        public float Velocity { get; private set; }

        public FlightSettings Settings => settings;

        /// <summary>True once the dragon has reached the ground.</summary>
        public bool IsGrounded => Height <= settings.Floor;

        /// <summary>Nose pitch for the model: up while rising, diving as it falls.</summary>
        public float Pitch => Clamp(Velocity * settings.PitchPerSpeed, settings.MaxPitchDown, settings.MaxPitchUp);

        public void Flap()
        {
            if (!IsGrounded)
            {
                Velocity = settings.FlapSpeed;
            }
        }

        /// <summary>Advances the motion by <paramref name="deltaTime"/> seconds.</summary>
        public void Step(float deltaTime)
        {
            if (deltaTime <= 0f || IsGrounded)
            {
                return;
            }
            Velocity = Math.Max(Velocity + settings.Gravity * deltaTime, settings.MaxFallSpeed);
            Height += Velocity * deltaTime;
            if (Height >= settings.Ceiling)
            {
                Height = settings.Ceiling;
                Velocity = Math.Min(Velocity, 0f);
            }
            if (Height <= settings.Floor)
            {
                Height = settings.Floor;
                Velocity = 0f;
            }
        }

        /// <summary>Places the dragon at a height with no vertical speed (used by restarts).</summary>
        public void Reset(float height)
        {
            Height = Clamp(height, settings.Floor + 0.01f, settings.Ceiling);
            Velocity = 0f;
        }

        static float Clamp(float value, float min, float max) => value < min ? min : value > max ? max : value;
    }
}
