using Godot;
using System.Threading.Tasks;


public partial class SaveLoader : Node
{
    public static SaveLoader Instance { get; private set; }
    // public SavedGame gameSession;
    public ConfigFile gameFile;

    private string saveFolder = ConstTerm.GAME_FOLDER + ConstTerm.SAVE_FOLDER;
    private string textSavePath;
    private string preSavePath;
    private string savePath;
    private string configPath;

    //=============================================================================
    // SECTION: Base Methods
    //=============================================================================

    public override void _Ready()
    {
        Instance = this;
        // Instance.gameSession = new();
        Instance.gameFile = new();

        SetupSavePaths();
    }

    private void SetupSavePaths()
    {
        DirAccess dir = DirAccess.Open(ConstTerm.GAME_FOLDER);
        if (!dir.DirExists(ConstTerm.SAVE_FOLDER)) { dir.MakeDir(ConstTerm.SAVE_FOLDER); }

        textSavePath = saveFolder + ConstTerm.SAVE_FILE + ConstTerm.TXT;
        preSavePath = saveFolder + ConstTerm.SAVE_FILE + ConstTerm.CFG;
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
        GetTree().CallGroup(ConstTerm.CONFIG + ConstTerm.DATA, ConstTerm.ON_SAVE + ConstTerm.CONFIG, GetConfigFile());
    }

    public void LoadConfig()
    {
        GetTree().CallGroup(ConstTerm.CONFIG + ConstTerm.DATA, ConstTerm.ON_LOAD + ConstTerm.CONFIG, GetConfigFile());
    }

    public string GetLangFile()
    {
        string language = ConstTerm.EN; // EDIT: Adjust to pull current language.
        return ConstTerm.GAME_FOLDER + ConstTerm.LANG_FOLDER + language + ConstTerm.FOLDER + ConstTerm.LANG_FILE;
    }

    //=============================================================================
    // SECTION: Save Methods
    //=============================================================================

    public async void SaveGame()
    {
        // SavedGame gameSave = new() {
        //     CharData = GatherBattlers()
        // };
        await SaveAllData(true);

        DirAccess dir = DirAccess.Open(ConstTerm.GAME_FOLDER);
        if (!dir.DirExists(ConstTerm.SAVE_FOLDER)) { dir.MakeDir(ConstTerm.SAVE_FOLDER); }

        Instance.gameFile.Save(preSavePath); // Save first version
        using FileAccess readSave = FileAccess.Open(preSavePath, FileAccess.ModeFlags.Read);
        using FileAccess fileSave = FileAccess.OpenEncryptedWithPass(savePath, FileAccess.ModeFlags.Write, ConstTerm.ENCRYPT_KEY); // EDIT: Temp encryption pass
        fileSave.StoreString(readSave.GetAsText()); // Save as encrypted file
        fileSave.Close();
        readSave.Close();
        // DirAccess.RemoveAbsolute(preSavePath); // Remove temporary Resource save
    }

    public Task SaveAllData(bool saveToFile)
    {
        Instance.gameFile ??= new ConfigFile();

        GatherSystemData(saveToFile);
        GatherPartyData(saveToFile);
        GatherInventoryData(saveToFile);
        GatherBattlers(saveToFile);
        GatherInteractData(saveToFile);
        // GatherMapData();

        return Task.CompletedTask;
    }

    public void GatherSystemData(bool saveToFile)
    {
        // if (!saveToFile) { GetTree().CallGroup(ConstTerm.SYSTEM + ConstTerm.DATA, ConstTerm.ON_SAVE + ConstTerm.GAME, Instance.gameSession); }
        GetTree().CallGroup(ConstTerm.SYSTEM + ConstTerm.DATA, ConstTerm.ON_SAVE + ConstTerm.FILE, Instance.gameFile); 
    }

    public void GatherPartyData(bool saveToFile)
    {
        // if (!saveToFile) { GetTree().CallGroup(ConstTerm.PARTY + ConstTerm.DATA, ConstTerm.ON_SAVE + ConstTerm.GAME, Instance.gameSession); }
        GetTree().CallGroup(ConstTerm.PARTY + ConstTerm.DATA, ConstTerm.ON_SAVE + ConstTerm.FILE, Instance.gameFile);
        // GD.Print(Instance.gameSession.PlayerData);
    }

    public void GatherInventoryData(bool saveToFile)
    {
        // if (!saveToFile)  { GetTree().CallGroup(ConstTerm.INVENTORY + ConstTerm.DATA, ConstTerm.ON_SAVE + ConstTerm.GAME, Instance.gameSession); }
        if (saveToFile) { GetTree().CallGroup(ConstTerm.INVENTORY + ConstTerm.DATA, ConstTerm.ON_SAVE + ConstTerm.FILE, Instance.gameFile); }
    }

    public void GatherBattlers(bool saveToFile)
    {
        GetTree().CallGroup(ConstTerm.BATTLER + ConstTerm.DATA, ConstTerm.ON_SAVE + ConstTerm.FILE, Instance.gameFile);
        // Dictionary<CharacterID, BattlerData> allBattlers = [];
        // if (!saveToFile) { GetTree().CallGroup(ConstTerm.BATTLER + ConstTerm.DATA, ConstTerm.ON_SAVE + ConstTerm.GAME, Instance.gameSession); }

        // GD.Print("CharData count - " + Instance.gameSession.CharData.Count);

        // return Instance.gameSession.CharData;
    }

    public void GatherInteractData(bool saveToFile)
    {
        // if (!saveToFile) { GetTree().CallGroup(ConstTerm.INTERACT + ConstTerm.DATA, ConstTerm.ON_SAVE + ConstTerm.GAME, Instance.gameSession); }
        GetTree().CallGroup(ConstTerm.INTERACT + ConstTerm.DATA, ConstTerm.ON_SAVE + ConstTerm.FILE, Instance.gameFile);
    }

    // public void GatherMapData()
    // {
    //     GetTree().CallGroup(ConstTerm.MAP + ConstTerm.DATA, ConstTerm.ON_SAVE + ConstTerm.GAME, Instance.gameSession);
    // }

    //=============================================================================
    // SECTION: Load Methods
    //=============================================================================

    public void LoadGame()
    {
        Instance.gameFile = LoadGameInfo();
        if (Instance.gameFile == null) { GD.PushWarning("No save data"); return; }
        
        // Instance.gameFile = gameSave;
        // await LoadAllData(true);
    }

    public ConfigFile LoadGameInfo()
    {
        using FileAccess file = FileAccess.OpenEncryptedWithPass(savePath, FileAccess.ModeFlags.Read, ConstTerm.ENCRYPT_KEY); // Load Encrypted save file // EDIT: Temp encryption pass
        if (file == null) { GD.Print("No file to load!"); return null; }

        using FileAccess fileSave = FileAccess.Open(preSavePath, FileAccess.ModeFlags.Write);
        fileSave.StoreString(file.GetAsText()); // Save as text file
        file.Close();
        fileSave.Close();

        // Error dir = DirAccess.RenameAbsolute(textSavePath, preSavePath); // Rename .txt to .tres
        // using SavedGame tempSave = ResourceLoader.Load(preSavePath) as SavedGame; // Load as Resource (.tres)
        // DirAccess.RemoveAbsolute(preSavePath); // Remove temporary Resource save

        ConfigFile loadedSave = new();
        Error checkSave = loadedSave.Load(preSavePath);

        if (checkSave != Error.Ok) { GD.PushError("No save file to load!"); }
        // DirAccess.RemoveAbsolute(preSavePath);
        return loadedSave;

        // return tempSave;
    }

    public async Task LoadAllData(bool loadFromFile)
    {
        // if (!loadFromFile) { if (Instance.gameSession == null) { GD.Print("No current game data."); return; } } 
        // else { if (Instance.gameFile == null) { GD.Print("No save file data."); return; } }
        if (!loadFromFile) { Instance.gameFile ??= new ConfigFile(); }
        else { GD.PushWarning("No save data found."); return; }

        await LoadSystemData(loadFromFile);
        await LoadPartyData(loadFromFile);
        await LoadInventoryData(loadFromFile);
        await LoadBattlerData(loadFromFile);
        await LoadInteractData(loadFromFile);
        return;
    }

    public Task LoadSystemData(bool loadFromFile)
    {
        // if (!loadFromFile) { GetTree().CallGroup(ConstTerm.SYSTEM + ConstTerm.DATA, ConstTerm.ON_LOAD + ConstTerm.GAME, Instance.gameSession.SystemData); }
        GetTree().CallGroup(ConstTerm.SYSTEM + ConstTerm.DATA, ConstTerm.ON_LOAD + ConstTerm.FILE, Instance.gameFile);
        return Task.CompletedTask;
    }

    public Task LoadPartyData(bool loadFromFile)
    {
        // if (!loadFromFile) { GetTree().CallGroup(ConstTerm.PARTY + ConstTerm.DATA, ConstTerm.ON_LOAD + ConstTerm.GAME, Instance.gameSession.PartyData); }
        GetTree().CallGroup(ConstTerm.PARTY + ConstTerm.DATA, ConstTerm.ON_LOAD + ConstTerm.FILE, Instance.gameFile);
        return Task.CompletedTask;
    }

    public Task LoadInventoryData(bool loadFromFile)
    {
        // if (!loadFromFile) { GetTree().CallGroup(ConstTerm.INVENTORY + ConstTerm.DATA, ConstTerm.ON_LOAD + ConstTerm.GAME, Instance.gameSession.InventoryData); }
        if (loadFromFile) { GetTree().CallGroup(ConstTerm.INVENTORY + ConstTerm.DATA, ConstTerm.ON_LOAD + ConstTerm.FILE, Instance.gameFile); }
        return Task.CompletedTask;
    }

    public Task LoadBattlerData(bool loadFromFile)
    {
        // if (!loadFromFile) { GetTree().CallGroup(ConstTerm.BATTLER + ConstTerm.DATA, ConstTerm.ON_LOAD + ConstTerm.GAME, Instance.gameSession.CharData); }
        GetTree().CallGroup(ConstTerm.BATTLER + ConstTerm.DATA, ConstTerm.ON_LOAD + ConstTerm.FILE, Instance.gameFile);
        return Task.CompletedTask;
    }

    public Task LoadInteractData(bool loadFromFile)
    {
        // if (!loadFromFile) { GetTree().CallGroup(ConstTerm.INTERACT + ConstTerm.DATA, ConstTerm.ON_LOAD + ConstTerm.GAME, Instance.gameSession.InteractData, (int)Instance.gameSession.SystemData.LoadCurrentMapID); }
        GetTree().CallGroup(ConstTerm.INTERACT + ConstTerm.DATA, ConstTerm.ON_LOAD + ConstTerm.FILE, Instance.gameFile);
        return Task.CompletedTask;
    }

    // public Task LoadMapData(bool loadFromFile)
    // {
    //     if (!loadFromFile) { GetTree().CallGroup(ConstTerm.MAP + ConstTerm.DATA, ConstTerm.ON_LOAD + ConstTerm.GAME, Instance.gameSession.MapData); } // EDIT: For MapData
    //     else { }
    //     return Task.CompletedTask;
    // }

    //=============================================================================
    // SECTION: Load Data Types
    //=============================================================================

    // public void BattlerGroup(SavedGame gameSave)
    // {
    //     GetTree().CallGroup(ConstTerm.BATTLERDATA, ConstTerm.ON_LOAD + ConstTerm.GAME, gameSave.CharData);
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
