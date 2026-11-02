using UnityEngine;

namespace FlappyDragon.Game
{
    /// <summary>
    /// Frames the play plane for any aspect ratio (the dragon stays a third in from the left
    /// and the ground near the bottom, also on phones in portrait) and adds screen shake.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraRig : MonoBehaviour
    {
        const float Distance = 18f;
        const float BaseHalfHeight = 6.2f;
        const float MinHalfWidth = 7.5f;

        Camera view;
        float aspect = -1f;
        Vector3 framed;
        float trauma;

        /// <summary>Half the visible width of the play plane (z = 0).</summary>
        public float HalfWidth { get; private set; } = 11f;

        public float CentreX => framed.x;

        public float LeftEdge => framed.x - HalfWidth;

        public float RightEdge => framed.x + HalfWidth;

        void Awake()
        {
            view = GetComponent<Camera>();
            Frame();
        }

        /// <summary>Adds screen shake; 1 is a strong hit. Shake fades out on its own.</summary>
        public void Shake(float amount) => trauma = Mathf.Clamp01(trauma + amount);

        void LateUpdate()
        {
            if (!Mathf.Approximately(aspect, view.aspect))
            {
                Frame();
            }
            trauma = Mathf.Max(trauma - Time.unscaledDeltaTime * 1.8f, 0f);
            float strength = trauma * trauma;
            float t = Time.unscaledTime * 32f;
            var offset = new Vector3(Mathf.PerlinNoise(t, 0.3f) - 0.5f, Mathf.PerlinNoise(0.7f, t) - 0.5f, 0f) * (0.9f * strength);
            transform.position = framed + offset;
            transform.rotation = Quaternion.Euler(0f, 0f, (Mathf.PerlinNoise(t, t) - 0.5f) * 4f * strength);
        }

        void Frame()
        {
            aspect = view.aspect;
            float halfHeight = Mathf.Max(BaseHalfHeight, MinHalfWidth / Mathf.Max(aspect, 0.1f));
            HalfWidth = halfHeight * aspect;
            view.fieldOfView = 2f * Mathf.Atan(halfHeight / Distance) * Mathf.Rad2Deg;
            float x = GameDirector.DragonX + 0.34f * HalfWidth;
            float y = GameDirector.GroundTop + 0.815f * halfHeight;
            framed = new Vector3(x, y, -Distance);
            transform.position = framed;
        }
    }
}
