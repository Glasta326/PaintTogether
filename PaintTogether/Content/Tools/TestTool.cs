using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Common;
using PaintTogether.Common.PaintLogger;
using PaintTogether.Common.Utilities;
using PaintTogether.Content.PaintCanvas;
using PaintTogether.Content.UI;

namespace PaintTogether.Content.Tools
{
    public class TestTool : Tool
    {
        protected override void LoadToolAssets(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            ToolShader = contentManager.Load<Effect>("Shaders/TestToolShader");
        }

        protected override Color? ToolDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Point startPos)
        {
            Point _start = startPos;
            Point _mouse = Canvas.ScreenToCanvas(MouseData.MousePosPoint());
            Rectangle drawArea = MathUtils.RectangleXYXY(_start,_mouse);

            clLogger.LogInfo($"{startPos}");

            ToolShader.Parameters["Color"].SetValue(ColorSelector.GetColor().ToVector4());

            spriteBatch.Begin(SpriteSortMode.Immediate,effect: ToolShader);

            ToolShader.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(CommonKeys.DummyTexture,drawArea,Color.White);

            spriteBatch.End();

            return null;
        }
    }
}