using Godot;
using System;

using ZAM.Interactions;

namespace ZAM.MapEvents
{
    public partial class Map1 : MapEventScript
    {
        //=============================================================================
        // SECTION: Event Methods
        //=============================================================================

        public void Event0(Interactable interactor)
        {
            GD.Print("Hello from the event!");
            Node2D[] movePositions = interactor.GetMovePositions();
            interactor.onInteractEventComplete += OnInteractEventComplete;

            interactor.SetCurrPosition(interactor.GetCurrPosition() + 1);
            if (interactor.GetCurrPosition() >= movePositions.Length) { interactor.SetCurrPosition(0); }
            interactor.GetMoveAgent().MoveToTarget(movePositions[interactor.GetCurrPosition()]);
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