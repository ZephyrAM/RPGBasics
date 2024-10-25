using Godot;

namespace ZAM.Control
{
    public partial class BattleController : Node
    {
        [ExportGroup("Variables")]
        [Export] private string animationPlayerName;
        [Export] private VBoxContainer commandList;
        [Export] private GridContainer skillList;
        [Export] private GridContainer itemList;
        [Export] private Vector2[] playerPositions;

        private AnimationPlayer playerAnim;

        private bool playerTurn = false;
        private string turnPhase = ConstTerm.WAIT;
        private string targetTeam = ConstTerm.BATTLERENEMY;

        private int currentCommand = 0;
        private int activeMember = 0;
        private int actionTarget = 0;
        
        private int enemyTeamSize = 0;
        private int playerTeamSize = 0;
        private int numColumn = 1;

        private bool battleOver = false;
        private bool isActive = false;

        // Delegate Events \\
        [Signal]
        public delegate void onTurnCycleEventHandler();
        [Signal]
        public delegate void onTurnEndEventHandler();
        [Signal]
        public delegate void onTargetChangeEventHandler();
        [Signal]
        public delegate void onAbilitySelectEventHandler(int index);
        [Signal]
        public delegate void onAbilityUseEventHandler(int index);
        [Signal]
        public delegate void onAbilityStopEventHandler(int index);
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
            turnPhase = ConstTerm.WAIT;
        }

        public override void _Input(InputEvent @event)
        {
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
                case ConstTerm.SKILL_SELECT:
                    SkillSelectPhase(@event);
                    break;
                case ConstTerm.SKILL_USE:
                    SkillUsePhase(@event);
                    break;
                default:
                    break;
            }
        }

        private void CommandPhase(InputEvent @event) // turnPhase == ConstTerm.COMMAND;
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT)) {
                CommandOption(currentCommand);
                currentCommand = 0;
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
            Defend();
            currentCommand = 0;
        }

        private void SkillSelectPhase(InputEvent @event) // turnPhase == ConstTerm.SKILL_SELECT;
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT)) {
                turnPhase = ConstTerm.SKILL_USE;
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
                SkillOption(currentCommand);
                currentCommand = 0;
            }
            else if (@event.IsActionPressed(ConstTerm.CANCEL)) {
                CancelSelect(ConstTerm.SKILL_SELECT);
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
                turnPhase = ConstTerm.ITEM_USE;
                // EmitSignal(SignalName.onAbilitySelect, currentCommand);
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


        //=============================================================================
        // SECTION: Selection Handling
        //=============================================================================

        private void CommandSelect(int change, Container targetList, string direction)
        {
            if (direction == ConstTerm.VERT) { change *= numColumn; }
            currentCommand = ChangeTarget(change, currentCommand, GetCommandCount(targetList));
            EmitSignal(SignalName.onTargetChange);
        }

        private void SkillSelect(int change, Container targetList, string direction)
        {
            if (direction == ConstTerm.VERT) { change *= numColumn; }
            currentCommand = ChangeTarget(change, currentCommand, GetCommandCount(targetList));
            EmitSignal(SignalName.onTargetChange);
        }

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
            turnPhase = phase;
            currentCommand = 0;
            SetTargetTeam(ConstTerm.BATTLERENEMY);
            SetNumColumn();

            EmitSignal(SignalName.onTargetChange);
        }


        //=============================================================================
        // SECTION: Command Options
        //=============================================================================

        private void CommandOption(int option)
        {
            ResetTarget(); // Reset targetting to correct team by...  reasons. EDIT
            
            if (option == 0) { turnPhase = ConstTerm.ATTACK; }
            else if (option == 1) { turnPhase = ConstTerm.DEFEND; }
            else if (option == 2) { turnPhase = ConstTerm.SKILL_SELECT; }
            else if (option == 3) { turnPhase = ConstTerm.ITEM_SELECT; }

            SetNumColumn();
        }

        private async void Attack()
        {
            EmitSignal(SignalName.onAbilityStop, -1);
            EmitSignal(SignalName.onTargetChange);

            turnPhase = ConstTerm.WAIT;
            currentCommand = 0;

            playerAnim.Queue(ConstTerm.ANIM_ATTACK);
            await ToSignal(playerAnim, ConstTerm.ANIM_FINISHED);

            EmitSignal(SignalName.onTurnEnd);
            ResetTarget();
        }

        private void Defend()
        {
            EmitSignal(SignalName.onAbilityUse, -1);

            turnPhase = ConstTerm.WAIT;
            currentCommand = 0;
            
            // playerAnim.Queue(ConstTerm.ANIM_DEFEND);
            // await ToSignal(playerAnim, ConstTerm.ANIM_FINISHED);

            // EmitSignal(SignalName.onTurnEnd);
        }

        private void SkillOption(int option)
        {
            EmitSignal(SignalName.onAbilityUse, option);

            turnPhase = ConstTerm.WAIT;
            currentCommand = 0;

            // playerAnim.Queue(ConstTerm.ANIM_CAST);
            // await ToSignal(playerAnim, ConstTerm.ANIM_FINISHED);

            // EmitSignal(SignalName.onTurnEnd);
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
            if (targetTeam == ConstTerm.BATTLERENEMY) { return enemyTeamSize; }
            else { return playerTeamSize; }
        }


        //=============================================================================
        // SECTION: External Access Methods
        //=============================================================================

        public int GetTarget()
        {
            return actionTarget;
        }

        public void SetActionTarget(int target) // Set target to 0 for each turn selection. Adjust if using 'saved selection' settings.
        {
            actionTarget = target;
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

        public void SetTurnPhase(string phase)
        {
            turnPhase = phase;
        }

        public string GetTurnPhase()
        {
            return turnPhase;
        }

        public void SetActiveMember(int member)
        {
            activeMember = member;
            playerAnim = GetChild(member).GetNode<AnimationPlayer>(animationPlayerName);
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

        public void SetNumColumn()
        {
            if (turnPhase == ConstTerm.SKILL_SELECT) { numColumn = skillList.Columns; }
            else if (turnPhase == ConstTerm.ITEM_SELECT) { numColumn = itemList.Columns; }
            else { numColumn = 1; }
        }

        public float GetNumColumn()
        {
            return numColumn;
        }

        public void SetPlayerTurn(bool value)
        {
            playerTurn = value;
        }

        public bool IsPlayerTurn()
        {
            return playerTurn;
        }

        public Vector2[] GetPlayerPositions()
        {
            return playerPositions;
        }

        public void SetBattleOver(bool check)
        {
            battleOver = check;
        }

        // public void TakeDamage()
        // {
        //     playerAnim.Play(TAKE_DAMAGE);
        // }
    }
}
