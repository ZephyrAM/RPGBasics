using Godot;

namespace ZAM.Stats
{
    public partial class HealthDisplay : CenterContainer
    {
        [ExportGroup("Nodes")]
        // [Export] Battler charBattler;
        [Export] private TextureProgressBar resourceBar;
        [Export(PropertyHint.Enum, "HP, MP")] private string resourceType = "";

        [ExportGroup("Variables")]
        [Export] private string battlerName;
        [Export] private string healthBarName;

        private Battler charBattler;
        private Health battlerHealth;

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
            resourceBar ??= GetNode<TextureProgressBar>(healthBarName);
        }

        // private void SetupHealthBar()
        // {
        //     resourceBar.MaxValue = battlerHealth.GetMaxHP();
        // }

        // private void SetupResourceBar()
        // {
        //     resourceBar.MaxValue = battlerHealth.GetMaxMP();
        // }

        public override void _Process(double delta)
        {
            // GlobalRotation = 0;
            if (resourceType == ConstTerm.HP) { UpdateHealthBar(); }
            else { UpdateResourceBar(); }
        }

        public void SetBattler(Battler setBattler)
        {
            charBattler = setBattler;
            battlerHealth = charBattler.GetHealth();

            if (resourceType == ConstTerm.HP) { resourceBar.MaxValue = battlerHealth.GetMaxHP(); }
            else { resourceBar.MaxValue = battlerHealth.GetMaxMP(); }
        }

        public void ForceHealthBarUpdate()
        {
            resourceBar.Value = battlerHealth.GetHP();
        }

        public void ForceResourceBarUpdate()
        {
            resourceBar.Value = battlerHealth.GetMP();
        }

        public void UpdateHealthBar()
        {
            float currHP = battlerHealth.GetHP();

            if (resourceBar.Value != currHP)
            {
                if (resourceBar.Value > currHP)
                { resourceBar.Value -= 0.5; }
                else if (resourceBar.Value < currHP) 
                { resourceBar.Value += 0.5; }
            }
        }

        public void UpdateResourceBar()
        {
            float currMP = battlerHealth.GetMP();

            if (resourceBar.Value != currMP)
            {
                if (resourceBar.Value > currMP)
                { resourceBar.Value -= 0.5; }
                else if (resourceBar.Value < currMP)
                { resourceBar.Value += 0.5; }
            }
        }
    }
}