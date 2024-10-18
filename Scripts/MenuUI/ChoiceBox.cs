using Godot;
using System;

namespace ZAM.MenuUI
{
    public partial class ChoiceBox : MarginContainer
    {
        [ExportGroup("Child Nodes")]
        [Export] private PackedScene choiceLabel = null;
        [Export] private MarginContainer marginBox = null;
        [Export] private VBoxContainer vertBox = null;

        [Export] private MarginContainer selectBox = null;
        [Export] private GridContainer selectList = null;
        [Export] private ColorRect selectBar = null;

        private int currentChoice = 0;

        //=============================================================================
        // SECTION: OnReady Methods
        //=============================================================================

        public override void _Ready()
        {
            IfNull();
            HideChoiceBox();
        }

        public void IfNull()
        {
            marginBox ??= GetNode<MarginContainer>(ConstTerm.TEXTBOX_CONTAINER);
            vertBox ??= marginBox.GetNode<VBoxContainer>(ConstTerm.VBOX_CONTAINER);

            selectBox ??= GetNode<MarginContainer>(ConstTerm.SELECT_CONTAINER);
            selectList ??= selectBox.GetNode<GridContainer>(ConstTerm.SELECT_LIST);
            selectBar ??= selectList.GetNode<ColorRect>(ConstTerm.COLOR_RECT);
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

        public void MoveCursor(int index)
        {
            selectBar.Position = new Vector2(0, selectBar.Size.Y * index);
        }

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