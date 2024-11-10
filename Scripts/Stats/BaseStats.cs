using Godot;
using System;
using System.Collections.Generic;

using ZAM.Abilities;

namespace ZAM.Stats
{
    public partial class BaseStats : Node
    {
        [Export] private Battler battler = null;
        
        [ExportGroup("Stat Values")]
        [Export] private Modifier [] stats;
        [Export] private Modifier [] levelUpValue;
        [Export] private Modifier [] upVariance;

        [ExportGroup("Options")]
        [Export] private bool shouldUseAddModifiers = true;
        [Export] private bool shouldUseBonusModifiers = true;

        private List<Stat> stat = null;
        private Dictionary<Stat, float> statSheet = null;

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
        }

        //=============================================================================
        // SECTION: Stat Methods
        //=============================================================================
        private void SetupStats()
        {
            stat ??= [];
            for (int s = 0; s < stats.Length; s++)
            {
                stat.Add(stats[s].Stat);
            }
            SetupStatSheet();
        }

        private void SetupStatSheet()
        {
            statSheet ??= [];
            for (int s = 0; s < stat.Count; s++)
            {
                statSheet[stat[s]] = stats[s].Value;
            }
        }

        public void LevelUpStats()
        {
            for (int s = 0; s < levelUpValue.Length; s++)
            {
                if (levelUpValue[s] == null) { continue;} // Stat doesn't provide an increase on leveling

                float incVal = levelUpValue[s].Value;
                if (upVariance[s] != null) {
                    Random variance = new();
                    float lowVal = levelUpValue[s].Value - upVariance[s].Value;
                    float highVal = levelUpValue[s].Value + upVariance[s].Value;
                    incVal = variance.Next((int)lowVal, (int)highVal + 1);
                }
                // GD.Print(levelUpValue[s].Stat + " + " + incVal);
                statSheet[levelUpValue[s].Stat] += incVal;
            }
        }

        public float GetStatValue(Stat stat)
        {
            float totalValue = statSheet[stat] * (1 + GetPercetageModifier(stat)) + GetAdditiveModifier(stat);
            // if (stat == Stat.Stamina) { GD.Print(stat + " " + totalValue); }
            return totalValue;
        }

        //=============================================================================
        // SECTION: Save System
        //=============================================================================

        public float[] SaveAllStats()
        {
            float[] saveStats = new float[statSheet.Count];
            for (int n = 0; n < statSheet.Count; n++)
            {
                saveStats[n] = statSheet[stat[n]];
            }
            return saveStats;
        }

        public void LoadAllStats(float[] loadStats)
        {
            for (int n = 0; n < statSheet.Count; n++)
            {
                statSheet[stat[n]] = loadStats[n];
            }
        }

        //=============================================================================
        // SECTION: Stat Adjustments
        //=============================================================================

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