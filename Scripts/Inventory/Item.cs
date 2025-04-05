using Godot;
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

        [Export(PropertyHint.Enum, "Ally,Enemy")] public string TargetType { get; private set;} // Ally or Enemy
        [Export(PropertyHint.Enum, "Single,Group")] public string TargetArea { get; private set;} // Single or Group

        [Export] public float NumericValue { get; private set; }

        [ExportGroup("Mechanics")]
        [Export] public string DamageType { get; private set; } // Physical, Magical, more to be added
        [Export] public string CallAnimation { get; private set; }

        [Export] public EffectState AddedState { get; private set; }

        [ExportGroup("Restrictions")]
        [Export] public bool UseableInBattle { get; private set; }
        [Export] public bool UseableOutOfBattle { get; private set; }
        [Export] public bool UseableOnDead { get; private set; }
        [Export] public bool CanStack { get; private set; }

        public int UniqueID { get; private set; } = 0;

        public void SetUniqueID(ref int id)
        { 
            if (UniqueID != 0) { GD.PushWarning("Attempting to re-declare UniqueID for " + ItemName); return; }
            UniqueID = id;
            id++;
        }
    }
}