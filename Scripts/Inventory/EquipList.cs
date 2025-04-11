using System;
using Godot;
using Godot.Collections;

using ZAM.Managers;

namespace ZAM.Inventory
{
    public partial class EquipList : Node
    {
        [ExportGroup("DefaultGear")]
        [Export] private string defaultMainHand = "";
        [Export] private string defaultOffHand = "";
        [Export] private string[] defaultArmor = [];
        [Export] private string[] defaultAccessories = [];

        private Dictionary<GearSlotID, Equipment> characterEquipment = [];
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
            characterEquipment = [];
            foreach (GearSlotID slotId in Enum.GetValues(typeof(GearSlotID))) {
                characterEquipment[slotId] = null;
            }
            // for (int e = 0; e < Enum.GetNames(typeof(GearSlotID)).Length; e++) {
            //     characterEquipment[(GearSlotID)e] = null;
            //     GD.Print((GearSlotID)e);
            // }
            // SetDefaultEquipList();
        }

        public void SetDefaultEquipList()
        {
            if (defaultMainHand != null && defaultMainHand != "") {
                if (weaponDictionary[defaultMainHand].GearSlot[0] == GearSlotID.UNDEFINED) { GD.PushError(defaultMainHand + " GearSlot undefined!"); }
                // EquipGear((int)GearSlotID.MainHand, weaponDictionary[defaultMainHand]);

                ItemBag.Instance.AddToBag(defaultMainHand, weaponDictionary[defaultMainHand].ItemType, 1);
            }

            if (defaultOffHand != null && defaultOffHand != "") {
                if (weaponDictionary[defaultOffHand].GearSlot.Contains(GearSlotID.OffHand)) {
                    if (weaponDictionary[defaultOffHand].GearSlot[0] == GearSlotID.UNDEFINED) { GD.PushError(defaultOffHand + " GearSlot undefined!"); }
                    // EquipGear((int)GearSlotID.OffHand, weaponDictionary[defaultOffHand]);

                    ItemBag.Instance.AddToBag(defaultOffHand, weaponDictionary[defaultOffHand].ItemType, 1);
            } else { GD.PushWarning("Default offhand not OffHand equippable"); }
            }
            
            if (defaultArmor.Length > 0) { 
                int nextSlot;
                for (int a = 0; a < defaultArmor.Length; a++) {
                    if (defaultArmor[a] == null || defaultArmor[a] == "") { continue; }
                    nextSlot = (int)armorDictionary[defaultArmor[a]].GearSlot[0];
                    if (nextSlot == 0) { GD.PushError(defaultArmor[a] + " GearSlot undefined!"); }
                    if (characterEquipment[(GearSlotID)nextSlot] != null) { GD.PushWarning("Multiple default armor set to same slot. Overwriting previous."); }
                    // EquipGear(nextSlot, armorDictionary[defaultArmor[a]]);

                    ItemBag.Instance.AddToBag(defaultArmor[a], armorDictionary[defaultArmor[a]].ItemType, 1);
                }
            }

            if (defaultAccessories.Length > 0) {
                int nextSlot;
                for (int a = 0; a < defaultAccessories.Length; a++) {
                    if (defaultAccessories[a] == null || defaultAccessories[a] == "") { continue; }
                    nextSlot = (int)accessoryDictionary[defaultAccessories[a]].GearSlot[0];
                    if (nextSlot == 0) { GD.PushError(defaultAccessories[a] + " GearSlot undefined!"); }
                    // EquipGear(nextSlot + a, accessoryDictionary[defaultAccessories[a]]);

                    ItemBag.Instance.AddToBag(defaultAccessories[a], accessoryDictionary[defaultAccessories[a]].ItemType, 1);
                }
            }
        }

        public void EquipGear(int slotId, Equipment gear)
        {
            // if (!gear.GearSlot.Contains(characterEquipment[slotId].Slot)) { GD.PushWarning("Attempt to equip item to invalid slot!"); return; }
            if (characterEquipment[(GearSlotID)slotId] != null) { RemoveGear(slotId); }
            if (gear == null) { RemoveGear(slotId); return; }

            characterEquipment[(GearSlotID)slotId] = gear;
            characterEquipment[(GearSlotID)slotId].SetIsEquipped(true);
            // ItemBag.Instance.RemoveItemFromBag(gear);
        }

        public void RemoveGear(int slotId)
        {
            if (characterEquipment[(GearSlotID)slotId] == null) { return; }

            characterEquipment[(GearSlotID)slotId].SetIsEquipped(false);
            // ItemBag.Instance.AddToEquipBag(characterEquipment[slotId].Equip);
            characterEquipment[(GearSlotID)slotId] = null;
        }

        public void SetCharEquipment(Dictionary<GearSlotID, Equipment> gearList)
        {
            characterEquipment = gearList;
        }

        public Dictionary<GearSlotID, Equipment> GetCharEquipment()
        {
            return characterEquipment;
        }


        //=============================================================================
        // SECTION: Save System
        //=============================================================================

        public Dictionary<GearSlotID, int> StoreEquipList()
        {
            Dictionary<GearSlotID, int> newList = [];

            foreach (GearSlotID data in characterEquipment.Keys) {
                int bagPos = ItemBag.Instance.GetEquipBag().IndexOf(characterEquipment[data]);
                newList[data] = bagPos;
            }

            return newList;
        }

        public void SetEquipList(Dictionary<GearSlotID, int> list)
        {
            SetupEquipList();
            foreach(GearSlotID data in list.Keys) {
                int bagPos = list[data];
                Equipment bagEquip;

                if (bagPos >= 0) { bagEquip = ItemBag.Instance.GetEquipBag()[bagPos]; }
                else { bagEquip = null; }

                if (bagPos >= 0) { characterEquipment[data] = bagEquip; }
            }
        }
    }
}