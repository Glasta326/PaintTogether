using System;
using System.Net.NetworkInformation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Content.Brushes;

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
        /// Simplified <see cref="DrawLine"/> that assumes brush using <see cref="Brush.BrushSize"/> and <see cref="MouseUtils.MoveHistory"/>
        /// </summary>
        public static bool DrawLine(this SpriteBatch _spriteBatch, Effect shader)
        {
            return _spriteBatch.DrawLine(MouseUtils.MoveHistory[0], MouseUtils.MoveHistory[1], shader, Brush.BrushSize);
        }

    }
}