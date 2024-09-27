using Godot;
using System.Collections.Generic;

using ZAM.Abilities;

namespace ZAM.Stats
{
    public partial class Battler : Node
    {
        [Export] private string battlerType;

        [ExportGroup("Nodes")]
        [Export] private Label battlerName;
        [Export] private CharacterBody2D battlerBody;
        [Export] private Sprite2D battlerSprite;
        [Export] private AnimationPlayer battlerAnim;
        // [Export] HealthDisplay battlerHealthBar;
        [Export] private Health battlerHealth;
        [Export] private BaseStats battlerStats;
        [Export] private SkillList battlerSkills;

        [ExportGroup("Variables")]
        [Export] private string labelName;
        [Export] private string bodyName;
        [Export] private string sprite2DName;
        [Export] private string animPlayerName;
        // [Export] private string healthBarName;
        [Export] private string healthName;
        [Export] private string statsName;
        [Export] private string skillsName;

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
            states = new();
            basePosition = new Vector2(GetSprite2D().Position.X, GetSprite2D().Position.Y);
        }

        //=============================================================================
        // SECTION: OnReady Methods
        //=============================================================================
        private void IfNull()
        {
            bodyName ??= ConstTerm.CHARBODY2D;
            sprite2DName ??= ConstTerm.SPRITE2D;
            animPlayerName ??= ConstTerm.ANIM_PLAYER;

            labelName ??= ConstTerm.NAMELABEL;
            healthName ??= ConstTerm.HEALTH;
            statsName ??= ConstTerm.BASESTATS;
            skillsName ??= ConstTerm.SKILL_LIST;

            battlerBody ??= GetNode<CharacterBody2D>("../" + bodyName);
            battlerSprite ??= GetNode<Sprite2D>("../" + sprite2DName);
            battlerAnim ??= GetNode<AnimationPlayer>("../" + animPlayerName);

            battlerName ??= GetNode<Label>("../" + labelName);
            battlerHealth ??= GetNode<Health>("../" + healthName);
            battlerStats ??= GetNode<BaseStats>("../" + statsName);
            // battlerSkills ??= GetNode<SkillList>("../" + skillsName);
        }

        // public IEnumerable<float> GetAdditiveModifiers(Stat stat)
        // {
        //     foreach(var state in states)
        //     {
        //         foreach (var modifier in state.AddModifier)
        //         {

        //         }
        //     }
        // }

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

        // public HealthDisplay GetHealthBar()
        // { return battlerHealthBar; }

        public Health GetHealth()
        { return battlerHealth; }

        public BaseStats GetStats()
        { return battlerStats; }

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
            // GetTree().Root.GetNode<BattleSystem>("BattleSystem").DamageEnemy(this);
        }

        public void HitPlayer()
        {
            EmitSignal(SignalName.onHitPlayer, this);
            // GetTree().Root.GetNode<BattleSystem>("BattleSystem").DamagePlayer(this);
        }

        public void AbilityHitEnemy()
        {
            EmitSignal(SignalName.onAbilityHitEnemy, this);
        }

        public void AbilityHitPlayer()
        {
            EmitSignal(SignalName.onAbilityHitPlayer, this);
        }
    }
}