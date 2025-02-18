using Godot;
using System.Collections.Generic;

using ZAM.Managers;

namespace ZAM.System
{
    public partial class Transitions : Area2D
    {
        [Export] private Node2D currentMap = null;
        [Export] private PartyManager playerParty = null;
        [Export] private Node2D spawnPoint = null;
        [Export] private string destinationMapName = null;

        // private List<Area2D> moveZone = [];

        public override void _Ready()
        {
            IfNull();
            // int travelCount = GetChildCount();

            // for (int t = 0; t < travelCount; t++)
            // {
            //     moveZone[t] = (Area2D)GetChild(t);
            //     moveZone[t].BodyEntered += OnBodyEntered;
            // }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            UnSubSignals();
        }

        private void IfNull()
        {
            SubSignals();
        }

        private void SubSignals()
        {
            BodyEntered += OnBodyEntered;
        }

        private void UnSubSignals()
        {
            BodyEntered -= OnBodyEntered;
        }

        public async void MapSceneSwitch(string newScenePath, Node2D oldScene)
        {
            playerParty.ChangePlayerActive(false);

            SaveLoader.Instance.SaveAllData(); // EDIT: Save player team data. Expand on this

            Fader.Instance.FadeOut();
            await ToSignal(Fader.Instance.GetAnimPlayer(), ConstTerm.ANIM_FINISHED);
            
            oldScene.QueueFree();
            // PackedScene moveToScene = ResourceLoader.Load<PackedScene>(newScenePath);
            Node2D moveToMap = ResourceLoader.Load<PackedScene>(newScenePath).Instantiate() as Node2D;
            MapSystem mapSystemNode = moveToMap.GetChild(0) as MapSystem;

            mapSystemNode.GetPartyManager().SetMemberArrays(playerParty.GetPartyArray(), playerParty.GetReserveArray());

            GetTree().Root.AddChild(moveToMap);
            // GetTree().Root.RemoveChild(oldScene);

            SaveLoader.Instance.LoadAllData(SaveLoader.Instance.gameSession);

            mapSystemNode.GetPartyManager().GetPlayer().GlobalPosition = mapSystemNode.GetTransitions()[0].GetSpawnPoint().GlobalPosition;

            Fader.Instance.FadeIn();
            await ToSignal(Fader.Instance.GetAnimPlayer(), ConstTerm.ANIM_FINISHED);
            mapSystemNode.GetPartyManager().ChangePlayerActive(true);
        }

        public Node2D GetSpawnPoint()
        {
            return spawnPoint;
        }

        private void OnBodyEntered(Node2D body)
        {
            if (body == playerParty.GetPlayer())
            { MapSceneSwitch(ConstTerm.MAP_SCENE + destinationMapName + ConstTerm.TSCN, currentMap); }
        }
    }
}