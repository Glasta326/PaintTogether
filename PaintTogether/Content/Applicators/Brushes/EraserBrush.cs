using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Common.PaintLogger;
using PaintTogether.Common.Utilities;

namespace PaintTogether.Content.Applicators.Brushes
{
    public class EraserBrush : Brush
    {
        BlendState Erase;

        protected override void LoadBrush()
        {
            Erase = new BlendState
            {
                ColorSourceBlend = Blend.Zero,
                AlphaSourceBlend = Blend.Zero,
                ColorDestinationBlend = Blend.InverseSourceAlpha,
                AlphaDestinationBlend = Blend.InverseSourceAlpha,
                ColorBlendFunction = BlendFunction.Add,
                AlphaBlendFunction = BlendFunction.Add
            };
        }

        protected override void LoadBrushAssets(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            BrushShader = contentManager.Load<Effect>("Shaders/PenBrushShader");
        }

        protected override Color? BrushDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, List<Point> drawPoints, Color _brushColor, int _brushSize, bool isPreview)
        {
            if (isPreview)
            {
                BrushShader.Parameters["BrushColor"]?.SetValue(new Color(64,0,0,64).ToVector4());
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, effect: BrushShader);
            }
            else
            {
                BrushShader.Parameters["BrushColor"]?.SetValue(Color.White.ToVector4());
                spriteBatch.Begin(SpriteSortMode.Immediate, blendState: Erase, effect: BrushShader);
            }

            for (int i = 1; i < drawPoints.Count; i++)
            {
                spriteBatch.DrawLine(drawPoints[i - 1], drawPoints[i], BrushShader, _brushSize);
            }

            spriteBatch.End();


            return null;
        }
    }
}