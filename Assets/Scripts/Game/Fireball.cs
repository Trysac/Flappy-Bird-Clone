using UnityEngine;

namespace FlappyDragon.Game
{
    /// <summary>A fireball breathed by the dragon: it flies ahead and burns the first bat it meets.</summary>
    public sealed class Fireball : MonoBehaviour
    {
        public const float Radius = 0.32f;
        public const float Speed = 15f;
        const float Lifetime = 1.6f;

        static Mesh ember;
        float age;
        Transform core;

        public Vector2 Centre => transform.position;

        public bool Expired => age >= Lifetime;

        public static Fireball Create(GameArt art, Vector3 position)
        {
            var fireball = new GameObject("Fireball").AddComponent<Fireball>();
            fireball.transform.position = position;
            ember ??= LowPoly.Blob(rings: 3, segments: 6, jitter: 0.2f, seed: 9);
            fireball.core = LowPoly.Part("Core", fireball.transform, ember, art.Fire, Vector3.zero, Vector3.zero,
                new Vector3(0.5f, 0.4f, 0.4f), castShadows: false);
            Effects.Trail(fireball.transform, art.Fire, ember);
            return fireball;
        }

        /// <summary>Flies ahead at its own speed; the world scrolls towards it.</summary>
        public void Advance(float deltaTime)
        {
            age += deltaTime;
            transform.position += Vector3.right * (Speed * deltaTime);
            core.localEulerAngles += new Vector3(0f, 0f, -720f * deltaTime);
        }
    }
}
