using Godot;
using System;

// Global Object \\
public partial class Fader : CanvasLayer
{
    [Export] private ColorRect fadeRect;
    [Export] private AnimationPlayer animPlayer;

    public static Fader Instance { get; private set; }

    [Signal]
    public delegate void onTransitionFinishedEventHandler();

    public override void _Ready()
    {
        Instance = this;
        fadeRect.Visible = false;
        // animPlayer.Connect(AnimationPlayer.SignalName.AnimationFinished, new Callable(this, MethodName.OnAnimationFinished));
    }

    public override void _EnterTree()
    {
        IfNull();
    }

    private void IfNull()
    {
        fadeRect ??= GetNode<ColorRect>(ConstTerm.COLOR_RECT);
        animPlayer ??= GetNode<AnimationPlayer>(ConstTerm.ANIM_PLAYER);
    }

    // public void Transition()
    // {
    //     fadeRect.Visible = true;
    //     animPlayer.Play(ConstTerm.FADE_OUT);
    // }

    public void FadeOut()
    {
        fadeRect.Visible = true;
        animPlayer.Play(ConstTerm.FADE_OUT);
    }

    public async void FadeIn()
    {
        animPlayer.Play(ConstTerm.FADE_IN);
        await ToSignal(animPlayer, ConstTerm.ANIM_FINISHED);

        fadeRect.Visible = false;
    }

    // public void OnAnimationFinished(string animName)
    // {
    //     if (animName == ConstTerm.FADE_OUT) {
    //         EmitSignal(SignalName.onTransitionFinished);
    //         animPlayer.Play(ConstTerm.FADE_IN);
    //     } else {
    //         fadeRect.Visible = false;
    //     }
    // }

    public AnimationPlayer GetAnimPlayer()
    {
        return animPlayer;
    }
}
