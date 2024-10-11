using Godot;
using System;
using System.Collections.Generic;

namespace ZAM.MenuUI
{
    public partial class TextBox : MarginContainer
    {
        [ExportGroup("Values")]
        [Export] private float textRate = 4;

        [ExportGroup("Nodes")]
        // [Export] private MarginContainer textBoxContainer = null;
        [Export] private Label startLabel = null;
        [Export] private RichTextLabel textLabel = null;
        [Export] private Label endLabel = null;

        private Tween textTween = null;
        private List<string> textQueue = [];

        private enum State {
            READY,
            ACTIVE,
            FINISHED
        };

        private State currentState = State.READY;

        //=============================================================================
        // SECTION: OnReady Methods
        //=============================================================================

        public override void _Ready()
        {
            IfNull();
            HideTextBox();
        }

        public override void _Process(double delta)
        {
            
        }

        private void IfNull()
        {
            // textBoxContainer ??= GetNode<MarginContainer>(ConstTerm.TEXTBOX_CONTAINER);
            startLabel ??= GetNode<MarginContainer>(ConstTerm.MARGIN_CONTAINER).GetNode<VBoxContainer>(ConstTerm.VBOX_CONTAINER).GetNode<Label>(ConstTerm.TEXT_START);
            endLabel ??= GetNode<MarginContainer>(ConstTerm.MARGIN_CONTAINER).GetNode<VBoxContainer>(ConstTerm.VBOX_CONTAINER).GetNode<Label>(ConstTerm.TEXT_END);
            textLabel ??= GetNode<MarginContainer>(ConstTerm.MARGIN_CONTAINER).GetNode<VBoxContainer>(ConstTerm.VBOX_CONTAINER).GetNode<RichTextLabel>(ConstTerm.LABEL);
        }

        //=============================================================================
        // SECTION: Manage Text Box
        //=============================================================================

        public void HideTextBox()
        {
            startLabel.Text = "";
            endLabel.Text = "";
            textLabel.Text = "";
            textLabel.VisibleRatio = 0;
            Hide();
        }

        public void ShowTextBox()
        {
            Show();
        }

        public void QueueText(string text)
        {
            textQueue.Add(text);
        }

        public async void AddText(string name, string nextText)
        {
            // HideTextBox();
            startLabel.Text = name;
            textLabel.Text = nextText;
            ShowTextBox();

            ChangeState(State.ACTIVE);
            textTween = CreateTween();
            textTween.TweenProperty(textLabel, ConstTerm.PERCENT_VISIBLE, 1, textLabel.Text.Length / textRate).SetTrans(Tween.TransitionType.Linear);

            await ToSignal(textTween, ConstTerm.TWEEN_FINISHED);
            ChangeState(State.FINISHED);
        }

        private void ChangeState(State nextState)
        {
            currentState = nextState;
            switch (currentState)
            {
                case State.READY:
                    GD.Print("Ready");
                    break;
                case State.ACTIVE:
                    GD.Print("Active");
                    break;
                case State.FINISHED:
                    // GD.Print(textLabel.MaxLinesVisible);
                    endLabel.Text = "_";
                    break;
            }
        }

        //=============================================================================
        // SECTION: Access Nodes
        //=============================================================================

        public bool IsTextComplete()
        {
            bool textComplete = textLabel.VisibleRatio == 1;
            return textComplete && Visible;
        }

        public void FasterText(bool active)
        {
            if (active) { textTween.SetSpeedScale(textRate * 5); }
            else { textTween.SetSpeedScale(textRate); }
        }

        
    }
}