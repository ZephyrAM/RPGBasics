using Godot;

namespace ZAM.Abilities
{
    public partial class Ability : Node
    {
        [ExportGroup("Details")]
        [Export] public string AbilityName { get; set; }
        [Export] public string AbilityDescription { get; set; }

        [Export] public string TargetType { get; set; } // Ally or Enemy
        [Export] public string TargetArea { get; set; } // Single or Group

        [Export] public float NumericValue { get; set; }

        [ExportGroup("Mechanics")]
        [Export] public string DamageType { get; set; }
        [Export] public string CallAnimation { get; set; }

        [Export] public EffectState AddedState { get; set; }

        [ExportGroup("Restrictions")]
        [Export] public bool UseableOutOfBattle { get; set; }
        [Export] public bool UseableOnDead { get; set; }
    }
}