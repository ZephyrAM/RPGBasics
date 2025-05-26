using Godot;
using Godot.Collections;
using System;
// using System.Collections.Generic;

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
        [Export] private PanelContainer usePanel = null;
        [Export] private EquipInfo equipPanel = null;

        [ExportGroup("UI Resources")]
        [Export] private PackedScene buttonLabel = null;
        [Export] private PackedScene equipLabel = null;
        [Export] private AudioStream cursorSound = null;
        [Export] private AudioStream cancelSound = null;
        [Export] private AudioStream errorSound = null;

        // [Export] private ColorRect memberBar = null;
        // [Export] private ColorRect commandBar = null;
        // [Export] private ColorRect skillBar = null;
        // [Export] private ColorRect itemBar = null;

        private Array<MemberInfo> partyMembers = [];

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
                Label newCommand = (Label)ResourceLoader.Load<PackedScene>(buttonLabel.ResourcePath).Instantiate();
                newCommand.Text = options[i];

                optionsList.AddChild(newCommand);
            }
        }

        // public ColorRect GetSelectBar(string selectBar)
        // {
        //     ColorRect tempBar = null;

        //     switch (selectBar)
        //     {
        //         case ConstTerm.COMMAND:
        //             tempBar = commandBar;
        //             break;
        //         case ConstTerm.MEMBER + ConstTerm.SELECT:
        //             tempBar = memberBar;
        //             break;
        //         case ConstTerm.SKILL + ConstTerm.SELECT:
        //             tempBar = skillBar;
        //             break;
        //         case ConstTerm.ITEM + ConstTerm.SELECT:
        //             tempBar = itemBar;
        //             break;
        //         default:
        //             GD.PushError("Invalid selection bar!");
        //             break;
        //     }

        //     return tempBar;
        // }

        //=============================================================================
        // SECTION: Member Info 
        //=============================================================================

        public void SetupParty(int size)
        {
            if (infoList.GetChildCount() > 0) { 
                foreach (Node child in infoList.GetChildren()) {
                    child.QueueFree();
                }
            }

            partyMembers = [];
            for (int i = 0; i < size; i++) {
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

        //=============================================================================
        // SECTION: External Access Methods
        //=============================================================================

        public VBoxContainer GetCommandList()
        {
            return optionsList;
        }

        public VBoxContainer GetMemberList()
        {
            return infoList;
        }

        public PanelContainer GetSkillPanel()
        {
            return skillPanel;
        }

        public PanelContainer GetItemPanel()
        {
            return itemPanel;
        }

        public PanelContainer GetUsePanel()
        {
            return usePanel;
        }

        public EquipInfo GetEquipPanel()
        {
            return equipPanel;
        }

        public PackedScene GetButtonLabel()
        {
            return buttonLabel;
        }

        public PackedScene GetEquipLabel()
        {
            return equipLabel;
        }

        // public ColorRect GetMemberBar()
        // {
        //     return memberBar;
        // }
    }
}