using Godot;
using System;

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
        animPlayer.Connect(AnimationPlayer.SignalName.AnimationFinished, new Callable(this, MethodName.OnAnimationFinished));
        // animPlayer.AnimationFinished += OnAnimationFinished;
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

    public void Transition()
    {
        fadeRect.Visible = true;
        animPlayer.Play(ConstTerm.FADE_OUT);
    }

    public void OnAnimationFinished(string animName)
    {
        GD.Print("AnimFinshed!");
        if (animName == ConstTerm.FADE_OUT) {
            EmitSignal(SignalName.onTransitionFinished);
            animPlayer.Play(ConstTerm.FADE_IN);
        } else {
            fadeRect.Visible = false;
            GD.Print("Fade in complete");
        }
    }
}
