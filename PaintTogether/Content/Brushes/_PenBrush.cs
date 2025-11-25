using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Content.UI;

namespace PaintTogether.Content.Brushes
{
    public class _PenBrush : _Brush
    {
        protected override void Load_BrushAssets(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            _BrushShader = contentManager.Load<Effect>("Shaders/PenBrushShader");
        }


        protected override Color? _BrushDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, List<Point> drawPoints, Color _brushColor, int _brushSize)
        {
            return ColorSelector.GetColor();
        }
    }
}