using Godot;
using Godot.Collections;

using ZAM.Interactions;

namespace ZAM.MapEvents
{
    public partial class MapEventScript : Node
    {
        // protected Dictionary<string, Dictionary<string, string>> mapText = [];

        // protected string mapNumber;
        // protected string eventNumber;
        // protected string textSource;
        
        // Delegate Events \\
        [Signal]
        public delegate void onEventCompleteEventHandler();

        public override void _Ready()
        {
            // LoadMapText();
        }

        // protected void LoadMapText()
        // {
        //     // using FileAccess textFile = FileAccess.Open("./Resources/Data/InteractTextData.json", FileAccess.ModeFlags.Read);
        //     string jsonFile = FileAccess.GetFileAsString(SaveLoader.Instance.GetLangFile());
        //     Json tempJson = new();
        //     tempJson.Parse(jsonFile);
        //     mapText = (Dictionary<string, Dictionary<string, string>>)tempJson.Data;
        //     // jsonFile = File.ReadAllText("./Resources/Data/InteractTextData.json");
        //     // mapText = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonFile);
        //     // mapText = (Dictionary<string, Dictionary<string, string>>)Json.ParseString(jsonFile);
        // }

        //=============================================================================
        // SECTION: External Access
        //=============================================================================

        // public string GetMapNumer()
        // {
        //     return mapNumber;
        // }

        // public Dictionary<string, Dictionary<string, string>> GetMapText()
        // {
        //     return mapText;
        // }

        //=============================================================================
        // SECTION: Signal Calls
        //=============================================================================

        // public void OnInteractEventComplete()
        // {
        //     EmitSignal(SignalName.onEventComplete);
        // }

        public void OnEndEventStep(Interactable interactor)
        {
            if (!interactor.IsEvent) { return; }

            if (interactor.StepCheck()) { EmitSignal(SignalName.onEventComplete); } // -> MapSystem
            else { Call(interactor.Name, interactor); } // -> Map#: Event# function
        }
    }
}