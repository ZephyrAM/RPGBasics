using Godot;

public partial class SaveLoader : Node
{
    public static SaveLoader Instance { get; private set; }
    public SavedGame gameSession;

    private string saveFolder = ConstTerm.GAME_FOLDER + ConstTerm.SAVE_FOLDER;
    private string textSavePath;
    private string resourceSavePath;
    private string savePath;
    private string configPath;

    //=============================================================================
    // SECTION: Base Methods
    //=============================================================================

    public override void _Ready()
    {
        Instance = this;
        Instance.gameSession = new();

        SetupSavePaths();
    }

    private void SetupSavePaths()
    {
        DirAccess dir = DirAccess.Open(ConstTerm.GAME_FOLDER);
        if (!dir.DirExists(ConstTerm.SAVE_FOLDER)) { dir.MakeDir(ConstTerm.SAVE_FOLDER); }

        textSavePath = saveFolder + ConstTerm.SAVE_FILE + ConstTerm.TXT;
        resourceSavePath = saveFolder + ConstTerm.SAVE_FILE + ConstTerm.TRES;
        savePath = saveFolder + ConstTerm.SAVE_FILE + ConstTerm.SAVE_TYPE;
        configPath = saveFolder + ConstTerm.CFG_FILE;
    }

    public string GetSavePath()
    {
        return saveFolder;
    }

    //=============================================================================
    // SECTION: Config Methods
    //=============================================================================

    public ConfigFile GetConfigFile()
    {
        ConfigFile configFile = new();
        Error checkConfig = configFile.Load(configPath);

        if (checkConfig != Error.Ok) { return new ConfigFile(); }
        return configFile;
    }

    public void SaveConfigFile(ConfigFile newConfig)
    {
        newConfig.Save(configPath);
    }

    public void SaveConfig()
    {
        GetTree().CallGroup(ConstTerm.CONFIGDATA, ConstTerm.ON_SAVECONFIG, GetConfigFile());
    }

    public void LoadConfig()
    {
        GetTree().CallGroup(ConstTerm.CONFIGDATA, ConstTerm.ON_LOADCONFIG, GetConfigFile());
    }

    public string GetLangFile()
    {
        string language = ConstTerm.EN; // EDIT: Adjust to pull current language.
        return ConstTerm.GAME_FOLDER + ConstTerm.LANG_FOLDER + language + ConstTerm.FOLDER + ConstTerm.LANG_FILE;
    }

    //=============================================================================
    // SECTION: Save Methods
    //=============================================================================

    public void SaveGame()
    {
        // SavedGame gameSave = new() {
        //     CharData = GatherBattlers()
        // };
        SaveAllData();

        DirAccess dir = DirAccess.Open(ConstTerm.GAME_FOLDER);
        if (!dir.DirExists(ConstTerm.SAVE_FOLDER)) { dir.MakeDir(ConstTerm.SAVE_FOLDER); }

        ResourceSaver.Save(Instance.gameSession, resourceSavePath); // Save as Resource (.tres)
        using FileAccess readSave = FileAccess.Open(resourceSavePath, FileAccess.ModeFlags.Read);
        using FileAccess fileSave = FileAccess.OpenEncryptedWithPass(savePath, FileAccess.ModeFlags.Write, "Conduit"); // EDIT: Temp encryption pass
        fileSave.StoreString(readSave.GetAsText()); // Save as encrypted file
        fileSave.Close();
        readSave.Close();
        DirAccess.RemoveAbsolute(resourceSavePath); // Remove temporary Resource save
    }

    public void SaveAllData()
    {
        GatherBattlers();
        GatherPartyData();
    }

    public void GatherBattlers()
    {
        // Dictionary<CharacterID, BattlerData> allBattlers = [];
        GetTree().CallGroup(ConstTerm.BATTLERDATA, ConstTerm.ON_SAVEGAME, Instance.gameSession);
        // GD.Print("CharData count - " + Instance.gameSession.CharData.Count);

        // return Instance.gameSession.CharData;
    }

    public void GatherPartyData()
    {
        GetTree().CallGroup(ConstTerm.PARTYDATA, ConstTerm.ON_SAVEGAME, Instance.gameSession);
        // GD.Print(Instance.gameSession.PlayerData);
    }

    //=============================================================================
    // SECTION: Load Methods
    //=============================================================================

    public void LoadGame()
    {
        using FileAccess file = FileAccess.OpenEncryptedWithPass(savePath, FileAccess.ModeFlags.Read, "Conduit"); // Load Encrypted save file // EDIT: Temp encryption pass
        using FileAccess fileSave = FileAccess.Open(textSavePath, FileAccess.ModeFlags.Write);
        fileSave.StoreString(file.GetAsText()); // Save as text file
        fileSave.Close();
        
        Error dir = DirAccess.RenameAbsolute(textSavePath, resourceSavePath); // Rename .txt to .tres
        SavedGame gameSave = ResourceLoader.Load(resourceSavePath) as SavedGame; // Load as Resource (.tres)
        DirAccess.RemoveAbsolute(resourceSavePath); // Remove temporary Resource save

        if (gameSave == null) { GD.Print("No save file!"); return; }

        LoadAllData(gameSave);
    }

    public void LoadAllData(SavedGame gameSave)
    {
        if (gameSave == null) { GD.Print("No save data"); return; }
        LoadBattlerData(gameSave);
        LoadPartyData(gameSave);
    }

    public void LoadBattlerData(SavedGame gameSave)
    {
        GetTree().CallGroup(ConstTerm.BATTLERDATA, ConstTerm.ON_LOADGAME, gameSave.CharData);
        // BattlerGroup(gameSave);
    }

    public void LoadPartyData(SavedGame gameSave)
    {
        GetTree().CallGroup(ConstTerm.PARTYDATA, ConstTerm.ON_LOADGAME, gameSave.PartyData);
    }

    //=============================================================================
    // SECTION: Load Data Types
    //=============================================================================

    // public void BattlerGroup(SavedGame gameSave)
    // {
    //     GetTree().CallGroup(ConstTerm.BATTLERDATA, ConstTerm.ON_LOADGAME, gameSave.CharData);
    //     // GD.Print(GetTree().GetNodeCountInGroup(ConstTerm.BATTLERDATA));
    //     //     Array<Node> tempGroup = GetTree().GetNodesInGroup(groupName);

    //     //     for (int n = 0; n < GetTree().GetNodeCountInGroup(groupName); n++)
    //     //     {
    //     //         if (tempGroup[n] is not Battler) { continue; }
    //     //         Battler tempBattler = (Battler)tempGroup[n];

    //     //         // if (tempBattler.GetCharID() == 0) { continue; } // EDIT = Better conditional
    //     //         tempBattler.OnLoadData(battlerGroup[tempBattler.GetCharID()]);
    //     //     }
    // }
}
