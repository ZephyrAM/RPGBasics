using Godot;
using System.Collections.Generic;

namespace ZAM.Control
{
    public partial class UIFunctions : Node // EDIT: In Progess. Template UI Controls
    {
        private int currentCommand = 0;
        private List<int> previousCommand = [];
        private int numColumn = 0;

        private string inputPhase;
        private List<string> previousPhase = [];

        private bool isActive = false;

        private Container activeList;
        private Button activeControl;
        private Button mouseFocus;

        //=============================================================================
        // SECTION: Selection Handling
        //=============================================================================

        public void CommandSelect(int change, Container targetList, string direction)
        {
            if (direction == ConstTerm.VERT) { change *= numColumn; }
            currentCommand = ChangeTarget(change, currentCommand, GetCommandCount(targetList));

            FocusOn(targetList);
            // EmitSignal(SignalName.onTargetChange);
        }

        public int ChangeTarget(int change, int target, int listSize)
        {
            // if (direction == ConstTerm.HORIZ) { change += change;}
            if (target + change > listSize - 1) { return 0; }
            else if (target + change < 0) { return listSize - 1; }
            else { return target += change; }
        }

        public void CancelSelect()
        {
            // if (inputPhase == ConstTerm.COMMAND) { MenuClose(); return; }

            FocusOff(activeList);
            ToggleMouseFilter(activeList, Godot.Control.MouseFilterEnum.Ignore);

            int oldCommand = previousCommand[^1];
            previousCommand.RemoveAt(previousCommand.Count - 1);
            currentCommand = oldCommand;

            string oldPhase = previousPhase[^1];
            previousPhase.RemoveAt(previousPhase.Count - 1);

            // SetMenuPhase(oldPhase);
            // SetNumColumn();

            ToggleMouseFilter(activeList, Godot.Control.MouseFilterEnum.Stop);
            FocusOn(activeList);

            // EmitSignal(SignalName.onTargetChange);
        }

        public void FocusOff(Container targetList)
        {
            if (targetList.GetChild(currentCommand).GetChildCount() > 0)
            {
                Button focusButton = targetList.GetChild(currentCommand).GetNode<Button>(ConstTerm.BUTTON);
                if (focusButton.HasFocus()) { focusButton.ReleaseFocus(); activeControl = null; }
            }
        }

        public void FocusOn(Container targetList)
        {
            if (targetList.GetChild(currentCommand).GetChildCount() > 0)
            {
                Button focusButton = targetList.GetChild(currentCommand).GetNode<Button>(ConstTerm.BUTTON);
                if (focusButton != null) { focusButton.GrabFocus(); activeControl = focusButton; }
            }
        }

        private void ToggleMouseFilter(Container targetList, Godot.Control.MouseFilterEnum value)
        {
            mouseFocus = null;
            for (int c = 0; c < targetList.GetChildCount(); c++)
            {
                targetList.GetChild(c).GetNode<Button>(ConstTerm.BUTTON).MouseFilter = value;
            }
        }

        //=============================================================================
        // SECTION: Access Methods
        //=============================================================================

        public void ResetMouseInput(Container targetList)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            ToggleMouseFilter(targetList, Godot.Control.MouseFilterEnum.Stop);
        }

        public int GetCommandCount(Container targetList)
        {
            return targetList.GetChildCount();
        }
    }
}