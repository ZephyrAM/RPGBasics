using Godot;
using Godot.Collections;
using System.Linq;

using ZAM.Abilities;

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
        private Dictionary<string, EffectState> stateDatabase = [];

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
            stateDatabase = DatabaseManager.Instance.GetStateDatabase();
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
                        AddToEquipBag((Equipment)addItem);
                        break;
                    case ItemType.Armor:
                        addItem = armorDatabase[newItem].Duplicate() as Equipment;
                        AddToEquipBag((Equipment)addItem);
                        break;
                    case ItemType.Accessory:
                        addItem = accessoryDatabase[newItem].Duplicate() as Equipment;
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
                    // Item addItem = newItem.Duplicate() as Item;
                    itemBag.Add(newItem, count);
                }
            } else {
                Item addItem = newItem.Duplicate() as Item;
                // addItem.SetUniqueID(ref DatabaseManager.Instance.GetUniqueCounter());
                itemBag.Add(addItem, 1);
            }
        }

        public Equipment GetEquipmentByType(string name, ItemType type, Equipment gear)
        {
            switch (type)
            {
                case ItemType.Weapon:
                    gear = weaponDatabase[name].Duplicate() as Equipment;
                    // AddToEquipBag((Equipment)gear);
                    break;
                case ItemType.Armor:
                    gear = armorDatabase[name].Duplicate() as Equipment;
                    // AddToEquipBag((Equipment)gear);
                    break;
                case ItemType.Accessory:
                    gear = accessoryDatabase[name].Duplicate() as Equipment;
                    // AddToEquipBag((Equipment)gear);
                    break;
                default:
                    GD.PushError("Invalid ItemType in GetEquipmentByType");
                    return null;
            }

            return gear;
        }

        public void AddToEquipBag(Equipment newEquip)
        {
            newEquip.SetUniqueID(ref DatabaseManager.Instance.GetUniqueCounter());
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

        // public Array<ItemData> StoreItemBag()
        // {
        //     Array<ItemData> newList = [];

        //     foreach (Item data in itemBag.Keys) {
        //         ItemData newItem = new();

        //         data.StoreDataDetails(ref newItem);
        //         data.StoreDataMechanics(ref newItem);
        //         data.StoreDataRestrictions(ref newItem);
        //         newItem.Amount = itemBag[data];

        //         newList.Add(newItem);
        //     }
        //     return newList;
        // }

        public void StoreItemBag(ConfigFile saveData)
        {
            saveData.SetValue(ConstTerm.ITEM + ConstTerm.DATA, ConstTerm.ITEM + ConstTerm.COUNT, itemBag.Count);

            int itemCounter = 0;
            foreach (Item item in itemBag.Keys) {
                // saveData.SetValue(ConstTerm.ITEM + ConstTerm.DATA, ConstTerm.ITEM + itemCount, item);
                if (!item.IsUnique) { 
                    saveData.SetValue(ConstTerm.ITEM + itemCounter + ConstTerm.DATA, ConstTerm.ITEM + ConstTerm.NAME, item.ItemName);
                } else {
                    item.StoreDetails(saveData, itemCounter);
                    item.StoreMechanics(saveData, itemCounter);
                    item.StoreRestrictions(saveData, itemCounter);

                    if (!item.AddedState.IsUnique) {
                        saveData.SetValue(ConstTerm.ITEM + itemCounter + ConstTerm.DATA, ConstTerm.STATE + ConstTerm.NAME, item.AddedState.StateName);
                    } else {
                        item.AddedState.StoreData(saveData, ConstTerm.ITEM + itemCounter + ConstTerm.DATA);
                    }
                }

                if (item.AddedState != null) {
                    saveData.SetValue(ConstTerm.ITEM + itemCounter + ConstTerm.DATA, ConstTerm.STATE + ConstTerm.IS + ConstTerm.UNIQUE, item.AddedState.IsUnique);
                }
                saveData.SetValue(ConstTerm.ITEM + itemCounter + ConstTerm.DATA, ConstTerm.ITEM + ConstTerm.UNIQUE + ConstTerm.ID, item.UniqueID);
                saveData.SetValue(ConstTerm.ITEM + itemCounter + ConstTerm.DATA, ConstTerm.ITEM + ConstTerm.IS + ConstTerm.UNIQUE, item.IsUnique);
                saveData.SetValue(ConstTerm.ITEM + itemCounter + ConstTerm.DATA, ConstTerm.ITEM + ConstTerm.COUNT, itemBag[item]);
                itemCounter++;
            }
        }

        public void StoreEquipBag(ConfigFile saveData)
        {
            saveData.SetValue(ConstTerm.EQUIP + ConstTerm.DATA, ConstTerm.EQUIP + ConstTerm.COUNT, equipBag.Count);

            int gearCounter = 0;
            foreach(Equipment gear in equipBag) {
                if (!gear.IsUnique) {
                    saveData.SetValue(ConstTerm.EQUIP + gearCounter + ConstTerm.DATA, ConstTerm.EQUIP + ConstTerm.NAME, gear.ItemName);
                    saveData.SetValue(ConstTerm.EQUIP + gearCounter + ConstTerm.DATA, ConstTerm.EQUIP + ConstTerm.TYPE, (int)gear.ItemType);
                } else {
                    gear.StoreDetails(saveData, gearCounter);
                    gear.StoreMechanics(saveData, gearCounter);
                    gear.StoreStats(saveData, gearCounter);
                    gear.StoreRestrictions(saveData, gearCounter);

                    if (!gear.AddedState.IsUnique) {
                        saveData.SetValue(ConstTerm.EQUIP + gearCounter + ConstTerm.DATA, ConstTerm.STATE + ConstTerm.NAME, gear.AddedState.StateName);
                    } else {
                        gear.AddedState.StoreData(saveData, ConstTerm.EQUIP + gearCounter + ConstTerm.DATA);
                    }
                }

                if (gear.AddedState != null) {
                    saveData.SetValue(ConstTerm.EQUIP + gearCounter + ConstTerm.DATA, ConstTerm.STATE + ConstTerm.IS + ConstTerm.UNIQUE, gear.AddedState.IsUnique);
                }
                saveData.SetValue(ConstTerm.EQUIP + gearCounter + ConstTerm.DATA, ConstTerm.EQUIP + ConstTerm.UNIQUE + ConstTerm.ID, gear.UniqueID);
                saveData.SetValue(ConstTerm.EQUIP + gearCounter + ConstTerm.DATA, ConstTerm.EQUIP + ConstTerm.IS + ConstTerm.UNIQUE, gear.IsUnique);
                saveData.SetValue(ConstTerm.EQUIP + gearCounter + ConstTerm.DATA, ConstTerm.IS + ConstTerm.EQUIPPED, gear.IsEquipped);
                gearCounter++;
            }
        }

        public void SetItemBag(ConfigFile loadData)
        {
            if (loadData.HasSection(ConstTerm.ITEM + ConstTerm.DATA)) {
                // int itemCount = (int)loadData.GetValue(ConstTerm.ITEM + ConstTerm.DATA, ConstTerm.ITEM + ConstTerm.COUNT);
                itemBag = [];

                int count = (int)loadData.GetValue(ConstTerm.ITEM + ConstTerm.DATA, ConstTerm.ITEM + ConstTerm.COUNT);
                for (int i = 0; i < count; i++) {
                    // Item newItem = (Item)loadData.GetValue(ConstTerm.ITEM + ConstTerm.DATA, ConstTerm.ITEM + c);
                    Item newItem = new();
                    bool testUnique = (bool)loadData.GetValue(ConstTerm.ITEM + i + ConstTerm.DATA, ConstTerm.ITEM + ConstTerm.IS + ConstTerm.UNIQUE);
                    if (!testUnique) { 
                        string itemName = (string)loadData.GetValue(ConstTerm.ITEM + i + ConstTerm.DATA, ConstTerm.ITEM + ConstTerm.NAME); 
                        newItem = itemDatabase[itemName];
                    } else {
                        newItem.SetDetails(loadData, i);
                        newItem.SetMechanics(loadData, i);
                        newItem.SetRestrictions(loadData, i);
                        newItem.SetIsUnique(true);

                        if (loadData.HasSectionKey(ConstTerm.ITEM + i + ConstTerm.DATA, ConstTerm.STATE + ConstTerm.NAME)) {
                            bool testState = (bool)loadData.GetValue(ConstTerm.ITEM + i + ConstTerm.DATA, ConstTerm.STATE + ConstTerm.IS + ConstTerm.UNIQUE);
                            if (!testState) {
                                string stateName = (string)loadData.GetValue(ConstTerm.ITEM + i + ConstTerm.DATA, ConstTerm.STATE + ConstTerm.NAME);
                                newItem.SetAddedState(stateDatabase[stateName]);
                            }
                            else {
                                newItem.AddedState.SetData(loadData, ConstTerm.ITEM + i + ConstTerm.DATA);
                                newItem.AddedState.SetIsUnique(true);
                            }
                        }
                    }
                    
                    newItem.LoadUniqueID((ulong)loadData.GetValue(ConstTerm.ITEM + i + ConstTerm.DATA, ConstTerm.ITEM + ConstTerm.UNIQUE + ConstTerm.ID));
                    int amount = (int)loadData.GetValue(ConstTerm.ITEM + i + ConstTerm.DATA, ConstTerm.ITEM + ConstTerm.COUNT);
                    AddToItemBag(newItem, amount);
                }
            }
        }

        public void SetEquipBag(ConfigFile loadData)
        {
            if (loadData.HasSection(ConstTerm.EQUIP + ConstTerm.DATA)) {
                equipBag = [];

                int count = (int)loadData.GetValue(ConstTerm.EQUIP + ConstTerm.DATA, ConstTerm.EQUIP + ConstTerm.COUNT);
                for (int e = 0; e < count; e++) {
                    Equipment newEquip = new();
                    bool testUnique = (bool)loadData.GetValue(ConstTerm.EQUIP + e + ConstTerm.DATA, ConstTerm.EQUIP + ConstTerm.IS + ConstTerm.UNIQUE);
                    if (!testUnique) {
                        string equipName = (string)loadData.GetValue(ConstTerm.EQUIP + e + ConstTerm.DATA, ConstTerm.EQUIP + ConstTerm.NAME);
                        ItemType equipType = (ItemType)(int)loadData.GetValue(ConstTerm.EQUIP + e + ConstTerm.DATA, ConstTerm.EQUIP + ConstTerm.TYPE);
                        newEquip = GetEquipmentByType(equipName, equipType, newEquip);
                    } else {
                        newEquip.SetDetails(loadData, e);
                        newEquip.SetMechanics(loadData, e);
                        newEquip.SetStats(loadData, e);
                        newEquip.SetRestrictions(loadData, e);
                        newEquip.SetIsUnique(true);

                        if (loadData.HasSectionKey(ConstTerm.EQUIP + e + ConstTerm.DATA, ConstTerm.STATE + ConstTerm.NAME)) {
                            bool testState = (bool)loadData.GetValue(ConstTerm.EQUIP + e + ConstTerm.DATA, ConstTerm.STATE + ConstTerm.IS + ConstTerm.UNIQUE);
                            if (!testState) {
                                string stateName = (string)loadData.GetValue(ConstTerm.EQUIP + e + ConstTerm.DATA, ConstTerm.STATE + ConstTerm.NAME);
                                newEquip.SetAddedState(stateDatabase[stateName]);
                            }
                            else {
                                newEquip.AddedState.SetData(loadData, ConstTerm.EQUIP + e + ConstTerm.DATA);
                            }
                        }
                    }
                    newEquip.LoadUniqueID((ulong)loadData.GetValue(ConstTerm.EQUIP + e + ConstTerm.DATA, ConstTerm.EQUIP + ConstTerm.UNIQUE + ConstTerm.ID));
                    newEquip.SetIsEquipped((bool)loadData.GetValue(ConstTerm.EQUIP + e + ConstTerm.DATA, ConstTerm.IS + ConstTerm.EQUIPPED));
                    AddToEquipBag(newEquip);
                }
            }
        }

        // public Array<EquipmentData> StoreEquipBag()
        // {
        //     Array<EquipmentData> newList = [];

        //     foreach (Equipment data in equipBag) {
        //         EquipmentData newEquip = new();

        //         data.SetDataDetails(ref newEquip);
        //         data.SetDataStats(ref newEquip);
        //         data.SetDataMechanics(ref newEquip);
        //         data.SetDataRestrictions(ref newEquip);

        //         newList.Add(newEquip);
        //     }
        //     return newList;
        // }

        // public void SetItemBag(Array<ItemData> list)
        // {
        //     itemBag = [];
        //     foreach(ItemData data in list) {
        //         Item newItem = new();
        //         newItem.SetDetails(data.ItemType, data.ItemName, data.ItemDescription, data.TargetType, data.TargetArea, data.NumericValue, data.UniqueID);
        //         newItem.SetStateDetails(data.AddedState.StateName, data.AddedState.StateDescription, data.AddedState.UniqueID);
        //         newItem.SetMechanics(data.DamageType, data.CallAnimation, data.AddedState.AddModifier, data.AddedState.PercentModifier);
        //         newItem.SetRestrictions(data.UseableInBattle, data.UseableOutOfBattle, data.UseableOnDead, data.CanStack, data.IsConsumable, data.AddedState.ExistsOutOfBattle);

        //         itemBag[newItem] = data.Amount;
        //     }
        // }

        // public void SetEquipBag(Array<EquipmentData> list)
        // {
        //     equipBag = [];
        //     foreach (EquipmentData data in list) {
        //         Equipment newEquip = new();
        //         newEquip.SetDetails(data.ItemType, data.ItemName, data.ItemDescription, data.TargetType, data.TargetArea, data.NumericValue, data.UniqueID);
        //         newEquip.SetEquipDetails(data.GearSlot);
        //         newEquip.SetStateDetails(data.AddedState.StateName, data.AddedState.StateDescription, data.AddedState.UniqueID);
        //         newEquip.SetEquipStats(data.AddModifier, data.PercentModifier);
        //         newEquip.SetMechanics(data.DamageType, data.CallAnimation, data.AddedState.AddModifier, data.AddedState.PercentModifier);
        //         newEquip.SetRestrictions(data.UseableInBattle, data.UseableOutOfBattle, data.UseableOnDead, data.CanStack, data.IsConsumable, data.AddedState.ExistsOutOfBattle);
        //         newEquip.SetEquipRestrictions(data.ClassEquip, data.UniqueEquip, data.IsEquipped);

        //         equipBag.Add(newEquip);
        //     }
        // }

        // public void OnSaveGame(SavedGame saveData)
        // {
        //     InventoryData newData = new()
        //     {
        //         ItemBag = StoreItemBag(),
        //         EquipBag = StoreEquipBag()
        //     };

        //     saveData.InventoryData = newData;
        // }

        // public void OnLoadGame(InventoryData loadData)
        // {
        //     InventoryData saveData = loadData;
        //     if (saveData == null) { GD.Print("Inventory Data - NULL"); return; }

        //     SetItemBag(saveData.ItemBag);
        //     SetEquipBag(saveData.EquipBag);
        // }

        public void OnSaveFile(ConfigFile saveData)
        {
            StoreItemBag(saveData);
            StoreEquipBag(saveData);
        }

        public void OnLoadFile(ConfigFile loadData)
        {
            SetItemBag(loadData);
            SetEquipBag(loadData);
        }
    }
}