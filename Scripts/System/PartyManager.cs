using Godot;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System;

namespace ZAM.System
{
    public partial class PartyManager : Node
    {
        // Assigned Variables \\
        [Export] PackedScene[] partyMember = null;
        [Export] PackedScene[] reserveMember = null;
        [Export] Script charController = null;

        // Setup Variables \\
        Node leaderMember = null;
        // List<CharacterBody2D> activeParty;

        private double timePlayed;
        private int gilTotal = 0;

        // Basic Methods \\
        public override void _Ready()
        {
            // If party doesn't exist, skip creation. Force create after manually assigning member to party.
            if (partyMember.Length < 1) { return; }
            // CreateParty();

            // Assure the leader never gets double loaded.
            if (!IsInstanceValid(leaderMember)) { LoadLeader(); }
        }

        // Utility Methods \\
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
        //     for (int n = 0; n < partyMember.Length; n++)
        //     {
        //         CharacterBody2D tempMember = partyMember[n].Instantiate() as CharacterBody2D;
        //         activeParty.Add(tempMember);
        //     }
        // }

        private void LoadLeader()
        {
            Node tempLead = partyMember[0].Instantiate();
            // GD.Print(tempLead.GetMeta("CharName"));
            AddChild(tempLead);

            Node tempChar = GetChild(0);
            leaderMember = SafeScriptAssign(tempChar, charController);
            leaderMember.GetNode<Label>(ConstTerm.NAMELABEL).Visible = false;
        }

        public CharacterBody2D GetPartyMember(int index)
        {
            // return activeParty[index];
            return partyMember[index].Instantiate() as CharacterBody2D;
        }

        public PackedScene[] GetPlayerParty()
        {
            return partyMember;
        }

        public int GetPartySize()
        {
            int partySize = 0;
            for (int n = 0; n < partyMember.Length; n++)
            {
                if (partyMember[n] != null)
                { partySize++; }
            }
            return partySize;
        }

        public int GetReservePartySize()
        {
            int partySize = 0;
            for (int n = 0; n < reserveMember.Length; n++)
            {
                if (reserveMember[n] != null)
                { partySize++; }
            }
            return partySize;
        }

        // Menu Info \\
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

        public int GetTotalGil()
        {
            return gilTotal;
        }

        public int GetStepsTaken()
        {
            // return playerMover.GetStepsTaken();
            return 0;
        }

        // Save System \\
        public JToken CaptureAsJToken()
        {
            JObject state = new();
            IDictionary<string, JToken> stateDict = state;

            for (int p = 0; p < GetPartySize(); p++)
            {
                string assetPath = partyMember[p].ResourcePath;
                stateDict["partyMember" + p.ToString()] = assetPath;
            }
            for (int r = 0; r < GetReservePartySize(); r++)
            {
                string assetPath = reserveMember[r].ResourcePath;
                stateDict["reserveMember" + r.ToString()] = assetPath;
            }
            stateDict["gilTotal"] = gilTotal;
            stateDict["partySize"] = GetPartySize();
            stateDict["reserveSize"] = GetReservePartySize();

            return state;
        }

        public void RestoreFromJToken(JToken state)
        {
            if (state is JObject jObject)
            {
                IDictionary<string, JToken> stateDict = jObject;
                stateDict.TryGetValue("partySize", out JToken pSize);
                stateDict.TryGetValue("reserveSize", out JToken rSize);
                stateDict.TryGetValue("gilTotal", out JToken gTotal);

                gilTotal = gTotal.ToObject<int>();
                int partySize = pSize.ToObject<int>();
                int reserveSize = rSize.ToObject<int>();

                for (int p = 0; p < partySize; p++)
                {
                    if (stateDict.TryGetValue("partyMember" + p.ToString(), out JToken pNext))
                    {
                        partyMember[p] = ResourceLoader.Load<PackedScene>(pNext.ToObject<string>());
                    }
                }

                for (int r = 0; r < reserveSize; r++)
                {
                    if (stateDict.TryGetValue("reserveMember" + r.ToString(), out JToken rNext))
                    {
                        reserveMember[r] = ResourceLoader.Load<PackedScene>(rNext.ToObject<string>());
                    }
                }
            }
        }
    }
}