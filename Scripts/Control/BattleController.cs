using Godot;
using System.Collections.Generic;

using ZAM.Inventory;

namespace ZAM.Control
{
    public partial class BattleController : Node, IUIFunctions
    {
        [Export] private string[] commandOptions = [];
        [Export] private float containerEdgeBuffer = 0;

        [ExportGroup("Lists")]
        [Export] private VBoxContainer commandList;
        [Export] private PanelContainer skillPanel = null;
        [Export] private PanelContainer itemPanel = null;

        [ExportGroup("Variables")]
        [Export] private Vector2[] playerPositions;

        private AnimationPlayer playerAnim;

        private GridContainer skillList;
        private GridContainer itemList;
        private float skillItemWidth = 0;

        private Dictionary<string, Container> listDict = [];
        private bool signalsDone = false;

        private string activeInput = ConstTerm.KEY_GAMEPAD;

        private Container activeList = null;
        private Button activeControl = null;
        private Button mouseFocus = null;

        private bool playerTurn = false;
        private string turnPhase = ConstTerm.WAIT;
        private string targetTeam = ConstTerm.ENEMY;
        private List<string> previousPhase = [];

        private int currentCommand = 0;
        private List<int> previousCommand = [];
        private int activeMember = 0;
        private int actionTarget = 0;
        
        private int enemyTeamSize = 0;
        private int playerTeamSize = 0;
        private int numColumn = 1;

        private bool battleOver = false;
        private bool isControlActive = false;
        private List<int> isTargetting = [];

        // Delegate Events \\
        [Signal]
        public delegate void onBattlePhaseEventHandler();
        [Signal]
        public delegate void onTurnCycleEventHandler();
        [Signal]
        public delegate void onTurnEndEventHandler();
        [Signal]
        public delegate void onTargetChangeEventHandler();
        [Signal]
        public delegate void onSelectCancelEventHandler();
        [Signal]
        public delegate void onAbilitySelectEventHandler(int index);
        [Signal]
        public delegate void onAbilityUseEventHandler(int index);
        [Signal]
        public delegate void onAbilityStopEventHandler(int index);
        [Signal]
        public delegate void onItemSelectEventHandler(int index);
        [Signal]
        public delegate void onItemUseEventHandler(int index);
        [Signal]
        public delegate void onBattleFinishEventHandler();

        // [Signal]
        // public delegate void onSaveGameEventHandler();
        // [Signal]
        // public delegate void onLoadGameEventHandler();

        //=============================================================================
        // SECTION: Base Methods
        //=============================================================================
        public override void _Ready()
        {
            IfNull();
            SetTurnPhase(ConstTerm.WAIT);
        }

        public override void _Input(InputEvent @event)
        {
            if (!isControlActive) { return; }
            if (!signalsDone) { SubSignals(); }
            if (battleOver && @event != null) {
                if (@event is InputEventMouseMotion) { return; }
                EmitSignal(SignalName.onBattleFinish);
            }

            if (IsPlayerTurn() == true) {
                PhaseCheck(@event);
                // Data test commands \\
                if (Input.IsActionJustPressed(ConstTerm.SAVE)) // EDIT: Temporary for debugging
                {
                    GD.Print("Saving game!");
                    SaveLoader.Instance.SaveGame();          
                    // EmitSignal(SignalName.onSaveGame);
                }
                else if (Input.IsActionJustPressed(ConstTerm.LOAD))
                {
                    GD.Print("Loading game!");
                    SaveLoader.Instance.LoadGame();
                    // EmitSignal(SignalName.onLoadGame);
                }
            }
        }

        //=============================================================================
        // SECTION: OnReady Methods
        //=============================================================================
        private void IfNull()
        {
            skillList = skillPanel.GetNode<GridContainer>(ConstTerm.TEXT + ConstTerm.LIST);
            itemList = itemPanel.GetNode<GridContainer>(ConstTerm.TEXT + ConstTerm.LIST);

            skillItemWidth = skillPanel.GetRect().Size.X / 2 - containerEdgeBuffer;

            // playerAnim ??= GetChild(0).GetNode<AnimationPlayer>(ConstTerm.ANIM_PLAYER);
            SetupListDict();
        }

        private void SubSignals()
        {
            SubLists(commandList);
            SubLists(itemList);
            signalsDone = true;
        }

        public void SubLists(Container targetList)
        {
            for (int c = 0; c < targetList.GetChildCount(); c++)
            {
                Node tempLabel = targetList.GetChild(c);
                targetList.GetChild(c).GetNode<Button>(ConstTerm.BUTTON).MouseEntered += () => OnMouseEntered(targetList, tempLabel);
                targetList.GetChild(c).GetNode<Button>(ConstTerm.BUTTON).Pressed += OnMouseClick;
            }
        }

        private void SetupListDict()
        {
            listDict.Add(ConstTerm.COMMAND, commandList);
            listDict.Add(ConstTerm.ITEM + ConstTerm.SELECT, itemList);
            listDict.Add(ConstTerm.SKILL + ConstTerm.SELECT, skillList);
        }


        //=============================================================================
        // SECTION: Phase Handling - Input
        //=============================================================================

        private void PhaseCheck(InputEvent @event)
        {
            if (@event is InputEventMouse && activeInput == ConstTerm.KEY_GAMEPAD) { activeInput = ConstTerm.MOUSE; 
                Input.MouseMode = Input.MouseModeEnum.Visible; if (mouseFocus != null) { mouseFocus.MouseFilter = Godot.Control.MouseFilterEnum.Stop; } }
            else if (@event is not InputEventMouse && activeInput == ConstTerm.MOUSE) { activeInput = ConstTerm.KEY_GAMEPAD; 
                Input.MouseMode = Input.MouseModeEnum.Hidden; if (mouseFocus != null) { mouseFocus.MouseFilter = Godot.Control.MouseFilterEnum.Ignore; } }

            switch (turnPhase)
            {
                case ConstTerm.COMMAND:
                    CommandPhase(@event);
                    break;
                case ConstTerm.ATTACK:
                    AttackPhase(@event);
                    break;
                case ConstTerm.DEFEND:
                    DefendPhase(@event);
                    break;
                case ConstTerm.SKILL + ConstTerm.SELECT:
                    SkillSelectPhase(@event);
                    break;
                case ConstTerm.SKILL + ConstTerm.USE:
                    SkillUsePhase(@event);
                    break;
                case ConstTerm.ITEM + ConstTerm.SELECT:
                    ItemSelectPhase(@event);
                    break;
                case ConstTerm.ITEM + ConstTerm.USE:
                    ItemUsePhase(@event);
                    break;
                default:
                    break;
            }

            if (@event is InputEventMouseButton) { 
                if (@event.IsActionPressed(ConstTerm.CANCEL + ConstTerm.CLICK)) {
                    CancelCycle(); } 
                if (@event.IsActionPressed(ConstTerm.ACCEPT + ConstTerm.CLICK)) {
                    BattlerClick();
                }
            }
        }

        private void AcceptInput()
        {
            activeControl = IUIFunctions.FocusOff(activeList, currentCommand);
            // FocusOff(activeList);
            previousPhase.Add(turnPhase);
        }


        private void CommandPhase(InputEvent @event) // turnPhase == ConstTerm.COMMAND;
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT)) {
                CommandAccept();
            }
            else if (@event.IsActionPressed(ConstTerm.UP)) {
                CommandSelect(-1, commandList, ConstTerm.VERT);
            }
            else if (@event.IsActionPressed(ConstTerm.DOWN)) {
                CommandSelect(1, commandList, ConstTerm.VERT);
            }
        }

        private void CommandAccept()
        {
            AcceptInput();
            IUIFunctions.ToggleMouseFilter(activeList, Godot.Control.MouseFilterEnum.Ignore, out mouseFocus);
            CommandOption(currentCommand);
        }

        private void AttackPhase(InputEvent @event) // turnPhase == ConstTerm.ATTACK;
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT)) {
                AttackAccept();
            }
            else if (@event.IsActionPressed(ConstTerm.CANCEL)) {
                CancelCycle();
            }
            else if (@event.IsActionPressed(ConstTerm.UP)) {
                TargetSelect(-1, ConstTerm.VERT);
            }
            else if (@event.IsActionPressed(ConstTerm.DOWN)) {
                TargetSelect(1, ConstTerm.VERT);
            }
            else if (@event.IsActionPressed(ConstTerm.LEFT)) {
                TargetSelect(-1, ConstTerm.HORIZ);
            }
            else if (@event.IsActionPressed(ConstTerm.RIGHT)) {
                TargetSelect(1, ConstTerm.HORIZ);
            }
        }

        private void AttackAccept()
        {
            AcceptInput();
            SetControlActive(false);
            Attack();
            currentCommand = 0;
        }

        private void DefendPhase(InputEvent @event) // turnPhase == ConstTerm.DEFEND;
        {
            DefendAccept();
        }

        private void DefendAccept()
        {
            AcceptInput();
            SetControlActive(false);
            Defend();
            currentCommand = 0;
        }

        private void SkillSelectPhase(InputEvent @event) // turnPhase == ConstTerm.SKILL_SELECT;
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT)) {
                SkillSelectAccept();
            }
            else if (@event.IsActionPressed(ConstTerm.CANCEL)) {
                CancelCycle();
            }
            else if (@event.IsActionPressed(ConstTerm.UP)) {
                CommandSelect(-1, skillList, ConstTerm.VERT);
            }
            else if (@event.IsActionPressed(ConstTerm.DOWN)) {
                CommandSelect(1, skillList, ConstTerm.VERT);
            }
            else if (@event.IsActionPressed(ConstTerm.LEFT)) {
                CommandSelect(-1, skillList, ConstTerm.HORIZ);
            }
            else if (@event.IsActionPressed(ConstTerm.RIGHT)) {
                CommandSelect(1, skillList, ConstTerm.HORIZ);
            }
        }


        private void SkillSelectAccept()
        {
            if (skillList.GetChild(currentCommand).GetNode<Button>(ConstTerm.BUTTON).Disabled == true) { InvalidOption(); return; }
            AcceptInput();
            SetTurnPhase(ConstTerm.SKILL + ConstTerm.USE);
            EmitSignal(SignalName.onAbilitySelect, currentCommand);
            SetNewCommand();
            // EmitSignal(SignalName.onAbilitySelect, currentCommand);
        }

        private void SkillUsePhase(InputEvent @event) // turnPhase == ConstTerm.SKILL_USE;
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT)) {
                SkillUseAccept();
            }
            else if (@event.IsActionPressed(ConstTerm.CANCEL)) {
                CancelCycle();
            }
            else if (@event.IsActionPressed(ConstTerm.UP)) {
                TargetSelect(-1, ConstTerm.VERT);
            }
            else if (@event.IsActionPressed(ConstTerm.DOWN)) {
                TargetSelect(1, ConstTerm.VERT);
            }
            else if (@event.IsActionPressed(ConstTerm.LEFT)) {
                TargetSelect(-1, ConstTerm.HORIZ);
            }
            else if (@event.IsActionPressed(ConstTerm.RIGHT)) {
                TargetSelect(1, ConstTerm.HORIZ);
            }
        }

        private void SkillUseAccept()
        {
            AcceptInput();
            SetControlActive(false);
            SkillOption(currentCommand);
            currentCommand = 0;
        }

        private void ItemSelectPhase(InputEvent @event) // turnPhase == ConstTerm.ITEM_SELECT
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
            SetTurnPhase(ConstTerm.ITEM + ConstTerm.USE);
            EmitSignal(SignalName.onItemSelect, currentCommand);
            SetNewCommand();
            // EmitSignal(SignalName.onItemSelect, currentCommand);
        }

        private void ItemUsePhase(InputEvent @event) // turnPhase == ConstTerm.ITEM_USE
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT))
            {
                ItemUseAccept();
            }
            else if (@event.IsActionPressed(ConstTerm.CANCEL))
            {
                CancelCycle();
            }
            else if (@event.IsActionPressed(ConstTerm.UP))
            {
                TargetSelect(-1, ConstTerm.VERT);
            }
            else if (@event.IsActionPressed(ConstTerm.DOWN))
            {
                TargetSelect(1, ConstTerm.VERT);
            }
            else if (@event.IsActionPressed(ConstTerm.LEFT))
            {
                TargetSelect(-1, ConstTerm.HORIZ);
            }
            else if (@event.IsActionPressed(ConstTerm.RIGHT))
            {
                TargetSelect(1, ConstTerm.HORIZ);
            }
        }

        private void ItemUseAccept()
        {
            AcceptInput();
            SetControlActive(false);
            ItemOption(currentCommand);
            currentCommand = 0;
        }

        public void BattlerClick()
        {
            if (isTargetting.Count <= 0) { return; }

            switch (turnPhase)
            {
                case ConstTerm.ATTACK:
                    AttackAccept();
                    break;
                case ConstTerm.SKILL + ConstTerm.USE:
                    SkillUseAccept();
                    break;
                case ConstTerm.ITEM + ConstTerm.USE:
                    ItemUseAccept();
                    break;
                case ConstTerm.COMMAND:
                case ConstTerm.DEFEND:
                case ConstTerm.SKILL + ConstTerm.SELECT:
                case ConstTerm.ITEM + ConstTerm.SELECT:
                default:
                    break;
            }
        }


        //=============================================================================
        // SECTION: Selection Handling
        //=============================================================================

        private void CommandSelect(int change, Container targetList, string direction)
        {
            change = IUIFunctions.CheckColumn(change, direction, numColumn);
            IUIFunctions.ChangeTarget(change, ref currentCommand, IUIFunctions.GetCommandCount(targetList));

            activeControl = IUIFunctions.FocusOn(targetList, currentCommand);

            // if (direction == ConstTerm.VERT) { change *= numColumn; }
            // currentCommand = ChangeTarget(change, currentCommand, GetCommandCount(targetList));
            // EmitSignal(SignalName.onTargetChange);
        }

        // private void SkillSelect(int change, Container targetList, string direction)
        // {
        //     if (direction == ConstTerm.VERT) { change *= numColumn; }
        //     currentCommand = ChangeTarget(change, currentCommand, GetCommandCount(targetList));
        //     EmitSignal(SignalName.onTargetChange);
        // }

        private void TargetSelect(int change, string direction) // EDIT: Set way to mouse focus over battler targets
        {
            change = IUIFunctions.CheckColumn(change, direction, numColumn);
            IUIFunctions.ChangeTarget(change, ref actionTarget, GetTargetTeamSize());

            // if (playerAnim.IsPlaying()) { return; }
            // actionTarget = ChangeTarget(change, actionTarget, GetTargetTeamSize());

            EmitSignal(SignalName.onTargetChange);
        }

        private void TargetSpecify(string team, int target)
        {
            SetTargetTeam(team);
            actionTarget = target;
            EmitSignal(SignalName.onTargetChange);
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
            if (turnPhase == ConstTerm.COMMAND) { return;}

            EmitSignal(SignalName.onSelectCancel);

            // SetTurnPhase(phase);
            // currentCommand = 0;
            // SetTargetTeam(ConstTerm.ENEMY);
            // SetNumColumn();

            // EmitSignal(SignalName.onTargetChange);

            activeControl = IUIFunctions.FocusOff(activeList, currentCommand);
            IUIFunctions.ToggleMouseFilter(activeList, Godot.Control.MouseFilterEnum.Ignore, out mouseFocus);

            string oldPhase = IUIFunctions.CancelSelect(out currentCommand, previousCommand, previousPhase);
            SetTurnPhase(oldPhase);
            // SetNumColumn();

            activeControl = IUIFunctions.FocusOn(activeList, currentCommand);
            IUIFunctions.ToggleMouseFilter(activeList, Godot.Control.MouseFilterEnum.Stop, out mouseFocus);

            SetTargetTeam(ConstTerm.ENEMY);
        }


        //=============================================================================
        // SECTION: Command Options
        //=============================================================================

        private void CommandOption(int option)
        {
            ResetTarget(); // Reset targetting to correct team by...  reasons. EDIT

            switch (commandOptions[option])
            {
                case ConstTerm.ATTACK:
                    SetTurnPhase(ConstTerm.ATTACK);
                    EmitSignal(SignalName.onTargetChange);
                    break;
                case ConstTerm.DEFEND:
                    SetTurnPhase(ConstTerm.DEFEND);
                    break;
                case ConstTerm.SKILL:
                    SetTurnPhase(ConstTerm.SKILL + ConstTerm.SELECT);
                    break;
                case ConstTerm.ITEM:
                    if (!ItemBag.Instance.BagIsEmpty()) { SetTurnPhase(ConstTerm.ITEM + ConstTerm.SELECT); }
                    else { InvalidOption(); return; }
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
            activeControl = IUIFunctions.FocusOn(activeList, currentCommand);
            IUIFunctions.ToggleMouseFilter(activeList, Godot.Control.MouseFilterEnum.Stop, out mouseFocus);
            // play 'cannot use' sound effect
        }

        private async void Attack()
        {
            EmitSignal(SignalName.onAbilityStop, -1);
            // EmitSignal(SignalName.onTargetChange);

            // turnPhase = ConstTerm.WAIT;
            // currentCommand = 0;

            playerAnim.Queue(ConstTerm.ANIM_ATTACK);
            await ToSignal(playerAnim, ConstTerm.ANIM_FINISHED);

            EmitSignal(SignalName.onTurnEnd);
            ResetTarget();
        }

        private void Defend()
        {
            EmitSignal(SignalName.onAbilityUse, -1);

            // turnPhase = ConstTerm.WAIT;
            // currentCommand = 0;
            
            // playerAnim.Queue(ConstTerm.ANIM_DEFEND);
            // await ToSignal(playerAnim, ConstTerm.ANIM_FINISHED);

            // EmitSignal(SignalName.onTurnEnd);
        }

        private void SkillOption(int option)
        {
            EmitSignal(SignalName.onAbilityUse, option);

            // turnPhase = ConstTerm.WAIT;
            // currentCommand = 0;

            // playerAnim.Queue(ConstTerm.ANIM_CAST);
            // await ToSignal(playerAnim, ConstTerm.ANIM_FINISHED);

            // EmitSignal(SignalName.onTurnEnd);
        }

        private void ItemOption(int option)
        {
            EmitSignal(SignalName.onItemUse, option);

            // turnPhase = ConstTerm.WAIT;
            // currentCommand = 0;
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

            switch (turnPhase)
            {
                case ConstTerm.COMMAND:
                    CommandAccept();
                    break;
                case ConstTerm.SKILL + ConstTerm.SELECT:
                    SkillSelectAccept();
                    break;
                case ConstTerm.ITEM + ConstTerm.SELECT:
                    ItemSelectAccept();
                    break;
                case ConstTerm.ATTACK:
                case ConstTerm.DEFEND:
                case ConstTerm.SKILL + ConstTerm.USE:
                case ConstTerm.ITEM + ConstTerm.USE:
                default:
                    break;
            }
        }

        public void OnBattlerSelect(bool hover, string team, int target) // EDIT: Add cursor targetting
        {
            if (hover) { isTargetting.Add(target);  }
            else { isTargetting.Remove(target); return; }            

            switch (turnPhase)
            {
                case ConstTerm.ATTACK:
                case ConstTerm.SKILL + ConstTerm.USE:
                case ConstTerm.ITEM + ConstTerm.USE:
                    TargetSpecify(team, target);
                    break;
                case ConstTerm.COMMAND:
                case ConstTerm.DEFEND:
                case ConstTerm.SKILL + ConstTerm.SELECT:
                case ConstTerm.ITEM + ConstTerm.SELECT:
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

        private int GetTargetTeamSize()
        {
            if (targetTeam == ConstTerm.ENEMY) { return enemyTeamSize; }
            else { return playerTeamSize; }
        }


        //=============================================================================
        // SECTION: External Access Methods
        //=============================================================================

        public void PlayerTurnStart()
        {
            currentCommand = 0;

            Button initialFocus = commandList.GetChild(currentCommand).GetNode<Button>(ConstTerm.BUTTON);
            initialFocus?.GrabFocus();

            IUIFunctions.ToggleMouseFilter(commandList, Godot.Control.MouseFilterEnum.Stop, out mouseFocus);
            SetTurnPhase(ConstTerm.COMMAND);
            SetPlayerTurn(true);
            SetNumColumn();
        }

        public bool IsPlayerTurn()
        {
            return playerTurn;
        }

        public float GetSkillItemWidth()
        {
            return skillItemWidth;
        }

        public string[] GetCommandOptions()
        {
            return commandOptions;
        }

        public int GetTarget()
        {
            return actionTarget;
        }

        public void ResetActionTarget() // Set target to 0 for each turn selection. Adjust if using 'saved selection' settings.
        {
            actionTarget = 0;
            ResetTarget();
        }

        public void ResetTarget()
        {
            TargetSelect(0, ConstTerm.HORIZ);
        }

        public int GetCommand()
        {
            return currentCommand;
        }

        public void SetCommand(int index)
        {
            currentCommand = index;
        }

        public void SetNewCommand()
        {
            previousCommand.Add(currentCommand);
            currentCommand = 0;

            activeControl = IUIFunctions.FocusOn(activeList, currentCommand);
        }

        public string GetTurnPhase()
        {
            return turnPhase;
        }

        public void SetTurnPhase(string phase)
        {
            turnPhase = phase;
            if (listDict.TryGetValue(phase, out Container value)) { activeList = value; }
            
            EmitSignal(SignalName.onBattlePhase);
            SetNumColumn();
        }

        public int GetNumColumn()
        {
            return numColumn;
        }

        public void SetNumColumn()
        {
            if (turnPhase == ConstTerm.SKILL + ConstTerm.SELECT) { numColumn = skillList.Columns; }
            else if (turnPhase == ConstTerm.ITEM + ConstTerm.SELECT) { numColumn = itemList.Columns; }
            else { numColumn = 1; }
        }

        public Vector2[] GetPlayerPositions()
        {
            return playerPositions;
        }

        public void SetActiveMember(int member)
        {
            activeMember = member;
            playerAnim = GetChild(member).GetNode<AnimationPlayer>(ConstTerm.ANIM_PLAYER);
        }

        public string GetTargetTeam()
        {
            return targetTeam;
        }

        public void SetTargetTeam(string team)
        {
            targetTeam = team;
            ResetTarget();
        }

        public void SetEnemyTeamSize(int size)
        {
            enemyTeamSize = size;
        }

        public void SetPlayerTeamSize(int size)
        {
            playerTeamSize = size;
        }

        public void SetPlayerTurn(bool value)
        {
            playerTurn = value;
        }

        public void SetBattleOver(bool check)
        {
            battleOver = check;
        }

        public void SetControlActive(bool active)
        {
            isControlActive = active;
        }

        public Container GetSkillList()
        {
            return skillList;
        }

        // public void TakeDamage()
        // {
        //     playerAnim.Play(TAKE_DAMAGE);
        // }
    }
}
