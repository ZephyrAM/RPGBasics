using Godot;

using ZAM.Control;

namespace ZAM.System
{
    public partial class EncounterArea : Area2D
    {
        [Export] PartyManager playerParty;
        [Export] int encounterFrequency = 2;

        MapSystem mapSystem;

        Area2D encounterArea;
        CharacterController playerInput;
        int battleCounter = 0;

        //=============================================================================
        // SECTION: Base Methods
        //=============================================================================

        public override void _Ready()
        {
            encounterArea = this;
        }
        public override void _EnterTree()
        {
            IfNull();
        }

        // public override void _ExitTree()
        // {
        //     UnSubSignals();
        // }

        //=============================================================================
        // SECTION: OnReady Methods
        //=============================================================================

        private async void IfNull()
        {
            mapSystem ??= GetTree().Root.GetNode<MapSystem>(ConstTerm.MAPSYSTEM);
            playerParty ??= GetNode<PartyManager>("../" + ConstTerm.PARTYMANAGER);
            await ToSignal(playerParty, SignalName.Ready);

            playerInput = playerParty.GetChild<CharacterController>(0);
            SubSignals();
        }

        private void SubSignals()
        {
            encounterArea.BodyEntered += OnBodyEntered;
            playerInput.onStepArea += OnStepArea;
        }

        private void UnSubSignals()
        {
            encounterArea.BodyEntered -= OnBodyEntered;
            playerInput.onStepArea -= OnStepArea;
        }

        //=============================================================================
        // SECTION: Delegate Methods
        //=============================================================================

        private void OnBodyEntered(Node2D body)
        {
            GD.Print("Entering!");
        }

        private void OnStepArea()
        {
            if (encounterArea.OverlapsBody(playerInput)) 
            { 
                battleCounter++;
                if (battleCounter == encounterFrequency)
                {
                    GD.Print("-- Encounter!");
                    battleCounter = 0;
                    mapSystem.LoadBattle();
                }
            }
        }
    }
}