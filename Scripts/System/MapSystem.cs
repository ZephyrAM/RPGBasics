using Godot;
using System;

using ZAM.Control;
using ZAM.Interactions;
using ZAM.Inventory;
using ZAM.MenuUI;

namespace ZAM.System
{
    public partial class MapSystem : Node
    {
        [Export] private PackedScene battleScene;
        [Export] private PartyManager playerParty;

        private CharacterController playerInput = null;
        private BattleSystem battleNode = null;
        private RayCast2D interactRay = null;
        private Interactable interactTarget;

        private CanvasLayer uiLayer = null;
        private TextBox textBox = null;
        private ChoiceBox choiceBox = null;
        private int choiceCommand = 0;

        //=============================================================================
        // SECTION: Base Methods
        //=============================================================================

        public override void _Ready()
        {
            IfNull();
            SubSignals();
            SaveLoader.Instance.gameSession.CharData = SaveLoader.Instance.GatherBattlers(); // Should only happen once, when game loads.
        }

        public override void _PhysicsProcess(double delta)
        {
            SpeedyText();
        }

        //=============================================================================
        // SECTION: OnReady Methods
        //=============================================================================

        private void IfNull()
        {
            battleScene ??= ResourceLoader.Load<PackedScene>(ConstTerm.BATTLE_SCENE);
            playerParty ??= GetNode<PartyManager>(ConstTerm.PARTYMANAGER);
            playerInput ??= playerParty.GetPlayer();

            uiLayer ??= GetNode<CanvasLayer>(ConstTerm.CANVAS_LAYER);
            textBox ??= uiLayer.GetNode<TextBox>(ConstTerm.TEXTBOX + ConstTerm.CONTAINER);
            choiceBox ??= uiLayer.GetNode<ChoiceBox>(ConstTerm.CHOICEBOX + ConstTerm.CONTAINER);

            // interactRay ??= playerInput.GetNode<RayCast2D>(ConstTerm.RAYCAST2D);
        }

        private void SubSignals()
        {
            playerInput.onInteractCheck += OnInteractCheck;
            playerInput.onSelectChange += OnSelectChange;
            playerInput.onTextProgress += OnTextProgress;
            playerInput.onChoiceSelect += OnChoiceSelect;
        }

        private void UnSubSignals() // Inactive: Mapsystem doesn't run _Ready again after _ExitTree
        {
            playerInput.onInteractCheck -= OnInteractCheck;
            playerInput.onSelectChange -= OnSelectChange;
            playerInput.onTextProgress -= OnTextProgress;
            playerInput.onChoiceSelect -= OnChoiceSelect;
        }

        // public override void _EnterTree()
        // {
        //     GD.Print("Map Tree Enter");
        // }

        // public override void _ExitTree()
        // {
        //     battleNode.onBuildPlayerTeam -= BuildParty;
        // }


        //=============================================================================
        // SECTION: Interaction Handling
        //=============================================================================

        private void SpeedyText()
        {
            if (playerInput.GetInputPhase() == ConstTerm.TEXT) { 
                interactTarget.GetTextBox().FasterText(playerInput.TextSpeedCheck()); }
        }

        private void InteractCheck()
        {
            interactRay.ForceRaycastUpdate();
            if (interactRay.IsColliding() && !textBox.Visible)
            {
                playerInput.SetIdleAnim();
                if (interactRay.GetCollider() is Interactable)
                {
                    SetInteractTarget();
                    interactTarget.TargetInteraction();
                }
            }
        }

        private void UpdateInteraction()
        {
            bool stepComplete = interactTarget.StepCheck();
            if (stepComplete)
            {
                textBox.HideTextBox();
                RemoveInteractTarget();
            }
            else
            {
                interactTarget.StepInteract(0);
            }
        }

        private void SelectChoice()
        {
            // playerInput.SetInputPhase(ConstTerm.INTERACT);
            choiceBox.HideChoiceBox();
            interactTarget.StepInteract(1);
            choiceCommand = 0;
        }

        private void SetInteractTarget()
        {
            playerInput.SetInteractToggle(true);
            interactTarget = (Interactable)interactRay.GetCollider();
            interactTarget.onPhaseSwitch += OnPhaseSwitch;
            interactTarget.onItemReceive += OnItemReceive;
        }

        private void RemoveInteractTarget()
        {
            interactTarget.onPhaseSwitch -= OnPhaseSwitch;
            interactTarget.onItemReceive -= OnItemReceive;
            interactTarget = null;
            playerInput.SetInteractToggle(false);
            playerInput.SetInputPhase(ConstTerm.MOVE);
        }


        //=============================================================================
        // SECTION: Access Methods
        //=============================================================================

        public async void LoadBattle(PackedScene randomGroup)
        {
            playerParty.ChangePlayerActive(false);
            Fader.Instance.Transition();
            await ToSignal(Fader.Instance, ConstTerm.TRANSITION_FINISHED);

            battleNode = (BattleSystem)battleScene.Instantiate();
            battleNode.SetEnemyGroup(randomGroup);
            battleNode.StoreMapScene(this);
            battleNode.SetBattleControlActive(true);

            GetTree().Root.AddChild(battleNode);
            GetTree().Root.RemoveChild(this);

            // QueueFree();
        }

        public PartyManager GetPartyManager()
        {
            return playerParty;
        }

        // private void OnSaveGame()
        // {

        // }


        //=============================================================================
        // SECTION: Signal Methods
        //=============================================================================

        private void OnInteractCheck(RayCast2D ray)
        {
            interactRay = ray;
            InteractCheck();
        }

        private void OnPhaseSwitch(string newPhase)
        {
            playerInput.SetInputPhase(newPhase);
        }

        private void OnSelectChange()
        {
            int index = playerInput.GetChoice();
            interactTarget.GetChoiceBox().MoveCursor(index);
            interactTarget.SetChoiceOption(index);
        }

        private void OnTextProgress()
        {
            if (interactTarget.GetTextBox().IsTextComplete()) {
                UpdateInteraction();
            }
        }

        private void OnChoiceSelect()
        {
            SelectChoice();
        }

        private void OnItemReceive(string newItem)
        {
            ItemBag.Instance.AddToItemBag(newItem);
        }
    }
}