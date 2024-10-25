using Godot;
using Godot.Collections;

using ZAM.MenuUI;

namespace ZAM.Interactions
{
    public partial class Interactable : CharacterBody2D
    {
        [Export] private Label objectName = null;

        [ExportGroup("Interactions")]
        [Export] private Array<InteractType> actionType = [];
        [Export] private int[] choicesGiven = [];
        [Export] private Dictionary<string, string> choiceText = [];
        [Export] private Dictionary<string, Resource> itemList = [];

        [ExportGroup("Values")]
        [Export] private bool isRepeatable = false; // SaveData
        [Export] private int noRepeatStep = 0;      // SaveData

        private int stepNumber = 0;
        private int choiceStep = 0;
        private int choiceOption = 0;
        private bool choiceActive = false;
        private bool hasPlayed = false;             // SaveData
        private bool isPlaying = false;

        private readonly char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        private CanvasLayer uiLayer = null;
        private TextBox textBox = null;
        private ChoiceBox choiceBox = null;
        // private PartyManager playerParty = null;
        // private CharacterController partyInput = null;

        // Delegate Events \\
        [Signal]
        public delegate void onPhaseSwitchEventHandler(string newPhase);

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

            uiLayer ??= GetNode<CanvasLayer>("../../" + ConstTerm.CANVAS_LAYER);
            textBox ??= uiLayer.GetNode<TextBox>(ConstTerm.TEXTBOX_CONTAINER);
            choiceBox ??= uiLayer.GetNode<ChoiceBox>(ConstTerm.CHOICEBOX_CONTAINTER);
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
            if (isRepeatable) { stepNumber = 0; choiceStep = 0;}
            isPlaying = true;

            StepInteract(0);
        }

        public void StepInteract(int adjust)
        {
            GD.Print(actionType[stepNumber + adjust]);
            switch (actionType[stepNumber + adjust])
            {
                case InteractType.TEXT:
                    AddText();
                    break;
                case InteractType.CHOICE:
                    AddChoiceText();
                    break;
                case InteractType.ITEM:
                    break;
            }
            stepNumber += adjust;
        }

        public void AddText()
        {
            textBox.ResetTextRatio();
            GD.Print(stepNumber);
            string outText;
            GD.Print("text" + stepNumber + alphabet[choiceOption] + " " + choiceActive);
            if (choiceActive) { outText = choiceText["text" + stepNumber + alphabet[choiceOption]]; }
            else { outText = choiceText["text" + stepNumber]; }
            textBox.AddText(objectName.Text, outText);
            choiceActive = false;
            choiceOption = 0;

            EmitSignal(SignalName.onPhaseSwitch, ConstTerm.TEXT);
        }

        public void AddChoiceText()
        {
            GD.Print(stepNumber);
            string[] options = new string[choicesGiven[choiceStep]];
            for (int c = 0; c < options.Length; c++)
            {
                options[c] = choiceText["choice" + stepNumber + alphabet[c]];
            }
            choiceBox.AddChoiceList(options);
            choiceBox.Show();
            choiceStep++;
            choiceActive = true;

            EmitSignal(SignalName.onPhaseSwitch, ConstTerm.CHOICE);
        }


        //=============================================================================
        // SECTION: External Access
        //=============================================================================

        public void SetChoiceOption(int index)
        {
            choiceOption = index;
        }

        public bool IsPlaying()
        {
            return isPlaying;
        }

        public bool StepCheck()
        {
            stepNumber++;
            GD.Print("Stepcheck = " + stepNumber);
            bool stepComplete = stepNumber >= actionType.Count;
            if (stepComplete) { isPlaying = false; }
            return stepComplete;
        }

        public TextBox GetTextBox()
        {
            return textBox;
        }
        
        public ChoiceBox GetChoiceBox()
        {
            return choiceBox;
        }
    }
}