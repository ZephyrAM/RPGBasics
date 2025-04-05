using Godot;
using Godot.Collections;
using System;

using ZAM.Abilities;
using ZAM.Inventory;
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
        [Export] private bool shouldUsePercentModifiers = true;

        private Array<StatID> stat = [];
        private Dictionary<StatID, float> statSheet = [];
        private Dictionary<StatID, float> modifiedStatSheet = [];
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
            if (battler.GetBattlerType() == ConstTerm.NPC) { return; }
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
            modifiedStatSheet ??= [];
            for (int s = 0; s < stat.Count; s++)
            {
                statSheet[stat[s]] = stats[s].Value;
                modifiedStatSheet[stat[s]] = stats[s].Value;
            }

            SetupHPMP();
        }

        private void SetupHPMP()
        {
            battler.GetHealth().SetMaxHP(GetMaxHP()); // EDIT: Find better place to load?
            battler.GetHealth().SetMaxMP(GetMaxMP());
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
            float addMod = shouldUseAddModifiers ? GetAdditiveModifier(stat) : 0;
            float percentMod = shouldUsePercentModifiers ? (1 + GetPercetageModifier(stat)) : 1;
            float totalValue = statSheet[stat] * percentMod + addMod;
            // if (stat == Stat.Stamina) { GD.Print(stat + " " + totalValue); }
            // if (battler.GetBattlerType() == ConstTerm.ENEMY) { totalValue *= ConfigSettings.GetDifficulty()} // Create Instance? Just use save file?
            return totalValue;
        }

        public float GetMaxHP()
        {
            return Mathf.Round(GetStatValue(StatID.Stamina) * 10f);
        }

        public float GetMaxMP()
        {
            return Mathf.Round(GetStatValue(StatID.Spirit) * 2f);
        }

        public Dictionary<StatID, float> GetStatSheet()
        {
            return statSheet;
        }

        public Dictionary<StatID, float> GetModifiedStatSheet()
        {
            for (int s = 1; s <= modifiedStatSheet.Count; s++)
            {
                modifiedStatSheet[(StatID)s] = GetStatValue((StatID)s);
            }

            return modifiedStatSheet;
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
            float total = 0;

            foreach (EffectState source in battler.GetStateList()) {
                for (int i = 0; i < source.AddModifier.Length; i++) {
                    if (source.AddModifier[i].Stat == stat) { total += source.AddModifier[i].Value; }
                }
            }

            foreach (EquipSlot source in battler.GetEquipList().GetCharEquipment()) {
                if (source.Equip == null) { continue; }
                for (int i = 0; i < source.Equip.AddModifier.Length; i++) {
                    if (source.Equip.AddModifier[i].Stat == stat) { total += source.Equip.AddModifier[i].Value; }
                }
            }
            return total;
        }

        private float GetPercetageModifier(StatID stat)
        {
            float total = 0;
            float modifier;

            foreach (EffectState source in battler.GetStateList()) {
                for (int i = 0; i < source.PercentModifier.Length; i++) {
                    if (source.PercentModifier[i].Stat == stat) { 
                        modifier = source.PercentModifier[i].Value; 
                        total += modifier;
                    }
                }
            }

            foreach (EquipSlot source in battler.GetEquipList().GetCharEquipment()) {
                if (source.Equip == null) { continue; }
                for (int i = 0; i < source.Equip.PercentModifier.Length; i++) {
                    if (source.Equip.PercentModifier[i].Stat == stat) {
                        modifier = source.Equip.PercentModifier[i].Value;
                        total += modifier;
                    }
                }
            }
            return total / 100;
        }
    }
}