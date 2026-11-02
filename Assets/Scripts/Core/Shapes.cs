using System;

namespace FlappyDragon.Core
{
    /// <summary>2D overlap tests on the gameplay plane (x right, y up).</summary>
    public static class Shapes
    {
        /// <summary>Does a circle overlap an axis-aligned rectangle given by its corners?</summary>
        public static bool CircleOverlapsRect(float cx, float cy, float radius,
            float left, float bottom, float right, float top)
        {
            float nearestX = Math.Max(left, Math.Min(cx, right));
            float nearestY = Math.Max(bottom, Math.Min(cy, top));
            float dx = cx - nearestX;
            float dy = cy - nearestY;
            return dx * dx + dy * dy < radius * radius;
        }

        public static bool CirclesOverlap(float ax, float ay, float aRadius, float bx, float by, float bRadius)
        {
            float dx = ax - bx;
            float dy = ay - by;
            float reach = aRadius + bRadius;
            return dx * dx + dy * dy < reach * reach;
        }
    }
}
