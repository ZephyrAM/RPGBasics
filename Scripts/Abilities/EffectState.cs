using Godot;
using Godot.Collections;

namespace ZAM.Abilities
{
    [GlobalClass]
    public partial class EffectState : Node
    {
        [ExportGroup("Details")]
        [Export] public string StateName { get; private set; }
        [Export] public string StateDescription { get; private set; }

        [ExportGroup("Mechanics")]
        [Export] public Array<Modifier> AddModifier { get; private set; }
        [Export] public Array<Modifier> PercentModifier { get; private set; }

        [ExportGroup("Restrictions")]
        [Export] public bool ExistsOutOfBattle { get; private set; }

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

            for (int m = 0; m < AddModifier.Count; m++) {
                if (AddModifier[m].Stat == stat) {index = m; break; }
            }

            return AddModifier[index].Value;
        }

        public float StatPercentModifier(StatID stat)
        {
            int index = 0;

            for (int m = 0; m < PercentModifier.Count; m++) {
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


        //=============================================================================
        // SECTION: Save System
        //=============================================================================

        public void SetDetails(string name, string description, int id)
        {
            StateName = name;
            StateDescription = description;

            UniqueID = id;
        }

        public void SetMechanics(Array<Modifier> add, Array<Modifier> percent)
        {
            AddModifier = add;
            PercentModifier = percent;
        }

        public void SetRestrictions(bool outOfBattle)
        {
            ExistsOutOfBattle = outOfBattle;
        }

        public void SetDataDetails(EffectStateData data)
        {
            data.StateName = StateName;
            data.StateDescription = StateDescription;

            data.UniqueID = UniqueID;
        }

        public void SetDataMechanics(EffectStateData data)
        {
            data.AddModifier = AddModifier;
            data.PercentModifier = PercentModifier;
        }

        public void SetDataRestrictions(EffectStateData data)
        {
            data.ExistsOutOfBattle = ExistsOutOfBattle;
        }
    }
}