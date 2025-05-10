using System.Reflection.Metadata;
using Godot;
using Godot.Collections;

using ZAM.Managers;

namespace ZAM.Abilities
{
    public partial class SkillList : Node
    {
        // [Export] Resource[] defaultSkills;
        [Export] private string[] defaultSkills;

        private Array<Ability> characterSkills = [];
        private Dictionary<string, Ability> abilityDictionary = [];
        private Dictionary<string, EffectState> stateDictionary = [];

        public override void _Ready()
        {
            IfNull();
            CreateSkillList();
        }

        public void IfNull()
        {
            abilityDictionary = DatabaseManager.Instance.GetAbilityDatabase();
            stateDictionary = DatabaseManager.Instance.GetStateDatabase();
        }

        public void CreateSkillList()
        {
            if (characterSkills.Count != 0) { return; }
            if (defaultSkills == null || defaultSkills.Length <= 0) { return; }

            for (int s = 0; s < defaultSkills.Length; s++)
            {
                Ability nextSkill = abilityDictionary[defaultSkills[s]];
                characterSkills.Add(nextSkill);
            }

            // foreach (Resource skill in defaultSkills) {
            //     characterSkills.Add((CombatAbilities)ResourceLoader.Load(skill.ResourcePath));
            // }
        }

        public void AddSkillToList(Ability skill)
        {
            characterSkills.Add(skill);
        }

        public Array<Ability> GetSkills()
        {
            return characterSkills;
        }

        // public Array<AbilityData> StoreSkills()
        // {
        //     Array<AbilityData> newList = [];

        //     foreach (Ability data in characterSkills) {
        //         AbilityData newAbility = new();

        //         data.StoreDataDetails(ref newAbility);
        //         data.StoreDataMechanics(ref newAbility);
        //         data.StoreDataRestrictions(ref newAbility);

        //         newList.Add(newAbility);
        //     }
        //     return newList;
        // }

        // public void SetSkills(Array<AbilityData> list)
        // {
        //     characterSkills = [];
        //     foreach (AbilityData data in list) {
        //         Ability newAbility = new();
        //         newAbility.SetDetails(data.AbilityName, data. AbilityDescription, data.TargetType, data.TargetArea, data.NumericValue, data.CostValue, data.UniqueID);
        //         newAbility.SetMechanics(data.DamageType, data.CallAnimation);
        //         newAbility.SetRestrictions(data.UseableInBattle, data.UseableOutOfBattle, data.UseableOnDead);

        //         characterSkills.Add(newAbility);
        //     }
        // }

        public void StoreSkills(ConfigFile saveData, string battlerID)
        {
            saveData.SetValue(battlerID, ConstTerm.SKILL + ConstTerm.COUNT, characterSkills.Count);

            int abilityCounter = 0;
            foreach (Ability ability in characterSkills) {
                if (!ability.IsUnique) {
                    saveData.SetValue(battlerID, ConstTerm.SKILL + abilityCounter + ConstTerm.ABILITY + ConstTerm.NAME, ability.AbilityName);
                } else {
                    ability.StoreDetails(saveData, battlerID, ConstTerm.SKILL + abilityCounter);
                    ability.StoreMechanics(saveData, battlerID, ConstTerm.SKILL + abilityCounter);
                    ability.StoreRestrictions(saveData, battlerID, ConstTerm.SKILL + abilityCounter);

                    if (ability.AddedState != null && !ability.AddedState.IsUnique) {
                        saveData.SetValue(battlerID, ConstTerm.SKILL + abilityCounter + ConstTerm.STATE + ConstTerm.NAME, ability.AddedState.StateName);
                    } else {
                        ability.AddedState.StoreData(saveData, battlerID, ConstTerm.SKILL + abilityCounter);
                    }
                }

                if (ability.AddedState != null) {
                    saveData.SetValue(battlerID, ConstTerm.SKILL + abilityCounter + ConstTerm.STATE + ConstTerm.IS + ConstTerm.UNIQUE, ability.AddedState.IsUnique); 
                }
                saveData.SetValue(battlerID, ConstTerm.SKILL + abilityCounter + ConstTerm.IS + ConstTerm.UNIQUE, ability.IsUnique);
                abilityCounter++;
            }
        }

        public void SetSkills(ConfigFile loadData, string battlerID)
        {
            if (!loadData.HasSectionKey(battlerID, ConstTerm.SKILL + ConstTerm.COUNT)) { return; }

            characterSkills = [];
            int skillCount = (int)loadData.GetValue(battlerID, ConstTerm.SKILL + ConstTerm.COUNT);

            for (int s = 0; s < skillCount; s++) {
                Ability newAbility = new();
                bool testUnique = (bool)loadData.GetValue(battlerID, ConstTerm.SKILL + s + ConstTerm.IS + ConstTerm.UNIQUE);
                if (!testUnique) {
                    string abilityName = (string)loadData.GetValue(battlerID, ConstTerm.SKILL + s + ConstTerm.ABILITY + ConstTerm.NAME);
                    newAbility = abilityDictionary[abilityName];
                } else {
                    newAbility.SetDetails(loadData, battlerID, ConstTerm.SKILL + s);
                    newAbility.SetMechanics(loadData, battlerID, ConstTerm.SKILL + s);
                    newAbility.SetRestrictions(loadData, battlerID, ConstTerm.SKILL + s);
                    newAbility.SetIsUnique(true);

                    if (loadData.HasSectionKey(battlerID, ConstTerm.SKILL + s + ConstTerm.STATE + ConstTerm.NAME)) {
                        bool testState = (bool)loadData.GetValue(battlerID, ConstTerm.SKILL + s + ConstTerm.STATE + ConstTerm.IS + ConstTerm.UNIQUE);
                        if (!testState) {
                            string stateName = (string)loadData.GetValue(battlerID, ConstTerm.SKILL + s + ConstTerm.STATE + ConstTerm.NAME);
                            newAbility.SetAddedState(stateDictionary[stateName]);
                        } else {
                            newAbility.AddedState.SetData(loadData, battlerID, ConstTerm.SKILL + s);
                            newAbility.AddedState.SetIsUnique(true);
                        }
                    }
                }

                characterSkills.Add(newAbility);
            }
        }
    }
}