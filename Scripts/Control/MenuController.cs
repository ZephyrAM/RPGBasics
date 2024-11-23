using Godot;

using ZAM.Inventory;

namespace ZAM.Control
{
    public partial class MenuController : Node
    {
        [Export] private string[] commandOptions = [];
        [Export] private int skillItemWidth = 0;

        [ExportGroup("Lists")]
        [Export] private VBoxContainer commandList = null;
        [Export] private VBoxContainer infoList = null;
        [Export] private GridContainer skillList;
        [Export] private GridContainer itemList;

        private CharacterController playerInput = null;

        private string inputPhase = ConstTerm.WAIT;
        private string memberOption;
        private bool isActive = false;

        private int currentCommand = 0;
        private int memberSelect = 0;
        private int numColumn = 1;

        // Delegate Events \\
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

        public override void _Input(InputEvent @event)
        {
            if (!isActive) { return; }

            PhaseCheck(@event);
        }

        public override void _PhysicsProcess(double delta)
        {
            if (!isActive) { return; }
            if (Input.IsActionJustPressed(ConstTerm.MENU)) { MenuClose(); }
        }

        //=============================================================================
        // SECTION: Phase Handling - Input
        //=============================================================================

        private void PhaseCheck(InputEvent @event)
        {
            if (@event.IsActionPressed(ConstTerm.ESC)) {  MenuClose(); return; }
            
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
                case ConstTerm.STATUS_SCREEN:
                    StatusPhase(@event);
                    break;
                case ConstTerm.SAVE:
                    SavePhase(@event);
                    break;
                default:
                    break;
            }
        }

        private void CommandPhase(InputEvent @event)
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT))
            {
                CommandOption(currentCommand);
            }
            else if (@event.IsActionPressed(ConstTerm.CANCEL))
            {
                MenuClose();
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

        private void MemberPhase(InputEvent @event)
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT))
            {
                if (memberOption == ConstTerm.STATUS) { SetMenuPhase(ConstTerm.STATUS_SCREEN); }
                else if (memberOption == ConstTerm.SKILL) { SetMenuPhase(ConstTerm.SKILL + ConstTerm.SELECT); }
                SetMemberSelect(currentCommand);
                currentCommand = 0;
            }
            else if (@event.IsActionPressed(ConstTerm.CANCEL))
            {
                CancelSelect(ConstTerm.COMMAND);
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

        private void ItemSelectPhase(InputEvent @event) // inputPhase == ConstTerm.ITEM_SELECT
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT))
            {
                SetMenuPhase(ConstTerm.ITEM + ConstTerm.USE);
                // EmitSignal(SignalName.onItemSelect, currentCommand);
                currentCommand = 0;
            }
            else if (@event.IsActionPressed(ConstTerm.CANCEL))
            {
                CancelSelect(ConstTerm.COMMAND);
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

        private void SkillSelectPhase(InputEvent @event) // inputPhase == ConstTerm.SKILL_SELECT;
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT))
            {
                SetMenuPhase(ConstTerm.SKILL + ConstTerm.USE);
                // EmitSignal(SignalName.onAbilitySelect, currentCommand);
                currentCommand = 0;
            }
            else if (@event.IsActionPressed(ConstTerm.CANCEL))
            {
                CancelSelect(ConstTerm.MEMBER + ConstTerm.SELECT);
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

        private void StatusPhase(InputEvent @event)
        {
            // Show status screen of selected member
            if (@event.IsActionPressed(ConstTerm.CANCEL))
            {
                CancelSelect(ConstTerm.MEMBER + ConstTerm.SELECT);
            }
        }

        private void SavePhase(InputEvent @event)
        {
            // Bring up save menu
            if (@event.IsActionPressed(ConstTerm.CANCEL))
            {
                CancelSelect(ConstTerm.COMMAND);
            }
        }

        //=============================================================================
        // SECTION: Selection Handling
        //=============================================================================

        private void CommandSelect(int change, Container targetList, string direction)
        {
            if (direction == ConstTerm.VERT) { change *= numColumn; }
            currentCommand = ChangeTarget(change, currentCommand, GetCommandCount(targetList));
            EmitSignal(SignalName.onTargetChange);
        }

        private int ChangeTarget(int change, int target, int listSize)
        {
            // if (direction == ConstTerm.HORIZ) { change += change;}
            if (target + change > listSize - 1) { return 0; }
            else if (target + change < 0) { return listSize - 1; }
            else { return target += change; }
        }

        private void CancelSelect(string phase)
        {
            SetMenuPhase(phase);
            // currentCommand = 0;
            SetNumColumn();

            EmitSignal(SignalName.onTargetChange);
        }

        //=============================================================================
        // SECTION: Command Options
        //=============================================================================

        private void CommandOption(int option)
        {
            // ResetTarget(); // Reset targetting to correct team by...  reasons. EDIT

            switch (commandOptions[option])
            {
                case ConstTerm.ITEM:
                    if(!ItemBag.Instance.BagIsEmpty()) { SetMenuPhase(ConstTerm.ITEM + ConstTerm.SELECT); }
                    else { InvalidOption(); }
                    break;
                case ConstTerm.SKILL:
                    // inputPhase = ConstTerm.SKILL + ConstTerm.SELECT;
                    // break;
                case ConstTerm.STATUS:
                    SetMenuPhase(ConstTerm.MEMBER + ConstTerm.SELECT);
                    memberOption = commandOptions[option];
                    break;
                default:
                    break;
            }

            SetNumColumn();
            currentCommand = 0;
        }

        private void InvalidOption()
        {
            // play 'cannot use' sound effect
        }

        //=============================================================================
        // SECTION: Internal Access Methods
        //=============================================================================

        private int GetCommandCount(Container targetList)
        {
            return targetList.GetChildCount();
        }

        //=============================================================================
        // SECTION: External Access Methods
        //=============================================================================

        public void MenuOpen()
        {
            currentCommand = 0;
            SetMenuPhase(ConstTerm.COMMAND);
            isActive = true;
        }

        public void MenuClose()
        {
            currentCommand = 0;
            SetMenuPhase(ConstTerm.WAIT);
            isActive = false;
            EmitSignal(SignalName.onMenuClose);
        }

        // public void UpdateInfo()
        // {
        //     EmitSignal(SignalName.onMenuUpdate);
        // }

        public int GetCommand()
        {
            return currentCommand;
        }

        public string[] GetCommandOptions()
        {
            return commandOptions;
        }

        public int GetSkillItemWidth()
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
        }

        public string GetMenuPhase()
        {
            return inputPhase;
        }

        public void SetMenuPhase(string phase)
        {
            inputPhase = phase;
            EmitSignal(SignalName.onMenuPhase);
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