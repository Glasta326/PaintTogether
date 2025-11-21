using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace PaintTogether.Core.UndoSystem
{
    /// <summary>
    /// Anything applied to the canvas inherits from one of these. <br/>
    /// Methods for what the action does, and how to undo it
    /// </summary>
    public class DrawCommand // TODO: probably best if we structure around sending these "Actions" to the server.
    {
        // Method that draws this drawCommand
        private Action<SpriteBatch, GraphicsDevice, object[]> _apply;

        // Method that undoes this drawCommand
        private Action _undo;

        public DrawCommand(Action<SpriteBatch, GraphicsDevice, object[]> apply, Action undo)
        {
            _apply = apply;
            _undo = undo;
        }

        public Action<SpriteBatch, GraphicsDevice, object[]> Apply => _apply;

        public Action Undo => _undo;
    }

    /*
    Example showing using an already defined function as a parameter, or using lambda syntax
    EXAMPLE USAGE:
    
    DrawCommand command = new DrawCommand(ExampleFunction, () => { return; } );

    private void ExampleFunction(SpriteBatch sb, GraphicsDevice gd, object[] data)
    {
        if (data is null)
        {
            sb.Draw();
        }
        return;
    }

    NOTES:
    The idea is you parse in external values and whatever, writing it like you normally would a function
    The .Apply() and .Undo() essentially capture the values it was given at the time
    So when you create a drawCommand that draws a string at 0,0 with red text and whatever
    Whenever you call .Apply() it essentially runs the function to draw the string at 0,0 with red text and whatever the string was at the time
    its like a hardcoded function
    */


    // This is copied from excel where i was figuring out how to make ctrlz/networking work
    // Do NOT want to lose this train of thought
    // basically every action performed (using excel as the example here) is stored locally, with an appropririate command to undo that action
    // if i write "hello" in an empty cell, before the program tells google to set the cell at XY to "hello", my client first notes down the cell was blank before we did anyhting to it
    // then, in a list somewhere a new action() is added with the undo() logic set to "make cell at XY = ' ' ". 
    // its probably better if instead the server is the one storing this list, along with a tag of *who* made that action
    // that way if user A does something, user B does something else and then user a hits ctrl+z, we can just ask the server to call undo() on the most recent action() that user A did
    // for paintTogether, the best way i can think of replicating this is basically
    // ok so you know how everything is just a shader over a rectangle with a dummy texture
    // Like in the drawLine method or the TestTool draw method rn
    // We could make it so the program first captures the pixels in the affected area, and says the undo() method should just set the area to be whatever those pixels were
    // even better: we compare the diff between the capture of the pixels before applying, and after applying, and then only the pixels changed should have their values set to whatever it was before
    // think of it like the diff being a bitmask over the whole saved rectangle of what it previously was
    // that way, if i wanted to undo a line draw, it wont set the whole x by y area of the line to be whatever it was before, and instead only the pixels that take up the line area that was drawn
    // TODO: IMPORTANT!!!!!!!

    /*
    ok so every action is stored with the execute and undo system
    in excel, when i write into a cell, the program stores the state of the cell before i did anything to it, and the new state
    then, the client? seems to store a list of these actions, and re-steps then applying the undo() (old state) to the affected cell when ctrlz happens
    */

}