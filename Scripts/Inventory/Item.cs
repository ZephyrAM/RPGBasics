using Godot;
using Godot.Collections;
using System;

using ZAM.Abilities;

namespace ZAM.Inventory
{
    public partial class Item : Node
    {        
        [ExportGroup("Details")]
        [Export] public ItemType ItemType { get; private set; }
        [Export] public string ItemName { get; private set; }
        [Export] public string ItemDescription { get; private set;}

        [Export(PropertyHint.Enum, "Ally,Enemy,Self")] public string TargetType { get; private set;} // Ally, Enemy, or Self
        [Export(PropertyHint.Enum, "Single,Group")] public string TargetArea { get; private set;} // Single or Group

        [Export] public float NumericValue { get; private set; }

        [ExportGroup("Mechanics")]
        [Export] public string DamageType { get; private set; } // Physical, Magical, more to be added
        [Export] public string CallAnimation { get; private set; }

        [Export] public EffectState AddedState { get; private set; } = new();

        [ExportGroup("Restrictions")]
        [Export] public bool UseableInBattle { get; private set; }
        [Export] public bool UseableOutOfBattle { get; private set; }
        [Export] public bool UseableOnDead { get; private set; }
        [Export] public bool CanStack { get; private set; }
        [Export] public bool IsConsumable { get; private set; } = true;

        public ulong UniqueID { get; private set; } = 0;

        public void SetUniqueID(ref ulong id)
        { 
            if (UniqueID != 0) { GD.PushWarning("Attempting to re-declare UniqueID for " + ItemName); return; }
            UniqueID = id;
            id++;
        }

        public void CreateUniqueInstance(ref ulong id)
        {
            UniqueID = id;
            id++;
        }

        //=============================================================================
        // SECTION: Save System
        //=============================================================================

        public void SetDetails(ItemType itemType, string name, string description, string type, string area, float value, ulong id)
        {
            ItemType = itemType;
            ItemName = name;
            ItemDescription = description;
            TargetType = type;
            TargetArea = area;
            NumericValue = value;

            UniqueID = id;
        }

        public void SetStateDetails(string name, string description, ulong id)
        {
            AddedState.SetDetails(name, description, id);
        }

        public void SetMechanics(string type, string animation, Array<Modifier> add, Array<Modifier> percent)
        {
            DamageType = type;
            CallAnimation = animation;

            AddedState.SetMechanics(add, percent);
        }

        public void SetRestrictions(bool inBattle, bool outBattle, bool onDead, bool canStack, bool consumable, bool exists)
        {
            UseableInBattle = inBattle;
            UseableOutOfBattle = outBattle;
            UseableOnDead = onDead;
            CanStack = canStack;
            IsConsumable = consumable;

            AddedState.SetRestrictions(exists);
        }

        public void SetDataDetails(ref ItemData data)
        {
            data.ItemType = ItemType;
            data.ItemName = ItemName;
            data.ItemDescription = ItemDescription;
            data.TargetType = TargetType;
            data.TargetArea = TargetArea;
            data.NumericValue = NumericValue;

            data.UniqueID = UniqueID;

            AddedState.SetDataDetails(data.AddedState);
        }

        public void SetDataMechanics(ref ItemData data)
        {
            data.DamageType = DamageType;
            data.CallAnimation = CallAnimation;

            data.AddedState.AddModifier = AddedState.AddModifier;
            data.AddedState.PercentModifier = AddedState.PercentModifier;

            AddedState.SetDataMechanics(data.AddedState);
        }

        public void SetDataRestrictions(ref ItemData data)
        {
            data.UseableInBattle = UseableInBattle;
            data.UseableOutOfBattle = UseableOutOfBattle;
            data.UseableOnDead = UseableOnDead;
            data.CanStack = CanStack;
            data.IsConsumable = IsConsumable;

            AddedState.SetDataRestrictions(data.AddedState);
        }
    }
}