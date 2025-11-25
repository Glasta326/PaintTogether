using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Common.Utilities;

namespace PaintTogether.Content.Tools
{
    public class TestLineTool : Tool
    {
        protected override void LoadToolAssets(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            ToolShader = contentManager.Load<Effect>("Shaders/PenBrushShader");
        }

        protected override Color? ToolDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Point toolStartPos, Point toolEndPos, Color toolColor, int toolSize)
        {
            ToolShader.Parameters["BrushColor"].SetValue(toolColor.ToVector4());
            spriteBatch.Begin(effect:ToolShader);

            for (int i = 1; i < ActiveCursorHistory.Count; i++)
            {
                spriteBatch.DrawLine(ActiveCursorHistory[i-1], ActiveCursorHistory[i], ToolShader, toolSize); // we need to capture toolsize!!!
            }

            spriteBatch.End();  
            return null;
        }
    }
}