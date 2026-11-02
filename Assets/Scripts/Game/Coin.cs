using FlappyDragon.Core;
using UnityEngine;

namespace FlappyDragon.Game
{
    /// <summary>A spinning coin floating in a gate's opening.</summary>
    public sealed class Coin : MonoBehaviour
    {
        public const float Radius = 0.38f;

        static Mesh disc;
        float spin;
        float baseY;

        public CoinKind Kind { get; private set; }

        public Vector2 Centre => transform.position;

        public static Coin Create(Transform parent, GameArt art, CoinKind kind, Vector3 position)
        {
            disc ??= LowPoly.Prism(10, 0.36f, 0.36f, 0.09f, seed: 1);
            var coin = new GameObject(kind + " coin").AddComponent<Coin>();
            coin.transform.SetParent(parent, true);
            coin.transform.position = position;
            coin.Kind = kind;
            coin.baseY = position.y;
            coin.spin = position.x * 40f;
            Material material = kind == CoinKind.Gold ? art.CoinGold : kind == CoinKind.Silver ? art.CoinSilver : art.CoinBronze;
            float size = kind == CoinKind.Gold ? 1.2f : kind == CoinKind.Silver ? 1.08f : 1f;
            // The disc stands on its edge facing the camera and spins around the vertical axis.
            var face = LowPoly.Part("Disc", coin.transform, disc, material, Vector3.zero, new Vector3(90f, 0f, 0f), Vector3.one * size);
            face.localPosition = new Vector3(0f, 0f, -0.045f * size);
            // A raised centre on both faces catches the light differently from the rim.
            var boss = LowPoly.Part("Boss", coin.transform, disc, material, Vector3.zero, new Vector3(90f, 0f, 0f), new Vector3(0.62f, 1.9f, 0.62f) * size);
            boss.localPosition = new Vector3(0f, 0f, -0.0855f * size);
            return coin;
        }

        void Update()
        {
            spin += 160f * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0f, spin, 0f);
            Vector3 p = transform.position;
            transform.position = new Vector3(p.x, baseY + Mathf.Sin(spin * 0.03f) * 0.08f, p.z);
        }
    }
}
