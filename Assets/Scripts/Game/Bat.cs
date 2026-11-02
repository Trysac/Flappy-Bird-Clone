using UnityEngine;

namespace FlappyDragon.Game
{
    /// <summary>A bat guarding the space before a gate: it bobs up and down on the spot.</summary>
    public sealed class Bat : MonoBehaviour
    {
        public const float Radius = 0.42f;
        public const int Bonus = 3;

        Transform nearWing;
        Transform farWing;
        float baseY;
        float amplitude;
        float phase;

        public Vector2 Centre => transform.position;

        public static Bat Create(Transform parent, GameArt art, Vector3 position, float amplitude, float phase)
        {
            var bat = new GameObject("Bat").AddComponent<Bat>();
            bat.transform.SetParent(parent, true);
            bat.transform.position = position;
            bat.baseY = position.y;
            bat.amplitude = amplitude;
            bat.phase = phase;
            bat.Build(art);
            return bat;
        }

        void Build(GameArt art)
        {
            var model = new GameObject("Model").transform;
            model.SetParent(transform, false);
            // Faces the dragon (-X), turned a little towards the camera.
            model.localEulerAngles = new Vector3(0f, 200f, 0f);
            model.localScale = Vector3.one * 1.3f;
            Mesh blob = LowPoly.Blob(seed: 21);
            Mesh smooth = LowPoly.Blob(rings: 4, segments: 8, jitter: 0.04f, seed: 4);
            Mesh ear = LowPoly.Prism(3, 0.5f, 0f, 1f, seed: 2);
            LowPoly.Part("Body", model, blob, art.Bat, Vector3.zero, Vector3.zero, new Vector3(0.36f, 0.4f, 0.34f));
            LowPoly.Part("Head", model, blob, art.Bat, new Vector3(0.18f, 0.3f, 0f), Vector3.zero, new Vector3(0.26f, 0.24f, 0.24f));
            foreach (float side in new[] { -1f, 1f })
            {
                LowPoly.Part("Ear", model, ear, art.BatWing, new Vector3(0.14f, 0.48f, 0.11f * side), new Vector3(0f, 0f, -10f), new Vector3(0.12f, 0.24f, 0.08f));
                LowPoly.Part("Eye", model, smooth, art.Fire, new Vector3(0.36f, 0.35f, 0.09f * side), Vector3.zero, Vector3.one * 0.06f);
            }
            Mesh wing = LowPoly.Membrane(new[]
            {
                new Vector2(0f, 0f), new Vector2(0.25f, 0f), new Vector2(0.2f, -0.6f), new Vector2(0.05f, -1.05f),
                new Vector2(-0.1f, -0.75f), new Vector2(-0.22f, -0.5f), new Vector2(-0.3f, -0.2f), new Vector2(-0.25f, 0f),
            });
            nearWing = Wing(model, wing, art, -1f);
            farWing = Wing(model, wing, art, 1f);
        }

        static Transform Wing(Transform model, Mesh mesh, GameArt art, float side)
        {
            var pivot = new GameObject("Wing").transform;
            pivot.SetParent(model, false);
            pivot.localPosition = new Vector3(0f, 0.12f, 0.15f * side);
            LowPoly.Part("Membrane", pivot, mesh, art.BatWing, Vector3.zero, Vector3.zero, new Vector3(1.2f, 1f, -1.2f * side));
            return pivot;
        }

        /// <summary>Flaps and bobs; <paramref name="time"/> only advances while the game runs.</summary>
        public void Animate(float time)
        {
            float angle = 10f + 55f * Mathf.Sin(time * 18f + phase);
            nearWing.localEulerAngles = new Vector3(angle, 0f, 0f);
            farWing.localEulerAngles = new Vector3(-angle, 0f, 0f);
            Vector3 p = transform.position;
            transform.position = new Vector3(p.x, baseY + Mathf.Sin(time * 2.2f + phase) * amplitude, p.z);
        }
    }
}
