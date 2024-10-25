using Godot;
using System;

namespace ZAM.Stats
{
    public partial class Experience : Node
    {
        [Export] private float[] expToLevel = [100];
        [Export] private float expOnKill;

        private float expTotal = 0;
        private int charLevel = 1;

        // Delegate Events \\
        [Signal]
        public delegate void onLevelUpEventHandler();

        public void AddExp(float exp)
        {
            expTotal += exp;
        }

        public float ExpNeededToLevel()
        {
            return expToLevel[0] * charLevel - expTotal;
        }
        public bool CheckLevelUp()
        {
            if (expTotal >= GetExpToLevel(charLevel))
            {
                DoLevelUp();
                EmitSignal(SignalName.onLevelUp);
                return true;
            }
            return false;
        }

        public void DoLevelUp()
        {
            charLevel++;
        }

        public float GetExpOnKill()
        {
            return expOnKill;
        }

        public float GetExpToLevel(int index)
        {
            // return expToLevel[index];
            return expToLevel[0] * index;
        }

        public float GetTotalExp()
        {
            return expTotal;
        }

        public int GetCurrentLevel()
        {
            return charLevel;
        }

        public void SetExpTotal(float exp)
        {
            expTotal = exp;
        }
        
        public void SetCurrentLevel(int level)
        {
            charLevel = level;
        }
    }
}