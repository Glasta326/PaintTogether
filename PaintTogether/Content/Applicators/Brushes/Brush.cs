using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Common;
using PaintTogether.Common.PaintLogger;
using PaintTogether.Common.Utilities;
using PaintTogether.Content.PaintCanvas;
using PaintTogether.Content.Applicators.Tools;
using PaintTogether.Content.UI;
using PaintTogether.Core;
using PaintTogether.Core.UndoSystem;
using PaintTogether.Core.Users;
using PaintTogether.Core.Networking;
using System.Collections.Concurrent;
using System.IO;

namespace PaintTogether.Content.Applicators.Brushes
{
    /// <summary>
    /// Base class for all brush types.<br/>
    /// Very similar to <see cref="DragTool"/> but application and stored data is different.
    /// </summary>
    public abstract class Brush : Element
    {
        #region Fields

        public override bool AutoUpdate => false; // Manually handled

        // Just read it from tool.cs, its meant to be shared across all cursor elements anyway
        protected static int BrushSize => DragTool.ToolSize;

        /// <summary>
        /// The actual shader that gets applied to the brush area
        /// </summary>
        protected virtual Effect BrushShader { get; set; }

        /// <summary>
        /// Contains a list of every point the cursor has been to while the brush is considered active/in use
        /// </summary>
        private List<Point> BrushStrokePoints = new List<Point>();

        /// <summary>
        /// Controls whether this brush actually draws and updates
        /// </summary>
        public bool Active = false;

        #endregion

        #region Loading

        public sealed override void Load()
        {
            LoadBrush();
        }

        public sealed override void LoadAssets(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            // Default, should get overitten by anything inheriting this class really
            BrushShader = contentManager.Load<Effect>("Shaders/PenBrushShader");
            LoadBrushAssets(graphicsDevice, contentManager);
        }

        /// <summary>
        /// Load any non-asset content here
        /// </summary>
        protected virtual void LoadBrush() { }

        /// <summary>
        /// Load asset-related content here
        /// </summary>
        protected virtual void LoadBrushAssets(GraphicsDevice graphicsDevice, ContentManager contentManager) { }

        /// <summary>
        /// Unloads anything loaded in <see cref="LoadBrush"/>
        /// </summary>
        protected virtual void UnloadBrush() { }

        #endregion

        #region Update logic

        public sealed override void Update()
        {
            // Whenever the brush is actually being used, we need to store all the points that compose the final brush stroke
            Active = GetActiveState();
            if (Active) // Only actually add the point if the mouse is moving otherwise it breaks with translucent brushes
            {
                if (MouseData.MoveDelta != Point.Zero)
                {
                    BrushStrokePoints.Add(MouseData.MousePosCanvasSpace());
                }
            }
            else
            {
                BrushStrokePoints.Clear(); // reset when not in use
            }

            // Control stroke width with mouse scroll wheel for now
            DragTool.ToolSize += (int)(MouseData.ScrollDelta * 0.00833333333333f);

            // invoke inheriting class's update logic
            UpdateBrush();
        }


        /// <summary>
        /// Determines if <see cref="Active"/> should be true or false <br/>
        /// Essentially controls whether this brush is in effect or not
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
            if (MouseData.JustClicked && !ColorSelector.isHovering && Canvas.InBounds(MouseData.MousePosCanvasSpace())) // Also avoid accidently initally clicking on the UI
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
        protected virtual void UpdateBrush() { } // <- Inheriting members override this to implement their own logic

        #endregion

        #region Drawing

        public override void BackgroundDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            HandleIncomingDrawRequests(spriteBatch, graphicsDevice);
        }

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

            // Pre-Process the region affected by the brush for networking and undo history
            if (MouseData.JustLetGo)
            {
                ApplyBrush(spriteBatch, graphicsDevice, BrushStrokePoints, Canvas.Layers.ActiveLayerIndex, ColorSelector.GetColor(), BrushSize, NetSorter.Myself.ClientID);
                return;
            }

            // When the user is still activley drawing to this brush stroke, 
            // we don't do anything externally yet and just draw what we have to the preview layer
            else
            {
                List<Point> brushStrokePoints = BrushStrokePoints;
                Color drawColor = ColorSelector.GetColor();
                Rectangle affectedArea = getAffectedArea(brushStrokePoints);
                int brushSize = BrushSize;


                graphicsDevice.SetRenderTarget(Canvas.PreviewLayer);
                Color? res = BrushDraw(spriteBatch, graphicsDevice, brushStrokePoints, affectedArea, drawColor, brushSize, true);
                if (res is null) { return; }

                // If the BrushDraw function returned us a non-null value,
                // That means the Brush wants to use the default draw logic with a certain color
                DefaultDraw(spriteBatch, graphicsDevice, brushStrokePoints, affectedArea, res.Value, brushSize);
            }
        }

        private Rectangle getAffectedArea(List<Point> drawPoints)
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
            affectedArea.Inflate(BrushSize * 0.5f + 1f, BrushSize * 0.5f + 1f);  // +1f because if toolsize is at 1 then a single pixel can leak outside sometimes
            return affectedArea;
        }

        private void ApplyBrush(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, List<Point> drawPoints, int layerIndex, Color drawColor, int brushSize, byte userID = 0)
        {
            // This basically ensures that just clicking in a single spot still works
            if (drawPoints.Count == 0)
            {
                drawPoints.Add(MouseData.MousePosCanvasSpace());
                drawPoints.Add(MouseData.MousePosCanvasSpace());
            }

            Rectangle affectedArea = getAffectedArea(drawPoints);

            // Something went real bad if we have negative area 
            if (affectedArea.Width < 0 || affectedArea.Height < 0)
            {
                clLogger.LogWarning($"Attempted to draw brush over bad area! Width: {affectedArea.Width}, Height: {affectedArea.Height}");
                return;
            }

            Action[] DoUndo = ApplyBrushAndGetActions(spriteBatch, graphicsDevice, drawPoints, affectedArea, layerIndex, drawColor, brushSize);
            _ = new UserAction(DoUndo[0], DoUndo[1], userID, Descriptor: GetType().Name);

            if (userID == NetSorter.Myself.ClientID && NetSorter.IsConnected)
            {
                SendDrawRequest(spriteBatch, graphicsDevice, drawPoints, layerIndex, drawColor, brushSize, userID);
            }
        }

        private Action[] ApplyBrushAndGetActions(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, List<Point> drawPoints, Rectangle affectedArea, int layerIndex, Color drawColor, int brushSize)
        {
            // Create a new rendertaget using the exact same formatting as the active canvas layer's rendertarget
            // Then draw the affected area of the active canvas layer into this new rendertarget
            // Essentially copying the region into the new rendertarget
            RenderTarget2D regionPreAffect = new RenderTarget2D(graphicsDevice, affectedArea.Width, affectedArea.Height, false, Canvas.Layers[layerIndex].Format, Canvas.Layers[layerIndex].DepthStencilFormat);
            DrawUtils.CopySection(spriteBatch, Canvas.Layers[layerIndex], affectedArea, regionPreAffect, new Rectangle(0, 0, affectedArea.Width, affectedArea.Height));

            // This attempts to actually draw the brush stroke to the currently active canvas layer, and set the draw func to be the overriden draw call
            // but if overriden draw call returns us a color value, we instead use the default draw call for the draw func with that defined color value
            graphicsDevice.SetRenderTarget(Canvas.Layers[layerIndex]);
            Func<SpriteBatch, GraphicsDevice, List<Point>, Rectangle, Color, int, bool, Color?> brushDrawFunc = DefaultDraw;
            Color? res = BrushDraw(spriteBatch, graphicsDevice, drawPoints, affectedArea, drawColor, brushSize);
            if (res is null)
            {
                brushDrawFunc = BrushDraw;
            }
            else
            {
                DefaultDraw(spriteBatch, graphicsDevice, drawPoints, affectedArea, drawColor, brushSize);
                brushDrawFunc = DefaultDraw;
            }

            // Once the draw func has been decided, we capture an instance of every single value that gets used by the draw call,
            // and then pass those captured values in instead.
            // Without this, there'd be issue with trying to use references to things which have changed since this ApplyBrush() was initally called
            int _activeLayerIndex = layerIndex;
            List<Point> _ActiveCursorHistory = drawPoints.ToList(); // Need to manually *copy* the list
            Color _BrushColor = drawColor;
            RenderTarget2D _regionPreAffect = regionPreAffect;
            Rectangle _affectedArea = affectedArea;
            int _brushSize = brushSize;
            bool _isPreview = false;
            Func<SpriteBatch, GraphicsDevice, List<Point>, Rectangle, Color, int, bool, Color?> _brushDrawFunc = brushDrawFunc; // This is unnessicary

            // There should never be a change in the graphics device, so using the reference to the main instance is ok
            // Create the new undoable action (This is automatically pushed to the undo history upon creation)
            Action apply =
            () =>
            {
                // Apply action. This applies our draw call and draws the brush stroke
                using (SpriteBatch sb = new SpriteBatch(Main.instance.GraphicsDevice))
                {
                    Main.instance.GraphicsDevice.SetRenderTarget(Canvas.Layers[_activeLayerIndex]);
                    _brushDrawFunc(sb, Main.instance.GraphicsDevice, _ActiveCursorHistory, _affectedArea, _BrushColor, _brushSize, _isPreview);
                }
            };
            Action undo =
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
            };

            return [apply, undo];
        }

        /// <summary>
        /// Default drawing logic. The defined shader and draws a line between each brush point. <br/>
        /// Behaviour is similar to a typical pen tool in something like MSPaint but using the override-defined BrushShader
        /// </summary>
        private Color? DefaultDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, List<Point> drawPoints, Rectangle affectedArea, Color brushColor, int brushSize, bool isPreview = false)
        {
            // TODO: this is slow as shit!!!!
            // As it turns out, for long brush strokes, the program tries to draw a line potentially hundreds of times, and drawing a line takes hundreds of brush operations
            Main.stopWatch.Restart();
            BrushShader.Parameters["BrushColor"]?.SetValue(brushColor.ToVector4());
            spriteBatch.Begin(SpriteSortMode.Immediate, effect: BrushShader);
            for (int i = 1; i < drawPoints.Count; i++)
            {
                spriteBatch.DrawLine(drawPoints[i - 1], drawPoints[i], BrushShader, brushSize);
            }
            spriteBatch.End();
            Main.stopWatch.Stop();

            //clLogger.LogInfo($"{Main.stopWatch.ElapsedMilliseconds}ms");

            return null; // Needs the same return type as _BrushDraw in order to fit in the func
        }

        /// <summary>
        /// Allows you to write custom draw logic for this _brush, or modify the color the default draw logic is drawn with <br/>
        /// Return a valid color to allow default draw logic <br/>
        /// Return null to cancel the default logic. <br/>
        /// Returns <see cref="Color.White"/> by default.
        /// </summary>
        protected virtual Color? BrushDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, List<Point> drawPoints, Rectangle affectedArea, Color _brushColor, int _brushSize, bool isPreview = false) { return Color.White; }

        /// <summary>
        /// For drawing anything ui-related for this brush on the preview layer of the canvas.
        /// </summary>
        public virtual void UIDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice) { }

        #endregion

        #region Networking

        /// <summary>
        /// Networking threads will enque packets to the appropriate tool's queue, and main thread will come along "occaisonally" and consume everything inside that queue.
        /// </summary>
        public static ConcurrentDictionary<string, ConcurrentQueue<RecievePacket>> IncomingRequestQueues = new ConcurrentDictionary<string, ConcurrentQueue<RecievePacket>>();


        private void HandleIncomingDrawRequests(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (!IncomingRequestQueues.TryGetValue(GetType().FullName, out ConcurrentQueue<RecievePacket> queue))
            {
                return;
            }

            while (queue.TryDequeue(out RecievePacket request))
            {
                // Not the biggest fan of all this *not drawing* math and logic inside of the drawing area but shrug
                BinaryReader reader = new BinaryReader(request.GetStream());

                int layerIndex = reader.ReadInt32();

                // position list
                int length = reader.ReadInt32();
                List<Point> brushDrawPoints = new List<Point>(length);
                for (int i = 0; i < length; i++)
                {
                    int x = reader.ReadInt32();
                    int y = reader.ReadInt32();
                    brushDrawPoints.Add(new Point(x, y));
                }

                byte r = reader.ReadByte();
                byte g = reader.ReadByte();
                byte b = reader.ReadByte();
                byte a = reader.ReadByte();

                int brushSize = reader.ReadInt32();

                Color brushColor = new Color(r, g, b, a);

                ApplyBrush(spriteBatch, graphicsDevice, brushDrawPoints, layerIndex, brushColor, brushSize, request.Owner);

                clLogger.LogInfo($"Draw packet recieved for {GetType().Name} containing {length} draw points. Packet type: {request.Type}");
            }
        }

        private void SendDrawRequest(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, List<Point> drawPoints, int layerIndex, Color drawColor, int brushSize, byte userID)
        {
            // GetType().FullName gets the overriding class, so if LineTool's ToolDraw() method is the one being used, this will return LineTool
            SendPacket packet = new SendPacket(userID, GetType().FullName);
            BinaryWriter writer = new BinaryWriter(packet.GetStream());

            writer.Write(layerIndex);

            // Write each element in the list along with how many elements there are
            int length = drawPoints.Count;
            writer.Write(length);
            for (int i = 0; i < length; i++)
            {
                writer.Write(drawPoints[i].X);
                writer.Write(drawPoints[i].Y);
            }

            writer.Write(drawColor.R);
            writer.Write(drawColor.G);
            writer.Write(drawColor.B);
            writer.Write(drawColor.A);

            writer.Write(brushSize);

            NetSorter.OutgoingPackets.Add(packet);

            clLogger.LogInfo($"Draw packet sent for {GetType().Name} containing {length} draw points");
        }


        #endregion
    }
}