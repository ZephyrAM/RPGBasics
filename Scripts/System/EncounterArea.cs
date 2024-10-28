using Godot;
using Godot.Collections;
using System;

using ZAM.Control;

namespace ZAM.System
{
    public partial class EncounterArea : Area2D
    {
        [Export] PartyManager playerParty;
        [Export] Array<PackedScene> enemyGroups = [];
        [Export] int encounterFrequency = 30; // Percent each second to proc a battle.

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

        private PackedScene GetEnemyGroup()
        {
            Random random = new();
            int groupNum = random.Next(0, enemyGroups.Count);
            
            return enemyGroups[groupNum];
        }

        private bool CheckBattleEncounter()
        {
            Random random = new();
            int chance = random.Next(0, 100);

            return chance < encounterFrequency;
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
                // battleCounter++;
                // if (battleCounter == encounterFrequency)
                if (CheckBattleEncounter())
                {
                    GD.Print("-- Encounter!");
                    battleCounter = 0;
                    mapSystem.LoadBattle(GetEnemyGroup());
                }
            }
        }
    }
}