using Godot;
using System;

namespace ZAM.Stats
{
    public partial class Experience : Node
    {
        [ExportGroup("Player")]
        [Export] private float expToLevel = 100;
        [Export] private float expToLevelMultiplier = 5;
        [Export] private float expMultiReduction = 0.2f;
        [Export] private float expReduceStartLevel = 2;

        [ExportGroup("Enemy")]
        [Export] private float expOnKill;

        private float expTotal = 0;
        private int charLevel = 1;

        // Delegate Events \\
        [Signal]
        public delegate void onLevelUpEventHandler();

        public void AddExp(float exp)
        {
            expTotal += exp;
            CheckLevelUp();
        }

        public bool CheckLevelUp()
        {
            if (expTotal >= GetExpNextLevel())
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
            expToLevel *= expToLevelMultiplier;

            if (charLevel >= expReduceStartLevel) { expToLevelMultiplier -= expMultiReduction; }
            if (expToLevelMultiplier < 1) { 
                expToLevelMultiplier = 1;
                expMultiReduction = 0;
            }
        }

        public float GetExpOnKill()
        {
            return expOnKill;
        }

        public float GetExpToLevel()
        {
            float toLevel;
            if (expTotal <= expToLevel) { toLevel = expToLevel - expTotal; }
            else { toLevel = expTotal - expToLevel;}
            
            return toLevel;
        }

        public float GetExpNextLevel()
        {
            return expToLevel;
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
        
        public void SetCurrentLevel(int level) // Use DoLevelUp() when possible
        {
            charLevel = level;
        }
    }
}