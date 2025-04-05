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
            SubSignals();

            BGMPlayer.Instance.SetSoundVolume(soundPlayer);
        }

        private void SubSignals()
        {
            // Connect(SignalName.Pressed, new Callable(this, MethodName.OnButtonPressed));
        }

        //=============================================================================
        // SECTION: External Access
        //=============================================================================

        public void SetQuasiDisabled(bool shouldDisable)
        {
            isDisabled = shouldDisable;

            if (GetParent() is Label) {
                if (isDisabled) { GetParent<Label>().SelfModulate = new Color(ConstTerm.GREY); }
                else { GetParent<Label>().SelfModulate = new Color(ConstTerm.WHITE); }
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

        //=============================================================================
        // SECTION: Signal Methods
        //=============================================================================

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