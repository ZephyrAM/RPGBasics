using Godot;

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

        [Export] public EffectState AddedState { get; private set; }

        [ExportGroup("Restrictions")]
        [Export] public bool UseableInBattle { get; private set; }
        [Export] public bool UseableOutOfBattle { get; private set; }
        [Export] public bool UseableOnDead { get; private set; }
        [Export] public bool CanStack { get; private set; }
        [Export] public bool IsConsumable { get; private set; } = true;

        public ulong UniqueID { get; private set; } = 0;
        public bool IsUnique { get; private set; } = false;

        public void SetUniqueID(ref ulong id)
        { 
            if (UniqueID != 0) { return; }
            UniqueID = id;
            id++;
        }

        public void LoadUniqueID(ulong id)
        {
            UniqueID = id;
        }

        public void CreateUniqueInstance(ref ulong id)
        {
            IsUnique = true;
            UniqueID = id;
            id++;
        }

        public void SetIsUnique(bool value)
        {
            IsUnique = value;
        }

        public void SetAddedState(EffectState state)
        {
            AddedState = state;
        }

        //=============================================================================
        // SECTION: Save System
        //=============================================================================

        // public void SetDetails(ItemType itemType, string name, string description, string type, string area, float value, ulong id)
        // {
        //     ItemType = itemType;
        //     ItemName = name;
        //     ItemDescription = description;
        //     TargetType = type;
        //     TargetArea = area;
        //     NumericValue = value;

        //     UniqueID = id;
        // }

        // public void SetStateDetails(string name, string description, ulong id)
        // {
        //     AddedState?.SetDetails(name, description, id);
        // }

        // public void SetMechanics(string type, string animation, Array<Modifier> add, Array<Modifier> percent)
        // {
        //     DamageType = type;
        //     CallAnimation = animation;

        //     AddedState?.SetMechanics(add, percent);
        // }

        // public void SetRestrictions(bool inBattle, bool outBattle, bool onDead, bool canStack, bool consumable, bool exists)
        // {
        //     UseableInBattle = inBattle;
        //     UseableOutOfBattle = outBattle;
        //     UseableOnDead = onDead;
        //     CanStack = canStack;
        //     IsConsumable = consumable;

        //     AddedState?.SetRestrictions(exists);
        // }

        // public void StoreDataDetails(ref ItemData data)
        // {
        //     data.ItemType = ItemType;
        //     data.ItemName = ItemName;
        //     data.ItemDescription = ItemDescription;
        //     data.TargetType = TargetType;
        //     data.TargetArea = TargetArea;
        //     data.NumericValue = NumericValue;

        //     data.UniqueID = UniqueID;

        //     AddedState?.StoreDataDetails(data.AddedState);
        // }

        // public void StoreDataMechanics(ref ItemData data)
        // {
        //     data.DamageType = DamageType;
        //     data.CallAnimation = CallAnimation;

        //     data.AddedState.AddModifier = AddedState.AddModifier;
        //     data.AddedState.PercentModifier = AddedState.PercentModifier;

        //     AddedState?.StoreDataMechanics(data.AddedState);
        // }

        // public void StoreDataRestrictions(ref ItemData data)
        // {
        //     data.UseableInBattle = UseableInBattle;
        //     data.UseableOutOfBattle = UseableOutOfBattle;
        //     data.UseableOnDead = UseableOnDead;
        //     data.CanStack = CanStack;
        //     data.IsConsumable = IsConsumable;

        //     AddedState?.StoreDataRestrictions(data.AddedState);
        // }

        public virtual void StoreDetails(ConfigFile saveData, int counter)
        {
            string itemID = ConstTerm.ITEM + counter + ConstTerm.DATA;

            saveData.SetValue(itemID, ConstTerm.ITEM + ConstTerm.TYPE, (int)ItemType);
            saveData.SetValue(itemID, ConstTerm.ITEM + ConstTerm.NAME, ItemName);
            saveData.SetValue(itemID, ConstTerm.ITEM + ConstTerm.DESCRIPTION, ItemDescription);
            saveData.SetValue(itemID, ConstTerm.TARGET + ConstTerm.TYPE, TargetType);
            saveData.SetValue(itemID, ConstTerm.TARGET + ConstTerm.AREA, TargetArea);
            saveData.SetValue(itemID, ConstTerm.NUMERIC + ConstTerm.VALUE, NumericValue);

            // saveData.SetValue(itemID, ConstTerm.UNIQUE + ConstTerm.ID, UniqueID);

            // AddedState?.StoreDetails(saveData, itemID);
        }

        public virtual void StoreMechanics(ConfigFile saveData, int counter)
        {
            string itemID = ConstTerm.ITEM + counter + ConstTerm.DATA;

            saveData.SetValue(itemID, ConstTerm.DAMAGE + ConstTerm.TYPE, DamageType);
            saveData.SetValue(itemID, ConstTerm.CALL_ANIM, CallAnimation);

            // AddedState?.StoreMechanics(saveData, itemID);
        }

        public virtual void StoreRestrictions(ConfigFile saveData, int counter)
        {
            string itemID = ConstTerm.ITEM + counter + ConstTerm.DATA;

            saveData.SetValue(itemID, ConstTerm.USEABLE + ConstTerm.IN + ConstTerm.BATTLE, UseableInBattle);
            saveData.SetValue(itemID, ConstTerm.USEABLE + ConstTerm.OUT_OF + ConstTerm.BATTLE, UseableOutOfBattle);
            saveData.SetValue(itemID, ConstTerm.USEABLE + ConstTerm.DEAD, UseableOnDead);
            saveData.SetValue(itemID, ConstTerm.CAN_STACK, CanStack);

            // if (AddedState != null) {
            //     if (!AddedState.IsUnique) {
            //         saveData.SetValue(itemID, ConstTerm.STATE, AddedState.StateName);
            //     } else {
            //         AddedState.StoreRestrictions(saveData, itemID);
            //     }
            // }   
        }

        public virtual void SetDetails(ConfigFile loadData, int index)
        {
            string itemID = ConstTerm.ITEM + index + ConstTerm.DATA;

            ItemType = (ItemType)(int)loadData.GetValue(itemID, ConstTerm.ITEM + ConstTerm.TYPE);
            ItemName = (string)loadData.GetValue(itemID, ConstTerm.ITEM + ConstTerm.NAME);
            ItemDescription = (string)loadData.GetValue(itemID, ConstTerm.ITEM + ConstTerm.DESCRIPTION);
            TargetType = (string)loadData.GetValue(itemID, ConstTerm.TARGET + ConstTerm.TYPE);
            TargetArea = (string)loadData.GetValue(itemID, ConstTerm.TARGET + ConstTerm.AREA);
            NumericValue = (int)loadData.GetValue(itemID, ConstTerm.NUMERIC + ConstTerm.VALUE);

            UniqueID = (ulong)loadData.GetValue(itemID, ConstTerm.ITEM + ConstTerm.UNIQUE + ConstTerm.ID);

            // AddedState?.SetDetails(loadData, itemID);
        }

        public virtual void SetMechanics(ConfigFile loadData, int index)
        {
            string itemID = ConstTerm.ITEM + index + ConstTerm.DATA;

            DamageType = (string)loadData.GetValue(itemID, ConstTerm.DAMAGE + ConstTerm.TYPE);
            CallAnimation = (string)loadData.GetValue(itemID, ConstTerm.CALL_ANIM);

            // AddedState?.SetDetails(loadData, itemID);
        }

        public virtual void SetRestrictions(ConfigFile loadData, int index)
        {
            string itemID = ConstTerm.ITEM + index + ConstTerm.DATA;

            UseableInBattle = (bool)loadData.GetValue(itemID, ConstTerm.USEABLE + ConstTerm.IN + ConstTerm.BATTLE);
            UseableOutOfBattle = (bool)loadData.GetValue(itemID, ConstTerm.USEABLE + ConstTerm.OUT_OF + ConstTerm.BATTLE);
            UseableOnDead = (bool)loadData.GetValue(itemID, ConstTerm.USEABLE + ConstTerm.DEAD);
            CanStack = (bool)loadData.GetValue(itemID, ConstTerm.CAN_STACK);

            // AddedState?.SetRestrictions(loadData, itemID);
        }
    }
}