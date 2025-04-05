using Godot;
using Godot.Collections;

using ZAM.Managers;
using ZAM.Stats;

namespace ZAM.Inventory
{
    // Global Object \\
    public partial class ItemBag : Node
    {
        private Array<Item> itemBag = [];
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

        public void AddToBag(string newItem, ItemType type)
        {
            Item addItem;

            switch (type)
            {
                case ItemType.Item:
                    addItem = itemDatabase[newItem].Duplicate() as Item;
                    AddToItemBag(addItem);
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

        public void AddToItemBag(Item newItem)
        {
            itemBag.Add(newItem);
        }

        public void AddToEquipBag(Equipment newEquip)
        {
            equipBag.Add(newEquip);
        }

        public void RemoveItemFromBag(Item instance)
        {
            // Item removeItem = itemBag[index];
            itemBag.Remove(instance);
        }

        public void RemoveEquipFromBag(Equipment instance)
        {
            equipBag.Remove(instance);
        }

        public Array<Item> GetItemBag()
        {
            return itemBag;
        }

        public Array<Equipment> GetEquipBag()
        {
            return equipBag;
        }

        public void SortEquipBag()
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
                    case GearSlotID.Accessory:
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

        public void OnSaveGame(SavedGame saveData)
        {
            InventoryData newData = new()
            {
                ItemBag = Instance.GetItemBag(),
                EquipBag = Instance.GetEquipBag()
            };

            saveData.InventoryData = newData;
        }

        public void OnLoadGame(InventoryData loadData)
        {
            InventoryData saveData = loadData;
            if (saveData == null) { GD.Print("Inventory Data - NULL"); return; }

            Instance.itemBag = saveData.ItemBag;
            Instance.equipBag = saveData.EquipBag;
        }
    }
}