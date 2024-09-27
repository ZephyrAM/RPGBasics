using Godot;

namespace ZAM.Stats
{
    public partial class Health : Node
    {
        [Export] float baseHP = 100;

        float currHP;

        public override void _Ready()
        {
            currHP = baseHP;
        }

        public void ChangeHP(float value)
        {
            currHP += value;
        }

        public float GetHP()
        {
            return currHP;
        }

        public float GetMaxHP()
        {
            return baseHP;
        }
    }
}