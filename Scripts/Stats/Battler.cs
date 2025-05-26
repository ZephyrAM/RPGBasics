using Godot;
using Godot.Collections;

using ZAM.Abilities;
using ZAM.Inventory;

namespace ZAM.Stats
{
    public partial class Battler : Node
    {
        [Export(PropertyHint.Enum, "Player,Enemy,NPC")] private string battlerType;
        [Export] private CharacterID battlerID;
        [Export] private ClassID battlerClass;
        // [Export] private bool hasBattleStats = true; // Implement if needed

        [ExportGroup("Nodes")]
        [Export] private Label battlerName = null;
        [Export] private string battlerTitle = "";
        [Export] private CharacterBody2D battlerBody = null;
        [Export] private Sprite2D battlerSprite = null;
        [Export] private Texture2D battlerPortrait = null;
        [Export] private AnimationPlayer battlerAnim = null;

        [Export] private Node2D battlerCursor = null;

        [Export] private Health battlerHealth = null;
        [Export] private BaseStats battlerStats = null;
        [Export] private Experience battlerExp = null;
        [Export] private SkillList battlerSkills = null;
        [Export] private EquipList battlerEquipment = null;

        private AnimationNodeStateMachinePlayback animState = null;
        private Array<EffectState> states = [];
        private Vector2 basePosition;

        public bool IsDead { get; set; } = false;

        private Dictionary<string, Callable> mouseSignal = [];

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
            basePosition = new Vector2(GetSprite2D().Position.X, GetSprite2D().Position.Y);
            if (battlerType != ConstTerm.NPC) { SetupHPMP(); }

            SubSignals();
        }

        protected override void Dispose(bool disposing)
        {
            UnSubSignals();
            base.Dispose(disposing);
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

            animState = (AnimationNodeStateMachinePlayback)battlerAnim.Get(ConstTerm.PARAM + ConstTerm.PLAYBACK);

            // Check if following nodes exist. Not required for some (ex. NPC's)
            battlerCursor ??= battlerBody.GetNodeOrNull<Node2D>(ConstTerm.CURSOR + ConstTerm.TARGET);

            battlerHealth ??= GetNodeOrNull<Health>(ConstTerm.HEALTH);
            battlerStats ??= GetNodeOrNull<BaseStats>(ConstTerm.BASESTATS);
            battlerExp ??= GetNodeOrNull<Experience>(ConstTerm.EXPERIENCE);
            battlerSkills ??= GetNodeOrNull<SkillList>(ConstTerm.SKILL + ConstTerm.LIST);
            battlerEquipment ??= GetNodeOrNull<EquipList>(ConstTerm.EQUIP + ConstTerm.LIST);            
        }

        private void SubSignals()
        {
            battlerExp.onLevelUp += OnLevelUp;
            mouseSignal[ConstTerm.MOUSE + ConstTerm.ENTERED] = Callable.From(() => OnMouseOver(true));
            mouseSignal[ConstTerm.MOUSE + ConstTerm.EXITED] = Callable.From(() => OnMouseOver(false));
            battlerBody.Connect(CharacterBody2D.SignalName.MouseEntered, mouseSignal[ConstTerm.MOUSE + ConstTerm.ENTERED]);
            battlerBody.Connect(CharacterBody2D.SignalName.MouseExited, mouseSignal[ConstTerm.MOUSE + ConstTerm.EXITED]);
            // battlerBody.MouseEntered += () => OnMouseOver(true);
            // battlerBody.MouseExited += () => OnMouseOver(false);
            // battlerBody.Connect(CharacterBody2D.SignalName.MouseEntered, new Callable(this, MethodName.OnMouseOver));
        }

        private void UnSubSignals()
        {
            battlerExp.onLevelUp -= OnLevelUp;
            battlerBody.Disconnect(CharacterBody2D.SignalName.MouseEntered, mouseSignal[ConstTerm.MOUSE + ConstTerm.ENTERED]);
            battlerBody.Disconnect(CharacterBody2D.SignalName.MouseExited, mouseSignal[ConstTerm.MOUSE + ConstTerm.EXITED]);
            mouseSignal.Remove(ConstTerm.MOUSE + ConstTerm.ENTERED);
            mouseSignal.Remove(ConstTerm.MOUSE + ConstTerm.EXITED);
            // battlerBody.MouseEntered -= () => OnMouseOver(true);
            // battlerBody.MouseExited -= () => OnMouseOver(false);
        }

        private void SetupHPMP()
        {
            battlerHealth.SetMaxHP(battlerStats.GetMaxHP());
            battlerHealth.SetMaxMP(battlerStats.GetMaxMP());

            battlerHealth.SetHP(battlerHealth.GetMaxHP());
            battlerHealth.SetMP(battlerHealth.GetMaxMP());
        }

        public void SetDefaultAnim()
        {
            GetAnimPlayer().Set(ConstTerm.PARAM + ConstTerm.IDLE + ConstTerm.BLEND, new Vector2(-1, 0));
            GetAnimState().Travel(ConstTerm.IDLE);
        }


        //=============================================================================
        // SECTION: Access Methods
        //=============================================================================

        public bool CheckCanUse(Ability skill, bool isInBattle)
        {
            bool battleState;
            if (isInBattle) { battleState = skill.UseableInBattle; } else { battleState = skill.UseableOutOfBattle;}
            bool checkDeath = !GetHealth().IsDead();
            bool haveResource = GetHealth().HasEnoughMP(skill.CostValue);

            bool result = battleState && checkDeath && haveResource;
            return result;
        }

        public bool CheckCanTarget(Battler target, Ability skill, Item item)
        {
            bool useableOnDead = false;
            if (skill != null) { useableOnDead = skill.UseableOnDead; }
            if (item != null) { useableOnDead = item.UseableOnDead; }
            bool checkDeath = !GetHealth().IsDead() || useableOnDead;
            bool needsHealth = !target.GetHealth().IsHPFull();

            bool result = checkDeath && needsHealth;
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
        { return battlerType; }

        public CharacterID GetCharID()
        { return battlerID; }

        public ClassID GetCharClass()
        { return battlerClass; }

        public string GetBattlerName()
        { return battlerName.Text; }

        public string GetBattlerTitle()
        { return battlerTitle; }

        public Node2D GetBattlerCursor()
        { return battlerCursor; }

        public Label GetNameLabel()
        { return battlerName; }

        public CharacterBody2D GetCharBody()
        { return battlerBody; }

        public Sprite2D GetSprite2D()
        { return battlerSprite; }

        public Texture2D GetPortrait()
        { return battlerPortrait; }

        public float GetWidth()
        { return battlerSprite.Texture.GetWidth() / battlerSprite.Hframes; }

        public float GetHeight()
        { return battlerSprite.Texture.GetHeight() / battlerSprite.Vframes; }

        public AnimationPlayer GetAnimPlayer()
        { return battlerAnim; }

        public AnimationNodeStateMachinePlayback GetAnimState()
        { return animState; }

        public Health GetHealth()
        { return battlerHealth; }

        public BaseStats GetStats()
        { return battlerStats; }

        public Experience GetExperience()
        { return battlerExp; }

        public SkillList GetSkillList()
        { return battlerSkills; }

        public EquipList GetEquipList()
        { return battlerEquipment; }

        public Array<EffectState> GetStateList()
        { return states; }

        public Vector2 GetBasePosition()
        { return basePosition; }

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

        // public void OnSaveGame(SavedGame saveData)
        // {
        //     GD.Print("Save " + GetCharID());
        //     if (GetCharID() == 0) { return; }

        //     BattlerData newData = new()
        //     {
        //         CurrentHP = GetHealth().GetHP(),
        //         CurrentMP = GetHealth().GetMP(),
        //         StatValues = GetStats().SaveAllStats(),
        //         CurrentExp = GetExperience().GetTotalExp(),
        //         CurrentLevel = GetExperience().GetCurrentLevel(),
        //         SkillList = GetSkillList().StoreSkills(),
        //         EquipList = GetEquipList().StoreEquipList()
        //     };
        //     // GD.Print("Battler " + GetHealth().GetHP());
        //     // GD.Print("Save - " + " " + GetCharID() + " battler HP at = " + saveData[GetCharID()].CurrentHP + "/" + newData.CurrentHP);
        //     saveData.CharData[GetCharID()] = newData;
        //     // GD.Print(saveData[GetCharID()].CurrentHP);
        // }

        // public void OnLoadGame(Dictionary<CharacterID, BattlerData> loadData)
        // {
        //     // GD.Print(this);
        //     // GD.Print("Load " + GetCharID());
        //     // GD.Print(loadData[GetCharID()].CurrentHP);
        //     BattlerData saveData = loadData[GetCharID()];
        //     if (saveData == null) { GD.Print("Battler data - NULL"); return; }

        //     // GD.Print("Load - " + " " + GetCharID() + " battler HP at = " + saveData.CurrentHP + "/" + GetHealth().GetHP());

        //     GetHealth().SetHP(saveData.CurrentHP);
        //     GetHealth().SetMP(saveData.CurrentMP);
        //     GetStats().LoadAllStats(saveData.StatValues);
        //     GetExperience().SetExpTotal(saveData.CurrentExp);
        //     GetExperience().SetCurrentLevel(saveData.CurrentLevel);
        //     GetSkillList().SetSkills(saveData.SkillList);
        //     GetEquipList().SetEquipList(saveData.EquipList);

        //     // GD.Print(GetHealth().GetHP());
        // }

        public void OnSaveFile(ConfigFile saveData)
        {
            string sectionID = ConstTerm.BATTLER + battlerID + ConstTerm.DATA;

            saveData.SetValue(sectionID, ConstTerm.CURRENT + ConstTerm.HP, GetHealth().GetHP());
            saveData.SetValue(sectionID, ConstTerm.CURRENT + ConstTerm.MP, GetHealth().GetMP());
            saveData.SetValue(sectionID, ConstTerm.STAT_VALUES, GetStats().GetStatSheet());
            // GetStats().SaveAllStats(saveData, sectionID);

            saveData.SetValue(sectionID, ConstTerm.CURRENT + ConstTerm.EXPERIENCE, GetExperience().GetTotalExp());
            saveData.SetValue(sectionID, ConstTerm.CURRENT + ConstTerm.LEVEL, GetExperience().GetCurrentLevel());

            GetSkillList().StoreSkills(saveData, sectionID);
            saveData.SetValue(sectionID, ConstTerm.EQUIP + ConstTerm.LIST, GetEquipList().StoreEquipList());
        }

        public void OnLoadFile(ConfigFile loadData)
        {
            if (loadData == null) { return; }
            string sectionID = ConstTerm.BATTLER + battlerID + ConstTerm.DATA;

            if (loadData.HasSection(sectionID)) {
                GetHealth().SetHP((float)loadData.GetValue(sectionID, ConstTerm.CURRENT + ConstTerm.HP));
                GetHealth().SetMP((float)loadData.GetValue(sectionID, ConstTerm.CURRENT + ConstTerm.MP));
                GetStats().SetStatSheet((Dictionary<StatID, float>)loadData.GetValue(sectionID, ConstTerm.STAT_VALUES));
                // GetStats().LoadAllStats(loadData, sectionID);

                GetExperience().SetExpTotal((float)loadData.GetValue(sectionID, ConstTerm.CURRENT + ConstTerm.EXPERIENCE));
                GetExperience().SetCurrentLevel((int)loadData.GetValue(sectionID, ConstTerm.CURRENT + ConstTerm.LEVEL));

                GetSkillList().SetSkills(loadData, sectionID);
                GetEquipList().SetEquipList((Dictionary<GearSlotID, ulong>)loadData.GetValue(sectionID, ConstTerm.EQUIP + ConstTerm.LIST));
            }
        }
    }
}