using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaintTogether.Common.PaintLogger;

namespace PaintTogether.Core.UndoSystem
{
    // TODO: Make it so Apply and Undo are exposed publicly, atleast get; should be.
    // We dont need the void Undo()/ void Apply() anymore as this isnt abstract
    // Instead the Historymanager can directly get the apply and undo actions from the UndoableAction
    // "oh but my logging" it's useless from here it just shows Applied: UndoableAction lmfao
    // do it from HistoryManager whenever ctrlz or ctrly is pressed
    public class UndoableAction
    {
        private Action _apply;

        private Action _undo;

        public UndoableAction(Action apply, Action Undo)
        {
            _apply = apply;
            _undo = Undo;

            // Automatically push this action to the undo history
            HistoryManager.CommandHistory.Push(this);

            // Whenever a new action by the user occurs,
            // redo history is cleared because otherwise id have to deal with multiple timelines and the fucking multiverse
            HistoryManager.CommandRedoHistory.Clear();
        }

        public void Apply()
        {
            clLogger.LogInfo($"Applied: {this.GetType().Name}");
            _apply();
        }

        public void Undo()
        {
            clLogger.LogInfo($"Undo: {this.GetType().Name}");
            _undo();
        }
    }
}