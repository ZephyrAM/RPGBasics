using Godot;
using System.Collections.Generic;

using ZAM.Control;
using ZAM.Stats;

namespace ZAM.Managers
{
    public partial class PartyManager : Node
    {
        // Assigned Variables \\
        [Export] private PackedScene[] partyMembers = null;
        [Export] private PackedScene[] reserveMembers = null;
        [Export] private Script charController = null;

        // Setup Variables \\
        private Node leaderMember = null;
        private List<Battler> activeParty = null;

        private double timePlayed;
        private int currencyTotal = 0;

        //=============================================================================
        // SECTION: Base Methods
        //=============================================================================

        public override void _Ready()
        {
            // If party doesn't exist, skip creation. Force create after manually assigning member to party.
            if (partyMembers.Length < 1) { return; }
            // CreateParty();

            // Assure the leader never gets double loaded.
            if (!IsInstanceValid(leaderMember)) { LoadLeader(); }
        }

        //=============================================================================
        // SECTION: Utility Methods
        //=============================================================================

        private static Node SafeScriptAssign(Node target, Script scriptAssign) // May shift to shared Utilities script
        {
            // This whole section... \\
            ulong charId = target.GetInstanceId();
            target.SetScript(ResourceLoader.Load(scriptAssign.ResourcePath));
            target = (Node)InstanceFromId(charId);

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

            Node tempLead = partyMembers[0].Instantiate();
            AddChild(tempLead);
            activeParty.Add(GetChild(0).GetNode<Battler>(ConstTerm.BATTLER));

            leaderMember = SafeScriptAssign(tempLead, charController);
            leaderMember.GetNode<Label>(ConstTerm.NAME).Visible = false;
            LoadParty();
        }

        private void LoadParty()
        {
            for (int p = 1; p < partyMembers.Length; p++)
            {
                CharacterBody2D tempMember = (CharacterBody2D)partyMembers[p].Instantiate();
                tempMember.Visible = false;
                tempMember.ProcessMode = ProcessModeEnum.Disabled;
                AddChild(tempMember);

                activeParty.Add(GetChild(p).GetNode<Battler>(ConstTerm.BATTLER));
            }
        }

        public void ChangePlayerActive(bool change)
        {
            GetPlayer().ChangeActive(change);
        }

        //=============================================================================
        // SECTION: Get Methods
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

        public int GetPartySize()
        {
            return partyMembers.Length;
            // int partySize = 0;
            // for (int n = 0; n < partyMembers.Length; n++)
            // {
            //     if (partyMembers[n] != null)
            //     { partySize++; }
            // }
            // return partySize;
        }

        public int GetReservePartySize()
        {
            return reserveMembers.Length;
            // int partySize = 0;
            // for (int n = 0; n < reserveMembers.Length; n++)
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