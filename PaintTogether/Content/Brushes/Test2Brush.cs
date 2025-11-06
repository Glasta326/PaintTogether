using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Common;
using PaintTogether.Common.Utilities;
using PaintTogether.Content.PaintCanvas;

namespace PaintTogether.Content.Brushes
{
    public class Test2Brush : Brush
    {
        protected override void LoadBrushAssets(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            BrushShader = contentManager.Load<Effect>("Shaders/test2");
            t = new Texture2D(graphicsDevice, 1, 1);
        }

        protected override void UpdateBrush()
        {
            var x = typeof(Test2Brush);
            base.UpdateBrush();
        }
        Texture2D t;
        protected override Color? BrushDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            graphicsDevice.SetRenderTarget(Canvas.Layers.ActiveLayer);
            BrushShader.Parameters["BrushColor"].SetValue(Color.White.ToVector4());

            BlendState Erase = new BlendState
            {
                ColorSourceBlend = Blend.Zero,
                AlphaSourceBlend = Blend.Zero,
                ColorDestinationBlend = Blend.InverseSourceAlpha,
                AlphaDestinationBlend = Blend.InverseSourceAlpha,
                ColorBlendFunction = BlendFunction.Add,
                AlphaBlendFunction = BlendFunction.Add
            };

            spriteBatch.Begin(SpriteSortMode.Immediate, blendState: Erase, effect: BrushShader);
            spriteBatch.DrawLine(Canvas.ScreenToCanvas(MouseData.MoveHistory[0]), Canvas.ScreenToCanvas(MouseData.MoveHistory[1]), BrushShader, BrushSize);
            spriteBatch.End();

            return null;
        }
    }
}