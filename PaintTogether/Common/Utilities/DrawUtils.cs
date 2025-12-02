using System;
using System.Net.NetworkInformation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Common.PaintLogger;
using PaintTogether.Content.Applicators.Tools;

namespace PaintTogether.Common.Utilities
{
    public static class DrawUtils
    {

        /// <summary>
        /// Draws a given brush shader over a line between two points
        /// </summary>
        /// <param name="_spriteBatch"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <param name="shader"></param>
        /// <param name="r">Radius of the square region the shader is drawn with. Essentially brush thickness</param>
        /// <returns></returns>
        public static bool DrawLine(this SpriteBatch _spriteBatch, Point startPos, Point endPos, Effect shader, int r)
        {
            // This is essentially just Bresenham's line algorithm with some tweaks
            // https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm

            int x0 = startPos.X;
            int y0 = startPos.Y;
            int x1 = endPos.X;
            int y1 = endPos.Y;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);

            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;

            int err = dx - dy;


            // please
            if (!CanvasUtils.InCanvas(startPos.ToVector2()) && !CanvasUtils.InCanvas(endPos.ToVector2()))
            {
                return true;
            }


            while (true)
            {
                if (CanvasUtils.InCanvas(new Vector2(x0, y0)))
                {
                    Rectangle region = MathUtils.SimpleSquare(new Point(x0, y0), r);
                    shader.CurrentTechnique.Passes[0].Apply();
                    _spriteBatch.Draw(CommonKeys.DummyTexture, region, Color.White);
                }

                if (x0 == x1 && y0 == y1)
                {
                    break;
                }

                int e2 = 2 * err;

                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
            return false;
        }

        /// <summary>
        /// Simplified <see cref="DrawLine"/> that assumes brush using <see cref="Brush.BrushSize"/> and <see cref="MouseData.MoveHistory"/>
        /// </summary>
        public static bool DrawLine(this SpriteBatch _spriteBatch, Effect shader)
        {
            return _spriteBatch.DrawLine(MouseData.MoveHistory[0], MouseData.MoveHistory[1], shader, DragTool.ToolSize);
        }

        /// <summary>
        /// Copies a region of pixels from a source rendertarget to a destination rendertarget
        /// </summary>
        /// <param name="source">rendertarget to copy FROM</param>
        /// <param name="sourceRect">area of rendertarget to copy FROM</param>
        /// <param name="dest">rendertarget to copy TO</param>
        /// <param name="destRect">area of rendertarget copied TO</param>
        public static void CopySection(this SpriteBatch sb, RenderTarget2D source, Rectangle sourceRect, RenderTarget2D dest, Rectangle destRect)
        {
            Main.instance.GraphicsDevice.SetRenderTarget(dest);
            sb.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp);
            sb.Draw(source, destRect, sourceRect, Color.White);
            sb.End();
            Main.instance.GraphicsDevice.SetRenderTarget(null);
        }

        /// <summary>
        /// Attempts to get the color value of a specfic pixel inside a rendertarget
        /// </summary>
        public static Color? TryGetPixel(this RenderTarget2D target, Point pos)
        {
            if (!target.Bounds.Contains(pos))
            {
                clLogger.LogWarning($"Could not retrieve pixel value from the target!\nThe position {pos} is outside the bounds {target.Bounds}");
                return null;
            }
            Color[] p = new Color[1];
            Rectangle r = new Rectangle(pos.X, pos.Y, 1, 1);
            target.GetData<Color>(0, r, p, 0, 1);
            return p[0];
        }

    }
}