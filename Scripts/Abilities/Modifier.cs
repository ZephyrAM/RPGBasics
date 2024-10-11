using Godot;
using Godot.Collections;

namespace ZAM.Abilities
{
    [GlobalClass]
    public partial class Modifier : Resource
    {
        [Export] public Stat Stat { get; set; }
        [Export] public float Value { get; set; }
    }
}