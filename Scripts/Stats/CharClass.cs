using Godot;
using System;

using ZAM.Abilities;

namespace ZAM.Stats
{
    public partial class CharClass : Node
    {
        [ExportGroup("Details")]
        [Export] public string ClassName { get; set; }
        [Export] public string ClassDescription { get; set; }

        [ExportGroup("Growth")] // EDIT: Automate this! The Editor is not reliable for this level of data!
        [Export] public Modifier[] LevelUpValue { get; set; } = new Modifier[Enum.GetValues(typeof(StatID)).Length];
        [Export] public Modifier[] LevelUpVariance { get; set; } = new Modifier[Enum.GetValues(typeof(StatID)).Length];

        [Export] public LearnSkill[] LearnSkills { get; set; }
    }
}