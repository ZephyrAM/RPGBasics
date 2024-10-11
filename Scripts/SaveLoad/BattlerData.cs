using Godot;
using System;

public partial class BattlerData : Resource
{
    [Export] public CharacterID CharID { get; set; }
    [Export] public float CurrentHP { get; set;}
    [Export] public float MaxHP { get; set; }
    [Export] public float[] StatValues { get; set;}
}