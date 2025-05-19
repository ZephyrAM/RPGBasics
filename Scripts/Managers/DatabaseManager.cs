using Godot;
using Godot.Collections;

using ZAM.Abilities;
using ZAM.Inventory;
using ZAM.Stats;

namespace ZAM.Managers
{
    // Global Object \\
    public partial class DatabaseManager : Node
    {
        [Export] private Node abilityList = null;
        [Export] private Node stateList = null;
        [Export] private Node classList = null;

        [Export] private Node itemList = null;
        [Export] private Node weaponList = null;
        [Export] private Node armorList = null;
        [Export] private Node accessoryList = null;

        private Dictionary<string, Ability> abilityDatabase = [];
        private Dictionary<string, EffectState> stateDatabase = [];
        private Dictionary<string, CharClass> classDatabase = [];

        private Dictionary<string, Item> itemDatabase = [];
        private Dictionary<string, Equipment> weaponDatabase = [];
        private Dictionary<string, Equipment> armorDatabase = [];
        private Dictionary<string, Equipment> accessoryDatabase = [];

        private ulong uniqueIDCounter = 1;

        public static DatabaseManager Instance { get; private set; }

        public override void _Ready()
        {
            Instance = this;
            IfNull();
            CreateDatabase();
        }

        private void IfNull()
        {
            abilityList ??= GetNode(ConstTerm.ABILITY + ConstTerm.DATABASE);
            stateList ??= GetNode(ConstTerm.STATE + ConstTerm.DATABASE);
            classList ??= GetNode(ConstTerm.CLASS + ConstTerm.DATABASE);

            itemList ??= GetNode(ConstTerm.ITEM + ConstTerm.DATABASE);
            weaponList ??= GetNode(ConstTerm.WEAPON + ConstTerm.DATABASE);
            armorList ??= GetNode(ConstTerm.ARMOR + ConstTerm.DATABASE);
            accessoryList ??= GetNode(ConstTerm.ACCESSORY + ConstTerm.DATABASE);
        }

        private void CreateDatabase()
        {
            for (int a = 0; a < abilityList.GetChildCount(); a++) {
                Ability tempAbility = (Ability)abilityList.GetChild(a);
                tempAbility.SetUniqueID(ref uniqueIDCounter);
                abilityDatabase[tempAbility.AbilityName] = tempAbility;
            }

            for (int s = 0; s < stateList.GetChildCount(); s++) {
                EffectState tempState = (EffectState)stateList.GetChild(s);
                tempState.SetUniqueID(ref uniqueIDCounter);
                stateDatabase[tempState.StateName] = tempState;
            }

            for (int c = 0; c < classList.GetChildCount(); c++) {
                CharClass tempClass = (CharClass)classList.GetChild(c);
                tempClass.SetUniqueID(ref uniqueIDCounter);
                classDatabase[tempClass.ClassName] = tempClass;
            }

            for (int i = 0; i < itemList.GetChildCount(); i++) {
                Item tempItem = (Item)itemList.GetChild(i);
                tempItem.SetUniqueID(ref uniqueIDCounter);
                itemDatabase[tempItem.ItemName] = tempItem;
            }

            for (int i = 0; i < weaponList.GetChildCount(); i++) {
                Equipment tempWeapon = (Equipment)weaponList.GetChild(i);
                // tempWeapon.SetUniqueID(ref uniqueIDCounter);
                weaponDatabase[tempWeapon.ItemName] = tempWeapon;
            }

            for (int i = 0; i < armorList.GetChildCount(); i++) {
                Equipment tempArmor = (Equipment)armorList.GetChild(i);
                // tempArmor.SetUniqueID(ref uniqueIDCounter);
                armorDatabase[tempArmor.ItemName] = tempArmor;
            }

            for (int i = 0; i < accessoryList.GetChildCount(); i++) {
                Equipment tempAccessory = (Equipment)accessoryList.GetChild(i);
                // tempAccessory.SetUniqueID(ref uniqueIDCounter);
                accessoryDatabase[tempAccessory.ItemName] = tempAccessory;
            }
        }

        public ref ulong GetUniqueCounter()
        {
            return ref uniqueIDCounter;
        }

        //=============================================================================
        // SECTION: External Access Methods
        //=============================================================================

        public Ability GetDefend()
        { return abilityDatabase[ConstTerm.DEFEND]; }

        public EffectState GetDefendState()
        { return stateDatabase[ConstTerm.DEFEND]; }

        public Dictionary<string, Ability> GetAbilityDatabase()
        { return abilityDatabase; }

        public Dictionary<string, EffectState> GetStateDatabase()
        { return stateDatabase; }

        public Dictionary<string, CharClass> GetClassDatabase()
        { return classDatabase; }

        public Dictionary<string, Item> GetItemDatabase()
        { return itemDatabase; }

        public Dictionary<string, Equipment> GetWeaponDatabase()
        { return weaponDatabase; }

        public Dictionary<string, Equipment> GetArmorDatabase()
        { return armorDatabase; }

        public Dictionary<string, Equipment> GetAccessoryDatabase()
        { return accessoryDatabase; }

        //=============================================================================
        // SECTION: Save System
        //=============================================================================

        // public void OnSaveGame(SavedGame saveData)
        // {

        // }

        // public void OnLoadGame(SystemData loadData)
        // {

        // }

        public void OnSaveFile(ConfigFile saveData)
        {
            saveData.SetValue(ConstTerm.SYSTEM + ConstTerm.DATA, ConstTerm.UNIQUE + ConstTerm.ID + ConstTerm.COUNT, uniqueIDCounter);
        }

        public void OnLoadFile(ConfigFile loadData)
        {
            if (loadData.HasSection(ConstTerm.SYSTEM + ConstTerm.DATA)) {
                uniqueIDCounter = (ulong)loadData.GetValue(ConstTerm.SYSTEM + ConstTerm.DATA, ConstTerm.UNIQUE + ConstTerm.ID + ConstTerm.COUNT);
            }
        }
    }
}