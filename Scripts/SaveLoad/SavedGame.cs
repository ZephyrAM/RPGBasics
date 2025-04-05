using Godot;
using Godot.Collections;

public partial class SavedGame : Resource
{
    [Export] public Dictionary<CharacterID, BattlerData> CharData { get; set; } = [];
    [Export] public PartyData PartyData { get; set; }
    [Export] public SystemData SystemData { get; set; }
    [Export] public InventoryData InventoryData { get; set; }
}
