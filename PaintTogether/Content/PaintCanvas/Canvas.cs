using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Common;
using PaintTogether.Common.PaintLogger;

namespace PaintTogether.Content.PaintCanvas
{
    public static class Canvas
    {
        public static CanvasLayers Layers = new CanvasLayers();

        public static CanvasCamera Camera = new CanvasCamera();

        /// <summary>
        /// Size of the canvas area in pixels.
        /// </summary>
        public static Point Resolution { get; set; }

        public static void Init(GraphicsDevice graphicsDevice)
        {
            // Initalise the canvas with a single white layer
            Layers.AddLayer();
            
            graphicsDevice.SetRenderTarget(Layers.ActiveLayer);
            graphicsDevice.Clear(Color.White);
        }

        /// <summary>
        /// Transforms the canvas coordinates into output display coordinates based on zoom level and camera position <br/>
        /// The canvas NEVER ACTUALLY MOVES. We simply modify where it is drawn and the scaling of it's drawing <br/>
        /// A pixel at (10,10) will always be accessable at canvas[10,10], <br/>
        /// but with a zoom of 2 and camera position of 100, 100, the pixel at [10,10] will be drawn to the screen position of [220,220] <br/>
        /// because we do translation first (10 + 100 = 110) <br/>
        /// and then apply the zoom (110 * 2) = 220
        /// </summary>
        public static Matrix CanvasTransform()
        {
            return Matrix.CreateTranslation(new Vector3(Camera.Position, 0f)) *
            Matrix.CreateScale(Camera.Zoom);
        }

        /// <summary>
        /// Likley useless as you can just get the screen coordinates of the mouse or something directly instead of inverting the canvas transform
        /// </summary>
        /// <param name="canvasPos">the "canvas coordinate",<br/>
        /// so the pixel coordinate you might feed it could be at the very middle of a 800x500 canvas,<br/>
        /// so it would be [400,250]<br/>
        /// and then with scale 2 and camerapos of [100,0], it would return 900,500 which is the Screen space position</param>
        public static Vector2 CanvasToScreen(Vector2 canvasPos)
        {
            Matrix inverse = CanvasTransform();
            return Vector2.Transform(canvasPos, inverse);
        }

        /// <summary>
        /// Transforms a screen coordinate into a canvas coordinate. <br/>
        /// Essentially the inverse of <see cref="CanvasTransform()"/> and applied to a vector2 <br/>
        /// So with zoom of 2 and camera position of 100,100, clicking at pixel [220,220] will result in : <br/>
        /// invert scale : [220,220] / 2 = [110,100]<br/>
        /// invert scale : [110,110] - [100,100] = [10,10]<br/>
        /// </summary>
        /// <param name="screenPos">The coordinates on the screen. Like the typical measurable ones using the pixels on the monitor <br/>
        /// So in the 800x450 game window, the top left is (0,0), the middle is 400,225, ect</param>
        public static Vector2 ScreenToCanvas(Vector2 screenPos)
        {
            Matrix inverse = Matrix.Invert(CanvasTransform());
            return Vector2.Transform(screenPos, inverse);
        }

        public static Point ScreenToCanvas(Point screenPos)
        {
            Matrix inverse = Matrix.Invert(CanvasTransform());
            return Vector2.Transform(screenPos.ToVector2(), inverse).ToPoint();
        }
    }
}