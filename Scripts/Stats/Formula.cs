using System;

using ZAM.Abilities;

namespace ZAM.Stats
{
    public static partial class Formula
    {
        public static float PhysDamage(Battler attacker, Battler defender, Ability ability)
        {
            float damageValue = 0;
            if (ability != null) { damageValue = ability.NumericValue;}
            
            float offense = attacker.GetStats().GetStatValue(StatID.Strength) + damageValue;
            float defense = defender.GetStats().GetStatValue(StatID.Stamina);
            float totalDamage = Math.Min(0, defense - offense);
            // GD.Print(" -- Attack = " + offense + " Defense = " + defense);
            return totalDamage;
        }

        public static float SpellDamage(Battler attacker, Battler defender, Ability ability)
        {
            float offense = attacker.GetStats().GetStatValue(StatID.Magic) + ability.NumericValue;
            float defense = defender.GetStats().GetStatValue(StatID.Spirit);
            float totalDamage = Math.Min(0, defense - offense);
            // GD.Print(" -- MagicAtk = " + offense + " MagicDef = " + defense);
            return totalDamage;
        }
    }
}