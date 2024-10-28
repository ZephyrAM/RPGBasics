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

        [ExportGroup("Nodes")]
        [Export] private Label battlerName;
        [Export] private CharacterBody2D battlerBody;
        [Export] private Sprite2D battlerSprite;
        [Export] private AnimationPlayer battlerAnim;
        [Export] private Health battlerHealth;
        [Export] private BaseStats battlerStats;
        [Export] private Experience battlerExp;
        [Export] private SkillList battlerSkills;

        [ExportGroup("Variables")]
        [Export] private string labelName;
        [Export] private string sprite2DName;
        [Export] private string animPlayerName;

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

        //=============================================================================
        // SECTION: OnReady Methods
        //=============================================================================
        private void IfNull()
        {
            sprite2DName ??= ConstTerm.SPRITE2D;
            animPlayerName ??= ConstTerm.ANIM_PLAYER;
            labelName ??= ConstTerm.NAMELABEL;

            battlerBody ??= GetNode<CharacterBody2D>("../" + ConstTerm.CHARBODY2D);
            battlerSprite ??= GetNode<Sprite2D>("../" + sprite2DName);
            battlerAnim ??= GetNode<AnimationPlayer>("../" + animPlayerName);

            battlerName ??= GetNode<Label>("../" + labelName);
            battlerHealth ??= GetNode<Health>("../" + ConstTerm.HEALTH);
            battlerStats ??= GetNode<BaseStats>("../" + ConstTerm.BASESTATS);
            battlerExp ??= GetNode<Experience>("../" + ConstTerm.EXPERIENCE);
            battlerSkills ??= GetNode<SkillList>("../" + ConstTerm.SKILL_LIST);
        }

        private void SubSignals()
        {
            battlerExp.onLevelUp += OnLevelUp;
        }
        
        private void UnSubSignals()
        {
            battlerExp.onLevelUp -= OnLevelUp;
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

        public string BattlerName()
        {
            return battlerName.Text;
        }

        public Label GetNameLabel()
        { return battlerName; }

        public CharacterBody2D GetCharBody()
        { return battlerBody; }

        public Sprite2D GetSprite2D()
        { return battlerSprite; }

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
            battlerStats.LevelUpStats();
        }

        //=============================================================================
        // SECTION: Save System
        //=============================================================================

        // GDScript BattlerData = GD.Load<GDScript>("res://Scripts/SaveLoad/battler_data.gd");

        public void OnSaveGame(Godot.Collections.Dictionary<CharacterID, BattlerData> saveData)
        {
            // if (GetCharID() == 0) { return; }

            BattlerData newData = new()
            {
                CharID = GetCharID(),
                CurrentHP = GetHealth().GetHP(),
                StatValues = GetStats().SaveAllStats(),
                CurrentExp = GetExperience().GetTotalExp(),
                CurrentLevel = GetExperience().GetCurrentLevel(),
                SkillList = GetSkillList().GetSkills()
            };
            saveData[GetCharID()] = newData;
        }

        public void OnLoadData(BattlerData saveData)
        {
            if (saveData is BattlerData)
            {
                if (GetCharID() != saveData.CharID) { GD.PushError("Battler loaded does not match!"); } // EDIT: Need better check

                GetHealth().SetHP(saveData.CurrentHP);
                GetStats().LoadAllStats(saveData.StatValues);
                GetExperience().SetExpTotal(saveData.CurrentExp);
                GetExperience().SetCurrentLevel(saveData.CurrentLevel);
                GetSkillList().SetSkills(saveData.SkillList);
            }
        }
    }
}