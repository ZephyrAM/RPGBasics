using Godot;
using System;

namespace ZAM.Control
{
    public partial class MenuController : Node
    {
        private CharacterController playerInput = null;
        private bool isActive = false;

        [Signal]
        public delegate void onMenuCloseEventHandler();

        public override void _Ready()
        {
            
        }

        public override void _PhysicsProcess(double delta)
        {
            if (!isActive) { return; }
            if (Input.IsActionJustPressed(ConstTerm.MENU)) { EmitSignal(SignalName.onMenuClose); }
        }

        public void SetMenuActive(bool active)
        {
            isActive = active;
        }

        public void SetPlayer(CharacterController player)
        {
            playerInput = player;
        }
    }
}