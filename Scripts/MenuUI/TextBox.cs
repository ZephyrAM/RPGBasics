using Godot;
using System;
using System.Text.RegularExpressions;
// using System.Collections.Generic;

namespace ZAM.MenuUI
{
    public partial class TextBox : Panel
    {
        [ExportGroup("Values")]
        [Export] private float textRate = 4;

        [ExportGroup("Text")]
        [Export] private Label startLabel = null;
        [Export] private RichTextLabel textLabel = null;
        [Export] private Label endLabel = null;

        [ExportGroup("Child Nodes")]
        [Export] private MarginContainer marginBox = null;
        [Export] private VBoxContainer vertBox = null;

        private Tween textTween = null;
        // private List<string> textQueue = [];        

        private StateType currentState = StateType.READY;

        //=============================================================================
        // SECTION: OnReady Methods
        //=============================================================================

        public override void _Ready()
        {
            IfNull();
            HideTextBox();
        }

        private void IfNull()
        {
            marginBox   ??= GetNode<MarginContainer>(ConstTerm.MARGIN + ConstTerm.CONTAINER);
            vertBox     ??= marginBox.GetNode<VBoxContainer>(ConstTerm.VBOX_CONTAINER);

            startLabel  ??= vertBox.GetNode<Label>(ConstTerm.START);
            endLabel    ??= vertBox.GetNode<Label>(ConstTerm.END);
            textLabel   ??= vertBox.GetNode<RichTextLabel>(ConstTerm.LABEL);
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

        public void QueueText(string name, string text)
        {
            // textLabel.Text = text;
            // GD.Print(textLabel.Text);
            // GD.Print("Text visible / total lines: " + textLabel.GetVisibleLineCount() + " " + textLabel.GetLineCount());
            // var varReg = MyRegex(); // EDIT: Useful?
            string getText = TranslationServer.Translate(text);
            string outText = $"{getText}";
            AddText(name, outText);
        }

        public async void AddText(string name, string nextText)
        {
            textLabel.VisibleRatio = 0;
            startLabel.Visible = name != "";

            startLabel.Text = name;
            textLabel.Text = nextText;
            
            ShowTextBox();

            ChangeState(StateType.ACTIVE);
            // EDIT: If text speed set to Instant - load text, skip to end.
            textTween = CreateTween();
            textTween.TweenProperty(textLabel, ConstTerm.PERCENT_VISIBLE, 1, textLabel.Text.Length / textRate).SetTrans(Tween.TransitionType.Linear);

            await ToSignal(textTween, ConstTerm.FINISHED_SIGNAL);
            ChangeState(StateType.FINISHED);
        }

        private void ChangeState(StateType nextState)
        {
            currentState = nextState;
            switch (currentState)
            {
                case StateType.READY:
                    // GD.Print("Ready");
                    break;
                case StateType.ACTIVE:
                    // GD.Print("Active");
                    break;
                case StateType.FINISHED:
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

        public bool IsTextWriting()
        {
            return textLabel.VisibleRatio > 0 && textLabel.VisibleRatio < 1;
        }

        public void ResetTextRatio()
        {
            textLabel.VisibleRatio = 0;
        }

        public void FasterText(bool active)
        {
            if (IsTextComplete()) { return; }
            
            if (active) { textTween.SetSpeedScale(textRate * 5); }
            else { textTween.SetSpeedScale(textRate); }
        }

        [GeneratedRegex(@"\{\w+?\}")]
        private static partial Regex MyRegex();

    }
}