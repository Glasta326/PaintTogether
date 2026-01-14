using System;
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

namespace PaintTogether.Content.Applicators.Tools
{
    public abstract class DragTool : Element
    {
        #region Fields

        public override bool AutoUpdate => false; // Manually handled

        private static int _toolSize = 1;

        /// <summary>
        /// The width of any strokes or new content added to the canvas
        /// </summary>
        public static int ToolSize
        {
            get
            {
                return _toolSize;
            }
            set
            {
                _toolSize = MathHelper.Clamp(value, 1, int.MaxValue);
            }
        }

        /// <summary>
        /// The shader that gets applied across the selected region
        /// </summary>
        protected virtual Effect ToolShader { get; set; }

        /// <summary>
        /// The original point the user started to hold down the mouse button from, so point A on a line tool for example. Locks in place upon first click
        /// </summary>
        private Point ToolStartPos; // There's no ToolEndPos, we can just get the current mouse position when the tool is applied.

        /// <summary>
        /// Controls whether this tool actually draws and updates
        /// </summary>
        public bool Active = false;

        #endregion

        #region Loading

        public sealed override void Load()
        {
            LoadTool();
        }

        public sealed override void LoadAssets(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            // Emergency fallback but this should be overriden
            ToolShader = contentManager.Load<Effect>("Shaders/FillRectShader");

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

        #region Update logic

        public sealed override void Update()
        {
            Active = GetActiveState();

            // As soon as we start to use the tool, immediately log where we first started clicking.
            // Also im fairly sure there's no point storing this as screen space so we insta-convert it to canvas space
            if (MouseData.JustClicked)
            {
                ToolStartPos = Canvas.ScreenToCanvas(MouseData.MousePosPoint());
            }

            // Control stroke width with mouse scroll wheel for now
            ToolSize += (int)(MouseData.ScrollDelta * 0.00833333333333f); // divide by 120

            // invoke inheriting class's update logic
            UpdateTool();
        }

        /// <summary>
        /// Determines if <see cref="Active"/> should be true or false <br/>
        /// Essentially controls whether this tool is in effect or not
        /// </summary>
        private bool GetActiveState()
        {
            // TODO: Custom input configs for the user

            // Always immediatly disable and stop using when we right click.
            // Or whatever the button to cancel the tool operation is
            if (MouseData.IsRightClick)
            {
                return false;
            }

            // Clicking now AND already clicking last frame
            if (MouseData.IsLeftClick && Active)
            {
                return true;
            }
            // NOTE: i think this is bad?, like, we check if we're clicking and were clicking last frame, and then seperatly check if we just clicked now?
            // Isnt it better to just check if we're clicking at all and return true on that?
            if (MouseData.JustClicked && !ColorSelector.isHovering)
            {
                return true;
            }

            // We also need to be considered active for the last frame of usage so we can actually apply the tool to the canvas and all that
            if (MouseData.JustLetGo && Active)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Allows you to write custom update logic for this tool
        /// </summary>
        protected virtual void UpdateTool() { } // <- Inheriting members override this

        #endregion

        #region Drawing

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
                Color? res = ToolDraw(spriteBatch, graphicsDevice, ToolStartPos, MouseData.MousePosCanvasSpace(), ColorSelector.GetColor(), ToolSize);
                if (res is null) { return; }

                // If we just got a color value back, then the tool wants to use the default draw logic with it's toolShader
                DefaultDraw(spriteBatch, graphicsDevice, ToolStartPos, MouseData.MousePosCanvasSpace(), res.Value, ToolSize);
            }
        }

        public void ApplyTool(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Point toolStartPos, Point toolEndPos)
        {
            Rectangle affectedArea = MathUtils.RectangleXYXY(toolStartPos, toolEndPos);
            // Account for things like the line tool which can draw at most half of the brush's width outside the area
            affectedArea.Inflate(ToolSize * 0.5f + 1f, ToolSize * 0.5f + 1f);  // +1f because if toolsize is at 1 then a single pixel can leak outside sometimes

            // No point doing anything if the affected area was zero
            if (affectedArea.Width == 0 || affectedArea.Height == 0)
            {
                return;
            }



            // Create a new rendertaget using the exact same formatting as the active canvas layer's rendertarget
            // Then draw the affected area of the active canvas layer into this new rendertarget
            // Essentially copying the region into the new rendertarget
            RenderTarget2D regionPreAffect = new RenderTarget2D(graphicsDevice, affectedArea.Width, affectedArea.Height, false, Canvas.Layers.ActiveLayer.Format, Canvas.Layers.ActiveLayer.DepthStencilFormat);
            graphicsDevice.SetRenderTarget(regionPreAffect);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate);
            spriteBatch.Draw(Canvas.Layers.ActiveLayer, new Rectangle(0, 0, affectedArea.Width, affectedArea.Height), affectedArea, Color.White);
            spriteBatch.End();

            graphicsDevice.SetRenderTarget(Canvas.Layers.ActiveLayer);

            // This attempts to actually draw the tool to the currently active canvas layer, and set the draw func to be the overriden draw call
            // but if overriden draw call returns us a color value, we instead use the default draw call for the draw func with that defined color value
            Func<SpriteBatch, GraphicsDevice, Point, Point, Color, int, Color?> toolDrawFunc = DefaultDraw;
            Color? res = ToolDraw(spriteBatch, graphicsDevice, ToolStartPos, MouseData.MousePosCanvasSpace(), ColorSelector.GetColor(), ToolSize);
            if (res is null)
            {
                toolDrawFunc = ToolDraw;
            }
            else
            {
                DefaultDraw(spriteBatch, graphicsDevice, ToolStartPos, MouseData.MousePosCanvasSpace(), ColorSelector.GetColor(), ToolSize);
                toolDrawFunc = DefaultDraw;
            }

            // Once the draw func has been decided, we capture an instance of every single value that gets used by the draw call,
            // and then pass those captured values in instead.
            // Without this, there'd be issue with trying to use references to things which have changed since this ApplyBrush() was initally called
            int _activeLayer = Canvas.Layers.ActiveLayerIndex;
            Point _ToolEndPos = MouseData.MousePosCanvasSpace();
            Point _ToolStartPos = ToolStartPos;
            Color _toolColor = ColorSelector.GetColor();
            RenderTarget2D _regionPreAffect = regionPreAffect;
            Rectangle _affectedArea = affectedArea;
            int _toolSize = ToolSize;
            Func<SpriteBatch, GraphicsDevice, Point, Point, Color, int, Color?> _toolDrawFunc = toolDrawFunc;

            // There should never be a change in the graphics device, so using the reference to the main instance is ok
            // Create the new undoable action (This is automatically pushed to the undo history upon creation)
            UndoableAction toolAction = new UndoableAction(
            () =>
            {
                // Apply action. This applies our draw call and draws the tool over the stored area on the stored canvas layer
                using (SpriteBatch sb = new SpriteBatch(Main.instance.GraphicsDevice))
                {
                    Main.instance.GraphicsDevice.SetRenderTarget(Canvas.Layers[_activeLayer]);
                    _toolDrawFunc(sb, Main.instance.GraphicsDevice, _ToolStartPos, _ToolEndPos, _toolColor, _toolSize);
                }
            },
            () =>
            {
                // Undo action. This re-draws whatever was underneat the affected area before this tool was drawn
                using (SpriteBatch sb = new SpriteBatch(Main.instance.GraphicsDevice))
                {
                    Main.instance.GraphicsDevice.SetRenderTarget(Canvas.Layers[_activeLayer]);
                    sb.Begin(SpriteSortMode.Immediate, BlendState.Opaque); // Important!, If we do any other blendstate, when it tries to re-draw transparent pixels, it will instead not override what's underneath them
                    sb.Draw(_regionPreAffect, _affectedArea, Color.White);
                    sb.End();
                }
                return;
            });

            if (clLogger.VerboseLogging)
            {
                clLogger.LogInfo($"New tool usage over area: {affectedArea}");
            }
        }


        /// <summary>
        /// Most tools essentially just create a rectangle between the start and end point, and then run some sort of shader that draws a circle or something
        /// As such, the default logic will be to draw a rectangle between start and end, and then apply whatever the tool shader is to it
        /// </summary>
        private Color? DefaultDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Point toolStartPos, Point toolEndPos, Color toolColor, int toolSize)
        {
            Point _start = toolStartPos;
            Point _mouse = toolEndPos;
            Rectangle drawArea = MathUtils.RectangleXYXY(_start, _mouse);

            // Could potentially not have these fields in the shader, so we need the null checks
            // Its the ["Color"]? <- little question mark because i know im going to forget
            ToolShader.Parameters["Color"]?.SetValue(toolColor.ToVector4());
            ToolShader.Parameters["Resolution"]?.SetValue(new Vector2(drawArea.Width, drawArea.Height));

            spriteBatch.Begin(SpriteSortMode.Immediate, effect: ToolShader);

            ToolShader.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(CommonKeys.DummyTexture, drawArea, Color.White);

            spriteBatch.End();

            return null; // Unforunatley i need this to also return color? to match the return type of the toolDraw hook so i can swap it around in the funcs
        }

        /// <summary>
        /// Allows you to write custom draw logic for this tool, or modify the color the default draw logic is drawn with <br/>
        /// Return a valid color to allow default draw logic <br/>
        /// Return null to cancel the default logic. <br/>
        /// Returns <see cref="Color.White"/> by default.
        /// </summary>
        public virtual Color? ToolDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Point toolStartPos, Point toolEndPos, Color toolColor, int toolSize) { return Color.White; }

        #endregion
    }
}