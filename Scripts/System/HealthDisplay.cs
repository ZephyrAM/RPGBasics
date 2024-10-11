using Godot;

using ZAM.Stats;

namespace ZAM.System
{
    public partial class HealthDisplay : CenterContainer
    {
        [ExportGroup("Nodes")]
        // [Export] Battler charBattler;
        [Export] TextureProgressBar healthBar;

        [ExportGroup("Variables")]
        [Export] string battlerName;
        [Export] string healthBarName;

        Battler charBattler;
        Health battlerHealth;

        // Basic Methods \\
        public override void _Ready()
        {
            IfNull();
            // SetupHealthBar();
        }

        private void IfNull()
        {
            battlerName ??= ConstTerm.BATTLER;
            healthBarName ??= ConstTerm.HEALTH_BAR;

            // charBattler ??= GetNode<Battler>("../" + battlerName);
            healthBar ??= GetNode<TextureProgressBar>(healthBarName);
        }

        private void SetupHealthBar()
        {
            battlerHealth = charBattler.GetHealth();
            healthBar.MaxValue = battlerHealth.GetMaxHP();
        }

        public override void _Process(double delta)
        {
            // GlobalRotation = 0;
            UpdateHealthBar();
        }

        public void SetBattler(Battler setBattler)
        {
            charBattler = setBattler;
            SetupHealthBar();
        }

        public void ForceHealthBarUpdate()
        {
            healthBar.Value = battlerHealth.GetHP();
        }

        public void UpdateHealthBar()
        {
            float currHP = battlerHealth.GetHP();

            if (healthBar.Value != currHP)
            {
                if (healthBar.Value > currHP)
                { healthBar.Value -= 0.5; }
                else if (healthBar.Value < currHP) 
                { healthBar.Value += 0.5; }
            }
        }
    }
}