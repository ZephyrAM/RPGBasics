using Godot;

namespace ZAM.Abilities
{
    public partial class Ability : Node
    {
        [ExportGroup("Details")]
        [Export] public string AbilityName { get; private set; }
        [Export] public string AbilityDescription { get; private set; }

        [Export] public string TargetType { get; private set; } // Ally or Enemy
        [Export] public string TargetArea { get; private set; } // Single or Group

        [Export] public float NumericValue { get; private set; }
        [Export] public float CostValue { get; private set; }

        [ExportGroup("Mechanics")]
        [Export] public string DamageType { get; private set; }
        [Export] public string CallAnimation { get; private set; }

        [Export] public EffectState AddedState { get; private set; }

        [ExportGroup("Restrictions")]
        [Export] public bool UseableOutOfBattle { get; private set; }
        [Export] public bool UseableOnDead { get; private set; }

        public int UniqueID { get; private set; } = 0;

        public void SetUniqueID(ref int id)
        { 
            if (UniqueID != 0) { GD.PushWarning("Attempting to re-declare UniqueID for " + AbilityName); return; }
            UniqueID = id;
            id++;
        }
    }
}