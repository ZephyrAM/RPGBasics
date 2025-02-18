using Godot;
using System;
using System.Collections.Generic;

using ZAM.Abilities;

namespace ZAM.Stats
{
    public partial class Battler : Node
    {
        [Export] private string battlerType;
        [Export] private CharacterID battlerID;
        [Export] private ClassID battlerClass;
        // [Export] private bool hasBattleStats = true; // Implement if needed

        [ExportGroup("Nodes")]
        [Export] private Label battlerName;
        [Export] private string battlerTitle;
        [Export] private CharacterBody2D battlerBody;
        [Export] private Sprite2D battlerSprite;
        [Export] private Texture2D battlerPortrait;
        [Export] private AnimationPlayer battlerAnim;
        [Export] private Node2D battlerCursor;

        [Export] private Health battlerHealth;
        [Export] private BaseStats battlerStats;
        [Export] private Experience battlerExp;
        [Export] private SkillList battlerSkills;

        private List<EffectState> states;
        private Vector2 basePosition;

        public bool IsDead { get; set; } = false;

        // Delegate Events \\
        [Signal]
        public delegate void onHitEnemyEventHandler(Battler user);
        [Signal]
        public delegate void onHitPlayerEventHandler(Battler user);
        [Signal]
        public delegate void onAbilityHitEnemyEventHandler(Battler user);
        [Signal]
        public delegate void onAbilityHitPlayerEventHandler(Battler user);
        [Signal]
        public delegate void onBattlerLevelUpEventHandler(Battler battler);
        [Signal]
        public delegate void onMouseTargetEventHandler(bool hover);
        // [Signal]
        // public delegate void onMouseClickEventHandler();

        //=============================================================================
        // SECTION: Base Methods
        //=============================================================================
        public override void _Ready()
        {
            IfNull();
            states = [];
            basePosition = new Vector2(GetSprite2D().Position.X, GetSprite2D().Position.Y);

            SubSignals();

            // battlerHealth.SetMaxHP(battlerStats.GetStatValue(Stat.Stamina) * 10); // EDIT: Find better place to load.
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            UnSubSignals();
        }

        //=============================================================================
        // SECTION: OnReady Methods
        //=============================================================================
        private void IfNull()
        {
            battlerBody ??= GetParent<CharacterBody2D>();
            battlerSprite ??= battlerBody.GetNode<Sprite2D>(ConstTerm.SPRITE2D);
            battlerAnim ??= battlerBody.GetNode<AnimationPlayer>(ConstTerm.ANIM_PLAYER);
            battlerName ??= battlerBody.GetNode<Label>(ConstTerm.NAME);

            if (battlerType != ConstTerm.NPC) { battlerCursor ??= battlerBody.GetNode<Node2D>(ConstTerm.CURSOR + ConstTerm.TARGET); }

            battlerHealth ??= GetNode<Health>(ConstTerm.HEALTH);
            battlerStats ??= GetNode<BaseStats>(ConstTerm.BASESTATS);
            battlerExp ??= GetNode<Experience>(ConstTerm.EXPERIENCE);
            battlerSkills ??= GetNode<SkillList>(ConstTerm.SKILL + ConstTerm.LIST);
            
        }

        private void SubSignals()
        {
            battlerExp.onLevelUp += OnLevelUp;
            battlerBody.MouseEntered += () => OnMouseOver(true);
            battlerBody.MouseExited += () => OnMouseOver(false);
            // battlerBody.Connect(CharacterBody2D.SignalName.MouseEntered, new Callable(this, MethodName.OnMouseOver));
        }

        private void UnSubSignals()
        {
            battlerExp.onLevelUp -= OnLevelUp;
            battlerBody.MouseEntered -= () => OnMouseOver(true);
            battlerBody.MouseExited -= () => OnMouseOver(false);
        }


        //=============================================================================
        // SECTION: Set Methods
        //=============================================================================


        public bool CheckCanUse(Ability skill)
        {
            bool outOfBattle = skill.UseableOutOfBattle;
            bool checkDeath = !GetHealth().IsDead() || (GetHealth().IsDead() && skill.UseableOnDead); // EDIT: Death check should be on TARGET, not CASTER

            bool result = outOfBattle && checkDeath;
            return result;
        }


        //=============================================================================
        // SECTION: Set Methods
        //=============================================================================
        public void AddState(EffectState state)
        {
            states.Add(state);
        }


        //=============================================================================
        // SECTION: Get Methods
        //=============================================================================

        public string GetBattlerType()
        {
            return battlerType;
        }

        public CharacterID GetCharID()
        {
            return battlerID;
        }

        public ClassID GetCharClass()
        {
            return battlerClass;
        }

        public string GetBattlerName()
        {
            return battlerName.Text;
        }

        public string GetBattlerTitle()
        {
            return battlerTitle;
        }

        public Node2D GetBattlerCursor()
        {
            return battlerCursor;
        }

        public Label GetNameLabel()
        { return battlerName; }

        public CharacterBody2D GetCharBody()
        { return battlerBody; }

        public Sprite2D GetSprite2D()
        { return battlerSprite; }

        public Texture2D GetPortrait()
        {
            return battlerPortrait;
        }

        public float GetWidth()
        {
            return battlerSprite.Texture.GetWidth() / battlerSprite.Hframes;
        }

        public float GetHeight()
        {
            return battlerSprite.Texture.GetHeight() / battlerSprite.Vframes;
        }

        public AnimationPlayer GetAnimPlayer()
        { return battlerAnim; }

        public Health GetHealth()
        { return battlerHealth; }

        public BaseStats GetStats()
        { return battlerStats; }

        public Experience GetExperience()
        { return battlerExp; }

        public SkillList GetSkillList()
        {
            return battlerSkills;
        }

        public List<EffectState> GetStateList()
        {
            return states;
        }

        public Vector2 GetBasePosition()
        {
            return basePosition;
        }

        //=============================================================================
        // SECTION: Animation Call Methods
        //=============================================================================

        public void HitEnemy()
        {
            EmitSignal(SignalName.onHitEnemy, this);
        }

        public void HitPlayer()
        {
            EmitSignal(SignalName.onHitPlayer, this);
        }

        public void AbilityHitEnemy()
        {
            EmitSignal(SignalName.onAbilityHitEnemy, this);
        }

        public void AbilityHitPlayer()
        {
            EmitSignal(SignalName.onAbilityHitPlayer, this);
        }

        //=============================================================================
        // SECTION: Signal Methods
        //=============================================================================

        public void OnLevelUp()
        {
            EmitSignal(SignalName.onBattlerLevelUp, this);
            battlerStats.LevelUpStats();
        }

        public void OnMouseOver(bool hover)
        {
            EmitSignal(SignalName.onMouseTarget, hover);
        }

        // public void OnMouseClick(Node viewport, InputEvent @event, long shapeIdx)
        // {
        //     if (@event.IsActionPressed(ConstTerm.ACCEPT + ConstTerm.CLICK)) { EmitSignal(SignalName.onMouseClick); }
        // }

        //=============================================================================
        // SECTION: Save System
        //=============================================================================

        // GDScript BattlerData = GD.Load<GDScript>("res://Scripts/SaveLoad/battler_data.gd");

        public void OnSaveGame(SavedGame saveData)
        {
            // GD.Print("Save " + GetCharID());
            // if (GetCharID() == 0) { return; }

            BattlerData newData = new()
            {
                CurrentHP = GetHealth().GetHP(),
                StatValues = GetStats().SaveAllStats(),
                CurrentExp = GetExperience().GetTotalExp(),
                CurrentLevel = GetExperience().GetCurrentLevel(),
                SkillList = GetSkillList().GetSkills()
            };
            // GD.Print("Battler " + GetHealth().GetHP());
            // GD.Print("Save - " + " " + GetCharID() + " battler HP at = " + saveData[GetCharID()].CurrentHP + "/" + newData.CurrentHP);
            saveData.CharData[GetCharID()] = newData;
            // GD.Print(saveData[GetCharID()].CurrentHP);
        }

        public void OnLoadGame(Godot.Collections.Dictionary<CharacterID, BattlerData> loadData)
        {
            // GD.Print(this);
            // GD.Print("Load " + GetCharID());
            // GD.Print(loadData[GetCharID()].CurrentHP);
            BattlerData saveData = loadData[GetCharID()];
            if (saveData == null) { GD.Print("Battler data - NULL"); return; }

            // GD.Print("Load - " + " " + GetCharID() + " battler HP at = " + saveData.CurrentHP + "/" + GetHealth().GetHP());

            GetHealth().SetHP(saveData.CurrentHP);
            GetStats().LoadAllStats(saveData.StatValues);
            GetExperience().SetExpTotal(saveData.CurrentExp);
            GetExperience().SetCurrentLevel(saveData.CurrentLevel);
            GetSkillList().SetSkills(saveData.SkillList);

            // GD.Print(GetHealth().GetHP());
        }
    }
}