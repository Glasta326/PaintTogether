using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Common.Utilities;

namespace PaintTogether.Content.Applicators.Tools
{
    public class TestLineTool : DragTool
    {
        protected override void LoadToolAssets(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            ToolShader = contentManager.Load<Effect>("Shaders/PenBrushShader");
        }

        public override Color? ToolDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Point toolStartPos, Point toolEndPos, Color toolColor, int toolSize)
        {
            ToolShader.Parameters["BrushColor"].SetValue(toolColor.ToVector4());
            spriteBatch.Begin(effect: ToolShader);
            
            spriteBatch.DrawLine(toolStartPos, toolEndPos, ToolShader, toolSize); 

            spriteBatch.End();
            return null;
        }
    }
}