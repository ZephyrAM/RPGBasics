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
            // mapNumber = "2" + ConstTerm.DIVIDER;
        }
        //=============================================================================
        // SECTION: Event Methods
        //=============================================================================

        public void Event1(Interactable interactor)
        {
            // eventNumber = interactor.Name.ToString()[^1] + ConstTerm.DIVIDER;
            // textSource = ConstTerm.MAP + mapNumber + ConstTerm.EVENT + eventNumber + ConstTerm.STEP;

            switch(interactor.GetStep())
            {
                case 0:
                    // interactor.AddText(mapText[textSource + interactor.GetStep()][ConstTerm.EN]);
                    interactor.AddText(MapID.Map2.ToString() + "." + MethodName.Event1 + "." + ConstTerm.TEXT + interactor.GetStep());
                    break;
                case 1: // EDIT: Needs to pause in between, without restricting player movement
                    interactor.GiveItem("Sword", 2, 1);
                    interactor.GiveItem("Breastplate", 3, 1);
                    interactor.GiveItem("Ring", 4, 1);
                    interactor.ItemMessage();
                    interactor.EndStep(ConstTerm.ITEM);
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