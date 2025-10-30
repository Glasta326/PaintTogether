using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PaintTogether.Common;
using PaintTogether.Common.Utilities;
using PaintTogether.Core;

namespace PaintTogether.Content.UI
{
    public class ColorSelector : Element
    {
        static string hexCode = "000000";
        Rectangle area;
        public static bool isFocused = false;

        public static Color drawColor = Color.Navy;
        public override void Load()
        {
            Main.instance.Window.TextInput += HandleText;

            area = new Rectangle(300, 50, 150, 50);
        }

        private static void HandleText(object sender, TextInputEventArgs e)
        {
            if (!isFocused)
            {
                return;
            }

            if (e.Character == '\b' && hexCode.Length > 0)
            {
                hexCode = hexCode[..^1];
            }
            else if (e.Character == '\n')
            {
                isFocused = false;
                return;
            }
            else if (hexCode.Length < 6 && e.Character != '\b' && !char.IsWhiteSpace(e.Character))
            {
                hexCode += e.Character;
            }
        }

        public override void Update()
        {
            drawColor = Color.Navy;
            if (area.Contains(MouseData.MousePosPoint()))
            {
                drawColor = Color.DarkGreen;
                if (MouseData.LeftClick == ButtonState.Pressed)
                {
                    isFocused = true;
                }
            }
            else
            {
                isFocused = false;
            }

            if (isFocused)
            {
                drawColor = Color.LightGray;
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(CommonKeys.WhitePixel, area, drawColor);
            spriteBatch.DrawString(Main.font, hexCode, new Vector2(area.X, area.Y), GetColor());
            spriteBatch.End();
        }

        public static Color GetColor()
        {
            try
            {
                if (hexCode.Length == 6)
                {
                    byte r = Convert.ToByte(hexCode[..2], 16);
                    byte g = Convert.ToByte(hexCode.Substring(2, 2), 16);
                    byte b = Convert.ToByte(hexCode.Substring(4, 2), 16);
                    return new Color(r, g, b);
                }
            }
            catch
            {
                hexCode = "";
            }
            return Color.White; // fallback
        }
    }
}