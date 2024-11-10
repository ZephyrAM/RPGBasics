using Godot;
using System;
using System.Collections.Generic;

namespace ZAM.MenuUI
{
    public partial class InfoMenu : PanelContainer
    {
        [Export] private PackedScene memberInfoBox = null;
        [Export] private VBoxContainer infoList = null;

        private List<MemberInfo> partyMembers = [];

        public override void _Ready()
        {
            // Visible = false;
        }

        public void SetupParty(int size)
        {
            if (infoList.GetChildCount() > 0) { 
                for (int i = infoList.GetChildCount(); i > 0; i--)
                { infoList.GetChild(i - 1).QueueFree(); }
            }

            partyMembers = [];
            for (int i = 0; i < size; i++)
            {
                partyMembers.Add(memberInfoBox.Instantiate() as MemberInfo);
                infoList.AddChild(partyMembers[i]);
            }
        }

        public int GetInfoSize()
        {
            return partyMembers.Count;
        }

        public void RemoveMember(int index)
        {
            partyMembers[index].QueueFree();
        }

        public void AddMember()
        {
            partyMembers.Add(memberInfoBox.Instantiate() as MemberInfo);
        }

        public MemberInfo GetMemberInfo(int index)
        {
            return partyMembers[index];
        }

        public void UpdateMemberInfo(int index, MemberInfo newInfo)
        {
            partyMembers[index] = newInfo;
        }
    }
}