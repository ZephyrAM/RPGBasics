using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ZAM.Abilities;
using ZAM.Control;
using ZAM.Stats;

namespace ZAM.System
{
    public partial class BattleSystem : Node
    {
        [ExportGroup("Battler Teams")]
        // [Export] PackedScene playerBattlers;
        [Export] private PackedScene enemyEncounter;

        [ExportGroup("Nodes")]
        [Export] private BattleController partyInput;
        [Export] private Node2D floatingText;
        [Export] private Node2D cursorTarget;
        [Export] private Camera2D baseCamera;
        [Export] private CanvasLayer battleUI;
        [Export] private PanelContainer commandPanel;
        [Export] private PanelContainer skillPanel;

        [ExportGroup("UI Resources")]
        [Export] private PackedScene statusList;
        [Export] private PackedScene labelSettings;

        [ExportGroup("Variables")]
        [Export] private string battleUIName;

        [Export] private string enemyTeamName;
        [Export] private string playerPartyName;
        [Export] private string enemyListName;
        [Export] private string playerListName;
        [Export] private string healthListName;
        [Export] private string healthBarName;

        [Export] private string cursorSpriteName;
        [Export] private string cursorAnimatorName;

        private MapSystem mapScene;
        private PartyManager playerParty;
        private CombatAbilities defendAbility;
        private CombatAbilities activeAbility;

        private List<Battler> battler = null;
        private List<Battler> enemyTeam = null;
        private List<Battler> playerTeam = null;
        private VBoxContainer enemyList = null;
        private VBoxContainer playerList = null;
        // HBoxContainer enemyHealthList = null;
        // HBoxContainer playerStatusList = null;

        private bool turnActive = false;
        private int nextFloatValue = 0;

        private Vector2 offset;
        private float cursorOffset, targetOffset;


        // Delegate Events \\
        [Signal]
        public delegate void onBuildPlayerTeamEventHandler();

        //=============================================================================
        // SECTION: Base Methods
        //=============================================================================
        public override void _Ready()
        {
            IfNull();
            BuildPlayerTeam();
            // mapScene.BuildParty();
            BuildEnemyTeam();

            cursorOffset = cursorTarget.GetNode<Sprite2D>(cursorSpriteName).Texture.GetHeight() / 2;
            targetOffset = enemyTeam[partyInput.GetTarget()].GetSprite2D().Texture.GetHeight() / 2;
            offset = new Vector2(0, targetOffset + cursorOffset);

            cursorTarget.GetNode<Sprite2D>(cursorSpriteName).GetNode<AnimationPlayer>(cursorAnimatorName).Play(ConstTerm.CURSOR_BOUNCE);
            defendAbility = (CombatAbilities)ResourceLoader.Load(ConstTerm.ABILITY_SOURCE + ConstTerm.DEFEND_ABILITY);

            SubSignals();

            battler = [];
            CheckBattlers();
            // SetCursorPosition();
            SaveLoader.Instance.FillData(SaveLoader.Instance.gameSession);
            InitialHealthBars();
        }

        // public override void _ExitTree()
        // {
        //     UnSubSignals();
        // }

        public override void _PhysicsProcess(double delta)
        {
            UIVisibility();
        }

        public void StoreMapScene(MapSystem map)
        {
            mapScene = map;
            playerParty = mapScene.GetPartyManager();
        }

        public async void ReturnMapScene()
        {
            Fader.Instance.Transition();
            await ToSignal(Fader.Instance, ConstTerm.TRANSITION_FINISHED);

            // mapScene.Reparent(GetTree().Root);
            GetTree().Root.AddChild(mapScene); // Causes error - already has parent? Still functions properly, without indication of error.
            playerParty.ChangePlayerActive(true);

            QueueFree();
        }

        //=============================================================================
        // SECTION: OnReady Methods
        //=============================================================================

        private void IfNull()
        {
            enemyTeamName ??= "EnemyTeam";
            playerPartyName ??= "PlayerParty";
            enemyListName ??= "EnemyList";
            playerListName ??= "PlayerList";
            healthListName ??= "StatusList";

            battleUIName ??= ConstTerm.BATTLE_UI;
            healthBarName ??= ConstTerm.HEALTH_DISPLAY;

            cursorAnimatorName ??= ConstTerm.ANIM_PLAYER;
            cursorSpriteName ??= ConstTerm.SPRITE2D;

            battleUI ??= GetNode<CanvasLayer>(battleUIName);
        }

        private void SubSignals()
        {
            partyInput.onTurnCycle += OnTurnCycle;
            partyInput.onTurnEnd += OnTurnEnd;
            partyInput.onTargetChange += OnTargetChange;
            partyInput.onAbilitySelect += OnAbilitySelect;
            partyInput.onAbilityUse += OnAbilityUse;
            partyInput.onAbilityStop += OnAbilityStop;

            // partyInput.onSaveGame += OnSaveGame;
            // partyInput.onLoadGame += OnLoadGame;

            // partyInput.onBattleFinish += OnBattleFinish;
        }

        private void UnSubSignals()
        {
            partyInput.onTurnCycle -= OnTurnCycle;
            partyInput.onTurnEnd -= OnTurnEnd;
            partyInput.onTargetChange -= OnTargetChange;
            partyInput.onAbilityUse -= OnAbilityUse;
            partyInput.onAbilityStop -= OnAbilityStop;
        }

        public void BuildPlayerTeam()
        {
            playerTeam = new List<Battler>();
            Node teamNode = GetNode(playerPartyName);

            // CharacterBody2D[] tempTeam = GetPlayerTeam(playerParty);
            // List<CharacterBody2D> tempTeam = playerParty.GetActiveParty();

            for (int n = 0; n < playerParty.GetPartySize(); n++)
            {
                CharacterBody2D tempChar = playerParty.GetPartyMember(n);
                tempChar.Position = partyInput.GetPlayerPositions()[n];

                // partyInput.AddChild(tempTeam[n]);
                // partyInput.MoveChild(tempTeam[n], n);

                // playerTeam.Add(teamNode.GetChild<CharacterBody2D>(n).GetNode<Battler>(ConstTerm.BATTLER));
                // playerTeam[n] = tempTeam[n].GetNode<Battler>(ConstTerm.BATTLER);
                // playerTeam.Add(tempTeam[n].GetNode<Battler>(ConstTerm.BATTLER));
                // CharacterBody2D nextMember = (CharacterBody2D)tempParty.GetPartyMember(n).Instantiate();
                // nextMember.Position = tempTeam[n].Position;

                partyInput.AddChild(tempChar);
                playerTeam.Add(tempChar.GetNode<Battler>(ConstTerm.BATTLER));
                playerTeam[n].onHitEnemy += OnHitEnemy;
                playerTeam[n].onAbilityHitPlayer += OnAbilityHitPlayer;

                // UI Bars \\
                var tempLine = ResourceLoader.Load<PackedScene>(statusList.ResourcePath).Instantiate();
                playerList = battleUI.GetNode<VBoxContainer>(playerListName);
                playerList.AddChild(tempLine);
                playerList.GetChild(n).GetNode<HealthDisplay>(healthBarName).SetBattler(playerTeam[n]);
                playerList.GetChild(n).GetNode<Label>(ConstTerm.NAMELABEL).Text = playerTeam[n].GetNameLabel().Text;
                playerTeam[n].GetNameLabel().Visible = false;
            }

            partyInput.SetPlayerTeamSize(playerTeam.Count);
        }

        private void InitialHealthBars()
        {
            for (int n = 0; n < playerList.GetChildCount(); n++)
            {
                playerList.GetChild(n).GetNode<HealthDisplay>(healthBarName).ForceHealthBarUpdate();
            }
        }

        private void BuildEnemyTeam()
        {
            enemyList = battleUI.GetNode<VBoxContainer>(enemyListName);

            enemyTeam = [];
            var encounterTeam = ResourceLoader.Load<PackedScene>(enemyEncounter.ResourcePath).Instantiate();
            AddChild(encounterTeam);
            BuildTeam(enemyList, enemyTeam, enemyTeamName);

            partyInput.SetEnemyTeamSize(enemyTeam.Count);
        }

        private void BuildTeam(VBoxContainer teamList, List<Battler> teamBattlers, string teamName)
        {
            Node2D teamNode = GetNode<Node2D>(teamName);
            int childCount = teamNode.GetChildCount();
            List<int> tempList = new();

            for (int i = 0; i < childCount; i++)
            {
                if (teamNode.GetChild(i) is CharacterBody2D)
                {
                    tempList.Add(i);
                }
            }

            for (int n = 0; n < tempList.Count; n++)
            {
                teamBattlers.Add(teamNode.GetChild<CharacterBody2D>(tempList[n]).GetNode<Battler>(ConstTerm.BATTLER));
                teamBattlers[n].onHitPlayer += OnHitPlayer;

                string tempName = teamBattlers[n].GetNameLabel().Text;
                teamBattlers[n].GetNameLabel().Text = tempName;
                teamBattlers[n].GetNameLabel().Reparent(teamList);
                teamBattlers[n].GetNode<HealthDisplay>("../" + healthBarName).SetBattler(teamBattlers[n]);
            }
        }

        //=============================================================================
        // SECTION: Turn Handling
        //=============================================================================

        public void UIVisibility()
        {
            cursorTarget.Visible = partyInput.GetTurnPhase() == ConstTerm.ATTACK || partyInput.GetTurnPhase() == ConstTerm.SKILL_USE;
            commandPanel.Visible = partyInput.GetTurnPhase() == ConstTerm.COMMAND;
            skillPanel.Visible   = partyInput.GetTurnPhase() == ConstTerm.SKILL_SELECT;
        }

        public async void CheckBattlers()
        {
            if (battler.Count <= 0)
            {
                battler = new();
                battler.AddRange(playerTeam);
                battler.AddRange(enemyTeam);

                await Task.Delay(30); // Wait for battlers to establish in scene
                OnTurnCycle();
            }
        }

        public List<Battler> SortSpeed(List<Battler> battlers)
        {
            List<Battler> sortedBattlers = battlers.OrderByDescending(x => x.GetStats().GetStatValue(Stat.Agility)).ToList();
            return sortedBattlers;
        }

        public void MoveCommandPanel(Battler current)
        {
            Vector2 commandOrigin = current.GetCharBody().GetGlobalTransformWithCanvas().Origin;
            float xOffset = commandPanel.Size.X + (current.GetWidth() / 2);
            float yOffset = commandPanel.Size.Y - (current.GetHeight() / 2);
            commandPanel.Position = commandOrigin - new Vector2(xOffset, yOffset);
        }

        public void SetCursorPosition()
        {
            int target = partyInput.GetTarget();
            List<Battler> targetTeam;

            if (activeAbility == null || activeAbility.TargetType == ConstTerm.ENEMY) 
            { targetTeam = enemyTeam; }
            else { targetTeam = playerTeam; }

            if (target >= targetTeam.Count || target < 0) { target = 0; }

            targetOffset = targetTeam[target].GetHeight() / 2;
            offset = new Vector2(0, targetOffset + cursorOffset);

            cursorTarget.Position = targetTeam[target].GetCharBody().GetGlobalTransformWithCanvas().Origin - offset;
            // GD.Print(target + " " + targetTeam[target].GetCharBody().GetGlobalTransformWithCanvas().Origin + " " + targetTeam[target].GetCharBody().Position);
        }

        private void ReturnToPosition(Battler currBattler)
        {
            if (currBattler.GetBattlerType() == ConstTerm.BATTLERPLAYER)
            {
                Vector2 battlerPos = currBattler.GetSprite2D().Position;
                currBattler.GetAnimPlayer().GetAnimation(ConstTerm.ANIM_RETURN).TrackSetKeyValue(0, 0, new Vector2(battlerPos.X, battlerPos.Y));
                currBattler.GetAnimPlayer().Queue(ConstTerm.ANIM_RETURN);
            }
        }

        //=============================================================================
        // SECTION: Battle Methods
        //=============================================================================

        public void KillTarget(Battler target, List<Battler> team)
        {
            team.Remove(target);
            battler.Remove(target);
            target.GetParent().QueueFree();
        }

        private void DamageHandling(Battler attacker, Battler defender)
        {
            float damage = 0;
            if (activeAbility == null) { damage = Formula.PhysDamage(attacker, defender, null); }
            else if (activeAbility.DamageType == ConstTerm.PHYSICAL) { damage = Formula.PhysDamage(attacker, defender, activeAbility); }
            else if (activeAbility.DamageType == ConstTerm.MAGICAL) { damage = Formula.SpellDamage(attacker, defender, activeAbility); }

            defender.GetHealth().ChangeHP(damage);
            FloatingNumbers(defender, damage.ToString(), ConstTerm.WHITE);
            // defender.GetHealthBar().UpdateHealthBar();
        }

        private async void FloatingNumbers(Battler defender, string value, string modColor)
        {
            Label nextValue = floatingText.GetChild<Node2D>(nextFloatValue).GetNode<Label>(ConstTerm.LABEL);
            nextFloatValue++;
            if (nextFloatValue > floatingText.GetChildCount() - 1) { nextFloatValue = 0; }

            Vector2 newPosition = defender.GetCharBody().GetGlobalTransformWithCanvas().Origin;
            Vector2 offset = new(defender.GetWidth() / 3, defender.GetHeight());
            AnimationPlayer textAnim = nextValue.GetNode<AnimationPlayer>(ConstTerm.ANIM_PLAYER);

            nextValue.GetParent<Node2D>().Position = newPosition - offset;
            nextValue.Text = value;
            nextValue.GetParent<Node2D>().Modulate = new Color(modColor);
            nextValue.Visible = true;

            textAnim.Play(ConstTerm.FADE_UP);

            await ToSignal(textAnim, ConstTerm.ANIM_FINISHED);
            nextValue.Visible = false;
        }

        public bool BattleEndCheck()
        {
            if (enemyTeam.Count <= 0)
            { 
                GD.Print("You win!"); 
                BattleWin();
                return true;
            }
            if (playerTeam.Count <= 0)
            { 
                GD.Print("You lose!");
                return true;
            }
            return false;
        }

        private async void BattleWin()
        {
            SaveLoader.Instance.gameSession.CharData = SaveLoader.Instance.GatherBattlers();

            partyInput.SetBattleOver(true);
            await ToSignal(partyInput, ConstTerm.BATTLE_FINISHED);
            ReturnMapScene();
        }

        public async void EnemyTurn()
        {
            GD.Print(" - Enemy Go");
            turnActive = true;

            AnimationPlayer target = battler[0].GetAnimPlayer();
            target.Queue(ConstTerm.ANIM_ENEMY_ATTACK);
            await ToSignal(target, ConstTerm.ANIM_FINISHED);
            OnTurnEnd();
        }

        public void PlayerTurn()
        {
            SetupSkillList(battler[0]);
            GD.Print(" - Player Go");
            partyInput.SetActiveMember(playerTeam.IndexOf(battler[0]));
            MoveCommandPanel(battler[0]);

            turnActive = true;
            partyInput.SetTurnPhase(ConstTerm.COMMAND);
            partyInput.SetPlayerTurn(true);
            partyInput.SetNumColumn();
            SetCursorPosition();
            // Wait for player/BattleController
        }

        private void SetupSkillList(Battler player)
        {
            foreach(Node child in skillPanel.GetNode(ConstTerm.SKILL_LIST).GetChildren())
            { child.QueueFree(); }

            foreach (CombatAbilities skill in player.GetSkillList().GetSkills())
            {
                Label newSkill = (Label)ResourceLoader.Load<PackedScene>(labelSettings.ResourcePath).Instantiate();
                newSkill.Text = skill.AbilityName;

                skillPanel.GetNode(ConstTerm.SKILL_LIST).AddChild(newSkill);
            }
        }

        //=============================================================================
        // SECTION: Animation Call Methods
        //=============================================================================

        public void OnHitEnemy(Battler player) // Called from Battler
        {
            if (activeAbility != null && activeAbility.TargetArea == ConstTerm.GROUP) {

                for (int n = 0; n < enemyTeam.Count; n++)
                {
                    Battler defender = enemyTeam[n];

                    defender.GetAnimPlayer().Queue(ConstTerm.TAKE_DAMAGE);
                    DamageHandling(player, defender);

                    if (defender.GetHealth().GetHP() <= 0)
                    {
                        defender.onHitPlayer -= OnHitPlayer;
                        KillTarget(defender, enemyTeam);

                        enemyList.GetChild(n).QueueFree();
                        partyInput.SetEnemyTeamSize(enemyTeam.Count);
                        n--;
                    }
                }
            }
            else {
                Battler defender = enemyTeam[partyInput.GetTarget()];

                defender.GetAnimPlayer().Queue(ConstTerm.TAKE_DAMAGE);
                DamageHandling(player, defender);

                if (defender.GetHealth().GetHP() <= 0)
                {
                    defender.onHitPlayer -= OnHitPlayer;
                    KillTarget(defender, enemyTeam);

                    enemyList.GetChild(partyInput.GetTarget()).QueueFree();
                    partyInput.SetEnemyTeamSize(enemyTeam.Count);

                    BattleEndCheck();
                }
            }
        }

        public void OnHitPlayer(Battler enemy) // Called from Battler
        {
            Random random = new();
            int target = random.Next(0, playerTeam.Count);

            Battler defender = playerTeam[target];
            defender.GetAnimPlayer().Queue(ConstTerm.TAKE_DAMAGE);
            DamageHandling(enemy, defender);

            if (defender.GetHealth().GetHP() <= 0)
            {
                defender.onHitEnemy -= OnHitEnemy;
                KillTarget(defender, playerTeam);

                playerList.GetChild(target).QueueFree();
                partyInput.SetPlayerTeamSize(playerTeam.Count);

                BattleEndCheck();
            }
        }

        public void OnAbilityHitPlayer(Battler user)
        {
            if (activeAbility.TargetArea == ConstTerm.GROUP) {
                foreach (Battler player in playerTeam)
                {
                    Battler target = player;
                    float healing = activeAbility.NumericValue;
                    target.GetHealth().ChangeHP(healing);
                    FloatingNumbers(target, healing.ToString(), ConstTerm.GREEN);
                }
            }
            else {
                Battler target = playerTeam[partyInput.GetTarget()];
                float healing = activeAbility.NumericValue;
                target.GetHealth().ChangeHP(healing);
                FloatingNumbers(target, healing.ToString(), ConstTerm.GREEN);
            }
        }


        //=============================================================================
        // SECTION: Signal Methods
        //=============================================================================


        private async void OnTurnCycle()
        {
            if (BattleEndCheck()) { return; }
            if (battler.Count > 0)
            {
                GD.Print(battler[0].GetNameLabel().Text + " Turn Start!");
                partyInput.SetTurnPhase(ConstTerm.WAIT);
                activeAbility = null;

                if (turnActive == false) { battler = SortSpeed(battler); }

                if (battler[0].GetBattlerType() == ConstTerm.BATTLERENEMY)
                {
                    EnemyTurn();
                }
                else
                {
                    PlayerTurn();
                }
            }
            CheckBattlers();

            await Task.Delay(120);
        }

        private void OnTurnEnd()
        {
            GD.Print("End turn.");
            partyInput.SetPlayerTurn(false);
            ReturnToPosition(battler[0]);
            battler.RemoveAt(0);
            turnActive = false;
            partyInput.SetTargetTeam(ConstTerm.BATTLERENEMY);

            // await Task.Yield();
            
            OnTurnCycle();
        }

        private void OnTargetChange()
        {
            if (enemyTeam.Count > 0)
            {
                if (partyInput.GetTurnPhase() == ConstTerm.ATTACK || partyInput.GetTurnPhase() == ConstTerm.SKILL_USE)
                {
                    SetCursorPosition();
                }
                else if (partyInput.GetTurnPhase() == ConstTerm.COMMAND)
                {
                    activeAbility = null;
                    ColorRect selectBar = commandPanel.GetNode<ColorRect>(ConstTerm.COLOR_RECT);
                    int index = partyInput.GetCommand();

                    selectBar.Position = new Vector2(0, selectBar.Size.Y * index); // EDIT - Temporary!
                }
                else if (partyInput.GetTurnPhase() == ConstTerm.SKILL_SELECT)
                {
                    activeAbility = null;
                    ColorRect selectBar = skillPanel.GetNode<GridContainer>(ConstTerm.SELECT_LIST).GetNode<ColorRect>(ConstTerm.COLOR_RECT);

                    float index = partyInput.GetCommand();
                    float column = index % partyInput.GetNumColumn();
                    float yPos = (float)(selectBar.Size.Y * Math.Ceiling(index / 2 - column));

                    selectBar.Position = new Vector2(column * selectBar.Size.X, yPos); // EDIT - Temporary!
                }
            }
        }

        private void OnAbilitySelect(int index)
        {
            activeAbility = battler[0].GetSkillList().GetSkills()[index];

            if (partyInput.IsPlayerTurn()) 
            {
                if (activeAbility.TargetType == ConstTerm.ALLY) { partyInput.SetTargetTeam(ConstTerm.BATTLERPLAYER); }
                else { partyInput.SetTargetTeam(ConstTerm.BATTLERENEMY); }
            }

            SetCursorPosition();
        }

        private async void OnAbilityUse(int index)
        {
            if (index == -1) { activeAbility = defendAbility; }
            // else {
            //     activeAbility = battler[0].GetSkillList().GetSkills()[index];
            // }

            if (activeAbility.AddedState != null) 
            {
                
                if (!battler[0].GetStateList().Contains(activeAbility.AddedState))
                {
                    battler[0].AddState(activeAbility.AddedState);
                }
                GD.Print(" -- Activate - " + battler[0].GetStateList().Last().StateName);
            }

            battler[0].GetAnimPlayer().Queue(activeAbility.CallAnimation);
            await ToSignal(battler[0].GetAnimPlayer(), ConstTerm.ANIM_FINISHED);

            OnTurnEnd();
        }

        private void OnAbilityStop(int index)
        {
            CombatAbilities newAbility;
            if (index == -1) { newAbility = defendAbility; }
            else
            {
                newAbility = battler[0].GetSkillList().GetSkills()[index];
            }

            if (newAbility.AddedState != null)
            {
                if (battler[0].GetStateList().Contains(newAbility.AddedState))
                {
                    battler[0].GetStateList().Remove(newAbility.AddedState);
                }
            }
        }

        // private void OnSaveGame()
        // {
        //     // Array<Resource> newSave = new();
        //     for (int n = 0; n < playerParty.GetPartySize(); n++)
        //     {
        //         playerTeam[n].SaveData();
        //     }
        // }

        // private void OnLoadGame()
        // {
        //     for (int n = 0; n < playerParty.GetPartySize(); n++)
        //     {
        //         playerTeam[n].LoadData();
        //     }
        // }

        // private void OnBattleFinish()
        // {

        // }

        // public void GetParty()
        // {
        //     Battler[] saveParty = new Battler[playerTeam.Count];

        //     for (int i = 0; i < playerTeam.Count; i++)
        //     {
        //         saveParty[i] = playerTeam[i];
        //     }
        //     SaveData.SaveAllData(saveParty);
        // }
    }
}
