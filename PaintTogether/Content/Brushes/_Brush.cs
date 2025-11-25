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

namespace PaintTogether.Content.Brushes
{
    public abstract class _Brush : Element
    {
        public override bool AutoUpdate => false;

        protected static int _BrushSize => Brush.BrushSize;

        public virtual Effect _BrushShader { get; protected set; }

        #region Loading

        public sealed override void Load()
        {
            Load_Brush();
        }

        public sealed override void LoadAssets(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            // Default, should get overitten by anything inheriting this class really
            _BrushShader = contentManager.Load<Effect>("Shaders/PenBrushShader");
            Load_BrushAssets(graphicsDevice, contentManager);
        }

        /// <summary>
        /// Load any non-asset content here
        /// </summary>
        protected virtual void Load_Brush() { }

        /// <summary>
        /// Load asset-related content here
        /// </summary>
        protected virtual void Load_BrushAssets(GraphicsDevice graphicsDevice, ContentManager contentManager) { }

        /// <summary>
        /// Unloads anything loaded in <see cref="LoadBrush"/>
        /// </summary>
        protected virtual void Unload_Brush() { }

        #endregion

        public List<Point> ActiveCursorHistory = new List<Point>();

        public bool Active = false;

        public sealed override void Update()
        {
            Active = GetActiveState();

            if (Active)
            {
                ActiveCursorHistory.Add(MouseData.MousePosCanvasSpace());
            }
            else
            {
                ActiveCursorHistory.Clear();
            }

            Brush.BrushSize += (int)(MouseData.ScrollDelta * 0.00833333333333f);

            Update_Brush();
        }


        // Carbon copy of Tool.cs -> GetActiveState()
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
            if (MouseData.JustClicked && !ColorSelector.isFocused) // Also avoid accidently initally clicking on the UI
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
        /// Allows you to write custom update logic for this brush
        /// </summary>
        protected virtual void Update_Brush() { }

        public void MainDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (!Active)
            {
                return;
            }

            // Pre-Process the region affected by the brush for networking and undo history
            if (MouseData.JustLetGo)
            {
                ApplyBrush(spriteBatch, graphicsDevice, ActiveCursorHistory);
                return;
            }

            // When the user is still activley drawing to this brush stroke, 
            // we don't do anything externally yet and just draw what we have to the temporary layer
            else
            {
                graphicsDevice.SetRenderTarget(Canvas.PreviewLayer);
                Color? res = _BrushDraw(spriteBatch, graphicsDevice, ActiveCursorHistory, ColorSelector.GetColor(), _BrushSize);
                if (res is null) { return; }

                // If the BrushDraw function returned us a non-null value,
                // That means the Brush wants to use the default draw logic with a certain color
                DefaultDraw(spriteBatch, graphicsDevice, ActiveCursorHistory, res.Value, _BrushSize);
            }
        }



        /// <summary>
        /// Allows you to write custom draw logic for this _brush, or modify the color the default draw logic is drawn with <br/>
        /// Return a valid color to allow default draw logic <br/>
        /// Return null to cancel the default logic. <br/>
        /// Returns <see cref="Color.White"/> by default.
        /// </summary>
        protected virtual Color? _BrushDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, List<Point> drawPoints, Color _brushColor, int _brushSize) { return Color.White; }

        private void ApplyBrush(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, List<Point> drawPoints)
        {
            // Slightly more complex to figure out the affected area than it is in Tool.cs
            // We essentially need to Look at the highest and lowest x and y values for each point in the activeCursorHistory to determine the region
            int highestX = 0;
            int lowestX = int.MaxValue;
            int highestY = 0;
            int lowestY = int.MaxValue;

            // I hate this
            // TODO: something better than this atrocity
            for (int i = 0; i < drawPoints.Count; i++)
            {
                Point p = drawPoints[i];
                if (p.X > highestX)
                {
                    highestX = p.X;
                }
                if (p.X < lowestX)
                {
                    lowestX = p.X;
                }

                if (p.Y > highestY)
                {
                    highestY = p.Y;
                }
                if (p.Y < lowestY)
                {
                    lowestY = p.Y;
                }
            }

            Rectangle affectedArea = MathUtils.RectangleXYXY(lowestX, lowestY, highestX, highestY);
            // Account for the fact if you were to draw right at the edge with a large brush,
            // the brush size leaks over the edge
            affectedArea.Inflate(_BrushSize * 0.5f + 1f, _BrushSize * 0.5f + 1f);  // +1f because if toolsize is at 1 then a single pixel can leak outside sometimes

            clLogger.LogInfo($"Area affected by brush: {affectedArea}");

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
            Func<SpriteBatch, GraphicsDevice, List<Point>, Color, int, Color?> _brushDrawFunc = DefaultDraw;
            Color? res = _BrushDraw(spriteBatch, graphicsDevice, ActiveCursorHistory, ColorSelector.GetColor(), _BrushSize);
            if (res is null)
            {
                _brushDrawFunc = _BrushDraw;
            }
            else
            {
                DefaultDraw(spriteBatch, graphicsDevice, ActiveCursorHistory, ColorSelector.GetColor(), _BrushSize);
                _brushDrawFunc = DefaultDraw;
            }

            // Capture EVERYTHING just to be safe
            int _activeLayerIndex = Canvas.Layers.ActiveLayerIndex;
            List<Point> _ActiveCursorHistory = ActiveCursorHistory.ToList(); // Need to manually *copy* the list
            Color __BrushColor = ColorSelector.GetColor();
            RenderTarget2D _regionPreAffect = regionPreAffect;
            Rectangle _affectedArea = affectedArea;
            int __brushSize = _BrushSize;
            Func<SpriteBatch, GraphicsDevice, List<Point>, Color, int, Color?> __brushDrawFunc = _brushDrawFunc;
            UndoableAction _brushAction = new UndoableAction(
            () =>
            {
                using (SpriteBatch sb = new SpriteBatch(Main.instance.GraphicsDevice))
                {
                    Main.instance.GraphicsDevice.SetRenderTarget(Canvas.Layers[_activeLayerIndex]);
                    __brushDrawFunc(sb, Main.instance.GraphicsDevice, _ActiveCursorHistory, __BrushColor, __brushSize);
                    foreach (var item in _ActiveCursorHistory)
                    {
                        clLogger.LogInfo(item);
                    }
                }
            },
            () =>
            {
                using (SpriteBatch sb = new SpriteBatch(Main.instance.GraphicsDevice))
                {
                    Main.instance.GraphicsDevice.SetRenderTarget(Canvas.Layers[_activeLayerIndex]);
                    sb.Begin();
                    sb.Draw(_regionPreAffect, _affectedArea, Color.White);
                    sb.End();
                }
                return;
            });
        }

        private Color? DefaultDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, List<Point> drawPoints, Color _brushColor, int _brushSize)
        {
            _BrushShader.Parameters["BrushColor"].SetValue(_brushColor.ToVector4());
            spriteBatch.Begin(SpriteSortMode.Immediate, effect: _BrushShader);

            for (int i = 1; i < drawPoints.Count; i++)
            {
                spriteBatch.DrawLine(drawPoints[i - 1], drawPoints[i], _BrushShader, _brushSize);
            }

            spriteBatch.End();

            return null; // Needs the same return type as _BrushDraw in order to fit in the func
        }
    }
}