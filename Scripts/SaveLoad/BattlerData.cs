using Godot;
using Godot.Collections;

using ZAM.Abilities;

public partial class BattlerData: Resource
{
    [Export] public CharacterID CharID { get; set; }
    [Export] public float CurrentHP { get; set; }
    [Export] public float[] StatValues { get; set; }
    [Export] public float CurrentExp { get; set; }
    [Export] public int CurrentLevel { get; set; }
    [Export] public Array<Ability> SkillList { get; set; }
}