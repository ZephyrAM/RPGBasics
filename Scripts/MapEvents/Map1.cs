using Godot;

using ZAM.Interactions;

namespace ZAM.MapEvents
{
    public partial class Map1 : MapEventScript
    {
        public override void _Ready()
        {
            base._Ready();
            mapNumber = "1" + ConstTerm.DIVIDER;
        }        
        //=============================================================================
        // SECTION: Event Methods
        //=============================================================================

        public void Event0(Interactable interactor)
        {
            eventNumber = interactor.Name.ToString()[interactor.Name.ToString().Length - 1] + ConstTerm.DIVIDER;
            textSource = ConstTerm.MAP + mapNumber + ConstTerm.EVENT + eventNumber + ConstTerm.STEP;

            // Version 1 - Not Async
            // GD.Print(interactor.GetStep());
            if (interactor.GetStep() > 0) { goto EventCycle; }
            // interactor.onInteractEventComplete += OnInteractEventComplete;
            interactor.GetMoveAgent().onEndEventStep += OnEndEventStep;
            interactor.GetMoveAgent().GetCollider().Disabled = true;

            EventCycle:
            switch (interactor.GetStep())
            {
                case 0:
                    interactor.AddMoveRoute();
                    break;
                case 1:
                    interactor.AddText(mapText[textSource + interactor.GetStep()][ConstTerm.EN]);
                    break;
                case 2:
                    interactor.AddMoveRoute();
                    break;
                default:
                    GD.Print("still going..." + interactor.GetStep());
                    EmitSignal(SignalName.onEventComplete);
                    break;
            }

            // Version 2 - Async
            // interactor.GetMoveAgent().onEndEventStep += OnEndEventStep;

            // // Step 1
            // interactor.AddMoveRoute();
            // await ToSignal(interactor.GetMoveAgent().GetNavAgent(), NavigationAgent2D.SignalName.NavigationFinished);
            // OnEndEventStep(interactor);

            // // Step 2
            // interactor.AddText("I'm now testing the event text progress. \nSo hello!");
            // await interactor.GetTextBox().IsTextComplete(); // EDIT: Not a valid Awaiter. Need solution.

            // EmitSignal(SignalName.onEventComplete);
        }

        public void Event1()
        {
            GD.Print("Event1 running!");
        }

        //=============================================================================
        // SECTION: Utility Methods
        //=============================================================================
    }
}