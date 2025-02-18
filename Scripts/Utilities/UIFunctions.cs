using Godot;
using System.Collections.Generic;

public interface IUIFunctions // EDIT: In Progess. Template UI Controls
{
    //=============================================================================
    // SECTION: Selection Handling
    //=============================================================================

    public static int CheckColumn(int change, string direction, int numColumn)
    {
        if (direction == ConstTerm.VERT) { change *= numColumn; }
        return change;
    }

    public static void ChangeTarget(int change, ref int target, int listSize)
    {
        // if (direction == ConstTerm.HORIZ) { change += change;}
        if (target + change > listSize - 1) { target = 0; }
        else if (target + change < 0) { target = listSize - 1; }
        else { target += change; }
    }

    public static string CancelSelect(out int currentCommand, List<int> previousCommand, List<string> previousPhase)
    {
        int oldCommand = previousCommand[^1];
        previousCommand.RemoveAt(previousCommand.Count - 1);
        currentCommand = oldCommand;

        string oldPhase = previousPhase[^1];
        previousPhase.RemoveAt(previousPhase.Count - 1);
        return oldPhase;
    }

    public static Button FocusOff(Container targetList, int currentCommand)
    {
        Button focusButton = null;
        if (targetList.GetChild(currentCommand).GetChildCount() > 0)
        {
            focusButton = targetList.GetChild(currentCommand).GetNode<Button>(ConstTerm.BUTTON);
            if (focusButton.HasFocus()) { focusButton.ReleaseFocus(); }
        }
        return focusButton;
    }

    public static Button FocusOn(Container targetList, int currentCommand)
    {
        Button focusButton = null;
        if (targetList.GetChild(currentCommand).GetChildCount() > 0)
        {
            focusButton = targetList.GetChild(currentCommand).GetNode<Button>(ConstTerm.BUTTON);
            if (focusButton != null) { focusButton.GrabFocus(); }
        }
        return focusButton;
    }

    //=============================================================================
    // SECTION: Access Methods
    //=============================================================================

    public static void ResetMouseInput(Container targetList, out Button mouseFocus)
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
        ToggleMouseFilter(targetList, Control.MouseFilterEnum.Stop, out mouseFocus);
    }

    public static void ToggleMouseFilter(Container targetList, Control.MouseFilterEnum value, out Button mouseFocus)
    {
        mouseFocus = null;
        for (int c = 0; c < targetList.GetChildCount(); c++)
        {
            targetList.GetChild(c).GetNode<Button>(ConstTerm.BUTTON).MouseFilter = value;
        }
    }

    public static int GetCommandCount(Container targetList)
    {
        return targetList.GetChildCount();
    }
}