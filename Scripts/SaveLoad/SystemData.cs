using Godot;
using System;

public partial class SystemData : Resource
{
    [Export] public MapID SavedSceneName { get; set; } // Scene being saved
    [Export] public MapID LoadCurrentMapID { get; set; } // For loading access purposes - map specific npcs, etc
}
