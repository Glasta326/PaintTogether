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
using PaintTogether.Core;
using PaintTogether.Core.UndoSystem;

namespace PaintTogether.Content.Applicators.ClickTools
{
    public abstract class ClickTool : Element
    {
        #region Fields

        public override bool AutoUpdate => false; // Manually handled

        /// <summary>
        /// The actual shader that gets applied to the ClickTool area
        /// </summary>
        protected virtual Effect ClickToolShader { get; set; }

        /// <summary>
        /// Controls whether this ClickTool actually draws and updates
        /// </summary>
        public bool Active = false;

        #endregion

        #region Loading

        public sealed override void Load()
        {
            LoadClickTool();
        }

        public sealed override void LoadAssets(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            LoadClickToolAssets(graphicsDevice, contentManager);
        }

        /// <summary>
        /// Load any non-asset content here
        /// </summary>
        protected virtual void LoadClickTool() { }

        /// <summary>
        /// Load asset-related content here
        /// </summary>
        protected virtual void LoadClickToolAssets(GraphicsDevice graphicsDevice, ContentManager contentManager) { }

        /// <summary>
        /// Unloads anything loaded in <see cref="LoadClickTool"/>
        /// </summary>
        protected virtual void UnloadClickTool() { }

        #endregion

        #region Update logic

        public sealed override void Update()
        {
            // Determine when this should activate
            Active = GetActiveState();
            if (Active)
            {
                OnClick();
            }
            else
            {

            }

            // invoke inheriting class's update logic
            UpdateClickTool();
        }


        /// <summary>
        /// Determines if <see cref="Active"/> should be true or false <br/>
        /// Essentially controls whether this ClickTool activates this frame
        /// </summary>
        private bool GetActiveState()
        {
            // TODO: Custom input configs for the user

            // Only active on the first clicking frame
            if (MouseData.JustClicked && !ColorSelector.isHovering)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Allows you to write custom update logic for this ClickTool
        /// </summary>
        protected virtual void UpdateClickTool() { } // <- Inheriting members override this to implement their own logic

        /// <summary>
        /// Allows you to write actions that happen when the tool is activated
        /// </summary>
        protected virtual void OnClick() {}

        #endregion

        #region Drawing

        /// <summary>
        /// Core draw function for this brush. <br/>
        /// Should be called from main or wherever drawing for core components is handled
        /// </summary>
        public void MainDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (!Active)
            {
                return;
            }
            // Activate the clicktool draw
            ApplyTool(spriteBatch, graphicsDevice);
        }


        // this is bad.
        // This uses a random layer slapped onto the canvas, and it requires essentially doing the draw twice, whichi is pointlessly expensive
        // Todo: better solution
        // i have an idea for a better solution
        // so currently the genereal algorithm for a tool on the undo stack is to:
        //1) calculate the area that will change after this tool is applied
        //2) take a snapshot of that area and store it
        //3) actually apply the tool
        //4) create a new action on the undo/redo stack 

        // New solution:
        // We don't know the area that will be affected yet, so we snapshot the ENTIRE canvas layer
        // apply the tool to the active layer
        // The tool's draw() returns the affected region, which should be easy to calculate inside the draw call
        // the snapshot of the whole layer is then cropped down into the region returned by draw()
        // create a new action using this as usual 
        private void ApplyTool(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            // Snapshot the contents of the ENTIRE active layer
            RenderTarget2D regionSnapshot = new RenderTarget2D(graphicsDevice, Canvas.Layers.ActiveLayer.Width, Canvas.Layers.ActiveLayer.Height, false, Canvas.Layers.ActiveLayer.Format, Canvas.Layers.ActiveLayer.DepthStencilFormat);
            Rectangle snapshotArea = new Rectangle(0, 0, Canvas.Layers.ActiveLayer.Width, Canvas.Layers.ActiveLayer.Height);
            DrawUtils.CopySection(spriteBatch, Canvas.Layers.ActiveLayer, snapshotArea, regionSnapshot, new Rectangle(0, 0, snapshotArea.Width, snapshotArea.Height));

            // Actually apply the tool's drawing function to the active layer
            // Also get the area the tool affected
            graphicsDevice.SetRenderTarget(Canvas.Layers.ActiveLayer);
            Rectangle affectedArea = ToolDraw(spriteBatch, graphicsDevice, Canvas.Layers.ActiveLayerIndex, MouseData.MousePosCanvasSpace(), ColorSelector.GetColor());
            
            // No point doing going any further if the tool changed nothing
            if (affectedArea.Width <= 0 || affectedArea.Height <= 0)
            {
                clLogger.LogWarning($"Attempted to draw clickTool over bad area! Width: {affectedArea.Width}, Height: {affectedArea.Height}");
                return;
            }

            Func<SpriteBatch, GraphicsDevice, int, Point, Color, Rectangle> toolDrawFunc = ToolDraw; // store the draw func

            // Essentially crop down the snapshotted area by copying over only the affected area to a new target of the size of the affected area
            RenderTarget2D regionPreAffect = new RenderTarget2D(graphicsDevice, affectedArea.Width, affectedArea.Height, false, Canvas.Layers.ActiveLayer.Format, Canvas.Layers.ActiveLayer.DepthStencilFormat);
            DrawUtils.CopySection(spriteBatch, regionSnapshot, affectedArea, regionPreAffect, new Rectangle(0, 0, affectedArea.Width, affectedArea.Height));

            // Capture an instance of every single value that gets used by the draw call,
            // and then pass those captured values in instead.
            // Without this, there'd be issue with trying to use references to things which have changed since this ApplyBrush() was initally called
            int _activeLayerIndex = Canvas.Layers.ActiveLayerIndex;
            Point _DrawPos = MouseData.MousePosCanvasSpace();
            Color _BrushColor = ColorSelector.GetColor();
            RenderTarget2D _regionPreAffect = regionPreAffect;
            Rectangle _affectedArea = affectedArea;
            Func<SpriteBatch, GraphicsDevice, int, Point, Color, Rectangle> _toolDrawFunc = toolDrawFunc; // This is unnessicary

            // There should never be a change in the graphics device, so using the reference to the main instance is ok
            // Create the new undoable action (This is automatically pushed to the undo history upon creation)
            UndoableAction _brushAction = new UndoableAction(
            () =>
            {
                // Apply action. This applies our draw call and draws the brush stroke
                using (SpriteBatch sb = new SpriteBatch(Main.instance.GraphicsDevice))
                {
                    Main.instance.GraphicsDevice.SetRenderTarget(Canvas.Layers[_activeLayerIndex]);
                    _toolDrawFunc(sb, Main.instance.GraphicsDevice, _activeLayerIndex, _DrawPos, _BrushColor);
                }
            },
            () =>
            {
                // Undo action. This undoes the brush stroke by instead re-drawing what it looked like before over the affected area
                using (SpriteBatch sb = new SpriteBatch(Main.instance.GraphicsDevice))
                {
                    Main.instance.GraphicsDevice.SetRenderTarget(Canvas.Layers[_activeLayerIndex]);
                    sb.Begin(SpriteSortMode.Immediate, BlendState.Opaque); // Important!, If we do any other blendstate, when it tries to re-draw transparent pixels, it will instead not override what's underneath them
                    sb.Draw(_regionPreAffect, _affectedArea, Color.White);
                    sb.End();
                }
                return;
            });

            if (clLogger.VerboseLogging)
            {
                clLogger.LogInfo($"ClickTool used at: {_DrawPos}");
            }
        }

        /// <summary>
        /// Allows you to write custom draw logic for this clickTool<br/>
        /// layerIndex CAN BE INVALID. USE CANVAS.TRYGET()
        /// </summary>
        protected virtual Rectangle ToolDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, int layerIndex, Point drawPoint, Color drawColor) { return Rectangle.Empty; }

        /// <summary>
        /// For drawing anything ui-related for this brush on the preview layer of the canvas.
        /// </summary>
        public virtual void UIDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice) { }

        #endregion

    }
}