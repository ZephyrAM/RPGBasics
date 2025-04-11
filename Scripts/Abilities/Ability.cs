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

        public int UniqueID { get; private set; } = 0;

        public void SetUniqueID(ref int id)
        { 
            if (UniqueID != 0) { GD.PushWarning("Attempting to re-declare UniqueID for " + AbilityName); return; }
            UniqueID = id;
            id++;
        }

        public void CreateUniqueInstance(ref int id)
        {
            UniqueID = id;
            id++;
        }

        //=============================================================================
        // SECTION: Save System
        //=============================================================================

        public void SetDetails(string name, string description, string type, string area, float value, float cost, int id)
        {
            AbilityName = name;
            AbilityDescription = description;
            TargetType = type;
            TargetArea = area;
            NumericValue = value;
            CostValue = cost;

            UniqueID = id;
        }

        public void SetMechanics(string type, string animation)
        {
            DamageType = type;
            CallAnimation = animation;
        }

        public void SetRestrictions(bool inBattle, bool outBattle, bool onDead)
        {
            UseableInBattle = inBattle;
            UseableOutOfBattle = outBattle;
            UseableOnDead = onDead;
        }

        public void SetDataDetails(ref AbilityData data)
        {
            data.AbilityName = AbilityName;
            data.AbilityDescription = AbilityDescription;
            data.TargetType = TargetType;
            data.TargetArea = TargetArea;
            data.NumericValue = NumericValue;
            data.CostValue = CostValue;

            data.UniqueID = UniqueID;
        }

        public void SetDataMechanics(ref AbilityData data)
        {
            data.DamageType = DamageType;
            data.CallAnimation = CallAnimation;
        }

        public void SetDataRestrictions(ref AbilityData data)
        {
            data.UseableInBattle = UseableInBattle;
            data.UseableOutOfBattle = UseableOutOfBattle;
            data.UseableOnDead = UseableOnDead;
        }
    }
}