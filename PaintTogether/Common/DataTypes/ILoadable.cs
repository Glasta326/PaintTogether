using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PaintTogether.Common.DataTypes
{
    public interface ILoadable
    {
        /// <summary>
        /// Can be used to sort loadable types to be loaded before others if it is needed. <br/>
        /// Higher values mean higher priority. Priority 10 is loaded before priority 5. <br/>
        /// Defaults to 0.
        /// </summary>
        int LoadPriority => 0;

        /// <summary>
        /// Load any non-asset content here
        /// </summary>
        void Load() { }

        /// <summary>
        /// Load asset-related content here
        /// </summary>
        void LoadAssets(GraphicsDevice graphicsDevice, ContentManager contentManager) { }

        /// <summary>
        /// Unloads anything loaded in <see cref="Load"/>
        /// </summary>
        void Unload() { }
    }
}