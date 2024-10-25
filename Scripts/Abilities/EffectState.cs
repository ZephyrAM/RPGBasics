using System;
using Godot;

namespace ZAM.Abilities
{
    [GlobalClass]
    public partial class EffectState : Node
    {
        [Export] public string StateName { get; set; }
        [Export] public string StateDescription { get; set; }
        
        [Export] public Modifier[] AddModifier { get; set; }
        [Export] public Modifier[] PercentModifier { get; set; }
    }
}