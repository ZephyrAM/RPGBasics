using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ZAM.Abilities;
using ZAM.Control;
using ZAM.Inventory;
using ZAM.Stats;
using ZAM.Managers;

namespace ZAM.System
{
    public partial class BattleSystem : Node
    {
        // [ExportGroup("Battler Teams")]
        // [Export] PackedScene playerBattlers;

        [ExportGroup("Nodes")]
        [Export] private BattleController partyInput = null;
        [Export] private Camera2D baseCamera = null;
        [Export] private CanvasLayer battleUI = null;

        [Export] private Node2D floatingText = null;
        // [Export] private Node2D cursorTarget = null;

        [Export] private PanelContainer commandPanel = null;
        [Export] private PanelContainer skillPanel = null;
        [Export] private PanelContainer itemPanel = null;

        [ExportGroup("UI Resources")]
        [Export] private PackedScene statusList = null;
        [Export] private PackedScene labelSettings = null;

        // private List<PackedScene> cursorTargetList = [];
        private PackedScene enemyEncounter = null;
        private Node2D mapScene;
        private PartyManager playerParty;

        private Ability defendAbility;
        private Ability activeAbility;
        private Item activeItem;

        private List<Battler> battler = null;
        private List<Battler> enemyTeam = null;
        private List<Battler> playerTeam = null;
        private VBoxContainer enemyList = null;
        private VBoxContainer playerList = null;

        // private ColorRect commandBar = null;
        // private ColorRect skillBar = null;
        // private ColorRect itemBar = null;

        private bool turnActive = false;
        private int nextFloatValue = 0;
        private Vector2 marginSize = new(4, 4);

        // private Vector2 offset;
        private float cursorOffset, targetOffset;
        private Button itemButton = null;


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

            // cursorOffset = cursorTarget.GetNode<Sprite2D>(ConstTerm.SPRITE2D).Texture.GetHeight() / 2;
            // targetOffset = enemyTeam[partyInput.GetTarget()].GetSprite2D().Texture.GetHeight() / 2;
            // offset = new Vector2(0, targetOffset + cursorOffset);

            // cursorTarget.GetNode<Sprite2D>(ConstTerm.SPRITE2D).GetNode<AnimationPlayer>(ConstTerm.ANIM_PLAYER).Play(ConstTerm.CURSOR_BOUNCE);
            defendAbility = DatabaseManager.Instance.GetDefend();

            SubSignals();

            battler = [];
            CheckBattlers();
            // SetCursorPosition();
            InitialHealthBars();
            SetupCommandList();
            SetupItemList();

            UIVisibility();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            UnSubSignals();
        }

        // public override void _Process(double delta)
        // {
        //     UIVisibility();
        // }

        public void StoreMapScene(Node2D map)
        {
            mapScene = map;
            playerParty = mapScene.GetChild<MapSystem>(0).GetPartyManager();
        }

        //=============================================================================
        // SECTION: OnReady Methods
        //=============================================================================

        private void IfNull()
        {
            battleUI ??= GetNode<CanvasLayer>(ConstTerm.BATTLE_UI);

            // commandBar ??= commandPanel.GetNode<MarginContainer>(ConstTerm.MARGIN_CONTAINER).GetNode<GridContainer>(ConstTerm.SELECT + ConstTerm.LIST).GetNode<ColorRect>(ConstTerm.COLOR_RECT);
            // skillBar ??= skillPanel.GetNode<MarginContainer>(ConstTerm.MARGIN_CONTAINER).GetNode<GridContainer>(ConstTerm.SELECT + ConstTerm.LIST).GetNode<ColorRect>(ConstTerm.COLOR_RECT);
            // itemBar ??= itemPanel.GetNode<MarginContainer>(ConstTerm.MARGIN_CONTAINER).GetNode<GridContainer>(ConstTerm.SELECT + ConstTerm.LIST).GetNode<ColorRect>(ConstTerm.COLOR_RECT);
        }

        private void SubSignals()
        {
            partyInput.onBattlePhase += OnBattlePhase;
            partyInput.onTurnCycle += OnTurnCycle;
            partyInput.onTurnEnd += OnTurnEnd;
            partyInput.onTargetChange += OnTargetChange;
            partyInput.onSelectCancel += OnSelectCancel;

            partyInput.onAbilitySelect += OnAbilitySelect;
            partyInput.onAbilityUse += OnAbilityUse;
            partyInput.onAbilityStop += OnAbilityStop;
            partyInput.onItemSelect += OnItemSelect;
            partyInput.onItemUse += OnItemUse;

            // partyInput.onSaveGame += OnSaveGame;
            // partyInput.onLoadGame += OnLoadGame;

            // partyInput.onBattleFinish += OnBattleFinish;
        }

        private void UnSubSignals()
        {
            partyInput.onBattlePhase -= OnBattlePhase;
            partyInput.onTurnCycle -= OnTurnCycle;
            partyInput.onTurnEnd -= OnTurnEnd;
            partyInput.onTargetChange -= OnTargetChange;
            partyInput.onSelectCancel -= OnSelectCancel;

            partyInput.onAbilitySelect -= OnAbilitySelect;
            partyInput.onAbilityUse -= OnAbilityUse;
            partyInput.onAbilityStop -= OnAbilityStop;
            partyInput.onItemSelect -= OnItemSelect;
            partyInput.onItemUse -= OnItemUse;
        }

        public void BuildPlayerTeam() // EDIT: Create cursor target for each member
        {
            playerTeam = new List<Battler>();
            Node teamNode = GetNode(ConstTerm.PLAYER + ConstTerm.PARTY);

            // CharacterBody2D[] tempTeam = GetPlayerTeam(playerParty);
            // List<CharacterBody2D> tempTeam = playerParty.GetActiveParty();

            for (int n = 0; n < playerParty.GetPartySize(); n++)
            {
                CharacterBody2D tempChar = playerParty.SpawnBattleMember(n);
                tempChar.Position = partyInput.GetPlayerPositions()[n];

                tempChar.GetNode<CollisionShape2D>(ConstTerm.COLLIDER2D).Position = new Vector2(0, 0);
                tempChar.GetNode<CollisionShape2D>(ConstTerm.COLLIDER2D).Scale = new Vector2(1.5f, 1.5f);

                // Node2D tempCursor = ResourceLoader.Load<PackedScene>(cursorBase.ResourcePath).Instantiate() as Node2D;
                // tempCursor.Position = tempChar.GetGlobalTransformWithCanvas().Origin - offset;
                // tempChar.AddChild(tempCursor);

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
                playerTeam[n].onBattlerLevelUp += OnBattlerLevelUp;
                int b = n;
                partyInput.GetChild(n).GetNode<Battler>(ConstTerm.BATTLER).onMouseTarget += (hover) => OnMouseBattlerTarget(hover, ConstTerm.PLAYER, b);
                // partyInput.GetChild(n).GetNode<Battler>(ConstTerm.BATTLER).onMouseClick += () => OnMouseBattlerClick(ConstTerm.PLAYER);

                // UI Bars \\
                Node tempLine = ResourceLoader.Load<PackedScene>(statusList.ResourcePath).Instantiate();
                playerList = battleUI.GetNode<VBoxContainer>(ConstTerm.PLAYER + ConstTerm.LIST);
                playerList.AddChild(tempLine);
                playerList.GetChild(n).GetNode<HealthDisplay>(ConstTerm.HEALTH_DISPLAY).SetBattler(playerTeam[n]);
                playerList.GetChild(n).GetNode<Label>(ConstTerm.NAME).Text = playerTeam[n].GetNameLabel().Text;
                playerTeam[n].GetNameLabel().Visible = false;
            }

            partyInput.SetPlayerTeamSize(playerTeam.Count);
        }

        private void InitialHealthBars()
        {
            for (int n = 0; n < playerList.GetChildCount(); n++)
            {
                playerList.GetChild(n).GetNode<HealthDisplay>(ConstTerm.HEALTH_DISPLAY).ForceHealthBarUpdate();
            }
        }

        private void BuildEnemyTeam() // EDIT: Create cursor target for each member
        {
            enemyList = battleUI.GetNode<VBoxContainer>(ConstTerm.ENEMY + ConstTerm.LIST);

            enemyTeam = [];
            Node encounterTeam = ResourceLoader.Load<PackedScene>(enemyEncounter.ResourcePath).Instantiate();
            AddChild(encounterTeam);
            BuildTeam(enemyList, enemyTeam, ConstTerm.ENEMY + ConstTerm.TEAM);

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
                int b = n;
                teamBattlers[n].onMouseTarget += (hover) => OnMouseBattlerTarget(hover, ConstTerm.ENEMY, b);
                // teamBattlers[n].onMouseClick += () => OnMouseBattlerClick(ConstTerm.ENEMY);

                string tempName = teamBattlers[n].GetNameLabel().Text;
                teamBattlers[n].GetNameLabel().Text = tempName;
                teamBattlers[n].GetNameLabel().Reparent(teamList);
                teamBattlers[n].GetNode<HealthDisplay>("../" + ConstTerm.HEALTH_DISPLAY).SetBattler(teamBattlers[n]);
            }
        }

        //=============================================================================
        // SECTION: Scene Handling
        //=============================================================================

        public async void ReturnMapScene()
        {
            SetBattleControlActive(false);
            Fader.Instance.FadeOut();
            await ToSignal(Fader.Instance.GetAnimPlayer(), ConstTerm.ANIM_FINISHED);

            // mapScene.Reparent(GetTree().Root);
            GD.Print("Add child - mapScene");
            GD.Print(mapScene);
            GetTree().Root.AddChild(mapScene); // Causes error - already has parent? Still functions properly, without indication of error.
            GetTree().Root.RemoveChild(this);

            Fader.Instance.FadeIn();
            await ToSignal(Fader.Instance.GetAnimPlayer(), ConstTerm.ANIM_FINISHED);
            playerParty.ChangePlayerActive(true);

            // GD.Print("Map enter tree - load data");
            SaveLoader.Instance.LoadBattlerData(SaveLoader.Instance.gameSession);
            // GD.Print(SaveLoader.Instance.gameSession.CharData[playerParty.GetPlayerParty()[0].GetCharID()].CurrentHP);
            // GD.Print(playerParty.GetPlayerParty()[0].GetHealth().GetHP());
        }

        public void SetBattleControlActive(bool active)
        {
            partyInput.SetControlActive(active);
        }

        public void SetEnemyGroup(PackedScene randomGroup)
        {
            enemyEncounter = randomGroup;
        }

        //=============================================================================
        // SECTION: Turn Handling
        //=============================================================================

        public void UIVisibility()
        {
            // cursorTarget.Visible = partyInput.GetTurnPhase() == ConstTerm.ATTACK || partyInput.GetTurnPhase() == ConstTerm.SKILL + ConstTerm.USE || partyInput.GetTurnPhase() == ConstTerm.ITEM + ConstTerm.USE;
            commandPanel.Visible = partyInput.GetTurnPhase() == ConstTerm.COMMAND;
            skillPanel.Visible   = partyInput.GetTurnPhase() == ConstTerm.SKILL + ConstTerm.SELECT;
            itemPanel.Visible    = partyInput.GetTurnPhase() == ConstTerm.ITEM + ConstTerm.SELECT;
        }

        public async void CheckBattlers()
        {
            if (battler.Count <= 0)
            {
                battler = [.. playerTeam, .. enemyTeam]; // AddRange simplification

                await Task.Delay(30); // Wait for battlers to establish in scene
                OnTurnCycle();
            }
        }

        public List<Battler> SortSpeed(List<Battler> battlers)
        {
            List<Battler> sortedBattlers = battlers.OrderByDescending(x => x.GetStats().GetStatValue(StatID.Agility)).ToList();
            return sortedBattlers;
        }

        public void MoveCommandPanel(Battler current)
        {
            Vector2 commandOrigin = current.GetCharBody().GetGlobalTransformWithCanvas().Origin;
            float xOffset = commandPanel.Size.X + (current.GetWidth() / 2);
            float yOffset = commandPanel.Size.Y - (current.GetHeight() / 2);
            commandPanel.Position = commandOrigin - new Vector2(xOffset, yOffset);
        }

        public void SetCursorPosition() // EDIT: Add cursor to each battler target. Toggle Visible if targetted - more effective for AoE
        {
            int target = partyInput.GetTarget();
            List<Battler> targetTeam = enemyTeam;
            string area = "";

            if (target >= targetTeam.Count || target < 0) { target = 0; }

            if (partyInput.GetTurnPhase() == ConstTerm.SKILL + ConstTerm.USE) {
                switch (activeAbility.TargetType)
                {
                    case ConstTerm.ALLY:
                        targetTeam = playerTeam;
                        break;
                    case ConstTerm.ENEMY:
                        targetTeam = enemyTeam;
                        break;
                    default:
                        GD.PushError("Invalid SKILL target type!");
                        break;
                }
                area = activeAbility.TargetArea;
            }
            else if (partyInput.GetTurnPhase() == ConstTerm.ITEM + ConstTerm.USE) {
                switch (activeItem.TargetType)
                {
                    case ConstTerm.ALLY:
                        targetTeam = playerTeam;
                        break;
                    case ConstTerm.ENEMY:
                        targetTeam = enemyTeam;
                        break;
                    default:
                        GD.PushError("Invalid ITEM target type!");
                        break;
                }
                area = activeItem.TargetArea;
            }
            else if (partyInput.GetTurnPhase() == ConstTerm.ATTACK) { area = ConstTerm.SINGLE; } // EDIT: Change target area to match attack parameters

            SetActiveCursors(targetTeam, target, area);

            // targetOffset = targetTeam[target].GetHeight() / 2;
            // offset = new Vector2(0, targetOffset + cursorOffset);

            // cursorTarget.Position = targetTeam[target].GetCharBody().GetGlobalTransformWithCanvas().Origin - offset;
            // GD.Print(target + " " + targetTeam[target].GetCharBody().GetGlobalTransformWithCanvas().Origin + " " + targetTeam[target].GetCharBody().Position);
        }

        private void SetActiveCursors(List<Battler> targetTeam, int target, string area)
        {
            ClearBattlerCursors(targetTeam, target, area);
            AnimationPlayer cursorAnim;

            if (area == ConstTerm.GROUP) {
                for (int t = 0; t < targetTeam.Count; t++) {
                    targetTeam[t].GetBattlerCursor().Visible = true;
                    cursorAnim = targetTeam[t].GetBattlerCursor().GetNode<Sprite2D>(ConstTerm.SPRITE2D).GetNode<AnimationPlayer>(ConstTerm.ANIM_PLAYER);
                    if (!cursorAnim.IsPlaying()) { cursorAnim.Play(ConstTerm.CURSOR_BOUNCE); }
                }
            } else {
                targetTeam[target].GetBattlerCursor().Visible = true;
                cursorAnim = targetTeam[target].GetBattlerCursor().GetNode<Sprite2D>(ConstTerm.SPRITE2D).GetNode<AnimationPlayer>(ConstTerm.ANIM_PLAYER);
                if (!cursorAnim.IsPlaying()) { cursorAnim.Play(ConstTerm.CURSOR_BOUNCE); }
            }
        }

        private void ClearBattlerCursors(List<Battler> team, int target, string area)
        {
            if (area == ConstTerm.GROUP) { return; }

            for (int b = 0; b < team.Count; b++)
            {
                if (b == target) { continue; }
                team[b].GetBattlerCursor().Visible = false;
                team[b].GetBattlerCursor().GetNode<Sprite2D>(ConstTerm.SPRITE2D).GetNode<AnimationPlayer>(ConstTerm.ANIM_PLAYER).Stop();
            }
        }

        private void ClearAllCursors()
        {
            for (int b = 0; b < battler.Count; b++)
            {
                battler[b].GetBattlerCursor().Visible = false;
                battler[b].GetBattlerCursor().GetNode<Sprite2D>(ConstTerm.SPRITE2D).GetNode<AnimationPlayer>(ConstTerm.ANIM_PLAYER).Stop();
            }
        }

        private void ReturnToPosition(Battler currBattler)
        {
            if (currBattler.GetBattlerType() == ConstTerm.PLAYER)   
            {
                Vector2 battlerPos = currBattler.GetSprite2D().Position;
                currBattler.GetAnimPlayer().GetAnimation(ConstTerm.ANIM_RETURN).TrackSetKeyValue(0, 0, new Vector2(battlerPos.X, battlerPos.Y));
                currBattler.GetAnimPlayer().Queue(ConstTerm.ANIM_RETURN);
            }
        }

        //=============================================================================
        // SECTION: Battle Methods
        //=============================================================================

        public void KillTarget(Battler target, List<Battler> team, List<Battler> attackTeam)
        {
            AwardKillExp(attackTeam, target);

            team.Remove(target);
            battler.Remove(target);
            target.GetParent().QueueFree();
        }

        public void AwardKillExp(List<Battler> team, Battler defender)
        {
            float getExp = defender.GetExperience().GetExpOnKill();
            getExp /= team.Count;
            for (int i = 0; i < team.Count; i++)
            {
                // GD.Print(team[i].GetBattlerName() + " gains " + getExp + " experience.");
                team[i].GetExperience().AddExp(getExp);
                // if (team[i].GetExperience().CheckLevelUp())
                // {
                //     FloatingNumbers(team[i], "Level Up!", ConstTerm.WHITE);
                //     GD.Print("New level is " + team[i].GetExperience().GetCurrentLevel());
                // }
                // GD.Print("Experience - " + team[i].GetExperience().GetTotalExp() + "/" + team[i].GetExperience().GetExpToLevel());
            }
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
                // GD.Print("You win!"); 
                BattleWin();
                return true;
            }
            if (playerTeam.Count <= 0)
            { 
                // GD.Print("You lose!");
                return true;
            }
            return false;
        }

        private async void BattleWin()
        {
            GD.Print("Battle end - save data");
            SaveLoader.Instance.GatherBattlers();
            // GD.Print(SaveLoader.Instance.gameSession.CharData[playerParty.GetPlayerParty()[0].GetCharID()].CurrentHP);
            // GD.Print(playerParty.GetPlayerParty()[0].GetHealth().GetHP());
            GD.Print(partyInput);
            partyInput.SetTurnPhase(ConstTerm.WAIT);
            partyInput.SetBattleOver(true);
            await ToSignal(partyInput, ConstTerm.BATTLE_FINISHED);
            ReturnMapScene();
        }

        public async void EnemyTurn()
        {
            // GD.Print(" - Enemy Go");
            turnActive = true;

            AnimationPlayer target = battler[0].GetAnimPlayer();
            target.Queue(ConstTerm.ANIM_ENEMY_ATTACK);
            await ToSignal(target, ConstTerm.ANIM_FINISHED);
            OnTurnEnd();
        }

        public void PlayerTurn()
        {
            SetupSkillList(battler[0]);
            // GD.Print(" - Player Go");
            partyInput.SetActiveMember(playerTeam.IndexOf(battler[0]));
            MoveCommandPanel(battler[0]);
            itemButton.Disabled = ItemBag.Instance.BagIsEmpty();

            turnActive = true;
            partyInput.PlayerTurnStart();
            // SetCursorPosition();
            // Wait for player/BattleController
        }

        //=============================================================================
        // SECTION: Menu UI
        //=============================================================================

        // private void SelectChange(ColorRect selectBar)
        // {
        //     ColorRect tempBar = selectBar;

        //     float index = partyInput.GetCommand();
        //     float numColumns = partyInput.GetNumColumn();
        //     float column = index % numColumns;
        //     float yPos = (float)(tempBar.Size.Y * Math.Ceiling(index / numColumns - column));

        //     tempBar.Position = new Vector2(column * tempBar.Size.X, yPos); // EDIT - Temporary!
        // }

        private void SetupCommandList()
        {
            for (int i = 0; i < partyInput.GetCommandOptions().Length; i++)
            {
                Label newCommand = (Label)ResourceLoader.Load<PackedScene>(labelSettings.ResourcePath).Instantiate();
                newCommand.Text = partyInput.GetCommandOptions()[i];
                if (newCommand.Text == ConstTerm.ITEM) { 
                    itemButton = newCommand.GetNode<Button>(ConstTerm.BUTTON);
                    itemButton.Disabled = ItemBag.Instance.BagIsEmpty();
                }

                commandPanel.GetNode(ConstTerm.COMMAND + ConstTerm.LIST).AddChild(newCommand);
            }
        }

        private void SetupSkillList(Battler player)
        {
            foreach(Node child in skillPanel.GetNode(ConstTerm.TEXT + ConstTerm.LIST).GetChildren())
            { child.Free(); }

            foreach (Ability skill in player.GetSkillList().GetSkills())
            {
                Label newSkill = (Label)ResourceLoader.Load<PackedScene>(labelSettings.ResourcePath).Instantiate();
                newSkill.Text = skill.AbilityName;
                newSkill.CustomMinimumSize = new Vector2(partyInput.GetSkillItemWidth(), 0);

                skillPanel.GetNode(ConstTerm.TEXT + ConstTerm.LIST).AddChild(newSkill);
            }

            partyInput.SubLists(partyInput.GetSkillList());
        }

        private void SetupItemList()
        {
            if (!CheckHasItems()) { return; };

            foreach(Node child in itemPanel.GetNode(ConstTerm.TEXT + ConstTerm.LIST).GetChildren())
            { child.QueueFree(); }

            foreach (Item item in ItemBag.Instance.GetItemBag())
            {
                Label newItem = (Label)ResourceLoader.Load<PackedScene>(labelSettings.ResourcePath).Instantiate();
                newItem.Text = item.ItemName;
                newItem.CustomMinimumSize = new Vector2(partyInput.GetSkillItemWidth(), 0);

                itemPanel.GetNode(ConstTerm.TEXT + ConstTerm.LIST).AddChild(newItem);
            }
        }

        private bool CheckHasItems()
        {
            int itemCommand = 0;
            for (int i = 0; i < partyInput.GetCommandOptions().Length; i++)
            {
                if (partyInput.GetCommandOptions()[i] == ConstTerm.ITEM) { itemCommand = i; break; }
            }

            if (ItemBag.Instance.BagIsEmpty())
            {
                commandPanel.GetNode<VBoxContainer>(ConstTerm.COMMAND + ConstTerm.LIST).GetChild<Label>(itemCommand).Modulate = new Color(ConstTerm.GREY);
                return false;
            } else {
                commandPanel.GetNode<VBoxContainer>(ConstTerm.COMMAND + ConstTerm.LIST).GetChild<Label>(itemCommand).Modulate = new Color(ConstTerm.WHITE);
                return true;
            }
        }

        private void ClearAbilityItem()
        {
            activeAbility = null;
            activeItem = null;
        }

        //=============================================================================
        // SECTION: Animation Call Methods
        //=============================================================================

        public void OnHitEnemy(Battler player) // Called from Battler
        {
            int currTarget = partyInput.GetTarget();
            CommandDone();
            if (activeAbility != null && activeAbility.TargetArea == ConstTerm.GROUP) {

                List<Battler> killTargets = [];
                for (int n = 0; n < enemyTeam.Count; n++)
                {
                    Battler defender = enemyTeam[n];

                    defender.GetAnimPlayer().Queue(ConstTerm.TAKE_DAMAGE);
                    DamageHandling(player, defender);

                    if (defender.GetHealth().GetHP() <= 0)
                    {
                        killTargets.Add(defender);
                        defender.onHitPlayer -= OnHitPlayer;

                        enemyList.GetChild(n).QueueFree();
                        // n--;
                    }
                }

                for (int k = 0; k < killTargets.Count; k++)
                {
                    KillTarget(killTargets[k], enemyTeam, playerTeam);
                    partyInput.SetEnemyTeamSize(enemyTeam.Count);
                }
            }
            else {
                Battler defender = enemyTeam[currTarget];

                defender.GetAnimPlayer().Queue(ConstTerm.TAKE_DAMAGE);
                DamageHandling(player, defender);

                if (defender.GetHealth().GetHP() <= 0)
                {
                    defender.onHitPlayer -= OnHitPlayer;
                    KillTarget(defender, enemyTeam, playerTeam);

                    enemyList.GetChild(currTarget).QueueFree();
                    partyInput.SetEnemyTeamSize(enemyTeam.Count);

                    // BattleEndCheck();
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
                defender.onAbilityHitPlayer -= OnAbilityHitPlayer;
                defender.onBattlerLevelUp -= OnBattlerLevelUp;
                KillTarget(defender, playerTeam, enemyTeam);

                playerList.GetChild(target).QueueFree();
                partyInput.SetPlayerTeamSize(playerTeam.Count);

                // BattleEndCheck();
            }
        }

        public void OnAbilityHitPlayer(Battler user) // EDIT: Add heal formula!
        {
            if (activeAbility != null)
            {
                if (activeAbility.TargetArea == ConstTerm.GROUP)
                {
                    foreach (Battler player in playerTeam)
                    {
                        Battler target = player;
                        float healing = activeAbility.NumericValue;
                        target.GetHealth().ChangeHP(healing);
                        FloatingNumbers(target, healing.ToString(), ConstTerm.GREEN);
                    }
                }
                else
                {
                    Battler target = playerTeam[partyInput.GetTarget()];
                    float healing = activeAbility.NumericValue;
                    target.GetHealth().ChangeHP(healing);
                    FloatingNumbers(target, healing.ToString(), ConstTerm.GREEN);
                }
            }
            else if (activeItem != null)
            {
                if (activeItem.TargetArea == ConstTerm.GROUP)
                {
                    foreach (Battler player in playerTeam)
                    {
                        Battler target = player;
                        float healing = activeItem.NumericValue;
                        target.GetHealth().ChangeHP(healing);
                        FloatingNumbers(target, healing.ToString(), ConstTerm.GREEN);
                    }
                }
                else
                {
                    Battler target = playerTeam[partyInput.GetTarget()];
                    float healing = activeItem.NumericValue;
                    target.GetHealth().ChangeHP(healing);
                    FloatingNumbers(target, healing.ToString(), ConstTerm.GREEN);
                }
            }
            
        }


        //=============================================================================
        // SECTION: Signal Methods
        //=============================================================================


        private void OnBattlePhase()
        {
            UIVisibility();
        }


        private async void OnTurnCycle()
        {
            if (BattleEndCheck()) { return; }
            if (battler.Count > 0)
            {
                if (battler[0] == null) { goto SkipToEnd; }
                // GD.Print(battler[0].GetNameLabel().Text + " Turn Start!");
                partyInput.SetTurnPhase(ConstTerm.WAIT);
                ClearAbilityItem();

                if (turnActive == false) { battler = SortSpeed(battler); }

                if (battler[0].GetBattlerType() == ConstTerm.ENEMY) 
                { EnemyTurn(); }
                else 
                { PlayerTurn(); }
            }
            SkipToEnd:
            CheckBattlers();

            await Task.Delay(120);
        }

        private void OnTurnEnd()
        {
            // GD.Print("End turn.");
            partyInput.SetPlayerTurn(false);
            partyInput.SetControlActive(true);
            turnActive = false;

            partyInput.ResetTarget();
            ReturnToPosition(battler[0]);
            battler.RemoveAt(0);

            partyInput.SetTargetTeam(ConstTerm.ENEMY);
            
            OnTurnCycle();
        }

        private void CommandDone()
        {
            partyInput.SetTurnPhase(ConstTerm.WAIT);
            partyInput.SetCommand(0);
            ClearAllCursors();

            // commandBar.Position = Vector2.Zero;
            // skillBar.Position = Vector2.Zero;
            // itemBar.Position = Vector2.Zero;
        }

        private void OnMouseBattlerTarget(bool hover, string team, int member)
        {
            if (team != partyInput.GetTargetTeam()) { return; }

            partyInput.OnBattlerSelect(hover, team, member);
        }

        // private void OnMouseBattlerClick(string team)
        // {
        //     if (team != partyInput.GetTargetTeam()) { return; }
        //     partyInput.OnBattlerClick();
        // }

        private void OnTargetChange()
        {
            if (enemyTeam.Count > 0)
            {
                switch (partyInput.GetTurnPhase())
                {
                    // case ConstTerm.COMMAND:
                    //     SelectChange(commandBar);
                    //     break;
                    // case ConstTerm.ITEM + ConstTerm.SELECT:
                    //     SelectChange(itemBar);
                    //     break;
                    // case ConstTerm.SKILL + ConstTerm.SELECT:
                    //     SelectChange(skillBar);
                    //     break;
                    case ConstTerm.ATTACK:
                    case ConstTerm.ITEM + ConstTerm.USE:
                    case ConstTerm.SKILL + ConstTerm.USE:
                        SetCursorPosition();
                        break;
                    default:
                        break;
                }
            }
        }

        private void OnSelectCancel()
        {
            ClearAllCursors();
            ClearAbilityItem();
        }

        private void OnAbilitySelect(int index)
        {
            activeAbility = battler[0].GetSkillList().GetSkills()[index];

            if (partyInput.IsPlayerTurn()) 
            {
                if (activeAbility.TargetType == ConstTerm.ALLY) { partyInput.SetTargetTeam(ConstTerm.PLAYER); }
                else { partyInput.SetTargetTeam(ConstTerm.ENEMY); }
                partyInput.ResetActionTarget();
            }

            // SetCursorPosition();
        }

        private async void OnAbilityUse(int index)
        {
            CommandDone();

            if (index == -1) { activeAbility = defendAbility; }
            // else {
            //     activeAbility = battler[0].GetSkillList().GetSkills()[index];
            // }

            if (activeAbility.AddedState != null) {
                if (!battler[0].GetStateList().Contains(activeAbility.AddedState)) {
                    battler[0].AddState(activeAbility.AddedState);
                }
                // GD.Print(" -- Activate - " + battler[0].GetStateList().Last().StateName);
            }

            battler[0].GetAnimPlayer().Queue(activeAbility.CallAnimation);
            await ToSignal(battler[0].GetAnimPlayer(), ConstTerm.ANIM_FINISHED);

            // partyInput.ResetTarget();

            OnTurnEnd();
        }

        private void OnAbilityStop(int index)
        {
            ClearAllCursors();

            Ability newAbility;
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

        private void OnItemSelect(int index)
        {
            activeItem = ItemBag.Instance.GetItemBag()[index];

            if (partyInput.IsPlayerTurn())
            {
                if (activeItem.TargetType == ConstTerm.ALLY) { partyInput.SetTargetTeam(ConstTerm.PLAYER); }
                else { partyInput.SetTargetTeam(ConstTerm.ENEMY); }
                partyInput.ResetActionTarget();
            }
            // SetCursorPosition();
        }

        private async void OnItemUse(int index)
        {
            CommandDone();

            battler[0].GetAnimPlayer().Queue(activeItem.CallAnimation);
            await ToSignal(battler[0].GetAnimPlayer(), ConstTerm.ANIM_FINISHED);
            ItemBag.Instance.RemoveItemFromBag(index);
            itemPanel.GetNode(ConstTerm.TEXT + ConstTerm.LIST).GetChild(index).QueueFree();

            // partyInput.ResetTarget();

            OnTurnEnd();
        }

        private void OnBattlerLevelUp(Battler battler)
        {
            FloatingNumbers(battler, "Level Up!", ConstTerm.WHITE);
            // GD.Print("New level is " + battler.GetExperience().GetCurrentLevel());
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
