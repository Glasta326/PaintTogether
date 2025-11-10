using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Content.Brushes;
using PaintTogether.Core;

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

        public override void Update()
        {
            base.Update();
        }  

        //lets have it similar to the Brush.cs but we check for the mouse being held, start drawing the preview and whatnot and then apply with the DrawCommand
        // when the mouse is let go of
        // once we get drawCommand working properly here we can apply it to brush considering the brush is basically just the line tool 60 times /s
    }
}