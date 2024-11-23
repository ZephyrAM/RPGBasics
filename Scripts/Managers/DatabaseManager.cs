using Godot;

using System.Collections.Generic;

using ZAM.Abilities;
using ZAM.Inventory;

namespace ZAM.Managers
{
    // Global Object \\
    public partial class DatabaseManager : Node
    {
        [Export] private Node abilityList = null;
        [Export] private Node stateList = null;
        [Export] private Node itemList = null;

        public Dictionary<string, Ability> abilityDatabase = [];
        public Dictionary<string, EffectState> stateDatabase = [];
        public Dictionary<string, Item> itemDatabase = [];

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
                stateDatabase[stateList.GetChild(s).Name] = (EffectState)stateList.GetChild(s);
            }

            for (int i = 0; i < itemList.GetChildCount(); i++)
            {
                itemDatabase[itemList.GetChild(i).Name] = (Item)itemList.GetChild(i);
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