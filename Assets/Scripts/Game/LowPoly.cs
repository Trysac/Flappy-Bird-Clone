using System.Collections.Generic;
using UnityEngine;

namespace FlappyDragon.Game
{
    /// <summary>
    /// Builds flat-shaded low-poly meshes in code (every triangle has its own vertices, so the
    /// lighting shows the facets). All the game's models are assembled from these shapes.
    /// </summary>
    public static class LowPoly
    {
        /// <summary>A tapered n-sided column standing on the origin, <paramref name="height"/> tall.</summary>
        public static Mesh Prism(int sides, float bottomRadius, float topRadius, float height,
            float jitter = 0f, int seed = 0, bool capTop = true, bool twist = true)
        {
            var random = new System.Random(seed);
            var bottom = new Vector3[sides];
            var top = new Vector3[sides];
            float turn = twist ? (float)random.NextDouble() * Mathf.PI * 2f : Mathf.PI / sides;
            for (int i = 0; i < sides; i++)
            {
                float angle = turn + i * Mathf.PI * 2f / sides;
                var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                bottom[i] = direction * (bottomRadius * (1f + Jitter(random, jitter)));
                top[i] = direction * (topRadius * (1f + Jitter(random, jitter))) + Vector3.up * height;
            }
            var triangles = new List<Vector3>();
            for (int i = 0; i < sides; i++)
            {
                int j = (i + 1) % sides;
                Quad(triangles, bottom[i], bottom[j], top[j], top[i]);
                if (capTop && topRadius > 0f)
                {
                    triangles.Add(Vector3.up * height);
                    triangles.Add(top[i]);
                    triangles.Add(top[j]);
                }
                triangles.Add(Vector3.zero);
                triangles.Add(bottom[j]);
                triangles.Add(bottom[i]);
            }
            return Build(triangles, "Prism");
        }

        /// <summary>A rough ellipsoid: a coarse sphere whose points are pushed in and out at random.</summary>
        public static Mesh Blob(int rings = 4, int segments = 7, float jitter = 0.12f, int seed = 0)
        {
            var random = new System.Random(seed);
            var points = new Vector3[rings + 1, segments];
            for (int r = 0; r <= rings; r++)
            {
                float polar = Mathf.PI * r / rings;
                for (int s = 0; s < segments; s++)
                {
                    float azimuth = Mathf.PI * 2f * s / segments + (r % 2) * Mathf.PI / segments;
                    var point = new Vector3(
                        Mathf.Sin(polar) * Mathf.Cos(azimuth),
                        Mathf.Cos(polar),
                        Mathf.Sin(polar) * Mathf.Sin(azimuth));
                    bool pole = r == 0 || r == rings;
                    points[r, s] = pole ? point : point * (1f + Jitter(random, jitter));
                }
            }
            var triangles = new List<Vector3>();
            for (int r = 0; r < rings; r++)
            {
                for (int s = 0; s < segments; s++)
                {
                    int t = (s + 1) % segments;
                    Vector3 a = points[r, s], b = points[r, t], c = points[r + 1, t], d = points[r + 1, s];
                    if (r > 0)
                    {
                        triangles.Add(a);
                        triangles.Add(b);
                        triangles.Add(d);
                    }
                    if (r < rings - 1)
                    {
                        triangles.Add(b);
                        triangles.Add(c);
                        triangles.Add(d);
                    }
                }
            }
            return Build(triangles, "Blob");
        }

        /// <summary>A flat polygon in the XZ plane fanned from its first point, visible from both sides.</summary>
        public static Mesh Membrane(IReadOnlyList<Vector2> outline)
        {
            var triangles = new List<Vector3>();
            Vector3 hub = new Vector3(outline[0].x, 0f, outline[0].y);
            for (int i = 1; i < outline.Count - 1; i++)
            {
                Vector3 a = new Vector3(outline[i].x, 0f, outline[i].y);
                Vector3 b = new Vector3(outline[i + 1].x, 0f, outline[i + 1].y);
                triangles.Add(hub);
                triangles.Add(a);
                triangles.Add(b);
                triangles.Add(hub);
                triangles.Add(b);
                triangles.Add(a);
            }
            return Build(triangles, "Membrane", faceOutwards: false);
        }

        /// <summary>A strip of hills: a ridge line along X with a flat back, for distant layers.</summary>
        public static Mesh Ridge(float width, float baseY, float[] heights, float depth)
        {
            var triangles = new List<Vector3>();
            float step = width / (heights.Length - 1);
            for (int i = 0; i < heights.Length - 1; i++)
            {
                float x0 = -width / 2f + i * step;
                float x1 = x0 + step;
                var frontBottom0 = new Vector3(x0, baseY, 0f);
                var frontBottom1 = new Vector3(x1, baseY, 0f);
                var peak0 = new Vector3(x0, heights[i], depth * 0.5f);
                var peak1 = new Vector3(x1, heights[i + 1], depth * 0.5f);
                var back0 = new Vector3(x0, baseY, depth);
                var back1 = new Vector3(x1, baseY, depth);
                Quad(triangles, frontBottom0, peak0, peak1, frontBottom1);
                Quad(triangles, peak0, back0, back1, peak1);
            }
            return Build(triangles, "Ridge", facing: Vector3.up);
        }

        /// <summary>Adds a child object that shows <paramref name="mesh"/> with <paramref name="material"/>.</summary>
        public static Transform Part(string name, Transform parent, Mesh mesh, Material material,
            Vector3 position, Vector3 euler, Vector3 scale, bool castShadows = true)
        {
            var part = new GameObject(name);
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localEulerAngles = euler;
            part.transform.localScale = scale;
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = castShadows
                ? UnityEngine.Rendering.ShadowCastingMode.On
                : UnityEngine.Rendering.ShadowCastingMode.Off;
            return part.transform;
        }

        static void Quad(List<Vector3> triangles, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            triangles.Add(a);
            triangles.Add(c);
            triangles.Add(b);
            triangles.Add(a);
            triangles.Add(d);
            triangles.Add(c);
        }

        /// <summary>
        /// Makes a mesh from a triangle list. With <paramref name="faceOutwards"/> every triangle is
        /// turned to face away from the shape's centre (right for the convex shapes here), or
        /// towards <paramref name="facing"/> when given.
        /// </summary>
        static Mesh Build(List<Vector3> vertices, string name, bool faceOutwards = true, Vector3? facing = null)
        {
            if (faceOutwards)
            {
                Vector3 centre = Vector3.zero;
                foreach (Vector3 v in vertices)
                {
                    centre += v;
                }
                centre /= Mathf.Max(vertices.Count, 1);
                for (int i = 0; i + 2 < vertices.Count; i += 3)
                {
                    Vector3 a = vertices[i], b = vertices[i + 1], c = vertices[i + 2];
                    Vector3 normal = Vector3.Cross(b - a, c - a);
                    Vector3 outwards = facing ?? (a + b + c) / 3f - centre;
                    if (Vector3.Dot(normal, outwards) < 0f)
                    {
                        vertices[i + 1] = c;
                        vertices[i + 2] = b;
                    }
                }
            }
            var indices = new int[vertices.Count];
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = i;
            }
            var mesh = new Mesh { name = name };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(indices, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        static float Jitter(System.Random random, float amount) => ((float)random.NextDouble() * 2f - 1f) * amount;
    }
}
