using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Common.DataTypes;
using PaintTogether.Common.PaintLogger;
using PaintTogether.Common.Utilities;

namespace PaintTogether.Content.Brushes
{
    public class TestBrush : Brush
    {
        protected override void LoadAssets(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            BrushShader = contentManager.Load<Effect>("Shaders/test2");
            t = new Texture2D(graphicsDevice, 1, 1);
        }

        protected override void Update()
        {
            base.Update();
        }

        Texture2D t;


        protected override Color? BrushDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            return Color.Aqua;
        }


    }
}