using Godot;
using System;

using ZAM.Abilities;
using ZAM.Stats;

namespace ZAM.System
{
    public static partial class Formula
    {
        public static float PhysDamage(Battler attacker, Battler defender, CombatAbilities ability)
        {
            float damageValue = 0;
            if (ability != null) { damageValue = ability.NumericValue;}
            
            float offense = attacker.GetStats().GetStatValue(Stat.Strength) + damageValue;
            float defense = defender.GetStats().GetStatValue(Stat.Stamina);
            float totalDamage = Math.Min(0, defense - offense);
            GD.Print(" -- Attack = " + offense + " Defense = " + defense);
            return totalDamage;
        }

        public static float SpellDamage(Battler attacker, Battler defender, CombatAbilities ability)
        {
            float offense = attacker.GetStats().GetStatValue(Stat.Magic) + ability.NumericValue;
            float defense = defender.GetStats().GetStatValue(Stat.Spirit);
            float totalDamage = Math.Min(0, defense - offense);
            GD.Print(" -- Attack = " + offense + " Defense = " + defense);
            return totalDamage;
        }
    }
}