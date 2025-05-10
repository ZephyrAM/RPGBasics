using Godot;

namespace ZAM.Inventory
{
    public partial class EquipSlot : Resource
    {
        public GearSlotID Slot { get; private set; }
        public Equipment Equip { get; set; }

        public EquipSlot() {}

        public EquipSlot(GearSlotID slot) { Slot = slot; }

        public EquipSlot(GearSlotID slot, Equipment equip) { Slot = slot; Equip = equip; }
    }
}