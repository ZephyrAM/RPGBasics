using Godot;

using ZAM.Stats;

namespace ZAM.Abilities
{
    [GlobalClass]
    public partial class CombatAbilities : Resource //, IModifierLookup
    {
        [Export] public string AbilityName { get; set; }
        [Export] public string AbilityDescription { get; set;}

        [Export] public string TargetType { get; set; } // Ally or Enemy
        [Export] public string TargetArea{ get; set; } // Single or Group

        [Export] public float NumericValue { get; set; }
        [Export] public string DamageType { get; set; }
        [Export] public string CallAnimation { get; set; }

        [Export] public EffectState AddedState { get; set; }

        // [Export] public Modifier[] AddModifier { get; set; }
        // [Export] public Modifier[] PercentModifier { get; set; }

        // [Export] public Godot.Collections.Array<Stat> StatAdd { get; set;}
        // [Export] public float[] AddValue { get; set;}
        // [Export] public Godot.Collections.Array<Stat> StatPercent { get; set; }
        // [Export] public float[] PercentValue { get; set; }

        // public IEnumerable<float> GetAdditiveModifiers(Stat stat)
        // {
        //     foreach (var modifier in AddModifier)
        //     {
        //         if (modifier.Stat == stat)
        //         {
        //             yield return modifier.Value;
        //         }
        //     }
        //     // for (int i = 0; i < StatAdd.Count; i++)
        //     // {
        //     //     yield return AddValue[i];
        //     // }
        // }

        // public IEnumerable<float> GetPercentageModifiers(Stat stat)
        // {
        //     foreach (var modifier in PercentModifier)
        //     {
        //         if (modifier.Stat == stat)
        //         {
        //             yield return modifier.Value;
        //         }
        //     }
        // }

    }
}