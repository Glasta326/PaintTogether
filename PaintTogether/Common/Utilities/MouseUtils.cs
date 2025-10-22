using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PaintTogether.Common.DataTypes;

namespace PaintTogether.Common.Utilities
{
    public static class MouseUtils
    {
        public static MouseState State;
        
        public static Vector2 MousePosVector() => new Vector2(State.X, State.Y);

        // TODO : change these hardcoded values when get canvas properly sorted
        public static Vector2 MousePosNormalised() => new Vector2((float)State.X / 800, (float)State.Y / 500);

        public static Point MousePosPoint() => new Point(State.X, State.Y);
        
        /// <summary>
        /// Information about LeftMouseButton
        /// </summary>
        public static ButtonState LeftClick => State.LeftButton;
        
        /// <summary>
        /// Information about RightMouseButton
        /// </summary>
        public static ButtonState RightClick => State.RightButton;

        /// <summary>
        /// Information about MiddleMouseButton
        /// </summary>
        public static ButtonState MiddleClick => State.MiddleButton;
        
        /// <summary>
        /// Stores the position of the mouse at [0] and the position of the mouse last frame at [1]
        /// </summary>
        public static readonly ShiftRegister<Point> MoveHistory = new ShiftRegister<Point>(2);

        public static readonly ShiftRegister<int> ScrollHistory = new ShiftRegister<int>(2);
        
        /// <summary>
        /// How far the scroll wheel has been scrolled this frame. <br/>
        /// Positive values mean scrolled UP. <br/>
        /// Negative values mean scrolled DOWN.
        /// </summary>
        public static int ScrollDelta => -(ScrollHistory[1] - ScrollHistory[0]);
    }
}