using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace PaintTogether.Common
{
    public static class WindowData
    {
        public static Vector2 WindowSize => new Vector2(Main.instance.Window.ClientBounds.Width, Main.instance.Window.ClientBounds.Height);

        public static Vector2 ResolutionMultiplier => WindowSize / new Vector2(Main.Canvas.Width, Main.Canvas.Height);
    }
}