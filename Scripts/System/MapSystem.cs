using Godot;
using System;

using ZAM.Control;
using ZAM.Stats;

namespace ZAM.System
{
    public partial class MapSystem : Node
    {
        [Export] PackedScene battleScene;
        [Export] PartyManager playerParty;

        BattleSystem battleNode;

        //=============================================================================
        // SECTION: Base Methods
        //=============================================================================

        public override void _Ready()
        {
            IfNull();
            // SubSignals();
            SaveLoader.Instance.gameSession.CharData = SaveLoader.Instance.GatherBattlers(); // Should only happen once, when game loads.
        }

        //=============================================================================
        // SECTION: OnReady Methods
        //=============================================================================

        private void IfNull()
        {
            battleScene ??= ResourceLoader.Load<PackedScene>(ConstTerm.BATTLE_SCENE);
            playerParty ??= GetNode<PartyManager>(ConstTerm.PARTYMANAGER);
        }

        // private void SubSignals()
        // {
        //     // playerParty.GetPlayer().onSaveGame += OnSaveGame;
        // }

        // public override void _EnterTree()
        // {
        //     GD.Print("Map Tree Enter");
        // }

        // public override void _ExitTree()
        // {
        //     battleNode.onBuildPlayerTeam -= BuildParty;
        // }

        //=============================================================================
        // SECTION: Access Methods
        //=============================================================================

        public async void LoadBattle()
        {
            playerParty.ChangePlayerActive(false);
            Fader.Instance.Transition();
            await ToSignal(Fader.Instance, ConstTerm.TRANSITION_FINISHED);

            battleNode = (BattleSystem)battleScene.Instantiate();
            battleNode.StoreMapScene(this);

            GetTree().Root.AddChild(battleNode);
            GetTree().Root.RemoveChild(this);

            // QueueFree();
        }

        public PartyManager GetPartyManager()
        {
            return playerParty;
        }

        // private void OnSaveGame()
        // {

        // }
    }
}