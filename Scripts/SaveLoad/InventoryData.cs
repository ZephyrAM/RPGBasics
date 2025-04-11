using Godot;
using Godot.Collections;

public partial class InventoryData : Resource
{
    [Export] public Array<ItemData> ItemBag { get; set; } = [];
    [Export] public Array<EquipmentData> EquipBag { get; set; } = [];
}
