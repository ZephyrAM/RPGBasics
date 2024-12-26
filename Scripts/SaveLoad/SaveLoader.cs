using Godot;

public partial class SaveLoader : Node
{
    public static SaveLoader Instance { get; private set; }
    public SavedGame gameSession;

    private string savePath = ConstTerm.GAME_FOLDER + ConstTerm.SAVE_PATH + ConstTerm.SAVE_FOLDER + ConstTerm.SAVE_FILE + ConstTerm.SAVE_TYPE;

    //=============================================================================
    // SECTION: Base Methods
    //=============================================================================

    public override void _Ready()
    {
        Instance = this;
        Instance.gameSession = new();
    }

    public string GetSavePath()
    {
        string folderPath = ConstTerm.GAME_FOLDER + ConstTerm.SAVE_PATH + ConstTerm.SAVE_FOLDER;
        return folderPath;
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

        var dir = DirAccess.Open(ConstTerm.GAME_FOLDER + ConstTerm.SAVE_PATH);
        if (!dir.DirExists(ConstTerm.SAVE_FOLDER)) { dir.MakeDir(ConstTerm.SAVE_FOLDER); }

        ResourceSaver.Save(Instance.gameSession, savePath);
    }

    private void SaveAllData()
    {
        GatherBattlers();
    }

    public void GatherBattlers()
    {
        // Dictionary<CharacterID, BattlerData> allBattlers = [];
        GetTree().CallGroup(ConstTerm.BATTLERDATA, ConstTerm.ON_SAVEGAME, Instance.gameSession.CharData);
        // GD.Print("CharData count - " + Instance.gameSession.CharData.Count);

        // return Instance.gameSession.CharData;
    }

    //=============================================================================
    // SECTION: Load Methods
    //=============================================================================

    public void LoadGame()
    {
        SavedGame gameSave = ResourceLoader.Load(savePath) as SavedGame;
        if (gameSave == null) { GD.Print("No save file!"); return; }

        LoadAllData(gameSave);
    }

    private void LoadAllData(SavedGame gameSave)
    {
        LoadBattlerData(gameSave);
    }

    public void LoadBattlerData(SavedGame gameSave)
    {
        if (gameSave.CharData == null) { GD.Print("No save data"); return; }
        BattlerGroup(gameSave);
    }

    //=============================================================================
    // SECTION: Load Data Types
    //=============================================================================

    public void BattlerGroup(SavedGame gameSave)
    {
        GetTree().CallGroup(ConstTerm.BATTLERDATA, ConstTerm.ON_LOADGAME, gameSave.CharData);
        // GD.Print(GetTree().GetNodeCountInGroup(ConstTerm.BATTLERDATA));
        //     Array<Node> tempGroup = GetTree().GetNodesInGroup(groupName);

        //     for (int n = 0; n < GetTree().GetNodeCountInGroup(groupName); n++)
        //     {
        //         if (tempGroup[n] is not Battler) { continue; }
        //         Battler tempBattler = (Battler)tempGroup[n];

        //         // if (tempBattler.GetCharID() == 0) { continue; } // EDIT = Better conditional
        //         tempBattler.OnLoadData(battlerGroup[tempBattler.GetCharID()]);
        //     }
    }
}
