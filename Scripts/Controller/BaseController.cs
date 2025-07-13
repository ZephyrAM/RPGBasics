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

        protected Dictionary<Node, Callable> activeSignals = [];

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

        protected override void Dispose(bool disposing)
        {
            // UnSubSignals(); // EDIT: Signals not disconnecting properly, but being dispoed. BUG?
            base.Dispose(disposing);
        }

        public virtual void SubLists(Container targetList)
        {
            // activeSignals = [];
            foreach (Node child in targetList.GetChildren())
            {
                activeSignals[child] = Callable.From(() => OnMouseEntered(targetList, child));
                // child.GetNode<ButtonUI>(ConstTerm.BUTTON).MouseEntered += () => OnMouseEntered(targetList, child);
                child.GetNode<ButtonUI>(ConstTerm.BUTTON).Pressed += OnMouseClick;
                child.GetNode<ButtonUI>(ConstTerm.BUTTON).Connect(ButtonUI.SignalName.MouseEntered, activeSignals[child]);
                // child.GetNode<ButtonUI>(ConstTerm.BUTTON).Connect(ButtonUI.SignalName.Pressed, Callable.From(OnMouseClick));

            }
            // for (int c = 0; c < targetList.GetChildCount(); c++)
            // {
            //     Node tempLabel = targetList.GetChild(c);
            //     activeSignals.Add(Callable.From(() => OnMouseEntered(targetList, tempLabel)));
            //     // targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).MouseEntered += () => OnMouseEntered(targetList, tempLabel);
            //     targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).Pressed += OnMouseClick;
            //     targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).Connect(ButtonUI.SignalName.MouseEntered, activeSignals[c]);
            //     // targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).Connect(ButtonUI.SignalName.Pressed, Callable.From(OnMouseClick));
            // }
        }

        public virtual void UnSubLists(Container targetList)
        {
            foreach (Node child in targetList.GetChildren())
            {
                child.GetNode<ButtonUI>(ConstTerm.BUTTON).Pressed -= OnMouseClick;
                child.GetNode<ButtonUI>(ConstTerm.BUTTON).Disconnect(ButtonUI.SignalName.MouseEntered, activeSignals[child]);

                activeSignals.Remove(child);
            }
            // for (int c = 0; c < targetList.GetChildCount(); c++)
            // {
            //     Node tempLabel = targetList.GetChild(c);
            //     targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).MouseEntered -= () => OnMouseEntered(targetList, tempLabel);
            //     targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).Pressed -= OnMouseClick;
            //     // targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).Disconnect(ButtonUI.SignalName.MouseEntered, Callable.From(() => OnMouseEntered(targetList, tempLabel)));
            //     // targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).Disconnect(ButtonUI.SignalName.Pressed, Callable.From(OnMouseClick));
            // }
        }

        protected virtual void SetupListDict()
        {

        }

        protected virtual void UnSubSignals()
        {
            
        }

        //=============================================================================
        // SECTION: Phase Handling - Input
        //=============================================================================

        protected virtual bool PhaseCheck(InputEvent @event)
        {
            if (@event is InputEventMouse && activeInput == ConstTerm.KEY_GAMEPAD)
            {
                activeInput = ConstTerm.MOUSE;
                Input.MouseMode = Input.MouseModeEnum.Visible; if (mouseFocus != null) { mouseFocus.MouseFilter = Control.MouseFilterEnum.Stop; }
            }
            else if (@event is not InputEventMouse && activeInput == ConstTerm.MOUSE)
            {
                activeInput = ConstTerm.KEY_GAMEPAD;
                Input.MouseMode = Input.MouseModeEnum.Hidden; if (mouseFocus != null) { mouseFocus.MouseFilter = Control.MouseFilterEnum.Ignore; }
            }

            if (!IsControlActive()) { return false; }
            // switch (inputPhase)
            // {
            //     default:
            //         break;
            // }

            if (inputPhase == ConstTerm.REBIND) { goto SkipCancel; }
            if (@event is InputEventMouseButton)
            {
                if (@event.IsActionPressed(ConstTerm.CANCEL + ConstTerm.CLICK))
                {
                    CancelCycle();
                }
            }

            SkipCancel:
            return true;
        }

        protected virtual bool AcceptInput()
        {
            activeControl = UIFunctions.FocusOff(activeList, currentCommand);
            if (activeControl.OnButtonPressed()) { InvalidOption(); return false; }

            previousPhase.Add(GetInputPhase());
            UIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Ignore, out mouseFocus);
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
            change = UIFunctions.CheckColumn(change, direction, numColumn);
            UIFunctions.ChangeTarget(change, ref currentCommand, UIFunctions.GetCommandCount(activeList));

            activeControl = UIFunctions.FocusOn(activeList, currentCommand);
        }

        protected virtual void SetNewCommand()
        {
            previousCommand.Add(currentCommand);
            currentCommand = 0;

            activeControl = UIFunctions.FocusOn(activeList, currentCommand);
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

            activeControl = UIFunctions.FocusOff(activeList, currentCommand);
            UIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Ignore, out mouseFocus);

            string oldPhase = UIFunctions.CancelSelect(out currentCommand, previousCommand, previousPhase);
            SetInputPhase(oldPhase);

            activeControl = UIFunctions.FocusOn(activeList, currentCommand);
            UIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Stop, out mouseFocus);
        }

        //=============================================================================
        // SECTION: Command Options
        //=============================================================================

        protected virtual void InvalidOption()
        {
            activeControl = UIFunctions.FocusOn(activeList, currentCommand);
            UIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Stop, out mouseFocus);
            // play 'cannot use' sound effect
        }

        //=============================================================================
        // SECTION: Signal Methods
        //=============================================================================

        protected virtual void OnMouseEntered(Container currList, Node currLabel)
        {
            // if (currList != activeList) { return; }
            activeList = currList;

            activeControl = UIFunctions.FocusOff(currList, currentCommand);
            currentCommand = currLabel.GetIndex();

            activeControl = UIFunctions.FocusOn(currList, currentCommand);
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

        public string GetPrevPhase()
        {
            return previousPhase[^1];
        }

        public virtual void SetInputPhase(string phase)
        {
            inputPhase = phase;
            if (listDict.TryGetValue(phase, out Container value))
            {
                activeList = value;
                UIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Stop, out mouseFocus);
            }
            else { return; }
            SetNumColumn();
        }

        public virtual void ChangeActiveList(Container list) // For use with multiple, simultaneously active lists
        {
            activeControl = UIFunctions.FocusOff(activeList, currentCommand);
            // UIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Ignore, out mouseFocus);

            activeList = list;
            UIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Stop, out mouseFocus);
            activeControl = UIFunctions.FocusOn(activeList, currentCommand);
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