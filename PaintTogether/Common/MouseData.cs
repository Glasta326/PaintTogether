using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PaintTogether.Common.DataTypes;

namespace PaintTogether.Common
{
    /// <summary>
    /// Wapper for data about the mouse in all useful forms
    /// </summary>
    public static class MouseData
    {
        public static MouseState State;
        
        public static Vector2 MousePosVector() => MousePosPoint().ToVector2();

        // TODO : change these hardcoded values when get canvas properly sorted
        public static Vector2 MousePosNormalised() => new Vector2((float)State.X / Main.instance.Window.ClientBounds.Width, (float)State.Y / Main.instance.Window.ClientBounds.Height);

        public static Point MousePosPoint() => new Point
        (
            (int)(State.X / WindowData.ResolutionMultiplier.X),
            (int)(State.Y / WindowData.ResolutionMultiplier.Y)
        );
        
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