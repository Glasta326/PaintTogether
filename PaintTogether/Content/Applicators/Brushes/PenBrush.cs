using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Core.Networking;

namespace PaintTogether.Content.Applicators.Brushes
{
    public class PenBrush : Brush, INetApplicable
    {
        protected override void LoadBrushAssets(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            BrushShader = contentManager.Load<Effect>("Shaders/PenBrushShader");
        }


        protected override Color? BrushDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, List<Point> drawPoints, Rectangle affectedArea, Color _brushColor, int _brushSize, bool isPreview = false)
        {
            return _brushColor;
        }

        public void RecieveNetCall(RecievePacket dataPacket)
        {
            var ourQueue = IncomingRequestQueues.GetOrAdd(dataPacket.Type, _ => new ConcurrentQueue<RecievePacket>());
            ourQueue.Enqueue(dataPacket);
            return;
        }
    }
}