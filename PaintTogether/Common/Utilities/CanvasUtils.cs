using Microsoft.Xna.Framework;

namespace PaintTogether.Common.Utilities
{
    public static class CanvasUtils
    {
        public static bool InCanvas(Vector2 pos)
        {
            if (pos.X < 0 || pos.Y < 0 || pos.X > Main.Canvas.Width || pos.Y > Main.Canvas.Height)
            {
                return false;
            }

            return true;
        }
    }
}