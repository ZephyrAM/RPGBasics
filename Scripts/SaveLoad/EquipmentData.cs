using Godot;
using Godot.Collections;
using System;

public partial class EquipmentData : ItemData
{
    [ExportGroup("Details")]
    [Export] public Array<GearSlotID> GearSlot { get;  set; } = []; // Gear slot it can be equipped in.
    [Export] public bool isEquipped = false;

    [ExportGroup("Stats")]
    [Export] public Array<Modifier> AddModifier { get;  set; } = [];
    [Export] public Array<Modifier> PercentModifier { get;  set; } = [];

    [ExportGroup("Restrictions")]
    [Export] public Array<ClassID> ClassEquip { get;  set; } = []; // Classes that can equip it.
    [Export] public int UniqueEquip { get;  set; } // If 0, not unique. Else, number of equips allowed.
}
