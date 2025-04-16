using Godot;
using System;

public partial class InteractData : Resource
{
    [Export] public bool DataIsEvent { get; set; }
    [Export] public bool DataIsInteractable { get; set; }
    [Export] public int NoRepeatStep { get; set; }
}
