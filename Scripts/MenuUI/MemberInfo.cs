using Godot;
using System;

namespace ZAM.MenuUI
{
    public partial class MemberInfo : Node
    {
        [ExportGroup("Character Info")]
        [Export] private TextureRect portrait = null;
        [Export] private Label charName = null;
        [Export] private Label charTitle = null;

        [ExportGroup("Character HP")]
        [Export] private Label hpCurrent = null;
        [Export] private Label hpMax = null;

        [ExportGroup("Character MP")]
        [Export] private Label mpCurrent = null;
        [Export] private Label mpMax = null;

        [ExportGroup("Character Level")]
        [Export] private Label lvlCurrent = null;
        [Export] private Label expNext = null;

        public void SetCharPortrait(Texture2D image)
        {
            portrait.Texture = image;
        }

        public void SetCharName(string name)
        {
            charName.Text = name;
        }

        public void SetCharTitle(string title)
        {
            charTitle.Text = title;
        }

        public void SetHPCurrent(float value)
        {
            hpCurrent.Text = value.ToString();
        }

        public void SetHPMax(float value)
        {
            hpMax.Text = value.ToString();
        }

        public void SetMPCurrent(float value)
        {
            mpCurrent.Text = value.ToString();
        }

        public void SetMPMax(float value)
        {
            mpMax.Text = value.ToString();
        }

        public void SetCurrentLevel(int value)
        {
            lvlCurrent.Text = value.ToString();
        }

        public void SetNextLevel(float value)
        {
            expNext.Text = value.ToString();
        }
    }
}