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

        public override void _Ready()
        {
            IfNull();
            CreateSkillList();
        }

        public void IfNull()
        {
            abilityDictionary = DatabaseManager.Instance.GetAbilityDatabase();
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

        public Array<AbilityData> StoreSkills()
        {
            Array<AbilityData> newList = [];

            foreach (Ability data in characterSkills) {
                AbilityData newAbility = new();

                data.SetDataDetails(ref newAbility);
                data.SetDataMechanics(ref newAbility);
                data.SetDataRestrictions(ref newAbility);

                newList.Add(newAbility);
            }
            return newList;
        }

        public void SetSkills(Array<AbilityData> list)
        {
            characterSkills = [];
            foreach (AbilityData data in list) {
                Ability newAbility = new();
                newAbility.SetDetails(data.AbilityName, data. AbilityDescription, data.TargetType, data.TargetArea, data.NumericValue, data.CostValue, data.UniqueID);
                newAbility.SetMechanics(data.DamageType, data.CallAnimation);
                newAbility.SetRestrictions(data.UseableInBattle, data.UseableOutOfBattle, data.UseableOnDead);

                characterSkills.Add(newAbility);
            }
        }
    }
}