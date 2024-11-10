using Godot;
using System;

namespace ZAM.MenuUI
{
    public partial class CollectBox : PanelContainer
    {
        [Export] private Label collectLabel = null;
        [Export] private AnimationPlayer animPlayer = null;
        [Export] private int timeToDisplay = 180;

        private bool isDisplayed = false;
        private int timeDisplayed = 0;

        public override void _Ready()
        {
            Visible = false;
            
            IfNull();
            animPlayer.Connect(AnimationPlayer.SignalName.AnimationFinished, new Callable(this, MethodName.OnAnimationFinished));
        }

        public override void _Process(double delta)
        {
            CheckFade();
        }

        private void IfNull()
        {
            collectLabel ??= GetNode<Label>(ConstTerm.LABEL);
            animPlayer ??= GetNode<AnimationPlayer>(ConstTerm.ANIM_PLAYER);
        }

        public void AddText(string newText)
        {
            collectLabel.Text = newText;
            Visible = true;
            animPlayer.Play(ConstTerm.FADE_IN);
        }

        private void CheckFade()
        {
            if (isDisplayed)
            {
                if (timeDisplayed >= timeToDisplay) {
                    isDisplayed = false;
                    animPlayer.Play(ConstTerm.FADE_OUT);
                    return;
                }
                timeDisplayed++;       
            }
        }

        private void OnAnimationFinished(string animName)
        {
            if (animName == ConstTerm.FADE_IN) {
                isDisplayed = true;
            } else {
                Visible = false;
                timeDisplayed = 0;
            }
        }
    }
}