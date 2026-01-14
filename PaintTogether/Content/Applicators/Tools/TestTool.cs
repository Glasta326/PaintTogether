using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Common.Utilities;
using PaintTogether.Core.Networking;

namespace PaintTogether.Content.Applicators.Tools
{
    // Fairly sure this is the rectangle tool
    public class TestTool : DragTool, INetApplicable
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
            Rectangle drawArea = MathUtils.RectangleXYXY(_start, _mouse);

            ToolShader.Parameters["Color"].SetValue(toolColor.ToVector4());
            ToolShader.Parameters["Resolution"].SetValue(new Vector2(drawArea.Width, drawArea.Height));
            ToolShader.Parameters["Width"].SetValue(toolSize);

            spriteBatch.Begin(SpriteSortMode.Immediate, effect: ToolShader);

            ToolShader.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(CommonKeys.DummyTexture, drawArea, Color.White);

            spriteBatch.End();




            return null;
        }

        public void RecieveNetCall(byte owner, BinaryReader reader)
        {
            int x = reader.ReadInt32();
            int y = reader.ReadInt32();
            Point toolStartPos = new Point(x, y);

            x = reader.ReadInt32();
            y = reader.ReadInt32();
            Point toolEndPos = new Point(x, y);

            byte[] colorData = reader.ReadBytes(4);
            Color toolColor = new Color(colorData[0], colorData[1], colorData[2], colorData[3]);

            int toolSize = reader.ReadInt32();
            

            //Element.Get<TestTool>().ApplyTool()


            //Element.Get<TestTool>().OnRecieve(22,new BinaryReader(null));
        }

        public void SendNetCall(BinaryWriter writer)
        {
            
        }
    }
}