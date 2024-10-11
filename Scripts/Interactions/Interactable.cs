using Godot;

using ZAM.MenuUI;

namespace ZAM.Interactions
{
    public partial class Interactable : CharacterBody2D
    {
        [Export] private Label objectName = null;

        private CanvasLayer uiLayer = null;
        private TextBox textBox = null;
        // private PartyManager playerParty = null;
        // private CharacterController partyInput = null;

        //=============================================================================
        // SECTION: OnReady Methods
        //=============================================================================

        public override void _Ready()
        {
            IfNull();
            // SubSignals();
        }

        // public override void _ExitTree()
        // {
        //     UnSubSignals();
        // }

        private void IfNull()
        {
            objectName ??= GetNode<Label>(ConstTerm.NAMELABEL);

            uiLayer ??= GetNode<CanvasLayer>("../" + ConstTerm.CANVAS_LAYER);
            textBox ??= uiLayer.GetNode<TextBox>(ConstTerm.TEXTBOX_CONTAINER);
            // playerParty ??= GetNode<PartyManager>("../" + ConstTerm.PARTYMANAGER);
            // partyInput ??= playerParty.GetChild<CharacterController>(0);
        }

        // private void SubSignals()
        // {
        //     partyInput.onInteractTarget += OnInteractTarget;
        // }

        // private void UnSubSignals()
        // {
        //     partyInput.onInteractTarget -= OnInteractTarget;
        // }

        // private void OnInteractTarget(GodotObject target)
        // {
        //     if (this == target) {
        //         GD.Print("Hello, I'm new here!");
        //     }
        // }

        //=============================================================================
        // SECTION: Interaction Results
        //=============================================================================

        public void TargetInteraction()
        {
            textBox.AddText(objectName.Text, "Look at the text! I'm testing everything again, just typing along, waiting until I have enough text to really test both the speed and the lenght. Have to make sure it does the word wrap properly and all.");
            // GD.Print("Hello! I'm an interactable!");
        }
    }
}