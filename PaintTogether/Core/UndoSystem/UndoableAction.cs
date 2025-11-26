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
    // Also there should be a manadatory "name" field for each undoableAction both for debugging reasons so i can see what the undo stack actually is composed of
    // and the log can print the name of each thing added to it, but also there could be UI to see what the undo history contains
    // maybe the user wants to know how many times they clicked the circle tool idfk
    // extending that thought maybe anything inheriting from brush or tool could also have a mandatory name field that is passed to the undoable action when the action is createad
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

            if (clLogger.VerboseLogging)
            {
                clLogger.LogInfo($"New action: {this.GetType().Name}");
            }
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