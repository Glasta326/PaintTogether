using System.Collections.Concurrent;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Common.PaintLogger;
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

        public override Color? ToolDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Point toolStartPos, Point toolEndPos, Rectangle affectedArea, Color toolColor, int toolSize)
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

        // From here, we need to read the correct params, and then somehow queue up the draw command?
        // unsure how we do that ngl
        // currently to draw normally the MainDraw() checks if justLetGo = true and then runs ToolDraw()
        // Perhaps we add something to MainDraw that checks the queue of drawFuncs and executes them all every time its called
        // Or we do it kind of like the undoSystem does?
        // where it creates objects with actions that can be done and undone whenever
        // except in our case instead of can be done whenever its "do it as soon as possible"
        public void RecieveNetCall(RecievePacket dataPacket)
        {
            var ourQueue = IncomingRequestQueues.GetOrAdd(dataPacket.Type, _ => new ConcurrentQueue<RecievePacket>());
            ourQueue.Enqueue(dataPacket);
            return;


            //Element.Get<TestTool>().ApplyTool()


            //Element.Get<TestTool>().OnRecieve(22,new BinaryReader(null));
        }

    }
}