using Godot;
using Godot.Collections;

public partial class PartyData : Resource
{
    [Export] public Array<PackedScene> PartyMembers { get; set; } = [];
    [Export] public Array<PackedScene> ReserveMembers { get; set; } = [];
    [Export] public Vector2 MapPosition { get; set; }
    [Export] public Vector2 FaceDirection { get; set; }
    [Export] public int TotalGold { get; set; }
    // [Export] public int TotalSteps { get; set; }
}
