using Godot;
using System;

namespace ZAM.Abilities
{
    public partial class Ability : Node
    {
        [Export] public string AbilityName { get; set; }
        [Export] public string AbilityDescription { get; set; }

        [Export] public string TargetType { get; set; } // Ally or Enemy
        [Export] public string TargetArea { get; set; } // Single or Group

        [Export] public float NumericValue { get; set; }
        [Export] public string DamageType { get; set; }
        [Export] public string CallAnimation { get; set; }

        [Export] public EffectState AddedState { get; set; }
    }
}