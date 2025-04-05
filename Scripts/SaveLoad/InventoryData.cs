using Godot;
using Godot.Collections;

using ZAM.Inventory;

public partial class InventoryData : Resource
{
    [Export] public Array<Item> ItemBag { get; set; } = [];
    [Export] public Array<Equipment> EquipBag { get; set; } = [];
}
