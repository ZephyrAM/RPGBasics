using Godot;
using Godot.Collections;
using System;

namespace ZAM.Inventory
{
    public partial class Equipment : Item
    {
        [ExportGroup("Details")]
        [Export] public Array<GearSlotID> GearSlot { get; private set; } = [GearSlotID.UNDEFINED]; // Gear slot it can be equipped in.
        // EDIT: Add weapon type?
        // EDIT: Add icon and equip graphics
        
        [ExportGroup("Stats")]
        [Export] public Array<Modifier> AddModifier { get; private set; } = [];
        [Export] public Array<Modifier> PercentModifier { get; private set; } = [];
        // [Export] public Array<StatID> AffectStat { get; private set; } = [StatID.UNDEFINED]; // Stat modifier on weapon
        // [Export] public Array<float> StatValue { get; private set; } = [0]; // Modifier value

        [ExportGroup("Restrictions")]
        [Export] public Array<ClassID> ClassEquip { get; private set; } = [ClassID.UNDEFINED]; // Classes that can equip it.
        [Export] public int UniqueEquip { get; private set; } = 0; // If 0, not unique. Else, number of equips allowed.

        public bool IsEquipped { get; private set; } = false;

        // private Dictionary<StatID, float> gearStatList = [];

        //=============================================================================
        // SECTION: Base Methods
        //=============================================================================

        // public override void _Ready()
        // {
        //     for (int s = 0; s < AffectStat.Count; s++) {
        //         gearStatList[AffectStat[s]] = StatValue[s];
        //     }
        // }

        //=============================================================================
        // SECTION: External Access Methods
        //=============================================================================


        public void SetIsEquipped(bool value)
        {
            IsEquipped = value;
        }

        public Array<float> GetStatModifiers()
        {
            Array<float> tempMods = [];
            for (int s = 0; s < Enum.GetValues(typeof(StatID)).Length; s++) { 
                tempMods.Add(0); 
            }

            // for (int p = 0; p < PercentModifier.Length; p++) {
            //     tempMods[(int)PercentModifier[p].Stat] = PercentModifier[p].Value + 1;
            // }

            for (int a = 0; a < AddModifier.Count; a++) {
                tempMods[(int)AddModifier[a].Stat] += AddModifier[a].Value;
            }            

            return tempMods;
        }

        // public Dictionary<StatID, float> GetGearStats()
        // {
        //     return gearStatList;
        // }

        //=============================================================================
        // SECTION: Group Call
        //=============================================================================

        public Dictionary<StatID, float> StatAddModifier(Dictionary<StatID, float> statSheet)
        {
            for (int s = 0; s < AddModifier.Count; s++) {
                statSheet[AddModifier[s].Stat] += AddModifier[s].Value;
            }
            return statSheet;
        }

        public Dictionary<StatID, float> StatPercentModifier(Dictionary<StatID, float> statSheet)
        {
            for (int s = 0; s < PercentModifier.Count; s++) {
                statSheet[PercentModifier[s].Stat] *= PercentModifier[s].Value;
            }
            return statSheet;
        }


        //=============================================================================
        // SECTION: Save System
        //=============================================================================
        
        public override void StoreDetails(ConfigFile saveData, int counter)
        {
            base.StoreDetails(saveData, counter);
            string itemID = ConstTerm.ITEM + counter + ConstTerm.DATA;

            saveData.SetValue(itemID, ConstTerm.GEAR_SLOTS + ConstTerm.COUNT, GearSlot.Count);
            for (int s = 0; s < GearSlot.Count; s++) {
                saveData.SetValue(itemID, ConstTerm.GEAR_SLOTS + s, (int)GearSlot[s]);
            }
        }

        public void StoreStats(ConfigFile saveData, int counter)
        {
            string itemID = ConstTerm.ITEM + counter + ConstTerm.DATA;

            if (AddModifier.Count > 0) {
                saveData.SetValue(itemID, ConstTerm.ADD + ConstTerm.MODIFIER + ConstTerm.COUNT, AddModifier.Count);
                for (int a = 0; a < AddModifier.Count; a++) {
                    AddModifier[a].StoreModifier(saveData, itemID, a + ConstTerm.ADD);
                }
            }
            if (PercentModifier.Count > 0) {
                saveData.SetValue(itemID, ConstTerm.PERCENT + ConstTerm.MODIFIER + ConstTerm.COUNT, PercentModifier.Count);
                for (int p = 0; p < PercentModifier.Count; p++) {
                    PercentModifier[p].StoreModifier(saveData, itemID, p + ConstTerm.PERCENT);
                }
            }
        }

        public override void StoreRestrictions(ConfigFile saveData, int counter)
        {
            base.StoreRestrictions(saveData, counter);
            string itemID = ConstTerm.ITEM + counter + ConstTerm.DATA;

            saveData.SetValue(itemID, ConstTerm.CLASS + ConstTerm.EQUIP + ConstTerm.COUNT, ClassEquip.Count);
            for (int c = 0; c < ClassEquip.Count; c++) {
                saveData.SetValue(itemID, ConstTerm.CLASS + ConstTerm.EQUIP + c, (int)ClassEquip[c]);
            }
            saveData.SetValue(itemID, ConstTerm.UNIQUE + ConstTerm.EQUIP, UniqueEquip);
        }

        public override void SetDetails(ConfigFile loadData, int counter)
        {
            base.SetDetails(loadData, counter);
            string itemID = ConstTerm.ITEM + counter + ConstTerm.DATA;

            int slotCount = (int)loadData.GetValue(itemID, ConstTerm.GEAR_SLOTS + ConstTerm.COUNT);
            if (slotCount <= 0) { return; }
            GearSlot = [];
            for (int s = 0; s < slotCount; s++) {
                GearSlot.Add((GearSlotID)(int)loadData.GetValue(itemID, ConstTerm.GEAR_SLOTS + s));
            }
        }

        public void SetStats(ConfigFile loadData, int counter)
        {
            string itemID = ConstTerm.ITEM + counter + ConstTerm.DATA;

            if (loadData.HasSectionKey(itemID, ConstTerm.ADD + ConstTerm.MODIFIER + ConstTerm.COUNT)) {
                int addCount = (int)loadData.GetValue(itemID, ConstTerm.ADD + ConstTerm.MODIFIER + ConstTerm.COUNT);
                if (addCount <= 0) { goto PercentMods; }
                AddModifier = [];
                for (int a = 0; a < addCount; a++) {
                    Modifier addTemp = new();
                    addTemp.SetModifier(loadData, itemID, a + ConstTerm.ADD);
                    AddModifier.Add(addTemp);
                }
            }
            
            PercentMods:
            if (loadData.HasSectionKey(itemID, ConstTerm.PERCENT + ConstTerm.MODIFIER + ConstTerm.COUNT)) {
                int percentCount = (int)loadData.GetValue(itemID, ConstTerm.PERCENT + ConstTerm.MODIFIER + ConstTerm.COUNT);
                if (percentCount <= 0) { return; }
                PercentModifier = [];
                for (int p = 0; p < percentCount; p++) {
                    Modifier percentTemp = new();
                    percentTemp.SetModifier(loadData, itemID, p + ConstTerm.PERCENT);
                    PercentModifier.Add(percentTemp);
                }
            }
        }

        public override void SetRestrictions(ConfigFile loadData, int counter)
        {
            base.SetRestrictions(loadData, counter);
            string itemID = ConstTerm.ITEM + counter + ConstTerm.DATA;

            UniqueEquip = (int)loadData.GetValue(itemID, ConstTerm.UNIQUE + ConstTerm.EQUIP);

            int classCount = (int)loadData.GetValue(itemID, ConstTerm.CLASS + ConstTerm.EQUIP + ConstTerm.COUNT);
            if (classCount <= 0) { return; }
            ClassEquip = [];
            for (int c = 0; c < classCount; c++) {
                ClassEquip.Add((ClassID)(int)loadData.GetValue(itemID, ConstTerm.CLASS + ConstTerm.EQUIP + c));
            }
        }

        // public void SetEquipDetails(Array<GearSlotID> slots)
        // {
        //     GearSlot = slots;
        // }

        // public void SetEquipStats(Array<Modifier> add, Array<Modifier> percent)
        // {
        //     AddModifier = add;
        //     PercentModifier = percent;
        // }

        // public void SetEquipRestrictions(Array<ClassID> classes, int uniqueEquip, bool equipped)
        // {
        //     ClassEquip = classes;
        //     UniqueEquip = uniqueEquip;
        //     IsEquipped = equipped;
        // }

        // public void SetDataDetails(ref EquipmentData data)
        // {
        //     data.ItemType = ItemType;
        //     data.ItemName = ItemName;
        //     data.ItemDescription = ItemDescription;
        //     data.TargetType = TargetType;
        //     data.TargetArea = TargetArea;
        //     data.NumericValue = NumericValue;

        //     data.UniqueID = UniqueID;

        //     data.GearSlot = GearSlot;

        //     AddedState.StoreDataDetails(data.AddedState);
        // }

        // public void SetDataStats(ref EquipmentData data)
        // {
        //     data.AddModifier = AddModifier;
        //     data.PercentModifier = PercentModifier;
        // }

        // public void SetDataMechanics(ref EquipmentData data)
        // {
        //     data.DamageType = DamageType;
        //     data.CallAnimation = CallAnimation;

        //     data.AddedState.AddModifier = AddedState.AddModifier;
        //     data.AddedState.PercentModifier = AddedState.PercentModifier;

        //     AddedState.StoreDataMechanics(data.AddedState);
        // }

        // public void SetDataRestrictions(ref EquipmentData data)
        // {
        //     data.UseableInBattle = UseableInBattle;
        //     data.UseableOutOfBattle = UseableOutOfBattle;
        //     data.UseableOnDead = UseableOnDead;
        //     data.CanStack = CanStack;
        //     data.IsConsumable = IsConsumable;

        //     data.ClassEquip = ClassEquip;
        //     data.UniqueEquip = UniqueEquip;
        //     data.IsEquipped = IsEquipped;

        //     AddedState.StoreDataRestrictions(data.AddedState);
        // }
    }
}