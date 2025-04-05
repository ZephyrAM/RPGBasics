using System.Threading.Tasks;
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
        GetTree().CallGroup(ConstTerm.CONFIG + ConstTerm.DATA, ConstTerm.ON_SAVECONFIG, GetConfigFile());
    }

    public void LoadConfig()
    {
        GetTree().CallGroup(ConstTerm.CONFIG + ConstTerm.DATA, ConstTerm.ON_LOADCONFIG, GetConfigFile());
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
        await SaveAllData();

        DirAccess dir = DirAccess.Open(ConstTerm.GAME_FOLDER);
        if (!dir.DirExists(ConstTerm.SAVE_FOLDER)) { dir.MakeDir(ConstTerm.SAVE_FOLDER); }

        ResourceSaver.Save(Instance.gameSession, resourceSavePath); // Save as Resource (.tres)
        using FileAccess readSave = FileAccess.Open(resourceSavePath, FileAccess.ModeFlags.Read);
        using FileAccess fileSave = FileAccess.OpenEncryptedWithPass(savePath, FileAccess.ModeFlags.Write, ConstTerm.ENCRYPT_KEY); // EDIT: Temp encryption pass
        fileSave.StoreString(readSave.GetAsText()); // Save as encrypted file
        fileSave.Close();
        readSave.Close();
        DirAccess.RemoveAbsolute(resourceSavePath); // Remove temporary Resource save
    }

    public Task SaveAllData()
    {
        GatherSystemData();
        GatherBattlers();
        GatherPartyData();
        GatherInventoryData();

        return Task.CompletedTask;
    }

    public void GatherSystemData()
    {
        GetTree().CallGroup(ConstTerm.SYSTEM + ConstTerm.DATA, ConstTerm.ON_SAVEGAME, Instance.gameSession);
    }

    public void GatherBattlers()
    {
        // Dictionary<CharacterID, BattlerData> allBattlers = [];
        GetTree().CallGroup(ConstTerm.BATTLER + ConstTerm.DATA, ConstTerm.ON_SAVEGAME, Instance.gameSession);
        // GD.Print("CharData count - " + Instance.gameSession.CharData.Count);

        // return Instance.gameSession.CharData;
    }

    public void GatherPartyData()
    {
        GetTree().CallGroup(ConstTerm.PARTY + ConstTerm.DATA, ConstTerm.ON_SAVEGAME, Instance.gameSession);
        // GD.Print(Instance.gameSession.PlayerData);
    }

    public void GatherInventoryData()
    {
        GetTree().CallGroup(ConstTerm.INVENTORY + ConstTerm.DATA, ConstTerm.ON_SAVEGAME, Instance.gameSession);
    }

    //=============================================================================
    // SECTION: Load Methods
    //=============================================================================

    public async void LoadGame()
    {
        using SavedGame gameSave = LoadGameInfo();
        if (gameSave == null) { GD.Print("No save data"); return; }

        // if (gameSave == null) { GD.Print("No save file!"); return; }

        await LoadAllData(gameSave);
    }

    public SavedGame LoadGameInfo()
    {
        using FileAccess file = FileAccess.OpenEncryptedWithPass(savePath, FileAccess.ModeFlags.Read, ConstTerm.ENCRYPT_KEY); // Load Encrypted save file // EDIT: Temp encryption pass
        if (file == null) { GD.Print("No file to load!"); return null; }

        using FileAccess fileSave = FileAccess.Open(textSavePath, FileAccess.ModeFlags.Write);
        fileSave.StoreString(file.GetAsText()); // Save as text file
        file.Close();
        fileSave.Close();

        Error dir = DirAccess.RenameAbsolute(textSavePath, resourceSavePath); // Rename .txt to .tres
        using SavedGame tempSave = ResourceLoader.Load(resourceSavePath) as SavedGame; // Load as Resource (.tres)
        DirAccess.RemoveAbsolute(resourceSavePath); // Remove temporary Resource save

        return tempSave;
    }

    public async Task LoadAllData(SavedGame gameSave)
    {
        if (gameSave == null) { GD.Print("No save data"); return; }
        await LoadSystemData(gameSave);
        await LoadBattlerData(gameSave);
        await LoadPartyData(gameSave);
        await LoadInventoryData(gameSave);

        return;
    }

    public Task LoadSystemData(SavedGame gameSave)
    {
        GetTree().CallGroup(ConstTerm.SYSTEM + ConstTerm.DATA, ConstTerm.ON_LOADGAME, gameSave.SystemData);
        return Task.CompletedTask;
    }

    public Task LoadBattlerData(SavedGame gameSave)
    {
        GetTree().CallGroup(ConstTerm.BATTLER + ConstTerm.DATA, ConstTerm.ON_LOADGAME, gameSave.CharData);
        return Task.CompletedTask;
    }

    public Task LoadPartyData(SavedGame gameSave)
    {
        GetTree().CallGroup(ConstTerm.PARTY + ConstTerm.DATA, ConstTerm.ON_LOADGAME, gameSave.PartyData);
        return Task.CompletedTask;
    }

    public Task LoadInventoryData(SavedGame gameSave)
    {
        GetTree().CallGroup(ConstTerm.INVENTORY + ConstTerm.DATA, ConstTerm.ON_LOADGAME, gameSave.InventoryData);
        return Task.CompletedTask;
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
