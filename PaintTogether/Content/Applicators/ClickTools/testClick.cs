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

            Color c = DrawUtils.GetPixel(Canvas.Layers.ActiveLayer,MouseData.MousePosPoint());
            if (clLogger.VerboseLogging)
            {
                clLogger.LogInfo($"Set color to: {c}");
            }
            ColorSelector.hexCode = $"{c.R:X2}{c.G:X2}{c.B:X2}";
        }

        protected override Rectangle ToolDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, int layerIndex, Point drawPoint, Color drawColor)
        {   
            return Rectangle.Empty;
        }

    }
}