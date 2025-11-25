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

        protected override Color? ToolDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Point toolStartPos, Point toolEndPos, Color toolColor, int toolSize)
        {
            Point _start = toolStartPos;
            Point _mouse = toolEndPos;
            Rectangle drawArea = MathUtils.RectangleXYXY(_start,_mouse);

            ToolShader.Parameters["Color"].SetValue(toolColor.ToVector4());
            ToolShader.Parameters["Resolution"].SetValue(new Vector2(drawArea.Width,drawArea.Height));

            spriteBatch.Begin(SpriteSortMode.Immediate,effect: ToolShader);

            ToolShader.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(CommonKeys.DummyTexture,drawArea,Color.White);

            spriteBatch.End();

            return null;
        }
    }
}