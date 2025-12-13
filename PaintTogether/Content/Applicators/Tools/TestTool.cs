using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Common.Utilities;
using PaintTogether.Core;

namespace PaintTogether.Content.Applicators.Tools
{
    public class TestTool : DragTool
    {
        protected override void LoadToolAssets(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            ToolShader = contentManager.Load<Effect>("Shaders/TestToolShader");
        }

       // private void test(binaryreader)
       // {
            // handle it like tmod does, where you get the reader for the right packet type and can do .readint32, .readstring and so on
            //ToolDraw(Main.spriteBatch, Main.instance.GraphicsDevice, data[0],data[1],data[2],data[3]);
        //}

        public override Color? ToolDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Point toolStartPos, Point toolEndPos, Color toolColor, int toolSize)
        {
            Point _start = toolStartPos;
            Point _mouse = toolEndPos;
            Rectangle drawArea = MathUtils.RectangleXYXY(_start,_mouse);

            ToolShader.Parameters["Color"].SetValue(toolColor.ToVector4());
            ToolShader.Parameters["Resolution"].SetValue(new Vector2(drawArea.Width,drawArea.Height));
            ToolShader.Parameters["Width"].SetValue(toolSize);

            spriteBatch.Begin(SpriteSortMode.Immediate,effect: ToolShader);

            ToolShader.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(CommonKeys.DummyTexture,drawArea,Color.White);

            spriteBatch.End();


            

            return null;
        }
    }
}