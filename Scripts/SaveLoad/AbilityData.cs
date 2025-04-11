using Godot;
using System;

public partial class AbilityData : Resource
{
    [ExportGroup("Details")]
    [Export] public string AbilityName { get; set; }
    [Export] public string AbilityDescription { get; set; }

    [Export(PropertyHint.Enum, "Ally,Enemy,Self")] public string TargetType { get; set; } // Ally, Enemy, or Self
    [Export(PropertyHint.Enum, "Single,Group")] public string TargetArea { get; set; } // Single or Group

    [Export] public float NumericValue { get; set; }
    [Export] public float CostValue { get; set; }

    [ExportGroup("Mechanics")]
    [Export] public string DamageType { get; set; }
    [Export] public string CallAnimation { get; set; }

    [Export] public EffectStateData AddedState { get; set; }

    [ExportGroup("Restrictions")]
    [Export] public bool UseableInBattle { get; set; }
    [Export] public bool UseableOutOfBattle { get; set; }
    [Export] public bool UseableOnDead { get; set; }

    [Export] public int UniqueID { get; set; }
}
