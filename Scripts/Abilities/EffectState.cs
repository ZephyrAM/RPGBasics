using Godot;
using Godot.Collections;

namespace ZAM.Abilities
{
    [GlobalClass]
    public partial class EffectState : Node
    {
        [Export] public string StateName { get; private set; }
        [Export] public string StateDescription { get; private set; }
        
        [Export] public Modifier[] AddModifier { get; private set; }
        [Export] public Modifier[] PercentModifier { get; private set; }

        public int UniqueID { get; private set; } = 0;

        public void SetUniqueID(ref int id)
        {
            if (UniqueID != 0) { GD.PushWarning("Attempting to re-declare UniqueID for " + StateName); return; }
            UniqueID = id;
            id++;
        }

        //=============================================================================
        // SECTION: Group Call
        //=============================================================================

        public float StatAddModifier(StatID stat)
        {
            int index = 0;

            for (int m = 0; m < AddModifier.Length; m++) {
                if (AddModifier[m].Stat == stat) {index = m; break; }
            }

            return AddModifier[index].Value;
        }

        public float StatPercentModifier(StatID stat)
        {
            int index = 0;

            for (int m = 0; m < PercentModifier.Length; m++) {
                if (PercentModifier[m].Stat == stat) {index = m; break; }
            }
            
            return PercentModifier[index].Value;
        }

        // public Dictionary<StatID, float> StatAddModifier()
        // {
        //     Dictionary<StatID, float> statSheet = [];

        //     for (int s = 0; s < AddModifier.Length; s++) {
        //         statSheet[AddModifier[s].Stat] = AddModifier[s].Value;
        //     }
        //     return statSheet;
        // }

        // public Dictionary<StatID, float> StatPercentModifier()
        // {
        //     Dictionary<StatID, float> statSheet = [];

        //     for (int s = 0; s < PercentModifier.Length; s++) {
        //         statSheet[PercentModifier[s].Stat] *= PercentModifier[s].Value;
        //     }
        //     return statSheet;
        // }
    }
}