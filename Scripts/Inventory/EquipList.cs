using System;
using Godot;
using Godot.Collections;

using ZAM.Managers;

namespace ZAM.Inventory
{
    public partial class EquipList : Node
    {
        [Export] private int numberAccSlots = 2;

        [ExportGroup("DefaultGear")]
        [Export] private string defaultMainHand = "";
        [Export] private string defaultOffHand = "";
        [Export] private string[] defaultArmor = [];
        [Export] private string[] defaultAccessories = [];

        private Array<EquipSlot> characterEquipment = [];
        private Dictionary<string, Equipment> weaponDictionary = [];
        private Dictionary<string, Equipment> armorDictionary = [];
        private Dictionary<string, Equipment> accessoryDictionary = [];

        public override void _Ready()
        {
            IfNull();
            SetupEquipList();
            // GD.Print(GetCharEquipment().Count);
            // for (int i = 0; i < GetCharEquipment().Count; i++) {
            //     if (GetCharEquipment()[i].Equip == null) { GD.Print("Empty"); continue; }
            //     GD.Print(GetCharEquipment()[i].Equip.ItemName);
            // }
        }

        private void IfNull()
        {
            weaponDictionary = DatabaseManager.Instance.GetWeaponDatabase();
            armorDictionary = DatabaseManager.Instance.GetArmorDatabase();
            accessoryDictionary = DatabaseManager.Instance.GetAccessoryDatabase();
        }

        private void SetupEquipList()
        {
            if (characterEquipment.Count != 0) { return; }
            for (int e = 0; e < Enum.GetNames(typeof(GearSlotID)).Length - 1; e++) {
                characterEquipment.Add(new EquipSlot((GearSlotID)e, null));
            }
            for (int a = 0; a < numberAccSlots; a++) {
                characterEquipment.Add(new EquipSlot(GearSlotID.Accessory, null));
            }
            // SetDefaultEquipList();
        }

        public void SetDefaultEquipList()
        {
            if (defaultMainHand != null && defaultMainHand != "") {
                if (weaponDictionary[defaultMainHand].GearSlot[0] == GearSlotID.UNDEFINED) { GD.PushError(defaultMainHand + " GearSlot undefined!"); }
                // EquipGear((int)GearSlotID.MainHand, weaponDictionary[defaultMainHand]);

                ItemBag.Instance.AddToBag(defaultMainHand, weaponDictionary[defaultMainHand].ItemType);
            }

            if (defaultOffHand != null && defaultOffHand != "") {
                if (weaponDictionary[defaultOffHand].GearSlot.Contains(GearSlotID.OffHand)) {
                    if (weaponDictionary[defaultOffHand].GearSlot[0] == GearSlotID.UNDEFINED) { GD.PushError(defaultOffHand + " GearSlot undefined!"); }
                    // EquipGear((int)GearSlotID.OffHand, weaponDictionary[defaultOffHand]);

                    ItemBag.Instance.AddToBag(defaultOffHand, weaponDictionary[defaultOffHand].ItemType);
            } else { GD.PushWarning("Default offhand not OffHand equippable"); }
            }
            
            if (defaultArmor.Length > 0) { 
                int nextSlot;
                for (int a = 0; a < defaultArmor.Length; a++) {
                    if (defaultArmor[a] == null || defaultArmor[a] == "") { continue; }
                    nextSlot = (int)armorDictionary[defaultArmor[a]].GearSlot[0];
                    if (nextSlot == 0) { GD.PushError(defaultArmor[a] + " GearSlot undefined!"); }
                    if (characterEquipment[nextSlot].Equip != null) { GD.PushWarning("Multiple default armor set to same slot. Overwriting previous."); }
                    // EquipGear(nextSlot, armorDictionary[defaultArmor[a]]);

                    ItemBag.Instance.AddToBag(defaultArmor[a], armorDictionary[defaultArmor[a]].ItemType);
                }
            }

            if (defaultAccessories.Length > 0) {
                int nextSlot;
                for (int a = 0; a < defaultAccessories.Length; a++) {
                    if (a > numberAccSlots) { GD.PushWarning("Too many default accessories."); return; }
                    if (defaultAccessories[a] == null || defaultAccessories[a] == "") { continue; }
                    nextSlot = (int)accessoryDictionary[defaultAccessories[a]].GearSlot[0];
                    if (nextSlot == 0) { GD.PushError(defaultAccessories[a] + " GearSlot undefined!"); }
                    // EquipGear(nextSlot + a, accessoryDictionary[defaultAccessories[a]]);

                    ItemBag.Instance.AddToBag(defaultAccessories[a], accessoryDictionary[defaultAccessories[a]].ItemType);
                }
            }
        }

        public void EquipGear(int slotId, Equipment gear)
        {
            // if (!gear.GearSlot.Contains(characterEquipment[slotId].Slot)) { GD.PushWarning("Attempt to equip item to invalid slot!"); return; }
            if (characterEquipment[slotId].Equip != null) { RemoveGear(slotId); }
            if (gear == null) { RemoveGear(slotId); return; }

            characterEquipment[slotId].Equip = gear;
            characterEquipment[slotId].Equip.SetIsEquipped(true);
            // ItemBag.Instance.RemoveItemFromBag(gear);
        }

        public void RemoveGear(int slotId)
        {
            if (characterEquipment[slotId].Equip == null) { return; }

            characterEquipment[slotId].Equip.SetIsEquipped(false);
            // ItemBag.Instance.AddToEquipBag(characterEquipment[slotId].Equip);
            characterEquipment[slotId].Equip = null;
        }

        public void SetCharEquipment(Array<EquipSlot> gearList)
        {
            characterEquipment = gearList;
        }

        public Array<EquipSlot> GetCharEquipment()
        {
            return characterEquipment;
        }
    }
}