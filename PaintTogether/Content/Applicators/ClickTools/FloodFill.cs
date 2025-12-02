using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Common.Utilities;
using PaintTogether.Content.PaintCanvas;

namespace PaintTogether.Content.Applicators.ClickTools
{
    public class FloodFill : ClickTool
    {
        protected override void LoadClickToolAssets(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            base.LoadClickToolAssets(graphicsDevice, contentManager);
        }


        protected override Rectangle ToolDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, int layerIndex, Point drawPoint, Color drawColor)
        {
            // get the pixel color we are filling in over
            Color? rootPixel = DrawUtils.TryGetPixel(Canvas.Layers[layerIndex], drawPoint);

            // And immediatly return if the user clicked somewhere bad
            if (!rootPixel.HasValue)
            {
                return Rectangle.Empty;
            }

            spriteBatch.Begin();

            // TODO: this is slow as shit!!1
            Queue<Point> adds = new Queue<Point>();
            List<Point> visited = new List<Point>();
            Point place = drawPoint;

            adds.Enqueue(drawPoint);
            while (adds.Count > 0)
            {
                place = adds.Dequeue();
                if (visited.Contains(place))
                {
                    continue;
                }
                visited.Add(place);

                Color? placeColor = DrawUtils.TryGetPixel(Canvas.Layers[layerIndex], place);

                if (placeColor.HasValue && placeColor.Value == rootPixel.Value)
                {
                    spriteBatch.Draw(CommonKeys.WhitePixel, place.ToVector2(), drawColor);
                    if (!visited.Contains(place + new Point(-1, 0)))
                    {
                        adds.Enqueue(place + new Point(-1, 0));
                    }
                    if (!visited.Contains(place + new Point(1, 0)))
                    {
                        adds.Enqueue(place + new Point(1, 0));
                    }
                    if (!visited.Contains(place + new Point(0, -1)))
                    {
                        adds.Enqueue(place + new Point(0, -1));
                    }
                    if (!visited.Contains(place + new Point(0, 1)))
                    {
                        adds.Enqueue(place + new Point(0, 1));
                    }
                }

                Console.WriteLine(visited.Count);
            }


            spriteBatch.End();



            return MathUtils.SimpleSquare(drawPoint, 1);
        }
    }
}