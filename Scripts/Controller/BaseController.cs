using Godot;
using Godot.Collections;
// using System.Collections.Generic;

using ZAM.MenuUI;

namespace ZAM.Controller
{
    public partial class BaseController : Node
    {
        protected ButtonUI mouseFocus = null;
        protected Container activeList = null;
        protected ButtonUI activeControl = null;

        protected Array<string> previousPhase = [];
        protected int numColumn = 1;
        protected int currentCommand = 0;
        protected Array<int> previousCommand = [];
        protected Dictionary<string, Container> listDict = [];

        protected bool signalsDone = false;
        protected bool controlActive = false;
        protected string activeInput = ConstTerm.KEY_GAMEPAD;
        protected string inputPhase = ConstTerm.WAIT;

        //=============================================================================
        // SECTION: Base Methods
        //=============================================================================

        public override void _Ready()
        {
            IfNull();
        }

        protected virtual void IfNull()
        {
            SetupListDict();
        }

        public virtual void SubLists(Container targetList)
        {
            for (int c = 0; c < targetList.GetChildCount(); c++)
            {
                Node tempLabel = targetList.GetChild(c);
                targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).MouseEntered += () => OnMouseEntered(targetList, tempLabel);
                targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).Pressed += OnMouseClick;
            }
        }

        public virtual void UnSubSignals(Container targetList)
        {
            for (int c = 0; c < targetList.GetChildCount(); c++)
            {
                Node tempLabel = targetList.GetChild(c);
                targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).MouseEntered -= () => OnMouseEntered(targetList, tempLabel);
                targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).Pressed -= OnMouseClick;
            }
        }

        protected virtual void SetupListDict()
        {

        }

        //=============================================================================
        // SECTION: Phase Handling - Input
        //=============================================================================

        protected virtual bool PhaseCheck(InputEvent @event)
        {
            if (@event is InputEventMouse && activeInput == ConstTerm.KEY_GAMEPAD) { activeInput = ConstTerm.MOUSE;
                Input.MouseMode = Input.MouseModeEnum.Visible; if (mouseFocus != null) { mouseFocus.MouseFilter = Control.MouseFilterEnum.Stop; } }
            else if (@event is not InputEventMouse && activeInput == ConstTerm.MOUSE) { activeInput = ConstTerm.KEY_GAMEPAD;
                Input.MouseMode = Input.MouseModeEnum.Hidden; if (mouseFocus != null) { mouseFocus.MouseFilter = Control.MouseFilterEnum.Ignore; } }

            if (!IsControlActive()) { return false; }
            // switch (inputPhase)
            // {
            //     default:
            //         break;
            // }

            if (@event is InputEventMouseButton) { 
                if (@event.IsActionPressed(ConstTerm.CANCEL + ConstTerm.CLICK)) {
                    CancelCycle(); } }
            
            return true;
        }

        protected virtual bool AcceptInput()
        {
            activeControl = IUIFunctions.FocusOff(activeList, currentCommand);
            if (activeControl.OnButtonPressed()) { InvalidOption(); return false; }

            previousPhase.Add(GetInputPhase());
            return true;
        }

        protected virtual void PhaseControls(InputEvent @event)
        {
            if (@event.IsActionPressed(ConstTerm.CANCEL)) {
                CancelCycle();
            }
            else if (@event.IsActionPressed(ConstTerm.UP)) {
                CommandSelect(-1, ConstTerm.VERT);
            }
            else if (@event.IsActionPressed(ConstTerm.DOWN)) {
                CommandSelect(1, ConstTerm.VERT);
            }
            else if (@event.IsActionPressed(ConstTerm.LEFT)) {
                CommandSelect(-1, ConstTerm.HORIZ);
            }
            else if (@event.IsActionPressed(ConstTerm.RIGHT)) {
                CommandSelect(1, ConstTerm.HORIZ);
            }
        }


        //=============================================================================
        // SECTION: Selection Handling
        //=============================================================================

        protected virtual void CommandSelect(int change, string direction)
        {
            change = IUIFunctions.CheckColumn(change, direction, numColumn);
            IUIFunctions.ChangeTarget(change, ref currentCommand, IUIFunctions.GetCommandCount(activeList));

            activeControl = IUIFunctions.FocusOn(activeList, currentCommand);
        }

        protected virtual void SetNewCommand()
        {
            previousCommand.Add(currentCommand);
            currentCommand = 0;
            
            activeControl = IUIFunctions.FocusOn(activeList, currentCommand);
        }

        protected virtual void ResetCommandPhase()
        {
            currentCommand = 0;
            previousCommand = [];
            previousPhase = [];
        }

        protected virtual void CancelCycle() // EDIT: Might not work through inheritance
        {
            // if (inputPhase == ConstTerm.COMMAND) { return; } // EDIT: If base screen, return

            activeControl = IUIFunctions.FocusOff(activeList, currentCommand);
            IUIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Ignore, out mouseFocus);

            string oldPhase = IUIFunctions.CancelSelect(out currentCommand, previousCommand, previousPhase);
            SetInputPhase(oldPhase);

            activeControl = IUIFunctions.FocusOn(activeList, currentCommand);
            IUIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Stop, out mouseFocus);
        }

        //=============================================================================
        // SECTION: Command Options
        //=============================================================================

        protected virtual void InvalidOption()
        {
            activeControl = IUIFunctions.FocusOn(activeList, currentCommand);
            IUIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Stop, out mouseFocus);
            // play 'cannot use' sound effect
        }

        //=============================================================================
        // SECTION: Signal Methods
        //=============================================================================

        protected virtual void OnMouseEntered(Container currList, Node currLabel)
        {
            if (currList != activeList) { return; }

            activeControl = IUIFunctions.FocusOff(currList, currentCommand);
            currentCommand = currLabel.GetIndex();

            activeControl = IUIFunctions.FocusOn(currList, currentCommand);
            mouseFocus = currLabel.GetNode<ButtonUI>(ConstTerm.BUTTON);
        }

        protected virtual void OnMouseClick()
        {
            // switch (inputPhase)
            // {
            //     default:
            //         break;
            // }
        }

        //=============================================================================
        // SECTION: External Access
        //=============================================================================

        public bool IsControlActive()
        {
            return controlActive;
        }

        public virtual void SetControlActive(bool value)
        {
            controlActive = value;
        }

        public string GetInputPhase()
        {
            return inputPhase;
        }

        public virtual void SetInputPhase(string phase)
        {
            inputPhase = phase;
            if (listDict.TryGetValue(phase, out Container value)) { activeList = value; }
            else { return; }
            SetNumColumn();
        }

        public int GetCommand()
        {
            return currentCommand;
        }

        public int GetPrevCommand()
        {
            return previousCommand[^1];
        }

        public int GetNumColumn()
        {
            return numColumn;
        }

        public virtual void SetNumColumn()
        {
            numColumn = 1;
            if ((bool)activeList.Get(ConstTerm.COLUMNS)) { numColumn = (int)activeList.Get(ConstTerm.COLUMNS); }
            // if (inputPhase == ConstTerm.SKILL + ConstTerm.SELECT) { numColumn = skillList.Columns; }
            // else if (inputPhase == ConstTerm.ITEM + ConstTerm.SELECT) { numColumn = itemList.Columns; }
            // else { numColumn = 1; }
        }
    }
}