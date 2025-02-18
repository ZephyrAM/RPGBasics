using Godot;
using System.Collections.Generic;

using ZAM.Inventory;

namespace ZAM.Control
{
    public partial class MenuController : Node, IUIFunctions
    {
        [Export] private string[] commandOptions = [];
        [Export] private float containerEdgeBuffer = 0;

        [ExportGroup("Lists")]
        [Export] private VBoxContainer commandList = null;
        [Export] private VBoxContainer infoList = null;
        [Export] private PanelContainer skillPanel = null;
        [Export] private PanelContainer itemPanel = null;

        private GridContainer skillList;
        private GridContainer itemList;
        private float skillItemWidth = 0;

        private Dictionary<string, Container> listDict = [];
        private bool signalsDone = false;

        private CharacterController playerInput = null;
        private string activeInput = ConstTerm.KEY_GAMEPAD;

        private string inputPhase = ConstTerm.WAIT;
        private List<string> previousPhase = [];
        private string memberOption;
        private bool isActive = false;

        private Container activeList = null;
        private Button activeControl = null;
        private Button mouseFocus = null;

        private int currentCommand = 0;
        private List<int> previousCommand = [];
        private int memberSelect = 0;
        private int numColumn = 1;

        // Delegate Events \\
        [Signal]
        public delegate void onMouseHandlingEventHandler(bool toggle);
        [Signal]
        public delegate void onMenuCloseEventHandler();
        [Signal]
        public delegate void onMenuPhaseEventHandler();
        // [Signal]
        // public delegate void onMenuUpdateEventHandler();
        [Signal]
        public delegate void onTargetChangeEventHandler();
        [Signal]
        public delegate void onMemberSelectEventHandler();

        //=============================================================================
        // SECTION: Base Methods
        //=============================================================================
        public override void _Ready()
        {
            IfNull();
        }

        private void IfNull()
        {
            skillList = skillPanel.GetNode<GridContainer>(ConstTerm.TEXT + ConstTerm.LIST);
            itemList = itemPanel.GetNode<GridContainer>(ConstTerm.TEXT + ConstTerm.LIST);

            skillItemWidth = skillPanel.GetRect().Size.X / 2 - containerEdgeBuffer;

            SetupListDict();
        }

        private void SetupListDict()
        {
            listDict.Add(ConstTerm.COMMAND, commandList);
            listDict.Add(ConstTerm.MEMBER + ConstTerm.SELECT, infoList);
            listDict.Add(ConstTerm.ITEM + ConstTerm.SELECT, itemList);
            listDict.Add(ConstTerm.SKILL + ConstTerm.SELECT, skillList);
        }

        private void SubSignals()
        {
            if (signalsDone) { return; }

            SubLists(commandList);
            SubLists(infoList);

            signalsDone = true;
        }

        private void SubLists(Container targetList)
        {
            for (int c = 0; c < targetList.GetChildCount(); c++)
            {
                Node tempLabel = targetList.GetChild(c);
                targetList.GetChild(c).GetNode<Button>(ConstTerm.BUTTON).MouseEntered += () => OnMouseEntered(targetList, tempLabel);
                targetList.GetChild(c).GetNode<Button>(ConstTerm.BUTTON).Pressed += OnMouseClick;
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (!isActive) { return; }

            PhaseCheck(@event);
        }

        public override void _PhysicsProcess(double delta)
        {
            if (!isActive) { return; }
            SubSignals();
            
            if (Input.IsActionJustPressed(ConstTerm.MENU)) { MenuClose(); }
        }

        //=============================================================================
        // SECTION: Phase Handling - Input
        //=============================================================================

        private void PhaseCheck(InputEvent @event)
        {
            if (@event.IsActionPressed(ConstTerm.ESC)) {  MenuClose(); return; }

            if (@event is InputEventMouse && activeInput == ConstTerm.KEY_GAMEPAD) { activeInput = ConstTerm.MOUSE; 
                Input.MouseMode = Input.MouseModeEnum.Visible; if (mouseFocus != null) { mouseFocus.MouseFilter = Godot.Control.MouseFilterEnum.Stop; } }
            else if (@event is not InputEventMouse && activeInput == ConstTerm.MOUSE) { activeInput = ConstTerm.KEY_GAMEPAD; 
                Input.MouseMode = Input.MouseModeEnum.Hidden; if (mouseFocus != null) { mouseFocus.MouseFilter = Godot.Control.MouseFilterEnum.Ignore; } }
            
            // if (@event is InputEventMouse) { EmitSignal(SignalName.onMouseHandling, false); }
            // else { EmitSignal(SignalName.onMouseHandling, true); }

            switch (inputPhase)
            {
                case ConstTerm.COMMAND:
                    CommandPhase(@event);
                    break;
                case ConstTerm.MEMBER + ConstTerm.SELECT:
                    MemberPhase(@event);
                    break;
                case ConstTerm.ITEM + ConstTerm.SELECT:
                    ItemSelectPhase(@event);
                    break;
                case ConstTerm.SKILL + ConstTerm.SELECT:
                    SkillSelectPhase(@event);
                    break;
                case ConstTerm.ITEM + ConstTerm.USE:
                    ItemUsePhase(@event);
                    break;
                case ConstTerm.SKILL + ConstTerm.USE:
                    SkillUsePhase(@event);
                    break;
                case ConstTerm.STATUS_SCREEN:
                    StatusPhase(@event);
                    break;
                case ConstTerm.SAVE:
                    SavePhase(@event);
                    break;
                default:
                    break;
            }

            if (@event is InputEventMouseButton) { 
                if (@event.IsActionPressed(ConstTerm.CANCEL + ConstTerm.CLICK)) {
                    CancelCycle(); } }
        }

        private void AcceptInput()
        {
            activeControl = IUIFunctions.FocusOff(activeList, currentCommand);
            // FocusOff(activeList);
            previousPhase.Add(inputPhase);
        }

        private void CommandPhase(InputEvent @event) // inputPhase == ConstTerm.COMMAND
        {
            if (activeList != commandList) { activeList = commandList; }

            if (@event.IsActionPressed(ConstTerm.ACCEPT))
            {
                CommandAccept();
            }
            else if (@event.IsActionPressed(ConstTerm.CANCEL))
            {
                CancelCycle();
            }
            else if (@event.IsActionPressed(ConstTerm.UP))
            {
                CommandSelect(-1, commandList, ConstTerm.VERT);
            }
            else if (@event.IsActionPressed(ConstTerm.DOWN))
            {
                CommandSelect(1, commandList, ConstTerm.VERT);
            }
        }

        private void CommandAccept()
        {
            AcceptInput();
            IUIFunctions.ToggleMouseFilter(activeList, Godot.Control.MouseFilterEnum.Ignore, out mouseFocus);
            CommandOption(currentCommand);
        }

        private void MemberPhase(InputEvent @event) // inputPhase == ConstTerm.MEMBER_SELECT
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT))
            {
                MemberAccept();
            }
            else if (@event.IsActionPressed(ConstTerm.CANCEL))
            {
                CancelCycle();
            }
            else if (@event.IsActionPressed(ConstTerm.UP))
            {
                CommandSelect(-1, infoList, ConstTerm.VERT);
            }
            else if (@event.IsActionPressed(ConstTerm.DOWN))
            {
                CommandSelect(1, infoList, ConstTerm.VERT);
            }
        }

        private void MemberAccept()
        {
            AcceptInput();
            IUIFunctions.ToggleMouseFilter(activeList, Godot.Control.MouseFilterEnum.Ignore, out mouseFocus);

            if (memberOption == ConstTerm.STATUS) { SetMenuPhase(ConstTerm.STATUS_SCREEN); }
            else if (memberOption == ConstTerm.SKILL) { SetMenuPhase(ConstTerm.SKILL + ConstTerm.SELECT); }
            SetMemberSelect(currentCommand);
            SetNewCommand();
        }

        private void ItemSelectPhase(InputEvent @event) // inputPhase == ConstTerm.ITEM_SELECT
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT))
            {
                ItemSelectAccept();
            }
            else if (@event.IsActionPressed(ConstTerm.CANCEL))
            {
                CancelCycle();
            }
            else if (@event.IsActionPressed(ConstTerm.UP))
            {
                CommandSelect(-1, itemList, ConstTerm.VERT);
            }
            else if (@event.IsActionPressed(ConstTerm.DOWN))
            {
                CommandSelect(1, itemList, ConstTerm.VERT);
            }
            else if (@event.IsActionPressed(ConstTerm.LEFT))
            {
                CommandSelect(-1, itemList, ConstTerm.HORIZ);
            }
            else if (@event.IsActionPressed(ConstTerm.RIGHT))
            {
                CommandSelect(1, itemList, ConstTerm.HORIZ);
            }
        }

        private void ItemSelectAccept()
        {
            if (itemList.GetChild(currentCommand).GetNode<Button>(ConstTerm.BUTTON).Disabled == true) { InvalidOption(); return; }
            AcceptInput();
            SetMenuPhase(ConstTerm.ITEM + ConstTerm.USE);
            SetNewCommand();
            // EmitSignal(SignalName.onItemSelect, currentCommand);
        }

        private void SkillSelectPhase(InputEvent @event) // inputPhase == ConstTerm.SKILL_SELECT;
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT))
            {
                SkillSelectAccept();
            }
            else if (@event.IsActionPressed(ConstTerm.CANCEL))
            {
                CancelCycle();
            }
            else if (@event.IsActionPressed(ConstTerm.UP))
            {
                CommandSelect(-1, skillList, ConstTerm.VERT);
            }
            else if (@event.IsActionPressed(ConstTerm.DOWN))
            {
                CommandSelect(1, skillList, ConstTerm.VERT);
            }
            else if (@event.IsActionPressed(ConstTerm.LEFT))
            {
                CommandSelect(-1, skillList, ConstTerm.HORIZ);
            }
            else if (@event.IsActionPressed(ConstTerm.RIGHT))
            {
                CommandSelect(1, skillList, ConstTerm.HORIZ);
            }
        }

        private void SkillSelectAccept()
        {
            if (skillList.GetChild(currentCommand).GetNode<Button>(ConstTerm.BUTTON).Disabled == true) { InvalidOption(); return; }
            AcceptInput();
            SetMenuPhase(ConstTerm.SKILL + ConstTerm.USE);
            SetNewCommand();
            // EmitSignal(SignalName.onAbilitySelect, currentCommand);
        }

        private void ItemUsePhase(InputEvent @event) // inputPhase == ConstTerm.ITEM_USE
        {

        }

        private void ItemUseAccept()
        {

        }
        
        private void SkillUsePhase(InputEvent @event) // inputPhase == ConstTerm.SKILL_USE
        {

        }

        private void SkillUseAccept()
        {

        }

        private void StatusPhase(InputEvent @event) // inputPhase == ConstTerm.STATUS_SCREEN
        {
            // Show status screen of selected member
            if (@event.IsActionPressed(ConstTerm.CANCEL))
            {
                CancelCycle();
            }
        }

        private void StatusAccept()
        {

        }

        private void SavePhase(InputEvent @event) // inputPhase == ConstTerm.SAVE
        {
            // Bring up save menu
            if (@event.IsActionPressed(ConstTerm.CANCEL))
            {
                CancelCycle();
            }
        }

        private void SaveAccept()
        {

        }

        //=============================================================================
        // SECTION: Selection Handling
        //=============================================================================

        private void CommandSelect(int change, Container targetList, string direction)
        {
            change = IUIFunctions.CheckColumn(change, direction, numColumn);
            IUIFunctions.ChangeTarget(change, ref currentCommand, IUIFunctions.GetCommandCount(targetList));

            activeControl = IUIFunctions.FocusOn(targetList, currentCommand);
            // EmitSignal(SignalName.onTargetChange);
        }

        // private int ChangeTarget(int change, int target, int listSize)
        // {
        //     // if (direction == ConstTerm.HORIZ) { change += change;}
        //     if (target + change > listSize - 1) { return 0; }
        //     else if (target + change < 0) { return listSize - 1; }
        //     else { return target += change; }
        // }

        private void CancelCycle()
        {
            if (inputPhase == ConstTerm.COMMAND) { MenuClose(); return; }

            activeControl = IUIFunctions.FocusOff(activeList, currentCommand);
            IUIFunctions.ToggleMouseFilter(activeList, Godot.Control.MouseFilterEnum.Ignore, out mouseFocus);

            string oldPhase = IUIFunctions.CancelSelect(out currentCommand, previousCommand, previousPhase);
            SetMenuPhase(oldPhase);
            // SetNumColumn();

            activeControl = IUIFunctions.FocusOn(activeList, currentCommand);
            IUIFunctions.ToggleMouseFilter(activeList, Godot.Control.MouseFilterEnum.Stop, out mouseFocus);
        }

        // private void CancelSelect()
        // {
        //     if (inputPhase == ConstTerm.COMMAND) { MenuClose(); return; }
            
        //     FocusOff(activeList);
        //     ToggleMouseFilter(activeList, Godot.Control.MouseFilterEnum.Ignore);

        //     int oldCommand = previousCommand[^1];
        //     previousCommand.RemoveAt(previousCommand.Count - 1);
        //     currentCommand = oldCommand;
            
        //     string oldPhase = previousPhase[^1];
        //     previousPhase.RemoveAt(previousPhase.Count - 1);

        //     SetMenuPhase(oldPhase);
        //     SetNumColumn();

        //     ToggleMouseFilter(activeList, Godot.Control.MouseFilterEnum.Stop);
        //     FocusOn(activeList);

        //     // EmitSignal(SignalName.onTargetChange);
        // }

        // private void ChangeFocus(Container targetList)
        // {
        //     if (targetList.GetChild(currentCommand).GetChildCount() > 0)
        //     {
        //         Button focusButton = targetList.GetChild(currentCommand).GetNode<Button>(ConstTerm.BUTTON);
        //         if (focusButton != null) {
        //             if (focusButton.HasFocus()) { focusButton.ReleaseFocus(); activeControl = null; }
        //             else { focusButton.GrabFocus(); activeControl = focusButton; }
        //         } 
        //     }
        // }

        // private void FocusOff(Container targetList)
        // {
        //     if (targetList.GetChild(currentCommand).GetChildCount() > 0)
        //     {
        //         Button focusButton = targetList.GetChild(currentCommand).GetNode<Button>(ConstTerm.BUTTON);
        //         if (focusButton.HasFocus()) { focusButton.ReleaseFocus(); activeControl = null; }
        //     }
        // }

        // private void FocusOn(Container targetList)
        // {
        //     if (targetList.GetChild(currentCommand).GetChildCount() > 0)
        //     {
        //         Button focusButton = targetList.GetChild(currentCommand).GetNode<Button>(ConstTerm.BUTTON);
        //         if (focusButton != null) { focusButton.GrabFocus(); activeControl = focusButton; }
        //     }
        // }

        //=============================================================================
        // SECTION: Command Options
        //=============================================================================

        private void CommandOption(int option)
        {
            // IUIFunctions.ToggleMouseFilter(activeList, Godot.Control.MouseFilterEnum.Ignore, out mouseFocus);

            switch (commandOptions[option])
            {
                case ConstTerm.ITEM:
                    if(!ItemBag.Instance.BagIsEmpty()) { SetMenuPhase(ConstTerm.ITEM + ConstTerm.SELECT); }
                    else { InvalidOption(); return; }
                    break;
                case ConstTerm.SKILL:
                    // inputPhase = ConstTerm.SKILL + ConstTerm.SELECT;
                    // break;
                // case ConstTerm.STATUS:
                    SetMenuPhase(ConstTerm.MEMBER + ConstTerm.SELECT);
                    memberOption = commandOptions[option];
                    break;
                default:
                    break;
            }
            
            IUIFunctions.ToggleMouseFilter(activeList, Godot.Control.MouseFilterEnum.Stop, out mouseFocus);
            // SetNumColumn();
            SetNewCommand();
        }

        private void InvalidOption()
        {
            IUIFunctions.ToggleMouseFilter(activeList, Godot.Control.MouseFilterEnum.Stop, out mouseFocus);
            // play 'cannot use' sound effect
        }

        //=============================================================================
        // SECTION: Signal Methods
        //=============================================================================

        private void OnMouseEntered(Container currList, Node currLabel)
        {
            if (currList != activeList) { return; }

            activeControl = IUIFunctions.FocusOff(currList, currentCommand);
            currentCommand = currLabel.GetIndex();

            activeControl = IUIFunctions.FocusOn(currList, currentCommand);
            mouseFocus = currLabel.GetNode<Button>(ConstTerm.BUTTON);
        }

        private void OnMouseClick()
        {
            // activeControl = IUIFunctions.FocusOff(activeList, currentCommand);

            switch (inputPhase)
            {
                case ConstTerm.COMMAND:
                    CommandAccept();
                    break;
                case ConstTerm.MEMBER + ConstTerm.SELECT:
                    MemberAccept();
                    break;
                case ConstTerm.ITEM + ConstTerm.SELECT:
                    ItemSelectAccept();
                    break;
                case ConstTerm.SKILL + ConstTerm.SELECT:
                    SkillSelectAccept();
                    break;
                case ConstTerm.ITEM + ConstTerm.USE:
                    ItemUseAccept();
                    break;
                case ConstTerm.SKILL + ConstTerm.USE:
                    SkillUseAccept();
                    break;
                // case ConstTerm.STATUS_SCREEN:
                //     StatusAccept();
                //     break;
                // case ConstTerm.SAVE:
                //     SaveAccept();
                //     break;
                default:
                    break;
            }
        }

        //=============================================================================
        // SECTION: Internal Access Methods
        //=============================================================================

        // private int GetCommandCount(Container targetList)
        // {
        //     return targetList.GetChildCount();
        // }

        // private void ToggleMouseFilter(Container targetList, Godot.Control.MouseFilterEnum value)
        // {
        //     mouseFocus = null;
        //     for (int c = 0; c < targetList.GetChildCount(); c++)
        //     {
        //         targetList.GetChild(c).GetNode<Button>(ConstTerm.BUTTON).MouseFilter = value;
        //     }
        // }

        //=============================================================================
        // SECTION: External Access Methods
        //=============================================================================

        public void MenuOpen()
        {
            currentCommand = 0;
            
            Button initialFocus = commandList.GetChild(currentCommand).GetNode<Button>(ConstTerm.BUTTON);
            initialFocus?.GrabFocus();

            SetMenuPhase(ConstTerm.COMMAND);
            isActive = true;

            IUIFunctions.ToggleMouseFilter(infoList, Godot.Control.MouseFilterEnum.Ignore, out mouseFocus);
        }

        public void MenuClose()
        {
            IUIFunctions.ResetMouseInput(commandList, out mouseFocus);
            
            currentCommand = 0;
            previousCommand = [];
            previousPhase = [];
            
            SetMenuPhase(ConstTerm.WAIT);
            isActive = false;
            EmitSignal(SignalName.onMenuClose);
        }

        // private void ResetMouseInput()
        // {
        //     Input.MouseMode = Input.MouseModeEnum.Visible;
        //     IUIFunctions.ToggleMouseFilter(commandList, Godot.Control.MouseFilterEnum.Stop, out mouseFocus);
        // }

        // public void UpdateInfo()
        // {
        //     EmitSignal(SignalName.onMenuUpdate);
        // }

        public int GetCommand()
        {
            return currentCommand;
        }

        public void SetNewCommand()
        {
            previousCommand.Add(currentCommand);
            currentCommand = 0;

            activeControl = IUIFunctions.FocusOn(activeList, currentCommand);
        }

        public string[] GetCommandOptions()
        {
            return commandOptions;
        }

        public float GetSkillItemWidth()
        {
            return skillItemWidth;
        }

        public int GetMemberCommand()
        {
            return memberSelect;
        }

        private void SetMemberSelect(int choice)
        {
            memberSelect = choice;

            EmitSignal(SignalName.onMemberSelect);
            SubLists(skillList);
        }

        public string GetMenuPhase()
        {
            return inputPhase;
        }

        public void SetMenuPhase(string phase)
        {
            inputPhase = phase;
            if (listDict.TryGetValue(phase, out Container value)) { activeList = value; }

            EmitSignal(SignalName.onMenuPhase);
            SetNumColumn();
        }

        public int GetNumColumn()
        {
            return numColumn;
        }

        public void SetNumColumn()
        {
            if (inputPhase == ConstTerm.SKILL + ConstTerm.SELECT) { numColumn = skillList.Columns; }
            else if (inputPhase == ConstTerm.ITEM + ConstTerm.SELECT) { numColumn = itemList.Columns; }
            else { numColumn = 1; }
        }

        public void SetMenuActive(bool active)
        {
            isActive = active;
        }

        public void SetPlayer(CharacterController player)
        {
            playerInput = player;
        }
    }
}