using System.Collections.Generic;

namespace ZAM.Stats
{
    public interface IModifierLookup
    {
        // float AddValues { get; set; }
        // float PercentValues { get; set; }
        
        IEnumerable<float> GetAdditiveModifiers(Stat stat);
        IEnumerable<float> GetPercentageModifiers(Stat stat);
    }
}