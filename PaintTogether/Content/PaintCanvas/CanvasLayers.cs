using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Common.PaintLogger;

namespace PaintTogether.Content.PaintCanvas
{
    public struct CanvasLayers
    {
        private List<RenderTarget2D> _layers;

        private int _activeLayerIndex;

        /// <summary>
        /// The index in the layer stack of the currently selected and generally "active" layer
        /// </summary>
        public int ActiveLayerIndex
        {
            readonly get => _activeLayerIndex;
            set
            {
                _activeLayerIndex = Math.Max(0, value);
            }
        }

        /// <summary>
        /// Reference to the currently active rendertarget layer
        /// </summary>
        public readonly RenderTarget2D ActiveLayer => _layers[ActiveLayerIndex];

        public CanvasLayers()
        {
            _layers = new List<RenderTarget2D>();
            ActiveLayerIndex = 0;
        }

        public readonly RenderTarget2D this[int x]
        {
            get
            {
                if (x < 0)
                {
                    throw new IndexOutOfRangeException();
                }
                else
                {
                    return _layers[x];
                }
            }
        }

        public readonly int Count => _layers.Count;

        /// <summary>
        /// Inserts a new drawing layer into the canvas layer stack. <br/>
        /// Defaults to placing at the top of the stack (after everything else)
        /// </summary>
        /// <param name="index">Will be inserted at this position in the layer stack.<br/>
        /// If stack is [ layer1, layer2, layer3, layer4 ], inserting into the stack at index 3 will result in:<br/>
        /// [ layer1, layer2, newlayer3, layer 4, layer 5 ] </param>
        public void AddLayer(int index = -1)
        {
            try
            {
                if (index == -1)
                {
                    _layers.Add(new RenderTarget2D(Main.instance.GraphicsDevice, Canvas.Resolution.X, Canvas.Resolution.Y,
                    false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents));
                }
                else
                {
                    _layers.Insert(index, new RenderTarget2D(Main.instance.GraphicsDevice, Canvas.Resolution.X, Canvas.Resolution.Y,
                    false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents));
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                clLogger.LogWarning($"Attempted to insert canvas layer at position: {index} Which is outside of the bounds of layer stack: {_layers.Count}");
                AddLayer(-1); // Recursion!? In MY paint program? It's more likely than you think.
            }
            int placedIndex = index == -1 ? _layers.Count - 1 : index;
            clLogger.LogInfo($"Created layer at index {placedIndex}");
        }

        /// <summary>
        /// See <see cref="AddLayer"/> for more details:<br/>
        /// Adds a basic transparent layer.<br/>
        /// Mostly a shortcut for doing: <br/>
        /// Canvas.Layers.AddLayer();<br/>
        /// GraphicsDevice.SetRenderTarget(Canvas.Layers[x]);<br/>
        /// GraphicsDevice.Clear(Color.Transparent);
        /// </summary>
        public void AddBasicLayer(int index = -1)
        {
            AddLayer(index);
            if (index == -1)
            {
                index = this.Count;
            }
            Main.instance.GraphicsDevice.SetRenderTarget(this[index - 1]);
            Main.instance.GraphicsDevice.Clear(Color.Transparent);
        }


        /// <summary>
        /// Removes a layer from the stack at a given index
        /// </summary>
        /// <param name="index">Which layer to remove</param>
        public void RemoveLayer(int index)
        {
            try
            {
                // If we're about to remove the currently active layer, switch to the layer beneath
                if (index == ActiveLayerIndex)
                {
                    ActiveLayerIndex--;

                    // ...And make sure to prevent negative layer index
                    ActiveLayerIndex = Math.Max(ActiveLayerIndex, 0);
                }
                _layers.RemoveAt(index);
            }
            catch (ArgumentOutOfRangeException)
            {
                clLogger.LogWarning($"Attempted to remove canvas layer at position: {index} Which is outside of the bounds of layer stack: {_layers.Count}");
                // Doesn't throw any errors or crash.
                // Suppose maybe we clamp the index to the bounds of the layer stack, or some way of "fixing the index", and then remove that layer
                // There's a potential to just delete an unexpected layer that had important drawing on it. Which is bad.
            }
            clLogger.LogInfo($"Removed layer at index {index}");
        }
    }
}