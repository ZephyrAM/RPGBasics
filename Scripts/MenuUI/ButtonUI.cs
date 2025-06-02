using Godot;
using System;

namespace ZAM.MenuUI
{
    public partial class ButtonUI : Button
    {
        [ExportGroup("Nodes")]
        // [Export] private Button activeButton = null;
        [Export] private AudioStreamPlayer soundPlayer = null;

        [ExportGroup("Sounds")]
        [Export] private AudioStream confirmSound = null;
        [Export] private AudioStream errorSound = null;

        private bool isDisabled = false;
        private float soundVolume = 100;

        //=============================================================================
        // SECTION: Base Methods
        //=============================================================================

        public override void _Ready()
        {
            SetAnchorsPreset(LayoutPreset.FullRect);

            BGMPlayer.Instance.SetSoundVolume(soundPlayer);
        }

        // public override void _ExitTree()
        // {
        //     base._ExitTree();
        //     for (int s = 0; s < GetIncomingConnections().Count; s++)
        //     {
        //         StringName signalName = (StringName)GetIncomingConnections()[s]["signal"];
        //         Callable signalCallable = (Callable)GetIncomingConnections()[s]["callable"];
        //         GD.Print(signalCallable.Target);
        //         if (signalCallable.Target == null) { continue; }
        //         Disconnect(signalName, signalCallable);
        //     }
        // }

        //=============================================================================
        // SECTION: External Access
        //=============================================================================

        public void SetQuasiDisabled(bool shouldDisable)
        {
            isDisabled = shouldDisable;

            if (GetParent().IsClass(ConstTerm.CANVAS_ITEM))
            {
                if (isDisabled) { GetParent().Set(CanvasItem.PropertyName.Modulate, new Color(ConstTerm.GREY)); }
                else { GetParent().Set(CanvasItem.PropertyName.Modulate, new Color(ConstTerm.WHITE)); }
            }
        }

        public bool GetIsDisabled()
        {
            return isDisabled;
        }

        public AudioStreamPlayer GetAudioPlayer()
        {
            return soundPlayer;
        }

        public bool OnButtonPressed()
        {
            bool isUIDisabled = GetIsDisabled();
            AudioStream playAudio = null;

            switch (isUIDisabled)
            {
                case false:
                    playAudio = confirmSound;
                    break;
                case true:
                    playAudio = errorSound;
                    break;
            }

            soundPlayer.Stream = playAudio;
            soundPlayer.Play();

            return isUIDisabled;
        }
        
        //=============================================================================
        // SECTION: Signal Methods
        //=============================================================================

        // private void OnVisibleChanged()
        // {
        //     GD.Print(GetSignalConnectionList(SignalName.MouseEntered));
        //     // EmitSignal(SignalName.OnSignalChange, Visible);
        // }


        //=============================================================================
        // SECTION: Save System
        //=============================================================================

        public void OnSaveConfig(ConfigFile config)
        {
            BGMPlayer.Instance.SetSoundVolume(soundPlayer);
        }

        public void OnLoadConfig(ConfigFile config)
        {
            BGMPlayer.Instance.SetSoundVolume(soundPlayer);
        }
    }
}