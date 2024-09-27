using Godot;
using System;
using ZAM.Control;

namespace ZAM.System
{
    public partial class MapSystem : Node
    {
        [Export] PackedScene battleScene;
        [Export] PartyManager playerParty;
        // [Export] Fader screenFader;

        BattleSystem battleNode;

        public override void _Ready()
        {
            IfNull();
            // battleNode.onBuildPlayerTeam += BuildParty;
            // screenFader.onTransitionFinished +=
        }

        private void IfNull()
        {
            battleScene ??= ResourceLoader.Load<PackedScene>("res://Scenes/BattleScene.tscn");
            playerParty ??= GetNode<PartyManager>(ConstTerm.PARTYMANAGER);
        }

        // public override void _EnterTree()
        // {
        //     GD.Print("Map Tree Enter");
        // }

        // public override void _ExitTree()
        // {
        //     battleNode.onBuildPlayerTeam -= BuildParty;
        // }

        public async void LoadBattle()
        {
            battleNode = (BattleSystem)battleScene.Instantiate();
            battleNode.StoreMapScene(this);
            Fader.Instance.Transition();
            await ToSignal(Fader.Instance, "onTransitionFinished");
            GetTree().Root.AddChild(battleNode);

            GetTree().Root.RemoveChild(this);
            // QueueFree();
        }

        public PartyManager GetPartyManager()
        {
            return playerParty;
        }

        // public void BuildParty()
        // {
        //     battleNode.BuildPlayerTeam(playerParty);
        // }
    }
}