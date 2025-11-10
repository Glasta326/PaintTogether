using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaintTogether.Core.UndoSystem
{
    /// <summary>
    /// Anything applied to the canvas inherits from one of these. <br/>
    /// Methods for what the action does, and how to undo it
    /// </summary>
    public class DrawCommand // TODO: probably best if we structure around sending these "Actions" to the server.
    {
        private Action _apply;

        private Action _undo;

        public DrawCommand(Action apply, Action undo)
        {
            _apply = apply;
            _undo = undo;
        }

        public void Apply() => _apply();

        public void Undo() => _undo();
    }
}