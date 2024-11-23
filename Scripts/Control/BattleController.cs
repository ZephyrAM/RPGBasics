using Godot;

using ZAM.Inventory;

namespace ZAM.Control
{
    public partial class BattleController : Node
    {
        [Export] private string[] commandOptions = [];
        [Export] private int skillItemWidth = 0;

        [ExportGroup("Lists")]
        [Export] private VBoxContainer commandList;
        [Export] private GridContainer skillList;
        [Export] private GridContainer itemList;

        [ExportGroup("Variables")]
        [Export] private string animationPlayerName;
        [Export] private Vector2[] playerPositions;

        private AnimationPlayer playerAnim;

        private bool playerTurn = false;
        private string turnPhase = ConstTerm.WAIT;
        private string targetTeam = ConstTerm.ENEMY;

        private int currentCommand = 0;
        private int activeMember = 0;
        private int actionTarget = 0;
        
        private int enemyTeamSize = 0;
        private int playerTeamSize = 0;
        private int numColumn = 1;

        private bool battleOver = false;
        private bool isControlActive = false;

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
            if (battleOver && @event != null) {
                if (@event is InputEventMouseMotion) { return; }
                EmitSignal(SignalName.onBattleFinish);
            }
            if (playerTurn == true) {
                PhaseCheck(@event);
                // Data test commands \\
                // if (Input.IsActionJustPressed("Save"))
                // {
                //     GD.Print("Saving game!");
                //     SaveLoader.Instance.SaveGame();          
                //     // EmitSignal(SignalName.onSaveGame);
                // }
                // else if (Input.IsActionJustPressed("Load"))
                // {
                //     GD.Print("Loading game!");
                //     SaveLoader.Instance.LoadGame();
                //     // EmitSignal(SignalName.onLoadGame);
                // }
            }
        }

        //=============================================================================
        // SECTION: OnReady Methods
        //=============================================================================
        private void IfNull()
        {
            animationPlayerName ??= ConstTerm.ANIM_PLAYER;

            // playerAnim ??= GetChild(0).GetNode<AnimationPlayer>(animationPlayerName);
        }


        //=============================================================================
        // SECTION: Phase Handling - Input
        //=============================================================================

        private void PhaseCheck(InputEvent @event)
        {
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
        }

        private void CommandPhase(InputEvent @event) // turnPhase == ConstTerm.COMMAND;
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT)) {
                CommandOption(currentCommand);
            }
            else if (@event.IsActionPressed(ConstTerm.UP)) {
                CommandSelect(-1, commandList, ConstTerm.VERT);
            }
            else if (@event.IsActionPressed(ConstTerm.DOWN)) {
                CommandSelect(1, commandList, ConstTerm.VERT);
            }
        }

        private void AttackPhase(InputEvent @event) // turnPhase == ConstTerm.ATTACK;
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT)) {
                SetControlActive(false);
                Attack();
                currentCommand = 0;
            }
            else if (@event.IsActionPressed(ConstTerm.CANCEL)) {
                CancelSelect(ConstTerm.COMMAND);
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

        private void DefendPhase(InputEvent @event) // turnPhase == ConstTerm.DEFEND;
        {
            SetControlActive(false);
            Defend();
            currentCommand = 0;
        }

        private void SkillSelectPhase(InputEvent @event) // turnPhase == ConstTerm.SKILL_SELECT;
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT)) {
                SetTurnPhase(ConstTerm.SKILL + ConstTerm.USE);
                EmitSignal(SignalName.onAbilitySelect, currentCommand);
                currentCommand = 0;
            }
            else if (@event.IsActionPressed(ConstTerm.CANCEL)) {
                CancelSelect(ConstTerm.COMMAND);
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

        private void SkillUsePhase(InputEvent @event) // turnPhase == ConstTerm.SKILL_USE;
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT)) {
                SetControlActive(false);
                SkillOption(currentCommand);
                currentCommand = 0;
            }
            else if (@event.IsActionPressed(ConstTerm.CANCEL)) {
                CancelSelect(ConstTerm.SKILL + ConstTerm.SELECT);
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

        private void ItemSelectPhase(InputEvent @event) // turnPhase == ConstTerm.ITEM_SELECT
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT))
            {
                SetTurnPhase(ConstTerm.ITEM + ConstTerm.USE);
                EmitSignal(SignalName.onItemSelect, currentCommand);
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

        private void ItemUsePhase(InputEvent @event) // turnPhase == ConstTerm.ITEM_USE
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT))
            {
                SetControlActive(false);
                ItemOption(currentCommand);
                currentCommand = 0;
            }
            else if (@event.IsActionPressed(ConstTerm.CANCEL))
            {
                CancelSelect(ConstTerm.ITEM + ConstTerm.SELECT);
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


        //=============================================================================
        // SECTION: Selection Handling
        //=============================================================================

        private void CommandSelect(int change, Container targetList, string direction)
        {
            if (direction == ConstTerm.VERT) { change *= numColumn; }
            currentCommand = ChangeTarget(change, currentCommand, GetCommandCount(targetList));
            EmitSignal(SignalName.onTargetChange);
        }

        // private void SkillSelect(int change, Container targetList, string direction)
        // {
        //     if (direction == ConstTerm.VERT) { change *= numColumn; }
        //     currentCommand = ChangeTarget(change, currentCommand, GetCommandCount(targetList));
        //     EmitSignal(SignalName.onTargetChange);
        // }

        private void TargetSelect(int change, string direction)
        {
            // if (playerAnim.IsPlaying()) { return; }
            actionTarget = ChangeTarget(change, actionTarget, GetTargetTeamSize());
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
            EmitSignal(SignalName.onSelectCancel);

            SetTurnPhase(phase);
            currentCommand = 0;
            SetTargetTeam(ConstTerm.ENEMY);
            SetNumColumn();

            // EmitSignal(SignalName.onTargetChange);
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
                    else { return; }
                    break;
                default:
                    break;
            }

            SetNumColumn();
            currentCommand = 0;
        }

        private async void Attack()
        {
            EmitSignal(SignalName.onAbilityStop, -1);
            EmitSignal(SignalName.onTargetChange);

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
        // SECTION: Internal Access Methods
        //=============================================================================

        private int GetCommandCount(Container targetList)
        {
            return targetList.GetChildCount();
        }

        private int GetTargetTeamSize()
        {
            if (targetTeam == ConstTerm.ENEMY) { return enemyTeamSize; }
            else { return playerTeamSize; }
        }


        //=============================================================================
        // SECTION: External Access Methods
        //=============================================================================

        public bool IsPlayerTurn()
        {
            return playerTurn;
        }

        public int GetSkillItemWidth()
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

        public string GetTurnPhase()
        {
            return turnPhase;
        }

        public void SetTurnPhase(string phase)
        {
            turnPhase = phase;
            EmitSignal(SignalName.onBattlePhase);
        }

        public float GetNumColumn()
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
            playerAnim = GetChild(member).GetNode<AnimationPlayer>(animationPlayerName);
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

        // public void TakeDamage()
        // {
        //     playerAnim.Play(TAKE_DAMAGE);
        // }
    }
}
