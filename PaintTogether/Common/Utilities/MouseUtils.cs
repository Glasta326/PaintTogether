using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace PaintTogether.Common.Utilities
{
    public static class MouseUtils
    {
        public static Vector2 MousePosVector()
        {
            return new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        }
        
        public static Point MousePosPoint()
        {
            return new Point(Mouse.GetState().X, Mouse.GetState().Y);
        }
    }
}