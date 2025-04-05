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
        [Export] public Modifier[] AddModifier { get; private set; } = [];
        [Export] public Modifier[] PercentModifier { get; private set; } = [];
        // [Export] public Array<StatID> AffectStat { get; private set; } = [StatID.UNDEFINED]; // Stat modifier on weapon
        // [Export] public Array<float> StatValue { get; private set; } = [0]; // Modifier value

        [ExportGroup("Restrictions")]
        [Export] public Array<ClassID> ClassEquip { get; private set; } = [ClassID.UNDEFINED]; // Classes that can equip it.
        [Export] public int UniqueEquip { get; private set; } = 0; // If 0, not unique. Else, number of equips allowed.

        private bool isEquipped  = false;

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

        public bool GetIsEquipped()
        {
            return isEquipped;
        }

        public void SetIsEquipped(bool value)
        {
            isEquipped = value;
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

            for (int a = 0; a < AddModifier.Length; a++) {
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
            for (int s = 0; s < AddModifier.Length; s++) {
                statSheet[AddModifier[s].Stat] += AddModifier[s].Value;
            }
            return statSheet;
        }

        public Dictionary<StatID, float> StatPercentModifier(Dictionary<StatID, float> statSheet)
        {
            for (int s = 0; s < PercentModifier.Length; s++) {
                statSheet[PercentModifier[s].Stat] *= PercentModifier[s].Value;
            }
            return statSheet;
        }
    }
}