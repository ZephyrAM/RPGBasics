using Godot;
using System;
using System.Collections.Generic;

using ZAM.Abilities;
using ZAM.Managers;

namespace ZAM.Stats
{
    public partial class BaseStats : Node
    {
        [Export] private Battler battler = null;
        
        [ExportGroup("Stat Values")]
        [Export] private Modifier [] stats;

        [ExportGroup("Options")]
        [Export] private bool shouldUseAddModifiers = true;
        [Export] private bool shouldUseBonusModifiers = true;

        private List<StatID> stat = null;
        private Dictionary<StatID, float> statSheet = null;
        private Dictionary<string, CharClass> classDatabase = [];
        private float[] diffMod = [0.75f, 1f, 1.25f, 1.75f];

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
            classDatabase = DatabaseManager.Instance.GetClassDatabase();
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
            CharClass battlerClass = classDatabase[Enum.GetName(typeof(ClassID), battler.GetCharClass())];
            for (int s = 0; s < battlerClass.LevelUpValue.Length; s++)
            {
                if (battlerClass.LevelUpValue[s] == null) { continue;} // Stat doesn't provide an increase on leveling

                float incVal = battlerClass.LevelUpValue[s].Value;
                if (battlerClass.LevelUpVariance[s] != null) {
                    Random variance = new();
                    float lowVal = battlerClass.LevelUpValue[s].Value - battlerClass.LevelUpVariance[s].Value;
                    float highVal = battlerClass.LevelUpValue[s].Value + battlerClass.LevelUpVariance[s].Value;
                    incVal = variance.Next((int)lowVal, (int)highVal + 1);
                }
                // GD.Print(battlerClass.LevelUpValue[s].Stat + " + " + incVal);
                statSheet[battlerClass.LevelUpValue[s].Stat] += incVal;
            }
        }

        public float GetStatValue(StatID stat)
        {
            float totalValue = statSheet[stat] * (1 + GetPercetageModifier(stat)) + GetAdditiveModifier(stat);
            // if (stat == Stat.Stamina) { GD.Print(stat + " " + totalValue); }
            // if (battler.GetBattlerType() == ConstTerm.ENEMY) { totalValue *= ConfigSettings.GetDifficulty()} // Create Instance? Just use save file?
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

        private float GetAdditiveModifier(StatID stat)
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

        private float GetPercetageModifier(StatID stat)
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