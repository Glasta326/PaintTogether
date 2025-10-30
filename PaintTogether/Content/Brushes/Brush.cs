using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PaintTogether.Common;
using PaintTogether.Common.DataTypes;
using PaintTogether.Common.Utilities;
using PaintTogether.Core;

namespace PaintTogether.Content.Brushes
{
    public abstract class Brush : Element
    {
        private static int _brushSize;

        /// <summary>
        /// Determines the width of this brush
        /// </summary>
        public static int BrushSize
        {
            get
            {
                return _brushSize;
            }
            private set
            {
                _brushSize = MathHelper.Clamp(value, 1, int.MaxValue);
            }
        }

        /// <summary>
        /// What this brush actually draws around a given point
        /// </summary>
        public virtual Effect BrushShader { get; protected set; }

        #region Loading

        public sealed override void Load()
        {
            LoadBrush();
        }

        public sealed override void LoadAssets(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            // Emergency fallback but this should be overriden
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

        protected bool BrushShouldDraw { get; private set; }

        /// <summary>
        /// Update logic for the selected brush. Should always be run from Main's Update()
        /// </summary>
        public void MainUpdate()
        {
            BrushShouldDraw = false;
            if (MouseData.LeftClick == ButtonState.Pressed)
            {
                BrushShouldDraw = true;
            }

            BrushSize += (int)(MouseData.ScrollDelta * 0.00833333333333f); // divide by 120
            UpdateBrush();
        }

        protected virtual void UpdateBrush() { }

        /// <summary>
        /// Draw logic for the selected brush. Should always be run from Main's Draw()
        /// </summary>
        public void MainDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (!BrushShouldDraw)
            {
                return;
            }
            // Prevent normal drawing if specified
            Color? res = BrushDraw(spriteBatch, graphicsDevice);
            if (res is null)
            {
                return;
            }
            DefaultDraw(spriteBatch, graphicsDevice, res.Value);
        }

        /// <summary>
        /// Allows you to write custom draw logic for this brush or modify the color the default draw call is drawn with <br/>
        /// Return a color to allow default draw logic <br/>
        /// Return null to cancel this. <br/>
        /// Returns <see cref="Color.White"/> by default.
        /// </summary>
        protected virtual Color? BrushDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice) { return Color.White; }

        /// <summary>
        /// Fallback draw logic. Draws a full circle of brushSize width at the cursor. Like a pen tool
        /// </summary>
        private void DefaultDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Color drawColor)
        {
            graphicsDevice.SetRenderTarget(Main.Canvas);
            BrushShader.Parameters["BrushColor"].SetValue(drawColor.ToVector4());

            spriteBatch.Begin(SpriteSortMode.Immediate, effect: BrushShader);
            spriteBatch.DrawLine(MouseData.MoveHistory[0], MouseData.MoveHistory[1], BrushShader, BrushSize);
            spriteBatch.End();
        }

        /// <summary>
        /// For drawing anything outside of the brush's functionality on the UI layer. Such as a draw region indicator
        /// </summary>
        public virtual void UiDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice) { }
    }
}