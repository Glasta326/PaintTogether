using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PaintTogether.Common;
using PaintTogether.Common.PaintLogger;
using PaintTogether.Content.Brushes;
using PaintTogether.Content.PaintCanvas;
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
            
            // If we arent on the final action of "mouse has let go we need to draw to the canvas now", we draw to the preview layer
            if (MouseData.JustLetGo)
            {
                graphicsDevice.SetRenderTarget(Canvas.Layers.ActiveLayer);
            }
            else
            {
                graphicsDevice.SetRenderTarget(Canvas.PreviewLayer);
            }
            // call whatever the draw function is to the rendertarget we selected
            Color? res = ToolDraw(spriteBatch, graphicsDevice, ToolStartPos);

            
            if (res is null)
            {
                return;
            }
            DefaultDraw(spriteBatch, graphicsDevice, res.Value, ToolStartPos);
        }

        /// <summary>
        /// Allows you to write custom draw logic for this tool, or modify the color the default draw logic is drawn with <br/>
        /// Return a valid color to allow default draw logic <br/>
        /// Return null to cancel the default logic. <br/>
        /// Returns <see cref="Color.White"/> by default.
        /// </summary>
        protected virtual Color? ToolDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Point startPos) { return Color.White; }

        // Most tools essentially just create a rectangle between the start and end point, and then run some sort of shader that draws a circle or something
        // As such, the default logic will be to draw a rectangle between start and end, and then apply whatever the tool shader is to it
        private void DefaultDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Color drawColor, Point startPos)
        {
            

        }
    }
}