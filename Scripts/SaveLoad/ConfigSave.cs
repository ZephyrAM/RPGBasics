using Godot;
using Godot.Collections;

public partial class ConfigSave : Node
{
    public ConfigFile configSave = new();
    private Dictionary<StringName, Array<InputEvent>> inputs = [];

    private string savePath = ConstTerm.GAME_FOLDER + ConstTerm.CFG_FILE;

    public override void _Ready()
    {
        GetInputActions();
    }

    public void GetInputActions()
    {
        for (int i = 0; i < InputMap.GetActions().Count; i++)
        {
            StringName tempName = InputMap.GetActions()[i];
            if (InputMap.GetActions()[i].ToString().StartsWith("ui_")) { continue; }
            else { inputs.Add(InputMap.GetActions()[i], InputMap.ActionGetEvents(tempName)); }
        }
        
        GD.Print(inputs["Menu"]);
        InputEventKey tempEvent = inputs["Menu"][1] as InputEventKey;
        GD.Print(tempEvent.PhysicalKeycode);
        Key tempKey = DisplayServer.KeyboardGetKeycodeFromPhysical(tempEvent.PhysicalKeycode);
        GD.Print(OS.GetKeycodeString(tempKey));

        GD.Print(inputs["Menu"]);
    }

    public void SaveInputMap()
    {

    }
}
