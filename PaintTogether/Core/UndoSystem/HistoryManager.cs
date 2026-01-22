using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using PaintTogether.Common;
using PaintTogether.Common.DataTypes;
using PaintTogether.Common.PaintLogger;

namespace PaintTogether.Core.UndoSystem
{
    public static class HistoryManager
    {
        public static ShiftRegister<UndoableAction> CommandHistory = new ShiftRegister<UndoableAction>(32);
        public static ShiftRegister<UndoableAction> CommandRedoHistory = new ShiftRegister<UndoableAction>(32);

        private static UndoableAction ActionToInvoke;
        private static bool ApplyOrUndo = true;

        public static void Update()
        {
            return;
            ActionToInvoke = null;
            if (KeyboardData.state.IsKeyDown(Keys.LeftControl) && KeyboardData.KeyJustPressed(Keys.Z))
            {
                if (clLogger.VerboseLogging)
                {
                    clLogger.LogInfo($"CTRL+Z just pressed");
                }
                UndoMostRecent();
            }
            // Cant have them both at the same time now can we
            else if (KeyboardData.state.IsKeyDown(Keys.LeftControl) && KeyboardData.KeyJustPressed(Keys.Y))
            {
                if (clLogger.VerboseLogging)
                {
                    clLogger.LogInfo($"CTRL+Z just pressed");
                }
                RedoMostRecent();
            }

            
        }

        private static void UndoMostRecent()
        {
            if (!CommandHistory.HasData)
            {
                return;
            }

            UndoableAction action = CommandHistory.Pop();
            if (action is null)
            {
                return;
            }

            CommandRedoHistory.Push(action);
            
            ActionToInvoke = action;
            ApplyOrUndo = false;
        }

        private static void RedoMostRecent()
        {
            if (!CommandRedoHistory.HasData)
            {
                return;
            }

            UndoableAction action = CommandRedoHistory.Pop();
            if (action is null)
            {
                return;   
            }
            
            CommandHistory.Push(action);

            ActionToInvoke = action;
            ApplyOrUndo = true;
        }

        public static void Draw()
        {
            if (ActionToInvoke is not null)
            {
                if (ApplyOrUndo)
                {
                    ActionToInvoke.Apply();
                }
                else
                {
                    ActionToInvoke.Undo();
                }
            }
        }
    }
}