using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaintTogether.Core.Users
{
    public class UserAction
    {
        private Action _apply;

        private Action _undo;

        private byte _ownerID;

        /// <summary>
        /// The <see cref="PaintUser"/> who performed this action.
        /// </summary>
        public PaintUser Owner => PaintUser.UserRegistry[_ownerID];

        /// <summary>
        /// Performs the action the user did when creating this action
        /// </summary>
        public void Apply() => _apply.Invoke();

        /// <summary>
        /// Restores the state of the context of this action to the state before the user performed this action
        /// </summary>
        public void Undo() => _undo.Invoke();

        /// <summary>
        /// Creates an action from the user which is invokable and undoable. Can be automatically pushed into the appropriate <see cref="PaintUser.UndoHistory"/>
        /// </summary>
        /// <param name="Apply">The action that applies this useraction to the canvas</param>
        /// <param name="Undo">The action that undoes the Apply action</param>
        /// <param name="OwnerID">The clientID of who performed this action</param>
        /// <param name="auto">Whether to automatically push into the appropriate user's <see cref="PaintUser.UndoHistory"/></param>
        public UserAction(Action Apply, Action Undo, byte OwnerID, bool auto = true)
        {
            _apply = Apply;
            _undo = Undo;
            _ownerID = OwnerID;

            // Automatically insert to the undo stack for the user
            if (auto)
            {
                Owner.UndoHistory.Push(this);
                Owner.RedoHistory.Clear(); // Cleared every time the user performs an action    
            }
        }
    }
}