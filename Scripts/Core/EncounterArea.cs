using Godot;
using Godot.Collections;
using System;

using ZAM.Controller;
using ZAM.Managers;

namespace ZAM.Core
{
    public partial class EncounterArea : Area2D
    {
        [Export] PartyManager playerParty;
        [Export] Array<PackedScene> enemyGroups = [];
        [Export] int encounterFrequency = 30; // Percent each second to proc a battle.

        // Area2D encounterArea;
        CharacterController playerInput;
        int battleCounter = 0;

        // Delegate Events \\
        [Signal]
        public delegate void onBattleTriggerEventHandler(PackedScene battleGroup);

        //=============================================================================
        // SECTION: Base Methods
        //=============================================================================

        public override void _Ready()
        {
            // encounterArea = this;
        }
        
        public override void _EnterTree()
        {
            IfNull();
        }

        protected override void Dispose(bool disposing)
        {
            UnSubSignals();
            base.Dispose(disposing);
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
            playerParty ??= GetNode<PartyManager>("../../" + ConstTerm.PARTYMANAGER);

            if (!playerParty.IsNodeReady()) { await ToSignal(playerParty, SignalName.Ready); }

            playerInput = playerParty.GetChild(0).GetNode<CharacterController>(ConstTerm.CHARACTER + ConstTerm.CONTROLLER);
            SubSignals();
        }

        private void SubSignals()
        {
            // BodyEntered += OnBodyEntered;
            playerInput.onStepArea += OnStepArea;
        }

        private void UnSubSignals()
        {
            // BodyEntered -= OnBodyEntered;
            playerInput.onStepArea -= OnStepArea;
        }

        private PackedScene GetEnemyGroup()
        {
            Random random = new();
            int groupNum = random.Next(0, enemyGroups.Count);
            
            return enemyGroups[groupNum];
        }

        private bool CheckBattleEncounter()
        {
            GD.Print("Check battle encounter");
            Random random = new();
            int chance = random.Next(1, 101); // Number between 1 - 100

            return chance < encounterFrequency + 1; // encounterFrequency between 0 - 100. 0 Never true, 100 always true.
        }

        //=============================================================================
        // SECTION: Delegate Methods
        //=============================================================================

        // private void OnBodyEntered(Node2D body)
        // {
        //     // GD.Print("Entering!");
        // }

        private void OnStepArea()
        {
            if (OverlapsBody(playerInput.GetCharBody())) 
            { 
                // battleCounter++;
                // if (battleCounter == encounterFrequency)
                if (CheckBattleEncounter())
                {
                    // GD.Print("-- Encounter!");
                    battleCounter = 0;
                    EmitSignal(SignalName.onBattleTrigger, GetEnemyGroup());
                }
            }
        }
    }
}