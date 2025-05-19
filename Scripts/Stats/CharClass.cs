using Godot;
using System;

using ZAM.Abilities;

namespace ZAM.Stats
{
    public partial class CharClass : Node
    {
        [ExportGroup("Details")]
        [Export] public string ClassName { get; private set; }
        [Export] public string ClassDescription { get; private set; }

        [ExportGroup("Growth")] // EDIT: Automate this! The Editor is not reliable for this level of data!
        [Export] public Modifier[] LevelUpValue { get; private set; } = new Modifier[Enum.GetValues(typeof(StatID)).Length];
        [Export] public Modifier[] LevelUpVariance { get; private set; } = new Modifier[Enum.GetValues(typeof(StatID)).Length];

        // [Export] public LearnSkill[] LearnSkills { get; private set; }

        public ulong UniqueID { get; private set; } = 0;

        public void SetUniqueID(ref ulong id)
        {
            if (UniqueID != 0) { GD.PushWarning("Attempting to re-declare UniqueID for " + ClassName); return; }
            UniqueID = id;
            id++;
        }
    }
}