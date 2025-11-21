using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PaintTogether.Common;
using PaintTogether.Common.PaintLogger;
using PaintTogether.Common.Utilities;
using PaintTogether.Content.Brushes;
using PaintTogether.Content.PaintCanvas;
using PaintTogether.Content.UI;
using PaintTogether.Core;
using PaintTogether.Core.UndoSystem;

namespace PaintTogether.Content.Tools
{
    public abstract class Tool : Element
    {
        public override bool AutoUpdate => false;

        // Just read the Brush's size value, it's shared anyway
        private static int ToolSize => Brush.BrushSize;

        /// <summary>
        /// What this tool actually does to the selected region
        /// </summary>
        public virtual Effect ToolShader { get; protected set; }

        #region Loading

        public sealed override void Load()
        {
            LoadTool();
        }

        public sealed override void LoadAssets(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            // Emergency fallback but this should be overriden
            ToolShader = contentManager.Load<Effect>("Shaders/PenBrushShader");

            LoadToolAssets(graphicsDevice, contentManager);
        }

        /// <summary>
        /// Load any non-asset content here
        /// </summary>
        protected virtual void LoadTool() { }

        /// <summary>
        /// Load asset-related content here
        /// </summary>
        protected virtual void LoadToolAssets(GraphicsDevice graphicsDevice, ContentManager contentManager) { }

        /// <summary>
        /// Unloads anything loaded in <see cref="LoadBrush"/>
        /// </summary>
        protected virtual void UnloadTool() { }

        #endregion

        /// <summary>
        /// The original point the user started to hold down the mouse button from, so point A on a line tool for example. Locks in place upon first click
        /// </summary>
        private Point ToolStartPos; // There's no ToolEndPos, we can just get the current mouse position when the tool is applied.

        /// <summary>
        /// Represents whether this tool is activley being used right now
        /// </summary>
        public bool Active = false;

        public override void Update()
        {
            // Ok so this seems a bit wierd but the idea is when mouse is .JustClicked, active will be set true after this check, so it remains true
            // Then on the next frame, active is still true when we get to this test, so we check for the button still being held
            // Essentially this also makes sure that the tool was also active the previous frame
            if (MouseData.LeftClick == ButtonState.Pressed && Active)
            {
                Active = true;
            }
            else
            {
                Active = false;
            }

            // As soon as we start to use the tool, immediatly log where we first started clicking.
            // Also im fairly sure there's no point storing this as screen space so we insta-convert it to canvas space
            if (MouseData.JustClicked)
            {
                Active = true;
                ToolStartPos = Canvas.ScreenToCanvas(MouseData.MousePosPoint());
            }

            // At this point, the mouse button is released, but we still need to be considered active this frame because now that the button is released,
            // We finally actually create the draw command and apply it to the canvas, instead of just doing all the preview stuff
            if (MouseData.JustLetGo)
            {   
                Active = true;
            }

            Brush.BrushSize += (int)(MouseData.ScrollDelta * 0.00833333333333f); // divide by 120
        }  

        protected virtual void UpdateTool() { }

        /// <summary>
        /// Draw logic for this tool. Should always be run from Main's Draw() <br/>
        /// </summary>
        public void MainDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (!Active)
            {
                return;
            }

            // We need to pre-process the applied region for networking and undo history and all that when the tool is actually used
            if (MouseData.JustLetGo)
            {
                ApplyTool(spriteBatch, graphicsDevice, ToolStartPos, MouseData.MousePosCanvasSpace());
                return;
            }
            
            // When the user is just aligning it though, we simply just call the tool's draw method on the preview layer
            else
            {
                graphicsDevice.SetRenderTarget(Canvas.PreviewLayer);
                Color? res = ToolDraw(spriteBatch, graphicsDevice, ToolStartPos, MouseData.MousePosCanvasSpace(), ColorSelector.GetColor());
                if (res is null) { return; }

                // If we just got a color value back, then the tool wants to use the default draw logic with it's toolShader
                DefaultDraw(spriteBatch, graphicsDevice, res.Value, ToolStartPos);
            }
        }

        /// <summary>
        /// Allows you to write custom draw logic for this tool, or modify the color the default draw logic is drawn with <br/>
        /// Return a valid color to allow default draw logic <br/>
        /// Return null to cancel the default logic. <br/>
        /// Returns <see cref="Color.White"/> by default.
        /// </summary>
        protected virtual Color? ToolDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Point toolStartPos, Point toolEndPos, Color toolColor) { return Color.White; }
        // Normally i would not provide ToolEndPos and just expect the tool to capture the mouse's current position but i program assuming whoever
        // is using my functions (me) is a complete fucking imbecile so im spoon feeding these morons (me) every single value they need to worry about


        // TODO: clean this up a tad, create a line tool, and then extend the line tool logic to the click and drag free brush made of lines concept
        // You know the one
        // so ctrlz actually works on free brush tool
        private void ApplyTool(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Point toolStartPos, Point toolEndPos)
        {
            Rectangle affectedArea = MathUtils.RectangleXYXY(toolStartPos, toolEndPos);
            clLogger.LogInfo($"Area affeced by tool: {affectedArea}");

            // No point doing anything if the affected area was zero
            if (affectedArea.Width == 0 || affectedArea.Height == 0)
            {
                return;
            }

            // Create a new rendertaget using the exact same formatting as the active canvas layer's rendertarget
            // Then draw the affected area of the active canvas layer into this new rendertarget
            // Essentially copying the region into the new rendertarget
            RenderTarget2D regionPreAffect = new RenderTarget2D(graphicsDevice,affectedArea.Width, affectedArea.Height, false, Canvas.Layers.ActiveLayer.Format, Canvas.Layers.ActiveLayer.DepthStencilFormat);
            graphicsDevice.SetRenderTarget(regionPreAffect);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate);
            spriteBatch.Draw(Canvas.Layers.ActiveLayer, new Rectangle(0, 0, affectedArea.Width, affectedArea.Height), affectedArea, Color.White);
            spriteBatch.End();


            graphicsDevice.SetRenderTarget(Canvas.Layers.ActiveLayer);
            // Now we have our rendertarget of the region before it was affected by this tool, we can actually draw the tool now
//            ToolDraw(spriteBatch, graphicsDevice, ToolStartPos, MouseData.MousePosCanvasSpace());


            // Ok so the rule for creating UndoableActions is to capture every single fucking variable before using it
            int _activeLayer = Canvas.Layers.ActiveLayerIndex;
            Point _ToolEndPos = MouseData.MousePosCanvasSpace();
            Point _ToolStartPos = ToolStartPos;
            Color _toolColor = ColorSelector.GetColor();
            UndoableAction toolAction = new UndoableAction(
            () => 
            {
                using (SpriteBatch sb = new SpriteBatch(Main.instance.GraphicsDevice))
                {
                    Main.instance.GraphicsDevice.SetRenderTarget(Canvas.Layers[_activeLayer]);
                    ToolDraw(sb, Main.instance.GraphicsDevice, _ToolStartPos, _ToolEndPos, _toolColor);
                }
            }, 
            () =>
            {
                using (SpriteBatch sb = new SpriteBatch(Main.instance.GraphicsDevice))
                {
                    Main.instance.GraphicsDevice.SetRenderTarget(Canvas.Layers[_activeLayer]);
                    sb.Begin();
                    sb.Draw(regionPreAffect, affectedArea, Color.White);
                    sb.End();
                }
                return;
            });

            // Unsure whether its better to create the undoableAction and apply it to make the "actual tool drawing to canvas" happen
            // Or if i just raw call ToolDraw() 
            // Doing this for now
            toolAction.Apply();

            
        }




        // Most tools essentially just create a rectangle between the start and end point, and then run some sort of shader that draws a circle or something
        // As such, the default logic will be to draw a rectangle between start and end, and then apply whatever the tool shader is to it
        // TODO: This
        private void DefaultDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Color drawColor, Point startPos)
        {
            

        }
    }
}