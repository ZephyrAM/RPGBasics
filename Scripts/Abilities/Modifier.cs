using Godot;

[GlobalClass]
public partial class Modifier : Resource
{
    [Export] public StatID Stat { get; private set; }
    [Export] public float Value { get; private set; }
}
