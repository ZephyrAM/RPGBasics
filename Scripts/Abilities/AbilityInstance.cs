using Godot;
using System;

namespace ZAM.Abilities
{
    public partial class AbilityInstance : Resource
    {
        public string AbilityName { get; private set; }
        public string AbilityDescription { get; private set; }

        public string TargetType { get; private set; } // Ally, Enemy, or Self
        public string TargetArea { get; private set; } // Single or Group

        public float NumericValue { get; private set; }
        public float CostValue { get; private set; }

        public string DamageType { get; private set; }
        public string CallAnimation { get; private set; }

        public EffectState AddedState { get; private set; }

        public bool UseableInBattle { get; private set; }
        public bool UseableOutOfBattle { get; private set; }
        public bool UseableOnDead { get; private set; }

        public int UniqueID { get; private set; } = 0;
    }
}