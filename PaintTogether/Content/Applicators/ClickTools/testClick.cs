using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Common;
using PaintTogether.Common.PaintLogger;
using PaintTogether.Common.Utilities;
using PaintTogether.Content.Applicators.Tools;
using PaintTogether.Content.PaintCanvas;
using PaintTogether.Content.UI;
using PaintTogether.Core;

namespace PaintTogether.Content.Applicators.ClickTools
{
    public class testClick : ClickTool
    {

        protected override void LoadClickToolAssets(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {

        }

        protected override void UpdateClickTool()
        {
            base.UpdateClickTool();
        }

        protected override void OnClick()
        {

            Color? c = DrawUtils.TryGetPixel(Canvas.Layers.ActiveLayer, MouseData.MousePosCanvasSpace());
            if (!c.HasValue)
            {
                clLogger.LogWarning($"Attempted to read color value outside of canvas");
                return;
            }

            if (clLogger.VerboseLogging)
            {
                clLogger.LogInfo($"Set color to: {c}");
            }
            ColorSelector.hexCode = $"{c.Value.R:X2}{c.Value.G:X2}{c.Value.B:X2}";
        }

        protected override Rectangle ToolDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, int layerIndex, Point drawPoint, Color drawColor)
        {
            //TestLineTool x = Element.Get<TestLineTool>();
            //x.ToolDraw(spriteBatch, graphicsDevice, Point.Zero, new Point(100, 100), Color.Red, 5);
            return Rectangle.Empty;
        }

    }
}