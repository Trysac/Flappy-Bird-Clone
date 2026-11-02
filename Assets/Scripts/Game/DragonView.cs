using UnityEngine;

namespace FlappyDragon.Game
{
    /// <summary>
    /// The dragon model, assembled from low-poly shapes, and its procedural animation: wing
    /// beats (faster right after a flap), a swaying tail, nose pitch and the crash tumble.
    /// The model faces +X; the camera looks at its left side.
    /// </summary>
    public sealed class DragonView : MonoBehaviour
    {
        const float GlideBeatsPerSecond = 3.2f;
        const float FlapBeatsPerSecond = 8.5f;
        const float FlapBoostSeconds = 0.28f;

        Transform model;
        Transform nearWing;
        Transform farWing;
        Transform[] tail;
        float beatPhase;
        float boost;
        float pitch;
        float tumble;

        public void Build(GameArt art)
        {
            model = new GameObject("Model").transform;
            model.SetParent(transform, false);
            // A slight turn towards the camera shows the dragon in three quarters.
            model.localEulerAngles = new Vector3(0f, -18f, 0f);

            Mesh blob = LowPoly.Blob(seed: 11);
            Mesh smooth = LowPoly.Blob(rings: 4, segments: 8, jitter: 0.04f, seed: 3);
            Mesh spike = LowPoly.Prism(4, 0.5f, 0f, 1f, seed: 5);

            LowPoly.Part("Body", model, blob, art.DragonScales, Vector3.zero, Vector3.zero, new Vector3(0.78f, 0.5f, 0.5f));
            LowPoly.Part("Belly", model, smooth, art.DragonBelly, new Vector3(0.08f, -0.14f, 0f), Vector3.zero, new Vector3(0.62f, 0.36f, 0.42f));
            LowPoly.Part("Neck", model, blob, art.DragonScales, new Vector3(0.52f, 0.16f, 0f), new Vector3(0f, 0f, -35f), new Vector3(0.34f, 0.24f, 0.26f));
            LowPoly.Part("Head", model, blob, art.DragonScales, new Vector3(0.82f, 0.32f, 0f), new Vector3(0f, 0f, -8f), new Vector3(0.36f, 0.28f, 0.3f));
            LowPoly.Part("Snout", model, smooth, art.DragonScales, new Vector3(1.08f, 0.24f, 0f), new Vector3(0f, 0f, -12f), new Vector3(0.26f, 0.15f, 0.2f));
            LowPoly.Part("Jaw", model, smooth, art.DragonBelly, new Vector3(1.0f, 0.15f, 0f), new Vector3(0f, 0f, -10f), new Vector3(0.22f, 0.08f, 0.16f));
            foreach (float side in new[] { -1f, 1f })
            {
                LowPoly.Part("Eye", model, smooth, art.EyeWhite, new Vector3(0.92f, 0.42f, 0.2f * side), Vector3.zero, Vector3.one * 0.09f);
                LowPoly.Part("Pupil", model, smooth, art.EyeDark, new Vector3(0.96f, 0.43f, 0.26f * side), Vector3.zero, Vector3.one * 0.05f);
                LowPoly.Part("Horn", model, spike, art.DragonHorn, new Vector3(0.7f, 0.5f, 0.12f * side), new Vector3(0f, 0f, 62f), new Vector3(0.12f, 0.34f, 0.12f));
                LowPoly.Part("Nostril", model, smooth, art.EyeDark, new Vector3(1.3f, 0.3f, 0.07f * side), Vector3.zero, Vector3.one * 0.03f);
            }
            for (int i = 0; i < 4; i++)
            {
                float x = 0.35f - i * 0.28f;
                LowPoly.Part("Spike", model, spike, art.DragonHorn, new Vector3(x, 0.42f - i * 0.03f, 0f), new Vector3(0f, 0f, 28f), new Vector3(0.12f, 0.2f - i * 0.02f, 0.08f));
            }
            foreach (float side in new[] { -1f, 1f })
            {
                LowPoly.Part("Leg", model, smooth, art.DragonScales, new Vector3(0.28f, -0.4f, 0.22f * side), new Vector3(0f, 0f, 30f), new Vector3(0.12f, 0.2f, 0.12f));
                LowPoly.Part("Leg", model, smooth, art.DragonScales, new Vector3(-0.32f, -0.38f, 0.22f * side), new Vector3(0f, 0f, 40f), new Vector3(0.14f, 0.22f, 0.13f));
            }

            tail = new Transform[4];
            Transform parent = model;
            Vector3 offset = new Vector3(-0.62f, 0.02f, 0f);
            for (int i = 0; i < tail.Length; i++)
            {
                var joint = new GameObject("Tail" + i).transform;
                joint.SetParent(parent, false);
                joint.localPosition = offset;
                float size = 0.3f - i * 0.06f;
                LowPoly.Part("Segment", joint, blob, art.DragonScales, new Vector3(-size * 0.8f, 0f, 0f), Vector3.zero, new Vector3(size * 1.1f, size * 0.7f, size * 0.7f));
                tail[i] = joint;
                parent = joint;
                offset = new Vector3(-size * 1.4f, 0.03f, 0f);
            }
            LowPoly.Part("TailTip", tail[tail.Length - 1], spike, art.DragonHorn, new Vector3(-0.22f, 0f, 0f), new Vector3(0f, 0f, 90f), new Vector3(0.2f, 0.3f, 0.06f));

            Mesh wing = LowPoly.Membrane(new[]
            {
                new Vector2(-0.05f, 0f), new Vector2(0.22f, 0f), new Vector2(0.12f, -0.75f),
                new Vector2(-0.3f, -1.45f), new Vector2(-0.5f, -1.0f), new Vector2(-0.46f, -0.62f),
                new Vector2(-0.62f, -0.3f), new Vector2(-0.4f, 0f),
            });
            nearWing = Wing("NearWing", wing, art, -1f);
            farWing = Wing("FarWing", wing, art, 1f);
        }

        Transform Wing(string name, Mesh mesh, GameArt art, float side)
        {
            var pivot = new GameObject(name).transform;
            pivot.SetParent(model, false);
            pivot.localPosition = new Vector3(0.1f, 0.3f, 0.16f * side);
            // The membrane is modelled towards -Z; the far wing is its mirror image.
            LowPoly.Part("Membrane", pivot, mesh, art.DragonWing, Vector3.zero, Vector3.zero, new Vector3(1.5f, 1f, -1.45f * side));
            return pivot;
        }

        /// <summary>Starts a strong downstroke.</summary>
        public void OnFlap()
        {
            boost = FlapBoostSeconds;
            beatPhase = 0.25f;
        }

        /// <summary>Advances the animation; <paramref name="targetPitch"/> is in degrees, nose up positive.</summary>
        public void Animate(float deltaTime, float targetPitch, bool crashed)
        {
            if (model == null || deltaTime <= 0f)
            {
                return;
            }
            boost = Mathf.Max(boost - deltaTime, 0f);
            float rate = crashed ? 0.6f : boost > 0f ? FlapBeatsPerSecond : GlideBeatsPerSecond;
            beatPhase = (beatPhase + rate * deltaTime) % 1f;
            // Phase 0.25 is the top of the stroke: sin peaks there.
            float angle = crashed ? 20f : 42f + 46f * Mathf.Sin(beatPhase * Mathf.PI * 2f);
            nearWing.localEulerAngles = new Vector3(angle, 0f, 0f);
            farWing.localEulerAngles = new Vector3(-angle, 0f, 0f);

            float sway = Mathf.Sin(Time.time * 3.1f);
            for (int i = 0; i < tail.Length; i++)
            {
                tail[i].localEulerAngles = new Vector3(0f, sway * 9f, sway * 6f + (i == 0 ? -pitch * 0.25f : 0f));
            }

            pitch = Mathf.Lerp(pitch, targetPitch, 1f - Mathf.Exp(-12f * deltaTime));
            tumble = crashed ? tumble + 520f * deltaTime : 0f;
            transform.localEulerAngles = new Vector3(0f, 0f, pitch - tumble);
        }

        /// <summary>Snaps back to the neutral pose (restarts and screenshots).</summary>
        public void ResetPose()
        {
            pitch = 0f;
            tumble = 0f;
            boost = 0f;
            transform.localEulerAngles = Vector3.zero;
        }
    }
}
