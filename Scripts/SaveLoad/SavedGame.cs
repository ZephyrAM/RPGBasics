using Godot;
using Godot.Collections;

public partial class SavedGame : Resource
{
    [Export] public SystemData SystemData { get; set; }
    [Export] public InventoryData InventoryData { get; set; }
    [Export] public PartyData PartyData { get; set; }
    [Export] public Dictionary<CharacterID, BattlerData> CharData { get; set; } = [];

    // [Export] public MapData MapData { get; set; }
    [Export] public MapID CurrentMap { get; set; }
    [Export] public Dictionary<MapID, Dictionary<string, InteractData>> InteractData { get; set; } = [];
}
