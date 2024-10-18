using Godot;
using System;
using System.Collections.Generic;

namespace ZAM.Abilities
{
    public partial class AbilityDatabase : Node
    {
        public Dictionary<string, Ability> abilityDatabase = [];

        public void CreateDatabase()
        {
            for (int a = 0; a < GetChildCount(); a++)
            {
                abilityDatabase[GetChild(a).Name] = (Ability)GetChild(a);
            }
        }

        public Dictionary<string, Ability> GetDatabase()
        {
            return abilityDatabase;
        }
    }
}