using Godot;
using Godot.Collections;
using System;

public partial class EffectStateData : Resource
{
    [ExportGroup("Details")]
    [Export] public string StateName { get; set; }
    [Export] public string StateDescription { get; set; }

    [ExportGroup("Mechanics")]
    [Export] public Array<Modifier> AddModifier { get; set; } = [];
    [Export] public Array<Modifier> PercentModifier { get; set; } = [];

    [ExportGroup("Restrictions")]
    [Export] public bool ExistsOutOfBattle { get; set; }

    public int UniqueID { get; set; } = 0;
}
