using Godot;
using System.Collections.Generic;

using ZAM.Managers;

namespace ZAM.Abilities
{
    public partial class SkillList : Node
    {
        // [Export] Resource[] defaultSkills;
        [Export] private string[] defaultSkills;

        private Godot.Collections.Array<Ability> characterSkills = [];
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

        public Godot.Collections.Array<Ability> GetSkills()
        {
            return characterSkills;
        }
        
        public void SetSkills(Godot.Collections.Array<Ability> list)
        {
            characterSkills = list;
            // characterSkills = new();
            // for (int s = 0; s < list.Count; s++)
            // {
            //     characterSkills.Add((Ability)list[s]);
            // }
        }
    }
}