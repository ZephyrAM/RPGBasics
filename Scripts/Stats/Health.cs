using System;
using Godot;

namespace ZAM.Stats
{
    public partial class Health : Node
    {
        private bool isDead = false;

        float maxHP, maxMP;
        float currHP, currMP;

        // public override void _Ready()
        // {
        //     currHP = maxHP;
        //     currMP = maxMP;
        // }

        public void ChangeHP(float value)
        {
            float newHP = Mathf.Max(0, currHP + value);
            currHP = Mathf.Min(newHP, maxHP);

            if (currHP <= 0) { isDead = true; }
        }

        public void ChangeMP(float value)
        {
            float newMP = Mathf.Max(0, currMP + value);
            currMP = Mathf.Min(newMP, maxMP);
        }

        public bool HasEnoughMP(float value)
        {
            return (currMP - value) >= 0;
        }

        public bool IsHPFull()
        {
            return GetHP() >= GetMaxHP();
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

        public float GetHP()
        {
            return currHP;
        }

        public float GetMP()
        {
            return currMP;
        }

        public void SetHP(float value)
        {
            currHP = Mathf.Min(value, maxHP);
        }

        public void SetMP(float value)
        {
            currMP = Mathf.Min(value, maxMP);
        }

        public float GetMaxHP()
        {
            return maxHP;
        }

        public float GetMaxMP()
        {
            return maxMP;
        }

        public void SetMaxHP(float value)
        {
            maxHP = value;
        }

        public void SetMaxMP(float value)
        {
            maxMP = value;
        }
    }
}