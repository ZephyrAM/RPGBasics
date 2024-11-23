using System;
using Godot;

namespace ZAM.Stats
{
    public partial class Health : Node
    {
        [Export] private float maxHP = 100;

        private bool isDead = false;

        float currHP;

        public override void _Ready()
        {
            currHP = maxHP;
        }

        public void ChangeHP(float value)
        {
            float newHP = currHP + value;
            currHP = MathF.Min(newHP, maxHP);

            if (currHP <= 0) { isDead = true; }
        }

        public bool IsDead()
        {
            return isDead;
        }

        public void Revive(float value)
        {
            isDead = false;
            SetHP(value);
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

        public void SetMaxHP(float value)
        {
            maxHP = value;
        }
    }
}