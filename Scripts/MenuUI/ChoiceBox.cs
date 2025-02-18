using Godot;
using System;

namespace ZAM.MenuUI
{
    public partial class ChoiceBox : PanelContainer
    {
        [ExportGroup("Child Nodes")]
        [Export] private PackedScene choiceLabel = null;
        [Export] private VBoxContainer vertBox = null;
        
        // [Export] private GridContainer selectList = null;
        // [Export] private ColorRect selectBar = null;

        private int currentChoice = 0;

        //=============================================================================
        // SECTION: OnReady Methods
        //=============================================================================

        public override void _Ready()
        {
            IfNull();
            HideChoiceBox();
        }

        // public override void _Process(double delta)
        // {
        //     if (vertBox.GetChildCount() > 0 && vertBox.GetChild(currentChoice).GetNode<Button>(ConstTerm.BUTTON).HasFocus()) { // EDIT: Option to animate
        //         vertBox.GetChild(currentChoice).GetNode<Button>(ConstTerm.BUTTON).GetNode<AnimationPlayer>(ConstTerm.ANIM_PLAYER).Play("focus_blink");
        //     }
        //     // if (Visible) { selectBar.GetNode<AnimationPlayer>(ConstTerm.ANIM_PLAYER).Play(ConstTerm.CURSOR_BLINK); }
        // }

        public void IfNull()
        {
            vertBox ??= GetNode<MarginContainer>(ConstTerm.MARGIN_CONTAINER).GetNode<VBoxContainer>(ConstTerm.VBOX_CONTAINER);

            // selectList ??= GetNode<MarginContainer>(ConstTerm.SELECT + ConstTerm.CONTAINER).GetNode<GridContainer>(ConstTerm.SELECT + ConstTerm.LIST);
            // selectBar ??= selectList.GetNode<ColorRect>(ConstTerm.COLOR_RECT);
        }

        //=============================================================================
        // SECTION: Manage Choice Box
        //=============================================================================

        public void HideChoiceBox()
        {
            Hide();
            RemoveChoices();
        }

        public void ShowChoiceBox()
        {
            Show();
        }

        public void AddChoiceList(string [] choices)
        {
            for (int l = 0; l < choices.Length; l++)
            {
                Label newChoice = (Label)choiceLabel.Instantiate();
                newChoice.Text = choices[l];
                vertBox.AddChild(newChoice);
            }
        }

        public void SetChoiceOption(int option)
        {
            currentChoice = option;
        }

        // public void MoveCursor(int index)
        // {
        //     selectBar.Position = new Vector2(0, selectBar.Size.Y * index);
        // }

        public void RemoveChoices()
        {
            foreach(Label child in vertBox.GetChildren())
            { child.QueueFree(); }
        }

        public int CountChoices()
        {
            return vertBox.GetChildCount();
        }
    }
}