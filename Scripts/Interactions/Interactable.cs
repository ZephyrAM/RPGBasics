using Godot;
using Godot.Collections;

using ZAM.MenuUI;

namespace ZAM.Interactions
{
    public partial class Interactable : CharacterBody2D
    {
        [Export] private Label objectName = null;

        [ExportGroup("Events")]
        [Export] private Node2D[] movePositions = [];

        [ExportGroup("Interactions")]
        [Export] private Array<InteractType> actionType = [];
        [Export] private int[] choicesGiven = [];
        [Export] private Dictionary<string, string> choiceText = [];
        [Export] private Dictionary<string, Resource> itemList = [];

        [ExportGroup("Values")]
        [Export] private bool doesMove = true;
        [Export] private bool isRepeatable = false; // SaveData
        [Export] private int noRepeatStep = 0;      // SaveData

        [Export] private bool isEvent = false;
        [Export] private bool eventDirectStart = false;

        public enum InteractType
        {
            TEXT = 0,
            CHOICE = 1,
            ITEM = 2
        }

        private string interactPhase = ConstTerm.MOVE;

        private int stepNumber = 0;
        private int choiceStep = 0;
        private int choiceOption = 0;
        private bool choiceActive = false;
        private bool hasPlayed = false;             // SaveData
        private bool isPlaying = false;

        private readonly char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        private NPCMove moveableBody = null;
        private CharacterBody2D playerTarget = null;
        private int currentPosition = 0;

        private CanvasLayer uiLayer = null;
        private TextBox textBox = null;
        private ChoiceBox choiceBox = null;
        private CollectBox collectBox = null;
        // private PartyManager playerParty = null;
        // private CharacterController partyInput = null;

        // Delegate Events \\
        [Signal]
        public delegate void onInteractPhaseEventHandler(string newPhase);
        [Signal]
        public delegate void onItemReceiveEventHandler(string newItem);
        [Signal]
        public delegate void onEventStartEventHandler();
        [Signal]
        public delegate void onInteractEventCompleteEventHandler();

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
            objectName ??= GetNode<Label>(ConstTerm.NAME);

            if (GetNode<NPCMove>(ConstTerm.NPCMOVE) != null) { moveableBody = GetNode<NPCMove>(ConstTerm.NPCMOVE); }

            uiLayer ??= GetNode<CanvasLayer>("../../" + ConstTerm.CANVAS_LAYER);
            
            Node interactLayer = uiLayer.GetNode(ConstTerm.INTERACT_TEXT);
            textBox ??= interactLayer.GetNode<TextBox>(ConstTerm.TEXTBOX + ConstTerm.CONTAINER);
            choiceBox ??= interactLayer.GetNode<ChoiceBox>(ConstTerm.CHOICEBOX + ConstTerm.CONTAINER);
            collectBox ??= interactLayer.GetNode<CollectBox>(ConstTerm.COLLECTBOX);
            // playerParty ??= GetNode<PartyManager>("../" + ConstTerm.PARTYMANAGER);
            // partyInput ??= playerParty.GetChild<CharacterController>(0);

            moveableBody.SetDoesMove(doesMove);
            ResetInteractPhase();
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

        public void TargetInteraction(Vector2 direction)
        {
            if (!isEvent) 
            {
                SetInteractPhase(ConstTerm.INTERACT);
                moveableBody?.FaceDirection(direction);

                if (isRepeatable) { stepNumber = 0; choiceStep = 0; }
                isPlaying = true;

                StepInteract(0);
            } else 
            {
                isPlaying = true;
                EmitSignal(SignalName.onEventStart); 
            }
        }

        public void StepInteract(int adjust)
        {
            // GD.Print(actionType[stepNumber + adjust]);
            switch (actionType[stepNumber + adjust])
            {
                case InteractType.TEXT:
                    AddText();
                    break;
                case InteractType.CHOICE:
                    AddChoiceText();
                    break;
                case InteractType.ITEM:
                    AddItemGive();
                    break;
            }
            stepNumber += adjust;
        }

        public void AddText()
        {
            textBox.ResetTextRatio();
            // GD.Print(stepNumber);
            string outText;
            // GD.Print("text" + stepNumber + alphabet[choiceOption] + " " + choiceActive);
            if (choiceActive) { outText = choiceText["text" + stepNumber + alphabet[choiceOption]]; }
            else { outText = choiceText["text" + stepNumber]; }
            textBox.QueueText(objectName.Text, outText);
            choiceActive = false;
            choiceOption = 0;

            EmitSignal(SignalName.onInteractPhase, ConstTerm.TEXT);
        }

        public void AddChoiceText()
        {
            // GD.Print(stepNumber);
            string[] options = new string[choicesGiven[choiceStep]];
            for (int c = 0; c < options.Length; c++)
            {
                options[c] = choiceText["choice" + stepNumber + alphabet[c]];
            }
            choiceBox.AddChoiceList(options);
            choiceBox.Show();
            choiceStep++;
            choiceActive = true;

            EmitSignal(SignalName.onInteractPhase, ConstTerm.CHOICE);
        }

        public void AddItemGive()
        {
            string outText = "Receieved " + choiceText["item" + stepNumber] + "!";
            collectBox.AddText(outText);
            
            choiceActive = false;
            choiceOption = 0;

            EmitSignal(SignalName.onItemReceive, choiceText["item" + stepNumber]);

            // EmitSignal(SignalName.onInteractPhase, ConstTerm.TEXT);
        }


        //=============================================================================
        // SECTION: External Access
        //=============================================================================

        public void SetPlayer(CharacterBody2D getPlayer)
        {
            playerTarget = getPlayer;
        }

        public void SetChoiceOption(int index)
        {
            choiceOption = index;
        }

        public bool IsPlaying()
        {
            return isPlaying;
        }

        public void SetIsPlaying(bool value)
        {
            isPlaying = value;
        }

        public bool StepCheck()
        {
            stepNumber++;
            // GD.Print("Stepcheck = " + stepNumber);
            bool stepComplete = stepNumber >= actionType.Count;
            if (stepComplete) { isPlaying = false; }
            return stepComplete;
        }

        public void EventComplete()
        {
            isPlaying = false;
            EmitSignal(SignalName.onInteractEventComplete);
        }

        public TextBox GetTextBox()
        {
            return textBox;
        }
        
        public ChoiceBox GetChoiceBox()
        {
            return choiceBox;
        }

        public string GetInteractPhase()
        {
            return interactPhase;
        }

        public void SetInteractPhase(string phase)
        {
            interactPhase = phase;
        }

        public void ResetInteractPhase()
        {
            if (moveableBody.DoesMove()) { SetInteractPhase(ConstTerm.WAIT); }
            else { SetInteractPhase(ConstTerm.DO_NOTHING); }
        }

        public Node2D[] GetMovePositions()
        {
            return movePositions;
        }

        public int GetCurrPosition()
        {
            return currentPosition;
        }

        public void SetCurrPosition(int value)
        {
            currentPosition = value;
        }

        public NPCMove GetMoveAgent()
        {
            return moveableBody;
        }

        public void ResetDirection()
        {
            moveableBody?.RevertDirection();
        }

        public bool IsEventAndInteractStart()
        {
            return isEvent && eventDirectStart;
        }

        public bool IsEvent()
        {
            return isEvent;
        }

        // public bool IsEventDirectStart()
        // {
        //     return eventDirectStart;
        // }
    }
}