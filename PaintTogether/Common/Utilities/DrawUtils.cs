using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PaintTogether.Common.Utilities
{
    public static class DrawUtils
    {
        public static bool DrawLine(Point startPos, Point endPos, Effect shader, SpriteBatch _spriteBatch , out Point hit, out int iters)
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

            hit = endPos;
            iters = 0;
            
            // please
            if (!CanvasUtils.InCanvas(startPos.ToVector2()) && !CanvasUtils.InCanvas(endPos.ToVector2()))
            {
                return true;
            }

            
            while (true)
            {
                if (CanvasUtils.InCanvas(new Vector2(x0,y0)))
                {
                    shader.Parameters["BrushCenter"].SetValue(new Vector2(x0 / 800f, y0 / 500f));
                    shader.CurrentTechnique.Passes[0].Apply();
                    _spriteBatch.Draw(Main.Canvas, Vector2.Zero, Color.White);
                }
                
                
                

                iters += 1;
                if (iters == 1)
                {
                    continue;
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
    }
}