using Godot;

using System.Collections.Generic;

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
        [Export] private Node itemList = null;
        [Export] private Node classList = null;

        public Dictionary<string, Ability> abilityDatabase = [];
        public Dictionary<string, EffectState> stateDatabase = [];
        public Dictionary<string, Item> itemDatabase = [];
        public Dictionary<string, CharClass> classDatabase = [];

        public static DatabaseManager Instance { get; private set; }

        public override void _Ready()
        {
            Instance = this;
            IfNull();
            CreateDatabase();
        }

        private void IfNull()
        {
            abilityList ??= GetNode(ConstTerm.ABILITYDATABASE);
            stateList ??= GetNode(ConstTerm.STATEDATABASE);
            itemList ??= GetNode(ConstTerm.ITEMDATABASE);
            classList ??= GetNode(ConstTerm.CLASSDATABASE);
        }

        private void CreateDatabase()
        {
            for (int a = 0; a < abilityList.GetChildCount(); a++)
            {
                Ability tempAbility = (Ability)abilityList.GetChild(a);
                abilityDatabase[tempAbility.AbilityName] = tempAbility;
            }

            for (int s = 0; s < stateList.GetChildCount(); s++)
            {
                EffectState tempState = (EffectState)stateList.GetChild(s);
                stateDatabase[tempState.StateName] = tempState;
            }

            for (int i = 0; i < itemList.GetChildCount(); i++)
            {
                Item tempItem = (Item)itemList.GetChild(i);
                itemDatabase[tempItem.ItemName] = tempItem;
            }

            for (int c = 0; c < classList.GetChildCount(); c++)
            {
                CharClass tempClass = (CharClass)classList.GetChild(c);
                classDatabase[tempClass.ClassName] = tempClass;
            }
        }

        public Dictionary<string, Ability> GetAbilityDatabase()
        {
            return abilityDatabase;
        }

        public Dictionary<string, EffectState> GetStateDatabase()
        {
            return stateDatabase;
        }

        public Dictionary<string, Item> GetItemDatabase()
        {
            return itemDatabase;
        }

        public Dictionary<string, CharClass> GetClassDatabase()
        {
            return classDatabase;
        }

        public Ability GetDefend()
        {
            return abilityDatabase[ConstTerm.DEFEND];
        }

        public EffectState GetDefendState()
        {
            return stateDatabase[ConstTerm.DEFEND];
        }
    }
}