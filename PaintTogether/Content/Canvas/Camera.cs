using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using PaintTogether.Common;

namespace PaintTogether.Content.Canvas
{
    public struct CanvasCamera
    {
        private float _zoomLevel;

        public float Zoom { get; set; }

        private Vector2 _cameraPosition;

        public Vector2 Position{ get; set; }

        public CanvasCamera()
        {
            Zoom = 1f;
            Position = Vector2.Zero;
        }

        /// <summary>
        /// Zooms towards a specific position in screen space
        /// </summary>
        /// <param name="zoomChange">Multiplier on the zoom level.<br/>
        /// A value of 1.1 will do currentZoomLevel *= 1.1f</param>
        /// <param name="zoomPos">Where in screen space (The actual window and pixels on the monitor) to zoom in to.<br/>
        /// Defaults to the center of the screen</param>
        public void ZoomToPosition(float zoomChange, Vector2? zoomPos = null)
        {
            // Default to just zooming on the midpoint of the screen
            zoomPos ??= WindowData.WindowSize * 0.5f;

            // Calculate the movement difference between zoom application
            Vector2 preZoom = Canvas.ScreenToCanvas(zoomPos.Value);
            Zoom *= zoomChange; // Multiply by a zoom change instead of adding a flat value so the amount scaled is the same relative to the zoom level
            Zoom = MathHelper.Clamp(Zoom, 0.1f, 20f); // I guess this techinically limits the user but they can just use the manual zoom set UI if they really need 100x zoomout
            Vector2 postZoom = Canvas.ScreenToCanvas(zoomPos.Value);

            // Adjust the camera position by the opposite of however zooming wouldve moved it
            Position += (postZoom - preZoom);
        }
        
        /// <summary>
        /// Sets the zoom level of the canvas
        /// </summary>
        /// <param name="zoomLevel">The new zoom level</param>
        /// <param name="zoomPos">Where in screen space (The actual window and pixels on the monitor) to zoom in to.<br/>
        /// Defaults to the center of the screen</param>
        public void SetZoomLevel(float zoomLevel, Vector2? zoomPos = null)
        {
            // Default to just zooming on the midpoint of the screen
            zoomPos ??= WindowData.WindowSize * 0.5f;

            // Calculate the movement difference between zoom application
            Vector2 preZoom = Canvas.ScreenToCanvas(zoomPos.Value);
            Zoom = zoomLevel; // Multiply by a zoom change instead of adding a flat value so the amount scaled is the same relative to the zoom level
            Vector2 postZoom = Canvas.ScreenToCanvas(zoomPos.Value);

            // Adjust the camera position by the opposite of however zooming wouldve moved it
            Position += (postZoom - preZoom);
        }
    }
}