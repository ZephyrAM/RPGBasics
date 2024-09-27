using Godot;
using System;
using System.Collections.Generic;

using ZAM.Abilities;

namespace ZAM.Stats
{
    public partial class BaseStats : Node
    {
        [ExportGroup("Stat Values")]
        [Export] float [] value;

        [ExportGroup("Options")]
        [Export] bool shouldUseAddModifiers = true;
        [Export] bool shouldUseBonusModifiers = true;

        List<Stat> stat = null;
        Battler battler;

        //=============================================================================
        // SECTION: Base Methods
        //=============================================================================
        public override void _Ready()
        {
            SetupStats();
            IfNull();
        }

        //=============================================================================
        // SECTION: OnReady Methods
        //=============================================================================
        private void IfNull()
        {
            battler ??= GetNode<Battler>("../Battler");
            // foreach (int s in stat.Select(v => (int)v)) {
            //     if (value[s] == 0) { value[s] = 2; }
            // }
        }

        //=============================================================================
        // SECTION: Stat Methods
        //=============================================================================
        private void SetupStats()
        {
            stat ??= new List<Stat>();
            foreach (Stat i in Enum.GetValues(typeof(Stat)))
            {
                stat.Add(i);
            }
        }

        public float GetStatValue(Stat stat)
        {
            float totalValue = value[(int)stat] * (1 + GetPercetageModifier(stat)) + GetAdditiveModifier(stat);
            // if (stat == Stat.Stamina) { GD.Print(stat + " " + totalValue); }
            return totalValue;
        }

        private float GetAdditiveModifier(Stat stat)
        {
            if (!shouldUseAddModifiers) { return 0; }
            float total = 0;

            foreach (EffectState source in battler.GetStateList())
            {
                for (int i = 0; i < source.AddModifier.Length; i++)
                {
                    if (source.AddModifier[i].Stat == stat) { total += source.AddModifier[i].Value; }
                }
                // foreach (float modifier in source.GetAdditiveModifiers(stat))
                // {
                //     total += modifier;
                // }
            }
            return total;
        }

        private float GetPercetageModifier(Stat stat)
        {
            if (!shouldUseBonusModifiers) { return 0; }
            float total = 0;
            float modifier;

            foreach (EffectState source in battler.GetStateList())
            {
                for (int i = 0; i < source.PercentModifier.Length; i++)
                {
                    // GD.Print(source.PercentModifier[i].Stat);
                    if (source.PercentModifier[i].Stat == stat) 
                    { 
                        modifier = source.PercentModifier[i].Value; 
                        // GD.Print("Percent Mod = " + modifier);
                        total += modifier;
                    }
                }
                // foreach (float modifier in source.GetPercentageModifiers(stat))
                // {
                //     total += modifier;
                // }
            }
            // foreach (IModifierLookup source in GetNodeOrNull<IModifierLookup>("IModifierLookup"))
            // {
            //     foreach (float modifier in source.GetPercentageModifiers(stat))
            //     {
            //         total += modifier;
            //     }
            // }
            return total / 100;
        }
    }
}