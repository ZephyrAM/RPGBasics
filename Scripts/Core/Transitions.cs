using Godot;

using ZAM.Managers;

namespace ZAM.Core
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
            SubSignals();
            // int travelCount = GetChildCount();

            // for (int t = 0; t < travelCount; t++)
            // {
            //     moveZone[t] = (Area2D)GetChild(t);
            //     moveZone[t].BodyEntered += OnBodyEntered;
            // }
        }

        protected override void Dispose(bool disposing)
        {
            UnSubSignals();
        }

        private void SubSignals()
        {
            BodyEntered += OnBodyEntered;
        }

        private void UnSubSignals()
        {
            BodyEntered -= OnBodyEntered;
        }

        public async void LoadSavedScene(SceneTree sceneTree, Node oldScene)
        {
            if (oldScene.GetChild(0) is MapSystem) { oldScene.GetNode<MapSystem>(ConstTerm.MAPSYSTEM).StopAllChase(); }

            SaveLoader.Instance.gameFile = SaveLoader.Instance.LoadGameInfo();
            // SaveLoader.Instance.LoadGame();

            string newScenePath = ConstTerm.MAP_SCENE +
            (MapID)(int)SaveLoader.Instance.gameFile.GetValue(ConstTerm.SYSTEM + ConstTerm.DATA, ConstTerm.SAVED + ConstTerm.MAP + ConstTerm.ID) + ConstTerm.TSCN;

            Node moveToMap = ResourceLoader.Load<PackedScene>(newScenePath).Instantiate();
            MapSystem mapSystemNode = moveToMap.GetNode<MapSystem>(ConstTerm.MAPSYSTEM);

            Fader.Instance.FadeOut();
            BGMPlayer.Instance.FadeInBGM(mapSystemNode.GetBGM());
            await ToSignal(Fader.Instance.GetAnimPlayer(), ConstTerm.ANIM_FINISHED);

            // mapSystemNode.GetPartyManager().LoadLeader();
            // mapSystemNode.GetPartyManager().SetMemberArrays(gameInfo.PartyData.PartyMembers, gameInfo.PartyData.ReserveMembers);

            mapSystemNode.GetPartyManager().LoadPartyPaths(SaveLoader.Instance.gameFile);

            sceneTree.Root.AddChild(moveToMap);
            sceneTree.Root.RemoveChild(oldScene);
            oldScene.QueueFree();
            await SaveLoader.Instance.LoadAllData(true);

            Fader.Instance.FadeIn();
            await ToSignal(Fader.Instance.GetAnimPlayer(), ConstTerm.ANIM_FINISHED);

            // mapSystemNode.GetPartyManager().ChangePlayerActive(true);
        }

        public async void MapSceneSwitch(string newScenePath, Node2D oldScene)
        {
            oldScene.GetNode<MapSystem>(ConstTerm.MAPSYSTEM).StopAllChase();
            playerParty.ChangePlayerActive(false);

            if (Fader.Instance.GetAnimPlayer().IsPlaying()) { await ToSignal(Fader.Instance.GetAnimPlayer(), ConstTerm.ANIM_FINISHED); }

            await SaveLoader.Instance.SaveAllData(false);

            Node2D moveToMap = ResourceLoader.Load<PackedScene>(newScenePath).Instantiate() as Node2D;
            MapSystem mapSystemNode = moveToMap.GetChild(0) as MapSystem;

            AudioStream newBgm = mapSystemNode.GetBGM();
            AudioStream oldBgm = oldScene.GetNode<MapSystem>(ConstTerm.MAPSYSTEM).GetBGM();
            
            Fader.Instance.FadeOut();
            BGMPlayer.Instance.TransitionBGM(oldBgm, newBgm);
            await ToSignal(Fader.Instance.GetAnimPlayer(), ConstTerm.ANIM_FINISHED);

            // PackedScene moveToScene = ResourceLoader.Load<PackedScene>(newScenePath);

            // Set party before adding mapNode to tree.
            // foreach (CharacterBody2D member in playerParty.GetChildren().Cast<CharacterBody2D>()) {
            //     member.Reparent(mapSystemNode.GetPartyManager());
            //     // member.Owner = mapSystemNode.GetPartyManager();
            // }
            mapSystemNode.GetPartyManager().LoadPartyPaths(SaveLoader.Instance.gameFile);

            GetTree().Root.AddChild(moveToMap);
            GetTree().Root.RemoveChild(oldScene);
            oldScene.QueueFree();
            await SaveLoader.Instance.LoadAllData(false);

            mapSystemNode.GetPartyManager().GetPlayer().GetCharBody().GlobalPosition = mapSystemNode.GetTransitions()[destinationID].GetSpawnPoint().GlobalPosition;

            Fader.Instance.FadeIn();
            await ToSignal(Fader.Instance.GetAnimPlayer(), ConstTerm.ANIM_FINISHED);

            // mapSystemNode.GetPartyManager().ChangePlayerActive(true);
        }

        public Node2D GetSpawnPoint()
        {
            return spawnPoint;
        }

        private void OnBodyEntered(Node2D body)
        {
            if (body == playerParty.GetPlayer().GetCharBody())
            { MapSceneSwitch(ConstTerm.MAP_SCENE + destinationMapName.ToString() + ConstTerm.TSCN, currentMap); }
        }
    }
}