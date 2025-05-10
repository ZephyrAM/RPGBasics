using Godot;

namespace ZAM.Abilities
{
    public partial class Ability : Node
    {
        [ExportGroup("Details")]
        [Export] public string AbilityName { get; private set; }
        [Export] public string AbilityDescription { get; private set; }

        [Export(PropertyHint.Enum, "Ally,Enemy,Self")] public string TargetType { get; private set; } // Ally, Enemy, or Self
        [Export(PropertyHint.Enum, "Single,Group")] public string TargetArea { get; private set; } // Single or Group

        [Export] public float NumericValue { get; private set; }
        [Export] public float CostValue { get; private set; }

        [ExportGroup("Mechanics")]
        [Export] public string DamageType { get; private set; }
        [Export] public string CallAnimation { get; private set; }

        [Export] public EffectState AddedState { get; private set; }

        [ExportGroup("Restrictions")]
        [Export] public bool UseableInBattle { get; private set; }
        [Export] public bool UseableOutOfBattle { get; private set; }
        [Export] public bool UseableOnDead { get; private set; }

        public ulong UniqueID { get; private set; } = 0;
        public bool IsUnique { get; private set; } = false;

        public void SetUniqueID(ref ulong id)
        { 
            if (UniqueID != 0) { GD.PushWarning("Attempting to re-declare UniqueID for " + AbilityName); return; }
            UniqueID = id;
            id++;
        }

        public void CreateUniqueInstance(ref ulong id)
        {
            UniqueID = id;
            id++;
        }

        public void SetIsUnique(bool value)
        {
            IsUnique = value;
        }

        public void SetAddedState(EffectState state)
        {
            AddedState = state;
        }

        //=============================================================================
        // SECTION: Save System
        //=============================================================================

        // public void SetDetails(string name, string description, string type, string area, float value, float cost, ulong id)
        // {
        //     AbilityName = name;
        //     AbilityDescription = description;
        //     TargetType = type;
        //     TargetArea = area;
        //     NumericValue = value;
        //     CostValue = cost;

        //     UniqueID = id;
        // }

        // public void SetMechanics(string type, string animation)
        // {
        //     DamageType = type;
        //     CallAnimation = animation;
        // }

        // public void SetRestrictions(bool inBattle, bool outBattle, bool onDead)
        // {
        //     UseableInBattle = inBattle;
        //     UseableOutOfBattle = outBattle;
        //     UseableOnDead = onDead;
        // }

        // public void StoreDataDetails(ref AbilityData data)
        // {
        //     data.AbilityName = AbilityName;
        //     data.AbilityDescription = AbilityDescription;
        //     data.TargetType = TargetType;
        //     data.TargetArea = TargetArea;
        //     data.NumericValue = NumericValue;
        //     data.CostValue = CostValue;

        //     data.UniqueID = UniqueID;

        //     AddedState.StoreDataDetails(data.AddedState);
        // }

        // public void StoreDataMechanics(ref AbilityData data)
        // {
        //     data.DamageType = DamageType;
        //     data.CallAnimation = CallAnimation;

        //     AddedState.StoreDataMechanics(data.AddedState);
        // }

        // public void StoreDataRestrictions(ref AbilityData data)
        // {
        //     data.UseableInBattle = UseableInBattle;
        //     data.UseableOutOfBattle = UseableOutOfBattle;
        //     data.UseableOnDead = UseableOnDead;

        //     AddedState.StoreDataRestrictions(data.AddedState);
        // }

        public void StoreDetails(ConfigFile saveData, string battlerID, string extraID)
        {
            saveData.SetValue(battlerID, extraID + ConstTerm.ABILITY + ConstTerm.NAME, AbilityName);
            saveData.SetValue(battlerID, extraID + ConstTerm.ABILITY + ConstTerm.DESCRIPTION, AbilityDescription);
            saveData.SetValue(battlerID, extraID + ConstTerm.TARGET + ConstTerm.TYPE, TargetType);
            saveData.SetValue(battlerID, extraID + ConstTerm.TARGET + ConstTerm.AREA, TargetArea);
            saveData.SetValue(battlerID, extraID + ConstTerm.NUMERIC + ConstTerm.VALUE, NumericValue);
            saveData.SetValue(battlerID, extraID + ConstTerm.COST + ConstTerm.VALUE, CostValue);

            saveData.SetValue(battlerID, extraID + ConstTerm.UNIQUE + ConstTerm.ID, UniqueID);

            // AddedState?.StoreDetails(saveData, battlerID, extraID);
        }

        public void StoreMechanics(ConfigFile saveData, string battlerID, string extraID)
        {
            saveData.SetValue(battlerID, extraID + ConstTerm.DAMAGE + ConstTerm.TYPE, DamageType);
            saveData.SetValue(battlerID, extraID + ConstTerm.CALL_ANIM, CallAnimation);

            // AddedState?.StoreMechanics(saveData, battlerID, extraID);
        }
        
        public void StoreRestrictions(ConfigFile saveData, string battlerID, string extraID)
        {
            saveData.SetValue(battlerID, extraID + ConstTerm.USEABLE + ConstTerm.IN + ConstTerm.BATTLE, UseableInBattle);
            saveData.SetValue(battlerID, extraID + ConstTerm.USEABLE + ConstTerm.OUT_OF + ConstTerm.BATTLE, UseableOutOfBattle);
            saveData.SetValue(battlerID, extraID + ConstTerm.USEABLE + ConstTerm.DEAD, UseableOnDead);

            // AddedState?.StoreRestrictions(saveData, battlerID, extraID);
        }

        public void SetDetails(ConfigFile loadData, string battlerID, string extraID)
        {
            AbilityName = (string)loadData.GetValue(battlerID, extraID + ConstTerm.ABILITY + ConstTerm.NAME);
            AbilityDescription = (string)loadData.GetValue(battlerID, extraID + ConstTerm.ABILITY + ConstTerm.DESCRIPTION);
            TargetType = (string)loadData.GetValue(battlerID, extraID + ConstTerm.TARGET + ConstTerm.TYPE);
            TargetArea = (string)loadData.GetValue(battlerID, extraID + ConstTerm.TARGET + ConstTerm.AREA);
            NumericValue = (int)loadData.GetValue(battlerID, extraID + ConstTerm.NUMERIC + ConstTerm.VALUE);
            CostValue = (int)loadData.GetValue(battlerID, extraID + ConstTerm.COST + ConstTerm.VALUE);

            UniqueID = (ulong)loadData.GetValue(battlerID, extraID + ConstTerm.UNIQUE + ConstTerm.ID);

            // AddedState?.SetDetails(loadData, battlerID, extraID);
        }

        public void SetMechanics(ConfigFile loadData, string battlerID, string extraID)
        {
            DamageType = (string)loadData.GetValue(battlerID, extraID + ConstTerm.DAMAGE + ConstTerm.TYPE);
            CallAnimation = (string)loadData.GetValue(battlerID, extraID + ConstTerm.CALL_ANIM);

            // AddedState?.SetMechanics(loadData, battlerID, extraID);
        }

        public void SetRestrictions(ConfigFile loadData, string battlerID, string extraID)
        {
            UseableInBattle = (bool)loadData.GetValue(battlerID, extraID + ConstTerm.USEABLE + ConstTerm.IN + ConstTerm.BATTLE);
            UseableOutOfBattle = (bool)loadData.GetValue(battlerID, extraID + ConstTerm.USEABLE + ConstTerm.OUT_OF + ConstTerm.BATTLE);
            UseableOnDead = (bool)loadData.GetValue(battlerID, extraID + ConstTerm.USEABLE + ConstTerm.DEAD);

            // AddedState?.SetRestrictions(loadData, battlerID, extraID);
        }
    }
}