using Godot;

using ZAM.MenuUI;

namespace ZAM.Controller
{
    public partial class MenuController : BaseController, IUIFunctions
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

        private CharacterController playerInput = null;
        private string memberOption;
        private int memberSelect = 0;

        // private Dictionary<string, Container> listDict = [];
        // private bool signalsDone = false;

        
        // private string activeInput = ConstTerm.KEY_GAMEPAD;

        // private string inputPhase = ConstTerm.WAIT;
        // private List<string> previousPhase = [];
       
        // private bool isActive = false;

        // private Container activeList = null;
        // private ButtonUI activeControl = null;
        // private ButtonUI mouseFocus = null;

        // private int currentCommand = 0;
        // private List<int> previousCommand = [];
        
        // private int numColumn = 1;

        // Delegate Events \\
        // [Signal]
        // public delegate void onMouseHandlingEventHandler(bool toggle);
        [Signal]
        public delegate void onMenuCloseEventHandler();
        [Signal]
        public delegate void onMenuPhaseEventHandler();
        [Signal]
        public delegate void onMemberSelectEventHandler();
        [Signal]
        public delegate void onCursorSelectEventHandler();
        [Signal]
        public delegate void onAcceptEventHandler();
        [Signal]
        public delegate void onErrorEventHandler();
        [Signal]
        public delegate void onCancelEventHandler();
        // [Signal]
        // public delegate void onMenuUpdateEventHandler();
        // [Signal]
        // public delegate void onTargetChangeEventHandler();

        //=============================================================================
        // SECTION: Base Methods
        //=============================================================================
        // public override void _Ready()
        // {
        //     IfNull();
        // }

        protected override void IfNull()
        {
            skillList = skillPanel.GetNode<GridContainer>(ConstTerm.TEXT + ConstTerm.LIST);
            itemList = itemPanel.GetNode<GridContainer>(ConstTerm.TEXT + ConstTerm.LIST);

            skillItemWidth = skillPanel.GetRect().Size.X / 2 - containerEdgeBuffer;

            base.IfNull();
        }

        protected override void SetupListDict()
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

        // private void SubLists(Container targetList)
        // {
        //     for (int c = 0; c < targetList.GetChildCount(); c++)
        //     {
        //         Node tempLabel = targetList.GetChild(c);
        //         targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).MouseEntered += () => OnMouseEntered(targetList, tempLabel);
        //         targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).Pressed += OnMouseClick;
        //     }
        // }

        public override void _Input(InputEvent @event)
        {
            if (!IsControlActive()) { return; }

            PhaseCheck(@event);
        }

        public override void _PhysicsProcess(double delta)
        {
            if (!IsControlActive()) { return; }
            SubSignals();
            
            if (Input.IsActionJustPressed(ConstTerm.MENU) || Input.IsActionJustPressed(ConstTerm.PAUSE)) { MenuClose(); }
        }

        //=============================================================================
        // SECTION: Phase Handling - Input
        //=============================================================================

        protected override bool PhaseCheck(InputEvent @event)
        {
            // if (@event is InputEventMouse && activeInput == ConstTerm.KEY_GAMEPAD) { activeInput = ConstTerm.MOUSE; 
            //     Input.MouseMode = Input.MouseModeEnum.Visible; if (mouseFocus != null) { mouseFocus.MouseFilter = Control.MouseFilterEnum.Stop; } }
            // else if (@event is not InputEventMouse && activeInput == ConstTerm.MOUSE) { activeInput = ConstTerm.KEY_GAMEPAD; 
            //     Input.MouseMode = Input.MouseModeEnum.Hidden; if (mouseFocus != null) { mouseFocus.MouseFilter = Control.MouseFilterEnum.Ignore; } }

            // if (@event is InputEventMouse) { EmitSignal(SignalName.onMouseHandling, false); }
            // else { EmitSignal(SignalName.onMouseHandling, true); }
            bool valid = base.PhaseCheck(@event);
            if (!valid) { return false; }

            switch (GetInputPhase())
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

            return true;

            // if (@event is InputEventMouseButton) { 
            //     if (@event.IsActionPressed(ConstTerm.CANCEL + ConstTerm.CLICK)) {
            //         CancelCycle(); } }
        }

        // private bool AcceptInput()
        // {
        //     activeControl = IUIFunctions.FocusOff(activeList, currentCommand);

        //     if (activeControl.OnButtonPressed()) { InvalidOption(); return false; }
        //     // if (activeControl.OnButtonPressed()) { IUIFunctions.InvalidOption(activeList, currentCommand, ref activeControl, out mouseFocus); return false; }
        //     // if (activeList.GetChild(currentCommand).GetNode<ButtonUI>(ConstTerm.BUTTON).Disabled == true) { InvalidOption(); return false; }
        //     // if (activeList.GetChild(currentCommand).Modulate.ToString() == ConstTerm.GREY) { InvalidOption(); return false; }

        //     previousPhase.Add(inputPhase);
        //     return true;
        // }

        private void CommandPhase(InputEvent @event) // inputPhase == ConstTerm.COMMAND
        {
            if (activeList != commandList) { activeList = commandList; }

            if (@event.IsActionPressed(ConstTerm.ACCEPT)) {
                CommandAccept();
            }
            else { PhaseControls(@event); }
        }

        private void CommandAccept()
        {
            if (!AcceptInput()) { return; }
            IUIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Ignore, out mouseFocus);
            CommandOption(currentCommand);
        }

        private void MemberPhase(InputEvent @event) // inputPhase == ConstTerm.MEMBER_SELECT
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT)) {
                MemberAccept();
            }
            else { PhaseControls(@event); }
        }

        private void MemberAccept()
        {
            if (!AcceptInput()) { return; }
            IUIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Ignore, out mouseFocus);

            if (memberOption == ConstTerm.STATUS) { SetInputPhase(ConstTerm.STATUS_SCREEN); }
            else if (memberOption == ConstTerm.SKILL) { SetInputPhase(ConstTerm.SKILL + ConstTerm.SELECT); }
            SetMemberSelect(currentCommand);
            SetNewCommand();
        }

        private void ItemSelectPhase(InputEvent @event) // inputPhase == ConstTerm.ITEM_SELECT
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT)) {
                ItemSelectAccept();
            }
            else { PhaseControls(@event); }
        }

        private void ItemSelectAccept()
        {
            if (!AcceptInput()) { return; }
            SetInputPhase(ConstTerm.ITEM + ConstTerm.USE);
            SetNewCommand();
            // EmitSignal(SignalName.onItemSelect, currentCommand);
        }

        private void SkillSelectPhase(InputEvent @event) // inputPhase == ConstTerm.SKILL_SELECT;
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT)) {
                SkillSelectAccept();
            }
            else { PhaseControls(@event); }
        }

        private void SkillSelectAccept()
        {
            if (!AcceptInput()) { return; }
            SetInputPhase(ConstTerm.SKILL + ConstTerm.USE);
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
            CancelCycle();
            // if (@event.IsActionPressed(ConstTerm.ACCEPT)) {
            //     SkillUseAccept();
            // }
            // else { PhaseControls(@event); }
        }

        private void SkillUseAccept()
        {
            if (!AcceptInput()) { return; }
            SetInputPhase(ConstTerm.SKILL + ConstTerm.SELECT);
            SetNewCommand();
        }

        private void StatusPhase(InputEvent @event) // inputPhase == ConstTerm.STATUS_SCREEN
        {
            // Show status screen of selected member
            if (@event.IsActionPressed(ConstTerm.CANCEL)) {
                CancelCycle();
            }
        }

        private void StatusAccept()
        {

        }

        private void SavePhase(InputEvent @event) // inputPhase == ConstTerm.SAVE
        {
            // Bring up save menu
            if (@event.IsActionPressed(ConstTerm.CANCEL)) {
                CancelCycle();
            }
        }

        private void SaveAccept()
        {

        }

        //=============================================================================
        // SECTION: Selection Handling
        //=============================================================================

        // private void CommandSelect(int change, Container targetList, string direction)
        // {
        //     change = IUIFunctions.CheckColumn(change, direction, numColumn);
        //     IUIFunctions.ChangeTarget(change, ref currentCommand, IUIFunctions.GetCommandCount(targetList));

        //     activeControl = IUIFunctions.FocusOn(targetList, currentCommand);
        //     // EmitSignal(SignalName.onTargetChange);
        // }

        // private int ChangeTarget(int change, int target, int listSize)
        // {
        //     // if (direction == ConstTerm.HORIZ) { change += change;}
        //     if (target + change > listSize - 1) { return 0; }
        //     else if (target + change < 0) { return listSize - 1; }
        //     else { return target += change; }
        // }

        protected override void CancelCycle()
        {
            if (GetInputPhase() == ConstTerm.COMMAND) { MenuClose(); return; }
            base.CancelCycle();
            // activeControl = IUIFunctions.FocusOff(activeList, currentCommand);
            // IUIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Ignore, out mouseFocus);

            // string oldPhase = IUIFunctions.CancelSelect(out currentCommand, previousCommand, previousPhase);
            // SetInputPhase(oldPhase);
            // // SetNumColumn();

            // activeControl = IUIFunctions.FocusOn(activeList, currentCommand);
            // IUIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Stop, out mouseFocus);
        }

        // private void CancelSelect()
        // {
        //     if (inputPhase == ConstTerm.COMMAND) { MenuClose(); return; }
            
        //     FocusOff(activeList);
        //     ToggleMouseFilter(activeList, Control.MouseFilterEnum.Ignore);

        //     int oldCommand = previousCommand[^1];
        //     previousCommand.RemoveAt(previousCommand.Count - 1);
        //     currentCommand = oldCommand;
            
        //     string oldPhase = previousPhase[^1];
        //     previousPhase.RemoveAt(previousPhase.Count - 1);

        //     SetInputPhase(oldPhase);
        //     SetNumColumn();

        //     ToggleMouseFilter(activeList, Control.MouseFilterEnum.Stop);
        //     FocusOn(activeList);

        //     // EmitSignal(SignalName.onTargetChange);
        // }

        // private void ChangeFocus(Container targetList)
        // {
        //     if (targetList.GetChild(currentCommand).GetChildCount() > 0)
        //     {
        //         ButtonUI focusButton = targetList.GetChild(currentCommand).GetNode<ButtonUI>(ConstTerm.BUTTON);
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
        //         ButtonUI focusButton = targetList.GetChild(currentCommand).GetNode<ButtonUI>(ConstTerm.BUTTON);
        //         if (focusButton.HasFocus()) { focusButton.ReleaseFocus(); activeControl = null; }
        //     }
        // }

        // private void FocusOn(Container targetList)
        // {
        //     if (targetList.GetChild(currentCommand).GetChildCount() > 0)
        //     {
        //         ButtonUI focusButton = targetList.GetChild(currentCommand).GetNode<ButtonUI>(ConstTerm.BUTTON);
        //         if (focusButton != null) { focusButton.GrabFocus(); activeControl = focusButton; }
        //     }
        // }

        //=============================================================================
        // SECTION: Command Options
        //=============================================================================

        private void CommandOption(int option)
        {
            switch (commandOptions[option])
            {
                case ConstTerm.ITEM:
                    SetInputPhase(ConstTerm.ITEM + ConstTerm.SELECT);
                    break;
                case ConstTerm.SKILL:
                    // inputPhase = ConstTerm.SKILL + ConstTerm.SELECT;
                    // break;
                // case ConstTerm.STATUS:
                    SetInputPhase(ConstTerm.MEMBER + ConstTerm.SELECT);
                    memberOption = commandOptions[option];
                    break;
                default:
                    break;
            }
            
            IUIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Stop, out mouseFocus);
            // SetNumColumn();
            SetNewCommand();
        }

        // private void InvalidOption()
        // {
        //     activeControl = IUIFunctions.FocusOn(activeList, currentCommand);
        //     IUIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Stop, out mouseFocus);
        //     // play 'cannot use' sound effect
        // }

        //=============================================================================
        // SECTION: Signal Methods
        //=============================================================================

        // private void OnMouseEntered(Container currList, Node currLabel)
        // {
        //     if (currList != activeList) { return; }

        //     activeControl = IUIFunctions.FocusOff(currList, currentCommand);
        //     currentCommand = currLabel.GetIndex();

        //     activeControl = IUIFunctions.FocusOn(currList, currentCommand);
        //     mouseFocus = currLabel.GetNode<ButtonUI>(ConstTerm.BUTTON);
        // }

        protected override void OnMouseClick()
        {
            // activeControl = IUIFunctions.FocusOff(activeList, currentCommand);

            switch (GetInputPhase())
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

        // private void ToggleMouseFilter(Container targetList, Control.MouseFilterEnum value)
        // {
        //     mouseFocus = null;
        //     for (int c = 0; c < targetList.GetChildCount(); c++)
        //     {
        //         targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).MouseFilter = value;
        //     }
        // }

        //=============================================================================
        // SECTION: External Access Methods
        //=============================================================================

        public void MenuOpen()
        {
            currentCommand = 0;
            
            ButtonUI initialFocus = commandList.GetChild(currentCommand).GetNode<ButtonUI>(ConstTerm.BUTTON);
            initialFocus?.GrabFocus();

            SetInputPhase(ConstTerm.COMMAND);
            SetControlActive(true);

            IUIFunctions.ToggleMouseFilter(infoList, Control.MouseFilterEnum.Ignore, out mouseFocus); // EDIT: Blocking control to other visible list
        }

        public void MenuClose()
        {
            IUIFunctions.ResetMouseInput(commandList, out mouseFocus);
            
            currentCommand = 0;
            previousCommand = [];
            previousPhase = [];
            
            SetInputPhase(ConstTerm.WAIT);
            SetControlActive(false);
            EmitSignal(SignalName.onMenuClose);
        }

        // private void ResetMouseInput()
        // {
        //     Input.MouseMode = Input.MouseModeEnum.Visible;
        //     IUIFunctions.ToggleMouseFilter(commandList, Control.MouseFilterEnum.Stop, out mouseFocus);
        // }

        // public void UpdateInfo()
        // {
        //     EmitSignal(SignalName.onMenuUpdate);
        // }

        // public int GetCommand()
        // {
        //     return currentCommand;
        // }

        // public void SetNewCommand()
        // {
        //     previousCommand.Add(currentCommand);
        //     currentCommand = 0;

        //     activeControl = IUIFunctions.FocusOn(activeList, currentCommand);
        // }

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

        // public string GetMenuPhase()
        // {
        //     return inputPhase;
        // }

        public override void SetInputPhase(string phase)
        {
            base.SetInputPhase(phase);
            EmitSignal(SignalName.onMenuPhase);
        }

        // public int GetNumColumn()
        // {
        //     return numColumn;
        // }

        // public void SetNumColumn()
        // {
        //     numColumn = 1;
        //     if ((bool)activeList.Get(ConstTerm.COLUMNS)) { numColumn = (int)activeList.Get(ConstTerm.COLUMNS); }
        // //     if (inputPhase == ConstTerm.SKILL + ConstTerm.SELECT) { numColumn = skillList.Columns; }
        // //     else if (inputPhase == ConstTerm.ITEM + ConstTerm.SELECT) { numColumn = itemList.Columns; }
        // //     else { numColumn = 1; }
        // }

        // public void SetMenuActive(bool active)
        // {
        //     isActive = active;
        // }

        public void SetPlayer(CharacterController player)
        {
            playerInput = player;
        }
    }
}