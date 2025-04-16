using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;

namespace ZAM.MenuUI
{
    public partial class CollectBox : PanelContainer
    {
        [Export] private Label collectLabel = null;
        [Export] private AnimationPlayer animPlayer = null;
        [Export] private int timeToDisplay = 180;

        private Array<string> collectQueue = [];
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

        public void QueueText(string text)
        {
            collectQueue.Add(text);
        }

        public void AddText(string newText)
        {
            collectLabel.Text = newText;
            collectQueue.Remove(newText);
            Visible = true;
            animPlayer.Play(ConstTerm.FADE_IN);
        }

        public async void CompleteCollectText()
        {
            if (!isDisplayed && animPlayer.IsPlaying()) {
                await ToSignal(animPlayer, AnimationPlayer.SignalName.AnimationFinished);
                GD.Print(collectQueue[0]);
                collectQueue.RemoveAt(0);
                if (collectQueue.Count <= 0) { return; }
            }
            GD.Print(collectQueue[0]);
            AddText(collectQueue[0]);
        }

        private void CheckFade()
        {
            if (isDisplayed)
            {
                if (timeDisplayed >= timeToDisplay) {
                    animPlayer.Play(ConstTerm.FADE_OUT);
                    isDisplayed = false;
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
                if (collectQueue.Count > 0) { CompleteCollectText(); }
            }
        }
    }
}