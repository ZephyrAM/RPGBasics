using Godot;

namespace ZAM.Abilities
{
    public partial class Modifier : Resource
    {
        [Export] public Stat Stat { get; set; }
        [Export] public float Value { get; set; }
    }
}