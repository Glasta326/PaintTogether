using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PaintTogether.Common.Utilities
{
    public static class Utils
    {
        public static Vector2 Size(this Texture2D texture)
        {
            return new Vector2(texture.Width, texture.Height);
        }




        

    }
}