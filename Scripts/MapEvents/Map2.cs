using Godot;
using System;

using ZAM.Interactions;

namespace ZAM.MapEvents
{
    public partial class Map2 : MapEventScript
    {
        public override void _Ready()
        {
            base._Ready();
            mapNumber = "2" + ConstTerm.DIVIDER;
        }
        //=============================================================================
        // SECTION: Event Methods
        //=============================================================================

        public void Event0(Interactable interactor)
        {
            eventNumber = interactor.Name.ToString()[interactor.Name.ToString().Length - 1] + ConstTerm.DIVIDER;
            textSource = ConstTerm.MAP + mapNumber + ConstTerm.EVENT + eventNumber + ConstTerm.STEP;

            switch(interactor.GetStep())
            {
                case 0:
                    interactor.AddText(mapText[textSource + interactor.GetStep()][ConstTerm.EN]);
                    break;
                case 1: // EDIT: Needs to pause in between, without restricting player movement
                    interactor.GiveItem("Sword", 2, 1);
                    interactor.GiveItem("Breastplate", 3, 1);
                    interactor.GiveItem("Ring", 4, 1);
                    interactor.ItemMessage();
                    OnEndEventStep(interactor);
                    break;
                default:
                    GD.Print("Complete!");
                    EmitSignal(SignalName.onEventComplete);
                    break;
            }
        }
    }
}