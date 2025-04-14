using System.Linq;
using Godot;
using Godot.Collections;

using ZAM.Managers;

namespace ZAM.Inventory
{
    // Global Object \\
    public partial class ItemBag : Node
    {
        private Dictionary<Item, int> itemBag = [];
        private Array<Equipment> equipBag = [];

        private Dictionary<string, Item> itemDatabase = [];
        private Dictionary<string, Equipment> weaponDatabase = [];
        private Dictionary<string, Equipment> armorDatabase = [];
        private Dictionary<string, Equipment> accessoryDatabase = [];

        public static ItemBag Instance { get; private set;}

        //=============================================================================
        // SECTION: Basic Methods
        //=============================================================================

        public override void _Ready()
        {
            Instance = this;
            IfNull();
        }
        
        private void IfNull()
        {
            itemDatabase = DatabaseManager.Instance.GetItemDatabase();
            weaponDatabase = DatabaseManager.Instance.GetWeaponDatabase();
            armorDatabase = DatabaseManager.Instance.GetArmorDatabase();
            accessoryDatabase = DatabaseManager.Instance.GetAccessoryDatabase();
        }

        //=============================================================================
        // SECTION: List Methods
        //=============================================================================

        public void AddToBag(string newItem, ItemType type, int count) // For adding newly gained/created items to bags.
        {
            Item addItem;

            for (int i = 0; i < count; i++) 
            {
                switch (type)
                {
                    case ItemType.Item:
                        // addItem = itemDatabase[newItem].Duplicate() as Item;
                        AddToItemBag(itemDatabase[newItem], 1);
                        break;
                    case ItemType.Weapon:
                        addItem = weaponDatabase[newItem].Duplicate() as Equipment;
                        addItem.SetUniqueID(ref DatabaseManager.Instance.GetUniqueCounter());
                        AddToEquipBag((Equipment)addItem);
                        break;
                    case ItemType.Armor:
                        addItem = armorDatabase[newItem].Duplicate() as Equipment;
                        addItem.SetUniqueID(ref DatabaseManager.Instance.GetUniqueCounter());
                        AddToEquipBag((Equipment)addItem);
                        break;
                    case ItemType.Accessory:
                        addItem = accessoryDatabase[newItem].Duplicate() as Equipment;
                        addItem.SetUniqueID(ref DatabaseManager.Instance.GetUniqueCounter());
                        AddToEquipBag((Equipment)addItem);
                        break;
                    default:
                        GD.PushError("Invalid ItemType in AddToBag");
                        return;
                }
            }
        }

        public void AddToItemBag(Item newItem, int count) // For shifting existing items between bags. Possibly over engineered.
        {
            if (newItem.CanStack) {
                if (itemBag.TryGetValue(newItem, out int value)) { itemBag[newItem] = value + count; } 
                else { 
                    for (int i = 0; i < count; i++) {
                        itemBag.Add(newItem, 1);
                    } 
                }
            } else {
                Item addItem = newItem.Duplicate() as Item;
                addItem.SetUniqueID(ref DatabaseManager.Instance.GetUniqueCounter());
                itemBag.Add(addItem, 1);
            }
        }

        public void AddToEquipBag(Equipment newEquip)
        {
            equipBag.Add(newEquip);
        }

        public void RemoveItemFromBag(Item instance)
        {
            // Item removeItem = itemBag[index];
            if (itemBag[instance] > 1) { itemBag[instance]--; } 
            else { itemBag.Remove(instance); }
        }

        public void RemoveEquipFromBag(Equipment instance)
        {
            equipBag.Remove(instance);
        }

        public Item GetItemFromBag(ulong id)
        {
            Item getItem = null;
            foreach (Item item in itemBag.Keys) {
                if (item.UniqueID == id) { getItem = item; break; }
            }

            if (getItem == null) { GD.PushError("ItemSearch not found"!); }
            return getItem;
        }

        public Dictionary<Item, int> GetItemBag()
        {
            return itemBag;
        }

        public Array<Equipment> GetEquipBag()
        {
            return equipBag;
        }

        public int FullItemCount()
        {
            return itemBag.Count + equipBag.Count;
        }

        public void SortItemBag()
        {
            itemBag = (Dictionary<Item, int>)itemBag.OrderBy(i => i.Key.ItemName);
        }

        public void SortEquipBag() // EDIT: Make sure this doesn't disrupt UniqueID's
        {
            Array<Equipment> tempMainHand = [];
            Array<Equipment> tempOffHand = [];
            Array<Equipment> tempHelmet = [];
            Array<Equipment> tempChest = [];
            Array<Equipment> tempAccessory = [];

            foreach(Equipment equip in equipBag) {
                switch (equip.GearSlot[0]) {
                    case GearSlotID.MainHand:
                        tempMainHand.Add(equip);
                        break;
                    case GearSlotID.OffHand:
                        tempOffHand.Add(equip);
                        break;
                    case GearSlotID.Head:
                        tempHelmet.Add(equip);
                        break;
                    case GearSlotID.Chest:
                        tempChest.Add(equip);
                        break;
                    case GearSlotID.Accessory1:
                        tempAccessory.Add(equip);
                        break;
                    case GearSlotID.Accessory2:
                        tempAccessory.Add(equip);
                        break;
                    default:
                        GD.PushWarning(equip.ItemName + " has invalid GearSlot in [0]");
                        break;
                }
            }

            tempMainHand.Sort(); tempOffHand.Sort(); tempHelmet.Sort(); tempChest.Sort(); tempAccessory.Sort(); // EDIT: Allow sorting by power, etc.
            equipBag = [];
            equipBag = tempMainHand + tempOffHand + tempHelmet + tempChest + tempAccessory;
            // tempMainHand = []; tempOffHand = []; tempHelmet = []; tempChest = []; tempAccessory = [];
        }

        public bool BagIsEmpty()
        {
            return itemBag.Count <= 0 && equipBag.Count <= 0;
        }

        //=============================================================================
        // SECTION: Display Contents
        //=============================================================================

        public Array<Equipment> GetSlotContents(GearSlotID slot, ClassID charClass)
        {
            Array<Equipment> displayBag = [];

            foreach (Equipment gear in equipBag) {
                foreach (GearSlotID slotID in gear.GearSlot) {
                    if (slotID == slot) { 
                        foreach (ClassID classID in gear.ClassEquip) {
                            if (classID == charClass || (classID == ClassID.UNDEFINED && gear.ClassEquip.Count == 1)) { 
                                displayBag.Add(gear); 
                                break;
                            }
                        } 
                        break;
                    }
                }
            }

            return displayBag;
        }


        //=============================================================================
        // SECTION: Save System
        //=============================================================================

        public Array<ItemData> StoreItemBag()
        {
            Array<ItemData> newList = [];

            foreach (Item data in itemBag.Keys) {
                ItemData newItem = new();

                data.SetDataDetails(ref newItem);
                data.SetDataMechanics(ref newItem);
                data.SetDataRestrictions(ref newItem);
                newItem.Amount = itemBag[data];

                newList.Add(newItem);
            }
            return newList;
        }

        public Array<EquipmentData> StoreEquipBag()
        {
            Array<EquipmentData> newList = [];

            foreach (Equipment data in equipBag) {
                EquipmentData newEquip = new();

                data.SetDataDetails(ref newEquip);
                data.SetDataStats(ref newEquip);
                data.SetDataMechanics(ref newEquip);
                data.SetDataRestrictions(ref newEquip);

                newList.Add(newEquip);
            }
            return newList;
        }

        public void SetItemBag(Array<ItemData> list)
        {
            itemBag = [];
            foreach(ItemData data in list) {
                Item newItem = new();
                newItem.SetDetails(data.ItemType, data.ItemName, data.ItemDescription, data.TargetType, data.TargetArea, data.NumericValue, data.UniqueID);
                newItem.SetStateDetails(data.AddedState.StateName, data.AddedState.StateDescription, data.AddedState.UniqueID);
                newItem.SetMechanics(data.DamageType, data.CallAnimation, data.AddedState.AddModifier, data.AddedState.PercentModifier);
                newItem.SetRestrictions(data.UseableInBattle, data.UseableOutOfBattle, data.UseableOnDead, data.CanStack, data.IsConsumable, data.AddedState.ExistsOutOfBattle);

                itemBag[newItem] = data.Amount;
            }
        }

        public void SetEquipBag(Array<EquipmentData> list)
        {
            equipBag = [];
            foreach (EquipmentData data in list) {
                Equipment newEquip = new();
                newEquip.SetDetails(data.ItemType, data.ItemName, data.ItemDescription, data.TargetType, data.TargetArea, data.NumericValue, data.UniqueID);
                newEquip.SetEquipDetails(data.GearSlot);
                newEquip.SetStateDetails(data.AddedState.StateName, data.AddedState.StateDescription, data.AddedState.UniqueID);
                newEquip.SetEquipStats(data.AddModifier, data.PercentModifier);
                newEquip.SetMechanics(data.DamageType, data.CallAnimation, data.AddedState.AddModifier, data.AddedState.PercentModifier);
                newEquip.SetRestrictions(data.UseableInBattle, data.UseableOutOfBattle, data.UseableOnDead, data.CanStack, data.IsConsumable, data.AddedState.ExistsOutOfBattle);
                newEquip.SetEquipRestrictions(data.ClassEquip, data.UniqueEquip, data.IsEquipped);

                equipBag.Add(newEquip);
            }
        }

        public void OnSaveGame(SavedGame saveData)
        {
            InventoryData newData = new()
            {
                ItemBag = StoreItemBag(),
                EquipBag = StoreEquipBag()
            };

            saveData.InventoryData = newData;
        }

        public void OnLoadGame(InventoryData loadData)
        {
            InventoryData saveData = loadData;
            if (saveData == null) { GD.Print("Inventory Data - NULL"); return; }

            SetItemBag(saveData.ItemBag);
            SetEquipBag(saveData.EquipBag);
        }
    }
}