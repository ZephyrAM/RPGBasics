using Godot;

namespace ZAM.Abilities
{
    [GlobalClass]
    public partial class Modifier : Resource
    {
        [Export] public StatID Stat { get; set; }
        [Export] public float Value { get; set; }
    }
}