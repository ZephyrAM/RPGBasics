using Godot;
using System;

public partial class ItemData : Resource
{
    [ExportGroup("Details")]
    [Export] public ItemType ItemType { get; set; }
    [Export] public string ItemName { get; set; }
    [Export] public string ItemDescription { get; set; }

    [Export(PropertyHint.Enum, "Ally,Enemy,Self")] public string TargetType { get; set; } // Ally, Enemy, or Self
    [Export(PropertyHint.Enum, "Single,Group")] public string TargetArea { get; set; } // Single or Group

    [Export] public float NumericValue { get; set; }

    [ExportGroup("Mechanics")]
    [Export] public string DamageType { get; set; } // Physical, Magical, more to be added
    [Export] public string CallAnimation { get; set; }

    [Export] public EffectStateData AddedState { get; set; } = new();

    [ExportGroup("Restrictions")]
    [Export] public bool UseableInBattle { get; set; }
    [Export] public bool UseableOutOfBattle { get; set; }
    [Export] public bool UseableOnDead { get; set; }
    [Export] public bool CanStack { get; set; }
    [Export] public bool IsConsumable { get; set; } = true;

    [Export] public ulong UniqueID { get; set; }
    [Export] public int Amount { get; set; }
}
