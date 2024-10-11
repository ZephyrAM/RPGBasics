using Godot;
using Godot.Collections;

using ZAM.Stats;

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

    //=============================================================================
    // SECTION: Save Methods
    //=============================================================================

    public Dictionary<CharacterID, BattlerData> GatherBattlers()
    {
        Dictionary<CharacterID, BattlerData> allBattlers = [];
        GetTree().CallGroup(ConstTerm.SAVEDATA, ConstTerm.ON_SAVEGAME, allBattlers);

        return allBattlers;
    }

    public void SaveGame()
    {
        SavedGame gameSave = new() {
            CharData = GatherBattlers()
        };

        var dir = DirAccess.Open(ConstTerm.GAME_FOLDER + ConstTerm.SAVE_PATH);
        if (!dir.DirExists(ConstTerm.SAVE_FOLDER)) { dir.MakeDir(ConstTerm.SAVE_FOLDER); }

        ResourceSaver.Save(gameSave, savePath);
    }

    //=============================================================================
    // SECTION: Load Methods
    //=============================================================================

    public void FillData(SavedGame gameSave)
    {
        if (gameSave.CharData == null) { return; }
        BattlerGroup(ConstTerm.SAVEDATA, gameSave.CharData);
    }

    public void LoadGame()
    {
        SavedGame gameSave = ResourceLoader.Load(savePath) as SavedGame;
        if (gameSave == null) { GD.Print("No save file!"); return; }

        FillData(gameSave);
    }

    //=============================================================================
    // SECTION: Load Data Types
    //=============================================================================

    public void BattlerGroup(string groupName, Dictionary<CharacterID, BattlerData> battlerGroup)
    {
        Array<Node> tempGroup = GetTree().GetNodesInGroup(groupName);

        for (int n = 0; n < GetTree().GetNodeCountInGroup(groupName); n++)
        {
            // GD.Print(tempGroup[n].GetType());
            if (tempGroup[n] is not Battler) { continue; }
            Battler tempBattler = (Battler)tempGroup[n];

            // if (tempBattler.GetCharID() == 0) { continue; } // EDIT = Better conditional
            tempBattler.OnLoadData(battlerGroup[tempBattler.GetCharID()]);
        }
    }
}
