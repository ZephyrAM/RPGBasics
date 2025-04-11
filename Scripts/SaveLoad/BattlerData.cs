using Godot;
using Godot.Collections;

public partial class BattlerData: Resource
{
    [Export] public CharacterID CharID { get; set; }
    [Export] public float CurrentHP { get; set; }
    [Export] public float CurrentMP { get; set;}
    [Export] public float[] StatValues { get; set; }
    [Export] public float CurrentExp { get; set; }
    [Export] public int CurrentLevel { get; set; }
    [Export] public Array<AbilityData> SkillList { get; set; } = [];
    [Export] public Dictionary<GearSlotID, int> EquipList { get; set; } = [];
}