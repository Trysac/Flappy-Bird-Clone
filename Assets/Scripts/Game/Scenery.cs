using System.Collections.Generic;
using UnityEngine;

namespace FlappyDragon.Game
{
    /// <summary>
    /// The scrolling world behind the gates: a grassy ground strip on the play plane, near hills,
    /// far snowy mountains and clouds. Each layer moves at its own fraction of the world speed
    /// and wraps around, so the scenery is endless without spawning anything.
    /// </summary>
    public sealed class Scenery : MonoBehaviour
    {
        sealed class Layer
        {
            public readonly List<Transform> Pieces = new List<Transform>();
            public float Factor;
            public float Width;
        }

        readonly List<Layer> layers = new List<Layer>();

        public void Build(GameArt art, float groundTop)
        {
            const float tile = 4f;
            const int tiles = 14;
            var ground = NewLayer(1f, tile * tiles);
            for (int i = 0; i < tiles; i++)
            {
                var piece = new GameObject("Ground").transform;
                piece.SetParent(transform, false);
                piece.localPosition = new Vector3(-tile * tiles / 2f + i * tile, groundTop, 0f);
                // A slab of dirt with a grass top, both slightly uneven.
                Mesh dirt = LowPoly.Prism(4, 2.83f, 2.83f, 3.2f, seed: 100 + i, twist: false);
                LowPoly.Part("Dirt", piece, dirt, art.Dirt, new Vector3(0f, -3.35f, 0.9f), Vector3.zero, new Vector3(1f, 1f, 0.75f), castShadows: false);
                Mesh grass = LowPoly.Prism(4, 2.83f, 2.83f, 0.35f, seed: 200 + i, twist: false);
                LowPoly.Part("Grass", piece, grass, art.Grass, new Vector3(0f, -0.3f, 0.9f), Vector3.zero, new Vector3(1f, 1f, 0.75f), castShadows: false);
                if (i % 3 == 1)
                {
                    Mesh bush = LowPoly.Blob(seed: 300 + i);
                    LowPoly.Part("Bush", piece, bush, art.Grass, new Vector3(0.8f, 0.05f, 1.8f), Vector3.zero, new Vector3(0.8f, 0.45f, 0.6f));
                }
                if (i % 4 == 2)
                {
                    Mesh stone = LowPoly.Blob(rings: 3, segments: 5, jitter: 0.2f, seed: 400 + i);
                    LowPoly.Part("Stone", piece, stone, art.RockDark, new Vector3(-1f, 0f, -0.6f), Vector3.zero, new Vector3(0.4f, 0.25f, 0.3f));
                }
                ground.Pieces.Add(piece);
            }

            // A flat meadow between the ground strip and the hills, so no sky shows through.
            var meadow = new GameObject("Meadow").transform;
            meadow.SetParent(transform, false);
            meadow.localPosition = new Vector3(0f, groundTop - 0.6f, 11f);
            LowPoly.Part("Field", meadow, LowPoly.Prism(4, 1f, 1f, 0.5f, twist: false), art.HillNear, Vector3.zero, Vector3.zero, new Vector3(120f, 1f, 14f), castShadows: false);

            BuildRidges(art.HillNear, null, factor: 0.45f, z: 16f, baseY: groundTop - 3f, peak: 3.2f, width: 60f, count: 3, seed: 10);
            BuildRidges(art.MountainFar, art.Snow, factor: 0.18f, z: 48f, baseY: groundTop - 4f, peak: 15f, width: 110f, count: 3, seed: 20);
            BuildClouds(art.Cloud, groundTop);
        }

        void BuildRidges(Material material, Material snow, float factor, float z, float baseY, float peak, float width, int count, int seed)
        {
            var layer = NewLayer(factor, width * count);
            var random = new System.Random(seed);
            for (int i = 0; i < count; i++)
            {
                var piece = new GameObject("Ridge").transform;
                piece.SetParent(transform, false);
                piece.localPosition = new Vector3(-width * count / 2f + width * (i + 0.5f), 0f, z);
                var heights = new float[17];
                for (int h = 0; h < heights.Length; h++)
                {
                    // Alternating peaks and saddles with random heights; the ends match so pieces tile.
                    bool end = h == 0 || h == heights.Length - 1;
                    float top = h % 2 == 0 ? peak * (0.55f + 0.45f * (float)random.NextDouble()) : peak * 0.3f * (float)random.NextDouble();
                    heights[h] = baseY + 4f + (end ? peak * 0.3f : top);
                }
                LowPoly.Part("Hills", piece, LowPoly.Ridge(width, baseY, heights, 10f), material, Vector3.zero, Vector3.zero, Vector3.one, castShadows: false);
                if (snow != null)
                {
                    for (int h = 2; h < heights.Length - 1; h += 2)
                    {
                        if (heights[h] - baseY < peak * 0.8f)
                        {
                            continue;
                        }
                        float x = -width / 2f + h * width / (heights.Length - 1);
                        Mesh cap = LowPoly.Prism(5, 3.2f, 0f, 3.6f, jitter: 0.15f, seed: seed + h);
                        LowPoly.Part("Snow", piece, cap, snow, new Vector3(x, heights[h] - 3.3f, 5f), Vector3.zero, new Vector3(1f, 1f, 0.6f), castShadows: false);
                    }
                }
                layer.Pieces.Add(piece);
            }
        }

        void BuildClouds(Material material, float groundTop)
        {
            const float width = 90f;
            var layer = NewLayer(0.3f, width);
            var random = new System.Random(5);
            Mesh puff = LowPoly.Blob(rings: 4, segments: 6, jitter: 0.1f, seed: 8);
            for (int i = 0; i < 9; i++)
            {
                var cloud = new GameObject("Cloud").transform;
                cloud.SetParent(transform, false);
                float y = groundTop + 11f + (float)random.NextDouble() * 6f;
                cloud.localPosition = new Vector3(-width / 2f + i * width / 9f, y, 26f + (float)random.NextDouble() * 10f);
                int puffs = 3 + random.Next(3);
                for (int p = 0; p < puffs; p++)
                {
                    float size = 1.4f + (float)random.NextDouble() * 1.2f;
                    var at = new Vector3((p - puffs / 2f) * 1.5f, (float)random.NextDouble() * 0.6f, 0f);
                    LowPoly.Part("Puff", cloud, puff, material, at, Vector3.zero, new Vector3(size * 1.3f, size * 0.8f, size), castShadows: false);
                }
                layer.Pieces.Add(cloud);
            }
        }

        Layer NewLayer(float factor, float width)
        {
            var layer = new Layer { Factor = factor, Width = width };
            layers.Add(layer);
            return layer;
        }

        /// <summary>Scrolls every layer by its share of <paramref name="distance"/>.</summary>
        public void Scroll(float distance)
        {
            foreach (Layer layer in layers)
            {
                float half = layer.Width / 2f;
                foreach (Transform piece in layer.Pieces)
                {
                    Vector3 p = piece.localPosition;
                    p.x -= distance * layer.Factor;
                    if (p.x < -half)
                    {
                        p.x += layer.Width;
                    }
                    piece.localPosition = p;
                }
            }
        }
    }
}
