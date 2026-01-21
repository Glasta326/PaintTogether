using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaintTogether.Core.Networking.Registry;
using PaintTogether.Core.Users;

namespace PaintTogether.Core.UndoSystem
{
    public static class History
    {
        /// <summary>
        /// Undo stacks for each user. Any actions are pushed into an UndoableAction stack for the respective user. Any undo calls are also pushed into the redo stack
        /// </summary>
        public static Dictionary<byte, Stack<UndoableAction>> ActionHistory = new Dictionary<byte, Stack<UndoableAction>>();

        /// <summary>
        /// Redo stacks for each user. Any actions redone here are pushed into the actionHistory. Any actions other than undoing will clear the redo history for that user
        /// </summary>
        public static Dictionary<byte, Stack<UndoableAction>> ActionRedoHistory = new Dictionary<byte, Stack<UndoableAction>>();

        public static void RegisterUser(PaintUser user)
        {
            
        }

        public static void Update()
        {

        }

        public static void Undo()
        {
            
        }

        public static void Redo()
        {
            
        }

        public static void Draw()
        {
            
        }
    }
}