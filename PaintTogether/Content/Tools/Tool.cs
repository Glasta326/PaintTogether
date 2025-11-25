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
        protected static int ToolSize => Brush.BrushSize;

        /// <summary>
        /// What this tool actually does to the selected region
        /// </summary>
        public virtual Effect ToolShader { get; protected set; }

        private Effect shader2;

        #region Loading

        public sealed override void Load()
        {
            LoadTool();
        }

        public sealed override void LoadAssets(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            // Emergency fallback but this should be overriden
            ToolShader = contentManager.Load<Effect>("Shaders/FillRectShader");
            shader2 = contentManager.Load<Effect>("Shaders/FillRectShader");

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
        /// Contains a list of every point the cursor has been to while the tool is considered active
        /// </summary>
        public List<Point> ActiveCursorHistory = new List<Point>();

        /// <summary>
        /// Represents whether this tool is activley being used right now
        /// </summary>
        public bool Active = false;

        public sealed override void Update()
        {
            Active = GetActiveState();

            // As soon as we start to use the tool, immediately log where we first started clicking.
            // Also im fairly sure there's no point storing this as screen space so we insta-convert it to canvas space
            if (MouseData.JustClicked)
            {
                ToolStartPos = Canvas.ScreenToCanvas(MouseData.MousePosPoint());
            }

            // Log mouse positions while tool is active
            if (Active)
            {
                ActiveCursorHistory.Add(MouseData.MousePosCanvasSpace());
            }
            else
            {
                ActiveCursorHistory.Clear();
            }

            Brush.BrushSize += (int)(MouseData.ScrollDelta * 0.00833333333333f); // divide by 120

            UpdateTool();
        }

        /// <summary>
        /// Determines what <see cref="Active"/> should be set to.
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
            if (MouseData.JustClicked)
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
                Color? res = ToolDraw(spriteBatch, graphicsDevice, ToolStartPos, MouseData.MousePosCanvasSpace(), ColorSelector.GetColor(), ToolSize);
                if (res is null) { return; }

                // If we just got a color value back, then the tool wants to use the default draw logic with it's toolShader
                DefaultDraw(spriteBatch, graphicsDevice, ToolStartPos, MouseData.MousePosCanvasSpace(), res.Value, ToolSize);
            }
        }

        /// <summary>
        /// Allows you to write custom draw logic for this tool, or modify the color the default draw logic is drawn with <br/>
        /// Return a valid color to allow default draw logic <br/>
        /// Return null to cancel the default logic. <br/>
        /// Returns <see cref="Color.White"/> by default.
        /// </summary>
        protected virtual Color? ToolDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Point toolStartPos, Point toolEndPos, Color toolColor, int toolSize) { return Color.White; }
        // Normally i would not provide ToolEndPos and just expect the tool to capture the mouse's current position but i program assuming whoever
        // is using my functions (me) is a complete fucking imbecile so im spoon feeding these morons (me) every single value they need to worry about


        // TODO: clean this up a tad, create a line tool, and then extend the line tool logic to the click and drag free brush made of lines concept
        // You know the one
        // so ctrlz actually works on free brush tool
        private void ApplyTool(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Point toolStartPos, Point toolEndPos)
        {
            Rectangle affectedArea = MathUtils.RectangleXYXY(toolStartPos, toolEndPos);
            // Account for things like the line tool which can draw at most half of the brush's width outside the area
            affectedArea.Inflate(ToolSize * 0.5f + 1f, ToolSize * 0.5f + 1f);  // +1f because if toolsize is at 1 then a single pixel can leak outside sometimes

            clLogger.LogInfo($"Area affeced by tool: {affectedArea}");

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
            shader2.Parameters["Color"].SetValue(Color.White.ToVector4());
            spriteBatch.Begin(SpriteSortMode.Immediate, effect: shader2);
            spriteBatch.Draw(CommonKeys.DummyTexture, affectedArea, Color.White);
            spriteBatch.End();


            graphicsDevice.SetRenderTarget(Canvas.Layers.ActiveLayer);

            // Figure out whether to use DefaultDraw or ToolDraw and store whichever one actually draws this tool
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



            // Ok so the rule for creating UndoableActions is to capture every single fucking variable before using it
            int _activeLayer = Canvas.Layers.ActiveLayerIndex;
            Point _ToolEndPos = MouseData.MousePosCanvasSpace();
            Point _ToolStartPos = ToolStartPos;
            Color _toolColor = ColorSelector.GetColor();
            RenderTarget2D _regionPreAffect = regionPreAffect;
            Rectangle _affectedArea = affectedArea;
            int _toolSize = ToolSize;
            Func<SpriteBatch, GraphicsDevice, Point, Point, Color, int, Color?> _toolDrawFunc = toolDrawFunc; // Literally like 2 lines up i create this but i dont trust glasta
            UndoableAction toolAction = new UndoableAction(
            () =>
            {
                using (SpriteBatch sb = new SpriteBatch(Main.instance.GraphicsDevice))
                {
                    Main.instance.GraphicsDevice.SetRenderTarget(Canvas.Layers[_activeLayer]);
                    _toolDrawFunc(sb, Main.instance.GraphicsDevice, _ToolStartPos, _ToolEndPos, _toolColor, _toolSize);
                }
            },
            () =>
            {
                using (SpriteBatch sb = new SpriteBatch(Main.instance.GraphicsDevice))
                {
                    Main.instance.GraphicsDevice.SetRenderTarget(Canvas.Layers[_activeLayer]);
                    sb.Begin();
                    sb.Draw(_regionPreAffect, _affectedArea, Color.White);
                    sb.End();
                }
                return;
            });

            // Update : We dont call it because the drawing methods are already called before we make the undoableAction
            // Unsure whether its better to create the undoableAction and apply it to make the "actual tool drawing to canvas" happen
            // Or if i just raw call ToolDraw() 
            // Doing this for now
            //toolAction.Apply();


        }

        // Most tools essentially just create a rectangle between the start and end point, and then run some sort of shader that draws a circle or something
        // As such, the default logic will be to draw a rectangle between start and end, and then apply whatever the tool shader is to it
        // TODO: This
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
    }
}