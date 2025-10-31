using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Common;
using PaintTogether.Common.Utilities;
using PaintTogether.Content.UI;

namespace PaintTogether.Content.Brushes
{
    public class PenBrush : Brush
    {
        Effect UI;

        protected override void LoadBrushAssets(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            BrushShader = contentManager.Load<Effect>("Shaders/PenBrushShader");
            UI = contentManager.Load<Effect>("Shaders/Eraser");
        }

        public override void UiDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {

            UI.Parameters["BrushColor"].SetValue(Color.White.ToVector4());

            spriteBatch.Begin(SpriteSortMode.Immediate, effect: UI);
            Rectangle region = MathUtils.SimpleSquare(MouseData.MousePosPoint(), BrushSize);
            UI.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(CommonKeys.DummyTexture, region, Color.White);
            spriteBatch.End();

            spriteBatch.Begin();

            spriteBatch.DrawString(Main.font, $"{MouseData.MousePosVector()}\n {WindowData.ResolutionMultiplier}", MouseData.MousePosVector(), Color.White);
            
            spriteBatch.End();
        }

        protected override Color? BrushDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Point offset)
        {
            return ColorSelector.GetColor();
        }
    }
}