using Godot;
using Godot.Collections;
using System;

public partial class SavedGame : Resource
{
    // [Export] public Array<BattlerData> battlerData { get; set; }
    [Export] public Dictionary<CharacterID, BattlerData> CharData { get; set; } = [];
}
