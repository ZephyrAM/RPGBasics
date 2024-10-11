using System;
using Godot;

namespace ZAM.Stats
{
    public partial class Health : Node
    {
        [Export] private float maxHP = 100;

        float currHP;

        public override void _Ready()
        {
            currHP = maxHP;
        }

        public void ChangeHP(float value)
        {
            float newHP = currHP + value;
            currHP = MathF.Min(newHP, maxHP);
        }

        public void SetHP(float value)
        {
            float newHP = value;
            currHP = MathF.Min(newHP, maxHP);
        }

        public float GetHP()
        {
            return currHP;
        }

        public float GetMaxHP()
        {
            return maxHP;
        }
    }
}