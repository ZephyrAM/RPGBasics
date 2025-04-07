using Godot;
using System.Threading.Tasks;

// Global Object \\
public partial class BGMPlayer : Node
{
    [Export] private AudioStreamPlayer bgm = null;
    [Export] private AnimationPlayer animPlayer = null;

    private float masterVolume = 1;
    private float bgmVolume = 100;
    private float soundVolume = 100;

    private float maxMaster = 1;
    private float maxVolume = 200;

    public static BGMPlayer Instance { get; private set;}

    //=============================================================================
    // SECTION: Basic Methods
    //=============================================================================

    public override void _Ready()
    {
        Instance = this;
    }

    //=============================================================================
    // SECTION: Fade Systems
    //=============================================================================

    public void FadeOutBGM()
    {
        animPlayer.PlayBackwards(ConstTerm.AUDIO_FADE);
    }

    public void FadeInBGM(AudioStream newBgm)
    {
        bgm.Stream = newBgm;
        Animation nextAnim = animPlayer.GetAnimation(ConstTerm.AUDIO_FADE);
        nextAnim.TrackSetKeyValue(0, nextAnim.TrackGetKeyCount(0) - 1, Mathf.LinearToDb(bgmVolume * masterVolume / 100));
        animPlayer.Play(ConstTerm.AUDIO_FADE);
        bgm.Play();
    }

    public async void TransitionBGM(AudioStream oldBgm, AudioStream newBgm)
    {
        if (oldBgm == newBgm && newBgm == bgm.Stream) { return; }
        
        FadeOutBGM();
        await ToSignal(animPlayer, ConstTerm.ANIM_FINISHED);

        FadeInBGM(newBgm);
    }

    public async Task FadeBGMTransition(AudioStream oldBgm, AudioStream newBgm)
    {
        Fader.Instance.FadeOut();

        Instance.TransitionBGM(oldBgm, newBgm);
        await ToSignal(Fader.Instance.GetAnimPlayer(), ConstTerm.ANIM_FINISHED);
    }

    //=============================================================================
    // SECTION: Volume Access
    //=============================================================================

    public void AdjustVolume(string type, float change)
    {
        if (type == ConstTerm.MASTER) { masterVolume = Mathf.Clamp(masterVolume + change / 100, 0, maxMaster); SetBGMVolume(); }
        else if (type == ConstTerm.BGM) { bgmVolume = Mathf.Clamp(bgmVolume + change, 0, maxVolume); SetBGMVolume(); } 
        else if (type == ConstTerm.SOUND) { soundVolume = Mathf.Clamp(soundVolume + change, 0, maxVolume); }
    }

    public float GetVolume(string type)
    {
        if (type == ConstTerm.MASTER) { return masterVolume * 100; }
        else if (type == ConstTerm.BGM) { return bgmVolume; }
        else if (type == ConstTerm.SOUND) { return soundVolume; }

        GD.PushError("Invalid volume type.");
        return 0;
        // float linVolume = Mathf.DbToLinear(bgm.VolumeDb);
        // return Mathf.Round(linVolume * 100);
    }

    public void SetBGMVolume()
    {
        bgm.VolumeDb = Mathf.LinearToDb(bgmVolume * masterVolume / 100);
    }

    public void SetSoundVolume(AudioStreamPlayer soundPlayer)
    {
        soundPlayer.VolumeDb = Mathf.LinearToDb(soundVolume * masterVolume / 100);
    }

    public float GetMaxVolume()
    {
        return maxVolume;
    }

    public float GetMaxMaster()
    {
        return maxMaster;
    }

    //=============================================================================
    // SECTION: External Access
    //=============================================================================

    public AnimationPlayer GetAnimPlayer()
    {
        return animPlayer;
    }

    public AudioStreamPlayer GetAudioPlayer()
    {
        return bgm;
    }

    //=============================================================================
    // SECTION: Save System
    //=============================================================================

    public void OnSaveConfig(ConfigFile config)
    {
        config ??= new ConfigFile();

        config.SetValue(ConstTerm.AUDIO, ConstTerm.MASTER, masterVolume);
        config.SetValue(ConstTerm.AUDIO, ConstTerm.BGM, bgmVolume);
        config.SetValue(ConstTerm.AUDIO, ConstTerm.SOUND, soundVolume);

        SaveLoader.Instance.SaveConfigFile(config);
    }

    public void OnLoadConfig(ConfigFile config)
    {
        if (config == null) { return; }
        if (!config.HasSection(ConstTerm.AUDIO)) { return; }

        masterVolume = (float)config.GetValue(ConstTerm.AUDIO, ConstTerm.MASTER);
        bgmVolume = (float)config.GetValue(ConstTerm.AUDIO, ConstTerm.BGM);
        soundVolume = (float)config.GetValue(ConstTerm.AUDIO, ConstTerm.SOUND);
    }
}
