using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PaintTogether.Common.DataTypes;
using PaintTogether.Common.Utilities;
using PaintTogether.Core.LoadSystem;

namespace PaintTogether.Content.Brushes
{
    public abstract class Brush : ILoadable
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

        // I'm fairly sure what im doing here is terrible and awful, but i'm not actually sure
        void ILoadable.Load()
        {
            Console.WriteLine($"Loading content for {this.ToString()}");
            Load();
        }

        void ILoadable.LoadAssets(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            Console.WriteLine($"Loading assets for {this.ToString()}");

            // Emergency fallback but this should be overriden almost immediatly
            BrushShader = contentManager.Load<Effect>("Shaders/test2");

            LoadAssets(graphicsDevice, contentManager);
        }

        void ILoadable.Unload()
        {
            Console.WriteLine($"Unloading content for {this.ToString()}");
        }

        /// <summary>
        /// Load any non-asset content here
        /// </summary>
        protected virtual void Load() { }

        /// <summary>
        /// Load asset-related content here
        /// </summary>
        protected virtual void LoadAssets(GraphicsDevice graphicsDevice, ContentManager contentManager) { }

        /// <summary>
        /// Unloads anything loaded in <see cref="Load"/>
        /// </summary>
        protected virtual void Unload() { }

        #endregion

        private bool DoDraw = false;

        /// <summary>
        /// Update logic for the selected brush. Should always be run from Main's Update()
        /// </summary>
        public void MainUpdate()
        {
            DoDraw = false;
            if (MouseUtils.LeftClick == ButtonState.Pressed)
            {
                DoDraw = true;
            }

            BrushSize += (int)(MouseUtils.ScrollDelta * 0.00833333333333f); // divide by 120
            Update();
        }

        protected virtual void Update() { }

        /// <summary>
        /// Draw logic for the selected brush. Should always be run from Main's Draw()
        /// </summary>
        public void MainDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (!DoDraw)
            {
                return;
            }
            // Prevent normal drawing if specified
            if (!Draw(spriteBatch, graphicsDevice)) 
            {
                return;
            }
            DefaultDraw(spriteBatch, graphicsDevice);
        }

        /// <summary>
        /// Allows you to write custom draw logic for this brush. <br/>
        /// Return false to prevent default draw logic from running <br/>
        /// Returns true by default.
        /// </summary>
        protected virtual bool Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice) { return true; }

        private void DefaultDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            graphicsDevice.SetRenderTarget(Main.Canvas);
            BrushShader.Parameters["BrushColor"].SetValue(Color.Red.ToVector4());

            spriteBatch.Begin(SpriteSortMode.Immediate, effect: BrushShader);
            DrawUtils.DrawLine(MouseUtils.MoveHistory[0], MouseUtils.MoveHistory[1], BrushShader, spriteBatch,null, out _, out _);
            spriteBatch.End();
        }
    }
}