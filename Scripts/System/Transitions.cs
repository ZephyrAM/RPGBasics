using System;
using Godot;

using ZAM.Managers;

namespace ZAM.System
{
    public partial class Transitions : Area2D
    {
        [Export] private Node2D currentMap = null;
        [Export] private PartyManager playerParty = null;
        [Export] private Node2D spawnPoint = null;
        [Export] private MapID destinationMapName = MapID.UNDEFINED;
        [Export] private int destinationID = 0;

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

        // protected override void Dispose(bool disposing)
        // {
        //     base.Dispose(disposing);
        //     UnSubSignals();
        // }

        private void IfNull()
        {
            SubSignals();
        }

        private void SubSignals()
        {
            BodyEntered += OnBodyEntered;
        }

        // private void UnSubSignals()
        // {
        //     BodyEntered -= OnBodyEntered;
        // }

        public async void LoadSavedScene(SceneTree sceneTree, Node oldScene)
        {
            SavedGame gameInfo = SaveLoader.Instance.LoadGameInfo();

            string newScenePath = ConstTerm.MAP_SCENE + gameInfo.SystemData.SceneName + ConstTerm.TSCN;

            Node moveToMap = ResourceLoader.Load<PackedScene>(newScenePath).Instantiate();
            MapSystem mapSystemNode = moveToMap.GetNode<MapSystem>(ConstTerm.MAPSYSTEM);

            Fader.Instance.FadeOut();
            BGMPlayer.Instance.FadeInBGM(mapSystemNode.GetBGM());
            await ToSignal(Fader.Instance.GetAnimPlayer(), ConstTerm.ANIM_FINISHED);

            oldScene.QueueFree();
            mapSystemNode.GetPartyManager().SetMemberArrays(gameInfo.PartyData.PartyMembers, gameInfo.PartyData.ReserveMembers);

            sceneTree.Root.AddChild(moveToMap);
            await SaveLoader.Instance.LoadAllData(gameInfo);

            Fader.Instance.FadeIn();
            await ToSignal(Fader.Instance.GetAnimPlayer(), ConstTerm.ANIM_FINISHED);

            // mapSystemNode.GetPartyManager().ChangePlayerActive(true);
        }

        public async void MapSceneSwitch(string newScenePath, Node2D oldScene)
        {
            oldScene.GetNode<MapSystem>(ConstTerm.MAPSYSTEM).StopAllChase();
            playerParty.ChangePlayerActive(false);

            if (Fader.Instance.GetAnimPlayer().IsPlaying()) { await ToSignal(Fader.Instance.GetAnimPlayer(), ConstTerm.ANIM_FINISHED); }

            await SaveLoader.Instance.SaveAllData();

            Node2D moveToMap = ResourceLoader.Load<PackedScene>(newScenePath).Instantiate() as Node2D;
            MapSystem mapSystemNode = moveToMap.GetChild(0) as MapSystem;

            AudioStream newBgm = mapSystemNode.GetBGM();
            AudioStream oldBgm = oldScene.GetNode<MapSystem>(ConstTerm.MAPSYSTEM).GetBGM();
            
            Fader.Instance.FadeOut();
            BGMPlayer.Instance.TransitionBGM(oldBgm, newBgm);
            await ToSignal(Fader.Instance.GetAnimPlayer(), ConstTerm.ANIM_FINISHED);
            
            // PackedScene moveToScene = ResourceLoader.Load<PackedScene>(newScenePath);

            mapSystemNode.GetPartyManager().SetMemberArrays(playerParty.GetPartyArray(), playerParty.GetReserveArray());

            oldScene.QueueFree();
            GetTree().Root.AddChild(moveToMap);
            // GetTree().Root.RemoveChild(oldScene);

            await SaveLoader.Instance.LoadAllData(SaveLoader.Instance.gameSession);

            mapSystemNode.GetPartyManager().GetPlayer().GlobalPosition = mapSystemNode.GetTransitions()[destinationID].GetSpawnPoint().GlobalPosition;

            Fader.Instance.FadeIn();
            await ToSignal(Fader.Instance.GetAnimPlayer(), ConstTerm.ANIM_FINISHED);
            oldScene.QueueFree();
        }

        public Node2D GetSpawnPoint()
        {
            return spawnPoint;
        }

        private void OnBodyEntered(Node2D body)
        {
            if (body == playerParty.GetPlayer())
            { MapSceneSwitch(ConstTerm.MAP_SCENE + destinationMapName.ToString() + ConstTerm.TSCN, currentMap); }
        }
    }
}