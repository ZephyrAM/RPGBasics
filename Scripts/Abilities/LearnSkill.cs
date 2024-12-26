using Godot;

namespace ZAM.Abilities
{
    [GlobalClass]
    public partial class LearnSkill : Resource
    {
        [Export] public string SkillName { get; set; }
        [Export] public int LearnLevel { get; set; }
    }
}