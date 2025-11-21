using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using PaintTogether.Common.DataTypes;

namespace PaintTogether.Common
{
    public static class KeyboardData
    {
        public static KeyboardState state;

        public static ShiftRegister<KeyboardState> KeyboardHistory = new ShiftRegister<KeyboardState>(2);

        public static bool KeyJustPressed(this Keys key) => KeyboardHistory[0].IsKeyDown(key) && KeyboardHistory[1].IsKeyUp(key);

        public static bool KeyJustReleased(this Keys key) => KeyboardHistory[0].IsKeyUp(key) && KeyboardHistory[1].IsKeyDown(key);

        
    }
}