using Godot;

using System.Collections.Generic;

using ZAM.Abilities;

public partial class BattlerData: Resource
{
    public CharacterID CharID { get; set; }
    public float CurrentHP { get; set; }
    public float[] StatValues { get; set; }
    public float CurrentExp { get; set; }
    public int CurrentLevel { get; set; }
    public List<Ability> SkillList { get; set; }
}