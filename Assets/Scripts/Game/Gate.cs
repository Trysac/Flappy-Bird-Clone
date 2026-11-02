using FlappyDragon.Core;
using UnityEngine;

namespace FlappyDragon.Game
{
    /// <summary>
    /// A pair of rock columns, one rising from the ground and one hanging from the sky, with
    /// an opening between them. Collision uses the rectangles of the columns on the play plane.
    /// </summary>
    public sealed class Gate : MonoBehaviour
    {
        public const float HalfWidth = 0.78f;
        const float SkyTop = 9f;

        public GatePlan Plan { get; private set; }

        /// <summary>True once the dragon has flown past it (it then no longer scores).</summary>
        public bool Scored { get; set; }

        public float X => transform.position.x;

        public static Gate Create(Transform parent, GameArt art, GatePlan plan, float x, float groundTop, int seed)
        {
            var gate = new GameObject("Gate").AddComponent<Gate>();
            gate.transform.SetParent(parent, false);
            gate.transform.position = new Vector3(x, 0f, 0f);
            gate.Plan = plan;
            gate.BuildColumn(art, groundTop - 1.5f, plan.GapBottom, rising: true, seed);
            gate.BuildColumn(art, plan.GapTop, SkyTop, rising: false, seed + 1);
            return gate;
        }

        void BuildColumn(GameArt art, float from, float to, bool rising, int seed)
        {
            float length = Mathf.Max(to - from, 0.2f);
            var column = new GameObject(rising ? "Spire" : "Hanging rock").transform;
            column.SetParent(transform, false);
            // Built upwards from its base; the hanging rock is flipped so its base is the sky.
            column.localPosition = new Vector3(0f, rising ? from : to, 0f);
            column.localEulerAngles = rising ? Vector3.zero : new Vector3(180f, 0f, 0f);

            const float radius = HalfWidth / 0.92f;
            Mesh body = LowPoly.Prism(7, radius * 1.08f, radius * 0.94f, length, jitter: 0.06f, seed: seed);
            LowPoly.Part("Rock", column, body, art.Rock, Vector3.zero, Vector3.zero, Vector3.one);
            // Darker bands make the column read as layered stone.
            for (float y = 1.4f; y < length - 0.6f; y += 1.9f)
            {
                Mesh band = LowPoly.Prism(7, radius * 1.1f, radius * 1.08f, 0.22f, jitter: 0.05f, seed: seed + (int)(y * 10f));
                LowPoly.Part("Band", column, band, art.RockDark, new Vector3(0f, y, 0f), Vector3.zero, Vector3.one, castShadows: false);
            }
            // A grassy (or mossy) lip around the end that faces the opening.
            Mesh lip = LowPoly.Prism(8, radius * 1.22f, radius * 1.05f, 0.34f, jitter: 0.08f, seed: seed + 7);
            LowPoly.Part("Lip", column, lip, rising ? art.Grass : art.RockDark, new Vector3(0f, length - 0.3f, 0f), Vector3.zero, Vector3.one);
            if (rising)
            {
                Mesh tuft = LowPoly.Prism(4, 0.16f, 0f, 0.45f, seed: seed + 9);
                for (int i = 0; i < 3; i++)
                {
                    float angle = (i * 120f + seed * 37f) * Mathf.Deg2Rad;
                    var at = new Vector3(Mathf.Cos(angle) * radius * 0.6f, length, Mathf.Sin(angle) * radius * 0.6f);
                    LowPoly.Part("Tuft", column, tuft, art.Grass, at, new Vector3(0f, i * 40f, (i - 1) * 14f), Vector3.one, castShadows: false);
                }
            }
        }

        public void Move(float distance) => transform.position += Vector3.left * distance;

        /// <summary>Does a circle on the play plane touch either column?</summary>
        public bool Hits(Vector2 centre, float radius)
        {
            float left = X - HalfWidth;
            float right = X + HalfWidth;
            return Shapes.CircleOverlapsRect(centre.x, centre.y, radius, left, -100f, right, Plan.GapBottom)
                || Shapes.CircleOverlapsRect(centre.x, centre.y, radius, left, Plan.GapTop, right, 100f);
        }
    }
}
