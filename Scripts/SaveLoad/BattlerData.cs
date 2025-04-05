using Godot;
using Godot.Collections;

using ZAM.Abilities;
using ZAM.Inventory;

public partial class BattlerData: Resource
{
    [Export] public CharacterID CharID { get; set; }
    [Export] public float CurrentHP { get; set; }
    [Export] public float CurrentMP { get; set;}
    [Export] public float[] StatValues { get; set; }
    [Export] public float CurrentExp { get; set; }
    [Export] public int CurrentLevel { get; set; }
    [Export] public Array<Ability> SkillList { get; set; } = [];
    [Export] public Array<EquipSlot> EquipList { get; set; } = [];
}