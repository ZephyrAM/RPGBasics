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

        public ulong UniqueID { get; private set; } = 0;
        public bool IsUnique { get; private set; } = false;

        public void SetUniqueID(ref ulong id)
        {
            if (UniqueID != 0) { GD.PushWarning("Attempting to re-declare UniqueID for " + StateName); return; }
            UniqueID = id;
            id++;
        }

        public void SetIsUnique(bool value)
        {
            IsUnique = value;
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

        // public void SetDetails(string name, string description, ulong id)
        // {
        //     StateName = name;
        //     StateDescription = description;

        //     UniqueID = id;
        // }

        // public void SetMechanics(Array<Modifier> add, Array<Modifier> percent)
        // {
        //     AddModifier = add;
        //     PercentModifier = percent;
        // }

        // public void SetRestrictions(bool outOfBattle)
        // {
        //     ExistsOutOfBattle = outOfBattle;
        // }

        // public void StoreDataDetails(EffectStateData data)
        // {
        //     data.StateName = StateName;
        //     data.StateDescription = StateDescription;

        //     data.UniqueID = UniqueID;
        // }

        // public void StoreDataMechanics(EffectStateData data)
        // {
        //     data.AddModifier = AddModifier;
        //     data.PercentModifier = PercentModifier;
        // }

        // public void StoreDataRestrictions(EffectStateData data)
        // {
        //     data.ExistsOutOfBattle = ExistsOutOfBattle;
        // }

        public void StoreData(ConfigFile saveData, string textID, string extraID = "")
        {
            StoreDetails(saveData, textID, extraID);
            StoreMechanics(saveData, textID, extraID);
            StoreRestrictions(saveData, textID, extraID);
        }

        public void SetData(ConfigFile loadData, string textID, string extraID = "")
        {
            SetDetails(loadData, textID, extraID);
            SetMechanics(loadData, textID, extraID);
            SetRestrictions(loadData, textID, extraID);
        }

        public void StoreDetails(ConfigFile saveData, string textID, string extraID = "")
        {
            saveData.SetValue(textID, extraID + ConstTerm.STATE + ConstTerm.NAME, StateName);
            saveData.SetValue(textID, extraID + ConstTerm.STATE + ConstTerm.DESCRIPTION, StateDescription);
            saveData.SetValue(textID, extraID + ConstTerm.STATE + ConstTerm.UNIQUE + ConstTerm.ID, UniqueID);
        }

        public void StoreMechanics(ConfigFile saveData, string textID, string extraID = "")
        {
            if (AddModifier.Count > 0) {
                saveData.SetValue(textID, extraID + ConstTerm.ADD + ConstTerm.MODIFIER + ConstTerm.COUNT, AddModifier.Count);
                for (int a = 0; a < AddModifier.Count; a++) {
                    AddModifier[a].StoreModifier(saveData, textID, extraID + ConstTerm.ADD + a);
                }
            }
            if (PercentModifier.Count > 0) {
                saveData.SetValue(textID, extraID + ConstTerm.PERCENT + ConstTerm.MODIFIER + ConstTerm.COUNT, PercentModifier.Count);
                for (int p = 0; p < PercentModifier.Count; p++) {
                    PercentModifier[p].StoreModifier(saveData, textID, extraID + ConstTerm.PERCENT + p);
                }
            }
        }

        public void StoreRestrictions(ConfigFile saveData, string textID, string extraID = "")
        {
            saveData.SetValue(textID, extraID + ConstTerm.EXISTS + ConstTerm.OUT_OF + ConstTerm.BATTLE, ExistsOutOfBattle);
        }

        public void SetDetails(ConfigFile loadData, string textID, string extraID = "")
        {
            StateName = (string)loadData.GetValue(textID, extraID + ConstTerm.STATE + ConstTerm.NAME);
            StateDescription = (string)loadData.GetValue(textID, extraID + ConstTerm.STATE + ConstTerm.DESCRIPTION);
            UniqueID = (ulong)loadData.GetValue(textID, extraID + ConstTerm.STATE + ConstTerm.UNIQUE + ConstTerm.ID);
        }

        public void SetMechanics(ConfigFile loadData, string textID, string extraID = "")
        {
            if (loadData.HasSectionKey(textID, extraID + ConstTerm.ADD + ConstTerm.MODIFIER + ConstTerm.COUNT)) {
                int addCount = (int)loadData.GetValue(textID, extraID + ConstTerm.ADD + ConstTerm.MODIFIER + ConstTerm.COUNT);
                if (addCount <= 0) { goto PercentMods; }
                AddModifier = [];
                for (int a = 0; a < addCount; a++) {
                    Modifier addTemp = new();
                    addTemp.SetModifier(loadData, textID, extraID + ConstTerm.ADD + a);
                    AddModifier.Add(addTemp);
                }
            }

            PercentMods:
            if (loadData.HasSectionKey(textID, extraID + ConstTerm.PERCENT + ConstTerm.MODIFIER + ConstTerm.COUNT)) {
                int percentCount = (int)loadData.GetValue(textID, extraID + ConstTerm.PERCENT + ConstTerm.MODIFIER + ConstTerm.COUNT);
                if (percentCount <= 0) { return; }
                PercentModifier = [];
                for (int p = 0; p < percentCount; p++) {
                    Modifier percentTemp = new();
                    percentTemp.SetModifier(loadData, textID, extraID + ConstTerm.PERCENT + p);
                    PercentModifier.Add(percentTemp);
                }
            }
        }

        public void SetRestrictions(ConfigFile loadData, string textID, string extraID = "")
        {
            ExistsOutOfBattle = (bool)loadData.GetValue(textID, extraID + ConstTerm.EXISTS + ConstTerm.OUT_OF + ConstTerm.BATTLE);
        }
    }
}