using Godot;
using Godot.Collections;

using ZAM.Controller;
using ZAM.Stats;

namespace ZAM.Managers
{
    public partial class PartyManager : Node
    {
        // Assigned Variables \\
        [Export] private Array<PackedScene> partyMembers = [];
        [Export] private Array<PackedScene> reserveMembers = [];
        [Export] private Script playerController = null;
        [Export] private Camera2D playerCamera = null;

        // Setup Variables \\
        private Array<CharacterBody2D> sceneParty = [];
        private Array<CharacterBody2D> sceneReserve = [];

        private Array<string> partyMemberPaths = [];
        private Array<string> reserveMemberPaths = [];

        private CharacterController characterController = null;
        private CharacterBody2D leaderMember = null;
        private Array<Battler> activeParty = [];

        // private bool hasLoaded = false;
        private double timePlayed;
        private int currencyTotal = 0;

        //=============================================================================
        // SECTION: Base Methods
        //=============================================================================

        // public PartyManager(){}

        // public PartyManager(PartyManager oldparty)
        // {
        //     partyMembers = oldparty.partyMembers;
        //     reserveMembers = oldparty.reserveMembers;

        //     playerController = oldparty.playerController;

        //     leaderMember = oldparty.leaderMember;
        //     activeParty = oldparty.activeParty;
        // }

        public override void _Ready()
        {
            ConvertPackedToString();
            // If party doesn't exist, skip creation. Force create after manually assigning member to party.
            // if (partyMembers.Count < 1) { return; }
            // CreateParty();

            // Assure the leader never gets double loaded.
            if (leaderMember == null) { 
                if (partyMembers.Count > 0 || partyMemberPaths.Count > 0) { LoadLeader(); }
                else if (GetChildCount() > 0) { CreateParty(); }
                else { GD.PushError("No party members!"); }
            }
        }

        // public override void _Input(InputEvent @event)
        // {
        //     if (@event.IsActionPressed(ConstTerm.PAUSE) && characterController.IsControlActive()) { // EDIT: Pause/System menu. Skip cutscene menu? Make conditional global?
		// 		GetTree().Paused = !GetTree().Paused;
		// 	}
        // }

        private void ConvertPackedToString()
        {
            if (partyMembers.Count <= 0 || partyMemberPaths.Count > 0) { return;}
            partyMemberPaths = [];
            reserveMemberPaths = [];

            foreach (PackedScene member in partyMembers) {
                partyMemberPaths.Add(member.ResourcePath);
            }
            foreach (PackedScene member in reserveMembers) {
                reserveMemberPaths.Add(member.ResourcePath);
            }
        }

        //=============================================================================
        // SECTION: Utility Methods
        //=============================================================================

        private static Node SafeScriptAssign(Node target, Script scriptAssign) // May shift to shared Utilities script
        {
            // This whole section... \\
            ulong charId = target.GetInstanceId();
            target.SetScript(ResourceLoader.Load(scriptAssign.ResourcePath));
            target = (Node2D)InstanceFromId(charId);

            target._Ready();
            target.SetProcess(true);
            target.SetPhysicsProcess(true);
            target.SetProcessInput(true);
            return target;
            // ... is a blasted mess. \\
        }

        // Party Member Management \\
        // public void LoadPartyMembers(PackedScene[] members)
        // {
        // 	for (int m = 0; m < members.Length; m++)
        // 	{

        // 	}
        // }

        // private void CreateParty()
        // {
        //     activeParty ??= new();
        //     for (int n = 0; n < partyMembers.Length; n++)
        //     {
        //         CharacterBody2D tempMember = partyMembers[n].Instantiate() as CharacterBody2D;
        //         activeParty.Add(tempMember);
        //     }
        // }

        public void LoadLeader()
        {
            activeParty = [];
            CharacterBody2D tempLead;
            
            if (partyMemberPaths.Count > 0) { tempLead = (CharacterBody2D)ResourceLoader.Load<PackedScene>(partyMemberPaths[0]).Instantiate(); }
            else if (partyMembers.Count > 0) { tempLead = (CharacterBody2D)partyMembers[0].Instantiate(); }
            else { GD.PushError("No party members to load!"); return; }

            RemoteTransform2D tempCamera = new() { RemotePath = playerCamera.GetPath() };
            tempLead.AddChild(tempCamera);

            AddChild(tempLead);

            activeParty.Add(GetChild(0).GetNode<Battler>(ConstTerm.BATTLER));

            leaderMember = SafeScriptAssign(tempLead, playerController) as CharacterBody2D;
            leaderMember.GetNode<Label>(ConstTerm.NAME).Visible = false;
            characterController = leaderMember as CharacterController;

            LoadParty();
        }

        private void LoadParty()
        {
            if (partyMemberPaths.Count > 1) {
                for (int p = 1; p < partyMemberPaths.Count; p++) {
                    CharacterBody2D tempMember = (CharacterBody2D)ResourceLoader.Load<PackedScene>(partyMemberPaths[p]).Instantiate();
                    tempMember.Visible = false;
                    tempMember.ProcessMode = ProcessModeEnum.Disabled;
                    AddChild(tempMember);

                    activeParty.Add(GetChild(p).GetNode<Battler>(ConstTerm.BATTLER));
                }
            }
            else if (partyMembers.Count > 1) {
                for (int p = 1; p < partyMembers.Count; p++) {
                    CharacterBody2D tempMember = (CharacterBody2D)partyMembers[p].Instantiate();
                    tempMember.Visible = false;
                    tempMember.ProcessMode = ProcessModeEnum.Disabled;
                    AddChild(tempMember);

                    activeParty.Add(GetChild(p).GetNode<Battler>(ConstTerm.BATTLER));
                }
            }
        }

        private void CreateParty() // EDIT: Add Reserve party.
        {
            activeParty = [];
            sceneParty = [];

            CharacterBody2D tempLead = (CharacterBody2D)GetChild(0);

            if (!tempLead.HasNode(ConstTerm.CAMERA2D)) {
                RemoteTransform2D tempCamera = new RemoteTransform2D
                { RemotePath = playerCamera.GetPath() };
                tempLead.AddChild(tempCamera);
            }

            sceneParty.Add(tempLead);
            activeParty.Add(tempLead.GetNode<Battler>(ConstTerm.BATTLER));
            leaderMember = (CharacterBody2D)SafeScriptAssign(tempLead, playerController);
            leaderMember.GetNode<Label>(ConstTerm.NAME).Visible = false;

            for (int p = 1; p < GetChildCount(); p++)
            {
                CharacterBody2D tempMember = (CharacterBody2D)GetChild(p);
                tempMember.Visible = false;
                tempMember.ProcessMode = ProcessModeEnum.Disabled;
                sceneParty.Add(tempMember);
                activeParty.Add(GetChild(p).GetNode<Battler>(ConstTerm.BATTLER));
            }
        }

        public void ChangePlayerActive(bool change)
        {
            GetPlayer().ChangeActive(change);
        }

        //=============================================================================
        // SECTION: Access Methods
        //=============================================================================

        public CharacterController GetPlayer()
        {
            return (CharacterController)leaderMember;
        }

        public CharacterBody2D SpawnBattleMember(int index)
        {
            return ResourceLoader.Load<PackedScene>(partyMemberPaths[index]).Instantiate() as CharacterBody2D;
        }

        public Array<Battler> GetPlayerParty()
        {
            return activeParty;
        }

        public Array<PackedScene> GetPartyArray()
        {
            return partyMembers;
        }

        public Array<PackedScene> GetReserveArray()
        {
            return reserveMembers;
        }

        public void SetMemberArrays(Array<PackedScene> party, Array<PackedScene> reserve)
        {
            partyMembers = party;
            reserveMembers = reserve;
        }

        public void SetNewParty(Array<Node> party)
        {
            partyMembers = [];
            for (int c = 0; c < party.Count; c++) {
                AddChild(party[c]);
            }
        }

        public int GetPartySize()
        {
            return partyMemberPaths.Count;
            // int partySize = 0;
            // for (int n = 0; n < partyMembers.Count; n++)
            // {
            //     if (partyMembers[n] != null)
            //     { partySize++; }
            // }
            // return partySize;
        }

        public int GetReservePartySize()
        {
            return reserveMemberPaths.Count;
            // int partySize = 0;
            // for (int n = 0; n < reserveMembers.Count; n++)
            // {
            //     if (reserveMembers[n] != null)
            //     { partySize++; }
            // }
            // return partySize;
        }

        //=============================================================================
        // SECTION: Menu Info
        //=============================================================================

        public int[] GetTimePlayed()
        {
            timePlayed = Time.GetUnixTimeFromSystem();

            int[] formatTime = new int[3];
            int seconds = Mathf.RoundToInt(timePlayed % 60);
            int minutes = Mathf.FloorToInt(timePlayed / 60);
            int hours = Mathf.FloorToInt(minutes / 60);

            formatTime[0] = seconds;
            formatTime[1] = minutes;
            formatTime[2] = hours;
            return formatTime;
        }

        public int GetTotalCurrency()
        {
            return currencyTotal;
        }

        public int GetStepsTaken()
        {
            // return playerMover.GetStepsTaken();
            return 0;
        }

        //=============================================================================
        // SECTION: Save System
        //=============================================================================

        // public void OnSaveGame(SavedGame saveData)
        // {
        //     PartyData newData = new()
        //     {
        //         PartyMembers = GetPartyArray(),
        //         ReserveMembers = GetReserveArray(),
        //         MapPosition = GetPlayer().GlobalPosition,
        //         FaceDirection = GetPlayer().GetFaceDirection(),
        //         TotalGold = GetTotalCurrency()
        //     };
            
        //     saveData.PartyData = newData;
        // }

        // public void OnLoadGame(PartyData loadData)
        // {
        //     PartyData saveData = loadData;
        //     if (saveData == null) { GD.Print("Party Data - NULL"); return; }
        //     hasLoaded = true;

        //     partyMembers = saveData.PartyMembers;
        //     reserveMembers = saveData.ReserveMembers;
        //     GetPlayer().GlobalPosition = saveData.MapPosition;
        //     GetPlayer().SetLookDirection(saveData.FaceDirection);
        //     currencyTotal = saveData.TotalGold;

        //     // if (leaderMember == null) { LoadLeader(); }
        // }

        public void OnSaveFile(ConfigFile saveData)
        {
            for (int m = 0; m < GetPartyArray().Count; m++) {
                saveData.SetValue(ConstTerm.MEMBER + ConstTerm.DATA, ConstTerm.MEMBER + m, GetPartyArray()[m].ResourcePath);
            }
            for (int m = 0; m < GetReserveArray().Count; m++) {
                saveData.SetValue(ConstTerm.RESERVE + ConstTerm.DATA, ConstTerm.RESERVE + m, GetReserveArray()[m].ResourcePath);
            }

            saveData.SetValue(ConstTerm.PARTY + ConstTerm.DATA, ConstTerm.MAP + ConstTerm.POSITION, GetPlayer().GlobalPosition);
            saveData.SetValue(ConstTerm.PARTY + ConstTerm.DATA, ConstTerm.PLAYER + ConstTerm.DIRECTION, GetPlayer().GetFaceDirection());
            saveData.SetValue(ConstTerm.PARTY + ConstTerm.DATA, ConstTerm.CURRENCY, GetTotalCurrency());
        }

        public void OnLoadFile(ConfigFile loadData)
        {
            if (loadData == null) { return; }
            // hasLoaded = true;

            LoadPartyPaths(loadData);

            if (loadData.HasSection(ConstTerm.PARTY + ConstTerm.DATA)) {
                if (loadData.HasSectionKey(ConstTerm.PARTY + ConstTerm.DATA, ConstTerm.MAP + ConstTerm.POSITION)) {
                    GetPlayer().GlobalPosition = (Vector2)loadData.GetValue(ConstTerm.PARTY + ConstTerm.DATA, ConstTerm.MAP + ConstTerm.POSITION);
                }
                if (loadData.HasSectionKey(ConstTerm.PARTY + ConstTerm.DATA, ConstTerm.PLAYER + ConstTerm.DIRECTION)) {
                    GetPlayer().SetLookDirection((Vector2)loadData.GetValue(ConstTerm.PARTY + ConstTerm.DATA, ConstTerm.PLAYER + ConstTerm.DIRECTION));
                }
                if (loadData.HasSectionKey(ConstTerm.PARTY + ConstTerm.DATA, ConstTerm.CURRENCY)) {
                    currencyTotal = (int)loadData.GetValue(ConstTerm.PARTY + ConstTerm.DATA, ConstTerm.CURRENCY);
                }
            }
        }

        public void LoadPartyPaths(ConfigFile loadData)
        {
            if (loadData.HasSection(ConstTerm.MEMBER + ConstTerm.DATA)) {
                partyMemberPaths = [];
                foreach (string member in loadData.GetSectionKeys(ConstTerm.MEMBER + ConstTerm.DATA)) {
                    partyMemberPaths.Add((string)loadData.GetValue(ConstTerm.MEMBER + ConstTerm.DATA, member));
                }
            }
            
            if (loadData.HasSection(ConstTerm.RESERVE + ConstTerm.DATA)) {
                reserveMemberPaths = [];
                foreach (string member in loadData.GetSectionKeys(ConstTerm.RESERVE + ConstTerm.DATA)) {
                    reserveMemberPaths.Add((string)loadData.GetValue(ConstTerm.RESERVE + ConstTerm.DATA, member));
                }
            }
        }

        // public void SaveData()
        // {
        //     SaveSystem newSave = new()
        //     {
        //         partyMembers = partyMembers,
        //         reserveMembers = reserveMembers,
        //         currencyTotal = GetTotalCurrency(),
        //         partySize = GetPartySize(),
        //         reserveSize = GetReservePartySize()
        //     };

        //     ResourceSaver.Save(newSave, "user://saveTest.tres");
        // }

        // public partial class SaveSystem : Resource
        // {
        //     public PackedScene[] partyMembers;
        //     public PackedScene[] reserveMembers;

        //     public int currencyTotal;
        //     public int partySize;
        //     public int reserveSize;
        // }

        // public Dictionary<string, Variant> SaveData()
        // {
        //     Dictionary<string, Variant> saveData = new();

        //     for (int p = 0; p < GetPartySize(); p++)
        //     {
        //         string assetPath = partyMembers[p].ResourcePath;
        //         saveData["partyMembers" + p.ToString()] = assetPath;
        //     }
        //     for (int r = 0; r < GetReservePartySize(); r++)
        //     {
        //         string assetPath = reserveMembers[r].ResourcePath;
        //         saveData["reserveMembers" + r.ToString()] = assetPath;
        //     }
        //     saveData["currencyTotal"] = currencyTotal;
        //     saveData["partySize"] = GetPartySize();
        //     saveData["reserveSize"] = GetReservePartySize();
            
        //     return saveData;
        // }
        // public JToken CaptureAsJToken()
        // {
        //     JObject state = new();
        //     IDictionary<string, JToken> stateDict = state;

        //     for (int p = 0; p < GetPartySize(); p++)
        //     {
        //         string assetPath = partyMembers[p].ResourcePath;
        //         stateDict["partyMembers" + p.ToString()] = assetPath;
        //     }
        //     for (int r = 0; r < GetReservePartySize(); r++)
        //     {
        //         string assetPath = reserveMembers[r].ResourcePath;
        //         stateDict["reserveMembers" + r.ToString()] = assetPath;
        //     }
        //     stateDict["gilTotal"] = gilTotal;
        //     stateDict["partySize"] = GetPartySize();
        //     stateDict["reserveSize"] = GetReservePartySize();

        //     return state;
        // }

        // public void RestoreFromJToken(JToken state)
        // {
        //     if (state is JObject jObject)
        //     {
        //         IDictionary<string, JToken> stateDict = jObject;
        //         stateDict.TryGetValue("partySize", out JToken pSize);
        //         stateDict.TryGetValue("reserveSize", out JToken rSize);
        //         stateDict.TryGetValue("gilTotal", out JToken gTotal);

        //         gilTotal = gTotal.ToObject<int>();
        //         int partySize = pSize.ToObject<int>();
        //         int reserveSize = rSize.ToObject<int>();

        //         for (int p = 0; p < partySize; p++)
        //         {
        //             if (stateDict.TryGetValue("partyMembers" + p.ToString(), out JToken pNext))
        //             {
        //                 partyMembers[p] = ResourceLoader.Load<PackedScene>(pNext.ToObject<string>());
        //             }
        //         }

        //         for (int r = 0; r < reserveSize; r++)
        //         {
        //             if (stateDict.TryGetValue("reserveMember" + r.ToString(), out JToken rNext))
        //             {
        //                 reserveMember[r] = ResourceLoader.Load<PackedScene>(rNext.ToObject<string>());
        //             }
        //         }
        //     }
        // }
    }
}