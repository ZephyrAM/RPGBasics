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

        public void SetEquipDetails(Array<GearSlotID> slots)
        {
            GearSlot = slots;
        }

        public void SetEquipStats(Array<Modifier> add, Array<Modifier> percent)
        {
            AddModifier = add;
            PercentModifier = percent;
        }

        public void SetEquipRestrictions(Array<ClassID> classes, int uniqueEquip, bool equipped)
        {
            ClassEquip = classes;
            UniqueEquip = uniqueEquip;
            IsEquipped = equipped;
        }

        public void SetDataDetails(ref EquipmentData data)
        {
            data.ItemType = ItemType;
            data.ItemName = ItemName;
            data.ItemDescription = ItemDescription;
            data.TargetType = TargetType;
            data.TargetArea = TargetArea;
            data.NumericValue = NumericValue;

            data.UniqueID = UniqueID;

            data.GearSlot = GearSlot;

            AddedState.SetDataDetails(data.AddedState);
        }

        public void SetDataStats(ref EquipmentData data)
        {
            data.AddModifier = AddModifier;
            data.PercentModifier = PercentModifier;
        }

        public void SetDataMechanics(ref EquipmentData data)
        {
            data.DamageType = DamageType;
            data.CallAnimation = CallAnimation;

            data.AddedState.AddModifier = AddedState.AddModifier;
            data.AddedState.PercentModifier = AddedState.PercentModifier;

            AddedState.SetDataMechanics(data.AddedState);
        }

        public void SetDataRestrictions(ref EquipmentData data)
        {
            data.UseableInBattle = UseableInBattle;
            data.UseableOutOfBattle = UseableOutOfBattle;
            data.UseableOnDead = UseableOnDead;
            data.CanStack = CanStack;
            data.IsConsumable = IsConsumable;

            data.ClassEquip = ClassEquip;
            data.UniqueEquip = UniqueEquip;
            data.IsEquipped = IsEquipped;

            AddedState.SetDataRestrictions(data.AddedState);
        }
    }
}