using Godot;
using System.Collections.Generic;

using ZAM.MenuUI;

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

    public static ButtonUI FocusOff(Container targetList, int currentCommand)
    {
        ButtonUI focusButton = null;
        if (targetList.GetChild(currentCommand).GetChildCount() > 0)
        {
            focusButton = targetList.GetChild(currentCommand).GetNode<ButtonUI>(ConstTerm.BUTTON);
            if (focusButton.HasFocus()) { focusButton.ReleaseFocus(); }
        }
        return focusButton;
    }

    public static ButtonUI FocusOn(Container targetList, int currentCommand)
    {
        ButtonUI focusButton = null;
        if (targetList.GetChild(currentCommand).GetChildCount() > 0)
        {
            focusButton = targetList.GetChild(currentCommand).GetNode<ButtonUI>(ConstTerm.BUTTON);
            if (focusButton != null) { focusButton.GrabFocus(); }
        }
        return focusButton;
    }

    public static void InvalidOption(Container targetList, int currentCommand, ref ButtonUI targetControl, out ButtonUI mouseFocus)
    {
        targetControl = FocusOn(targetList, currentCommand);
        ToggleMouseFilter(targetList, Control.MouseFilterEnum.Stop, out mouseFocus);
    }

    //=============================================================================
    // SECTION: Access Methods
    //=============================================================================

    // public static void DisableOption(Label option)
    // {
    //     option.Modulate = new Color(ConstTerm.GREY);

    //     ButtonUI optionButton = option.GetNode<ButtonUI>(ConstTerm.BUTTON);
    //     optionButton.Disabled = true;
    // }

    // public static void EnableOption(Label option)
    // {
    //     option.Modulate = new Color(ConstTerm.WHITE);

    //     ButtonUI optionButton = option.GetNode<ButtonUI>(ConstTerm.BUTTON);
    //     optionButton.Disabled = false;
    // }

    public static void ResetMouseInput(Container targetList, out ButtonUI mouseFocus)
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
        ToggleMouseFilter(targetList, Control.MouseFilterEnum.Stop, out mouseFocus);
    }

    public static void ToggleMouseFilter(Container targetList, Control.MouseFilterEnum value, out ButtonUI mouseFocus)
    {
        mouseFocus = null;
        for (int c = 0; c < targetList.GetChildCount(); c++)
        {
            targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).MouseFilter = value;
        }
    }

    public static int GetCommandCount(Container targetList)
    {
        return targetList.GetChildCount();
    }
}