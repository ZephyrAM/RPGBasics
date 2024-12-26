using Godot;
using System;

namespace ZAM.MapEvents
{
    public partial class MapEventScript : Node
    {
        // Delegate Events \\
        [Signal]
        public delegate void onEventCompleteEventHandler();

        //=============================================================================
        // SECTION: Signal Calls
        //=============================================================================

        public void OnInteractEventComplete()
        {
            EmitSignal(SignalName.onEventComplete);
        }
    }
}