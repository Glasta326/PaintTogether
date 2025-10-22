using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Common.DataTypes;
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
            var x = typeof(TestBrush);
            Console.WriteLine(BrushSize);
            base.Update();
        }

        Texture2D t;

        protected override bool Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            graphicsDevice.SetRenderTarget(Main.Canvas);
            BrushShader.Parameters["BrushColor"].SetValue(Color.Red.ToVector4());

            spriteBatch.Begin(SpriteSortMode.Immediate, effect: BrushShader);

            DrawUtils.DrawLine(MouseUtils.MoveHistory[0], MouseUtils.MoveHistory[1], BrushShader, spriteBatch, t, out _, out _, BrushSize);
            spriteBatch.End();

            return false;
        }

        
    }
}