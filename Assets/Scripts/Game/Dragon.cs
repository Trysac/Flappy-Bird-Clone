using FlappyDragon.Core;
using UnityEngine;

namespace FlappyDragon.Game
{
    /// <summary>The player: a <see cref="FlightModel"/> driving the dragon model's height and pose.</summary>
    public sealed class Dragon : MonoBehaviour
    {
        public const float Radius = 0.42f;

        DragonView view;
        float hoverTime;

        public FlightModel Flight { get; private set; }

        public Vector2 Centre => new Vector2(transform.position.x, Flight.Height);

        public static Dragon Create(Transform parent, GameArt art, FlightSettings settings, float x)
        {
            var dragon = new GameObject("Dragon").AddComponent<Dragon>();
            dragon.transform.SetParent(parent, false);
            dragon.transform.position = new Vector3(x, 0f, 0f);
            dragon.Flight = new FlightModel(settings, 0f);
            dragon.view = dragon.gameObject.AddComponent<DragonView>();
            dragon.view.Build(art);
            dragon.Place();
            return dragon;
        }

        public void Flap()
        {
            Flight.Flap();
            view.OnFlap();
        }

        /// <summary>Flies under gravity (playing, or tumbling after a crash).</summary>
        public void Fly(float deltaTime, bool crashed)
        {
            Flight.Step(deltaTime);
            Place();
            view.Animate(deltaTime, Flight.Pitch, crashed);
        }

        /// <summary>Bobs in place while waiting for the first flap.</summary>
        public void Hover(float deltaTime, float height)
        {
            hoverTime += deltaTime;
            Flight.Reset(height + Mathf.Sin(hoverTime * 2.6f) * 0.35f);
            Place();
            view.Animate(deltaTime, Mathf.Cos(hoverTime * 2.6f) * 8f, crashed: false);
        }

        public void ResetTo(float height)
        {
            hoverTime = 0f;
            Flight.Reset(height);
            view.ResetPose();
            Place();
        }

        void Place()
        {
            Vector3 p = transform.position;
            transform.position = new Vector3(p.x, Flight.Height, p.z);
        }
    }
}
