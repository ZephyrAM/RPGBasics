using Godot;
using System;
using System.Collections.Generic;

namespace ZAM.MenuUI
{
    public partial class InfoMenu : PanelContainer
    {
        [ExportGroup("Nodes")]
        [Export] private PackedScene memberInfoBox = null;
        [Export] private VBoxContainer infoList = null;
        [Export] private VBoxContainer optionsList = null;
        [Export] private PanelContainer skillPanel = null;
        [Export] private PanelContainer itemPanel = null;

        [ExportGroup("UI Resources")]
        [Export] private PackedScene labelSettings = null;
        [Export] private ColorRect memberBar = null;
        [Export] private ColorRect commandBar = null;
        [Export] private ColorRect skillBar = null;
        [Export] private ColorRect itemBar = null;

        private List<MemberInfo> partyMembers = [];

        public override void _Ready()
        {
            Visible = false;
        }

        //=============================================================================
        // SECTION: Command List
        //=============================================================================

        public void SetupCommandList(string[] options)
        {
            for (int i = 0; i < options.Length; i++)
            {
                Label newCommand = (Label)ResourceLoader.Load<PackedScene>(labelSettings.ResourcePath).Instantiate();
                newCommand.Text = options[i];

                optionsList.AddChild(newCommand);
            }
        }

        public ColorRect GetSelectBar(string selectBar)
        {
            ColorRect tempBar = null;

            switch (selectBar)
            {
                case ConstTerm.COMMAND:
                    tempBar = commandBar;
                    break;
                case ConstTerm.MEMBER + ConstTerm.SELECT:
                    tempBar = memberBar;
                    break;
                case ConstTerm.SKILL + ConstTerm.SELECT:
                    tempBar = skillBar;
                    break;
                case ConstTerm.ITEM + ConstTerm.SELECT:
                    tempBar = itemBar;
                    break;
                default:
                    GD.PushError("Invalid selection bar!");
                    break;
            }

            return tempBar;
        }

        //=============================================================================
        // SECTION: Member Info 
        //=============================================================================

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

        public VBoxContainer GetCommandList()
        {
            return optionsList;
        }

        public PanelContainer GetSkillPanel()
        {
            return skillPanel;
        }

        public PanelContainer GetItemPanel()
        {
            return itemPanel;
        }

        public ColorRect GetMemberBar()
        {
            return memberBar;
        }
    }
}