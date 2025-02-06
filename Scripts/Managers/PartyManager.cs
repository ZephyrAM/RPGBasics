using Godot;
using Godot.Collections;
using System.Collections.Generic;

using ZAM.Control;
using ZAM.Stats;

namespace ZAM.Managers
{
    public partial class PartyManager : Node
    {
        // Assigned Variables \\
        [Export] private Array<PackedScene> partyMembers = [];
        [Export] private Array<PackedScene> reserveMembers = [];
        [Export] private Script charController = null;

        // Setup Variables \\
        private CharacterBody2D leaderMember = null;
        private List<Battler> activeParty = [];

        private double timePlayed;
        private int currencyTotal = 0;

        //=============================================================================
        // SECTION: Base Methods
        //=============================================================================

        public PartyManager(){}

        // public PartyManager(PartyManager oldparty)
        // {
        //     partyMembers = oldparty.partyMembers;
        //     reserveMembers = oldparty.reserveMembers;

        //     charController = oldparty.charController;

        //     leaderMember = oldparty.leaderMember;
        //     activeParty = oldparty.activeParty;
        // }

        public override void _Ready()
        {
            // If party doesn't exist, skip creation. Force create after manually assigning member to party.
            // if (partyMembers.Count < 1) { return; }
            // CreateParty();

            // Assure the leader never gets double loaded.
            if (leaderMember == null) { 
                if (partyMembers.Count > 0) { LoadLeader(); }
                else if (GetChildCount() > 0) { CreateParty(); }
                else { GD.PushError("No party members!"); }
            }
        }

        //=============================================================================
        // SECTION: Utility Methods
        //=============================================================================

        private static Node2D SafeScriptAssign(Node2D target, Script scriptAssign) // May shift to shared Utilities script
        {
            // This whole section... \\
            ulong charId = target.GetInstanceId();
            target.SetScript(ResourceLoader.Load(scriptAssign.ResourcePath));
            target = (Node2D)InstanceFromId(charId);

            target._Ready();
            target.SetProcess(true);
            target.SetPhysicsProcess(true);
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

        private void LoadLeader()
        {
            activeParty = [];

            CharacterBody2D tempLead = (CharacterBody2D)partyMembers[0].Instantiate();
            AddChild(tempLead);
            activeParty.Add(GetChild(0).GetNode<Battler>(ConstTerm.BATTLER));

            leaderMember = (CharacterBody2D)SafeScriptAssign(tempLead, charController);
            leaderMember.GetNode<Label>(ConstTerm.NAME).Visible = false;
            LoadParty();
        }

        private void LoadParty()
        {
            for (int p = 1; p < partyMembers.Count; p++)
            {
                CharacterBody2D tempMember = (CharacterBody2D)partyMembers[p].Instantiate();
                tempMember.Visible = false;
                tempMember.ProcessMode = ProcessModeEnum.Disabled;
                AddChild(tempMember);

                activeParty.Add(GetChild(p).GetNode<Battler>(ConstTerm.BATTLER));
            }
        }

        private void CreateParty()
        {
            activeParty = [];

            CharacterBody2D tempLead = (CharacterBody2D)GetChild(0);
            activeParty.Add(tempLead.GetNode<Battler>(ConstTerm.BATTLER));
            leaderMember = (CharacterBody2D)SafeScriptAssign(tempLead, charController);
            leaderMember.GetNode<Label>(ConstTerm.NAME).Visible = false;

            for (int p = 1; p < GetChildCount(); p++)
            {
                CharacterBody2D tempMember = (CharacterBody2D)GetChild(p);
                tempMember.Visible = false;
                tempMember.ProcessMode = ProcessModeEnum.Disabled;

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
            return partyMembers[index].Instantiate() as CharacterBody2D;
        }

        public List<Battler> GetPlayerParty()
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
            // GD.Print("Setting Members...");
            partyMembers = party;
            reserveMembers = reserve;
        }

        public int GetPartySize()
        {
            return partyMembers.Count;
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
            return reserveMembers.Count;
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

        public void OnSaveGame(SavedGame saveData)
        {
            PartyData newData = new()
            {
                PartyMembers = GetPartyArray(),
                ReserveMembers = GetReserveArray(),
                MapPosition = GetPlayer().GlobalPosition,
                FaceDirection = GetPlayer().GetFaceDirection(),
                TotalGold = GetTotalCurrency()
            };
            
            saveData.PartyData = newData;
        }

        public void OnLoadGame(PartyData loadData)
        {
            PartyData saveData = loadData;
            if (saveData == null) { GD.Print("Party Data - NULL"); return; }

            partyMembers = saveData.PartyMembers;
            reserveMembers = saveData.ReserveMembers;
            GetPlayer().GlobalPosition = saveData.MapPosition;
            GetPlayer().SetLookDirection(saveData.FaceDirection);
            currencyTotal = saveData.TotalGold;
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