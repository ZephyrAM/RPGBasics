using Godot;
using System;
using System.Collections.Generic;

namespace ZAM.Abilities
{
    public partial class SkillList : Node
    {
        [Export] Resource[] defaultSkills;
        List<CombatAbilities> characterSkills = new();

        public override void _Ready()
        {
            CreateSkillList();
        }

        public void CreateSkillList()
        {
            if (characterSkills.Count != 0) { return; }
            if (defaultSkills == null || defaultSkills.Length <= 0) { return; }

            foreach (Resource skill in defaultSkills) {
                characterSkills.Add((CombatAbilities)ResourceLoader.Load(skill.ResourcePath));
            }
        }

        public List<CombatAbilities> GetSkills()
        {
            return characterSkills;
        }
    }
}