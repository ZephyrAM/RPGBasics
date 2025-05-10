using Godot;
using Godot.Collections;

// using ZAM.MapEvents;
using ZAM.MenuUI;

namespace ZAM.Interactions
{
    public partial class Interactable : CharacterBody2D
    {
        [ExportGroup("Nodes")]
        // [Export] private Node2D eventNode = null;
        [Export] private Label objectName = null;

        [ExportGroup("Events")]
        [Export] private Node2D[] movePositions = [];
        [Export] private int eventStepCount = 0;
        [Export] public bool IsEvent { get; private set; } = false;

        [ExportGroup("Interactions")]
        [Export] private Array<InteractType> actionType = [];
        // [Export] private Dictionary<string, Dictionary<InteractType, string>> actionMap = []; // Needs work. Doesn't work in Editor, Dictionary only basic types.
        [Export] private int[] choicesGiven = [];
        [Export] private Dictionary<string, string> choiceText = [];
        // [Export] private Dictionary<string, Resource> itemList = [];

        [ExportGroup("Values")]
        [Export] public bool IsInteractable { get; private set; } = true;
        [Export] public bool DoesMove { get; private set; } = true;
        [Export] public bool IsRepeatable { get; private set; } = false; // SaveData
        [Export] private int noRepeatStep = 0;      // SaveData

        [Export] public bool ShouldChasePlayer { get; private set; } = false;
        [Export] public bool IsAutoBattle { get; private set; } = false;
        [Export] private PackedScene battleGroup = null;

        // private MapEventScript mapEventScript;
        // private Dictionary<string, Dictionary<string, string>> mapText;

        private string interactPhase = ConstTerm.MOVE;

        private int stepNumber = 0;
        private int choiceStep = 0;
        private int choiceOption = 0;

        private bool choiceActive = false;
        private bool hasPlayed = false;             // SaveData
        public bool IsPlaying { get; private set; } = false;

        private readonly char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        private NPCMove moveableBody = null;
        private CharacterBody2D playerTarget = null;
        private int currentMovePos = 0;

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
        public delegate void onItemReceiveEventHandler(string newItem, int newType, int count);
        [Signal]
        public delegate void onEventStartEventHandler();
        // [Signal]
        // public delegate void onClickInteractEventHandler(Interactable target);
        // [Signal]
        // public delegate void onInteractClearEventHandler();


        //=============================================================================
        // SECTION: OnReady Methods
        //=============================================================================

        public override void _Ready()
        {
            IfNull();
            // InputEvent += OnInputEvent;
            // MouseExited += OnMouseExited;
            // SubSignals();
        }

        private void IfNull()
        {
            objectName ??= GetNode<Label>(ConstTerm.NAME);

            if (GetNode<NPCMove>(ConstTerm.NPCMOVE) != null) { moveableBody = GetNode<NPCMove>(ConstTerm.NPCMOVE); }

            uiLayer = GetNode<CanvasLayer>("../../" + ConstTerm.CANVAS_LAYER);
            
            Node interactLayer = uiLayer.GetNode(ConstTerm.INTERACT_TEXT);
            textBox = interactLayer.GetNode<TextBox>(ConstTerm.TEXTBOX + ConstTerm.CONTAINER);
            choiceBox = interactLayer.GetNode<ChoiceBox>(ConstTerm.CHOICEBOX + ConstTerm.CONTAINER);
            collectBox = interactLayer.GetNode<CollectBox>(ConstTerm.COLLECTBOX);
            // playerParty ??= GetNode<PartyManager>("../" + ConstTerm.PARTYMANAGER);
            // partyInput ??= playerParty.GetChild<CharacterController>(0);

            moveableBody.SetDoesMove(DoesMove);
            ResetInteractPhase();
        }

        private void LoadChoiceText()
        {
            // mapEventScript = (MapEventScript)eventNode.GetScript();
            // mapText = mapEventScript.GetMapText();
        }

        // private void SubSignals()
        // {
        //     moveableBody.onEndNavigation += OnEndNavigation;
        // }

        //=============================================================================
        // SECTION: Interaction Results
        //=============================================================================

        public bool CheckValidInteract()
        {
            return IsInteractable;
        }

        public void TargetInteraction(Vector2 direction)
        {
            // GD.Print("Target Interaction");
            // LoadChoiceText();
            if (IsAutoBattle) { moveableBody.TriggerBattler(); return; }

            if (!IsEvent) 
            {
                if (actionType.Count <= 0) { return; }
                SetInteractPhase(ConstTerm.INTERACT);
                moveableBody?.FaceDirection(direction);

                if (IsRepeatable) { stepNumber = 0; choiceStep = 0; }
                IsPlaying = true;

                StepInteract(0);
            } else 
            {
                IsPlaying = true;
                EmitSignal(SignalName.onEventStart); // -> MapSystem - OnEventStart
            }
        }

        public void StepInteract(int adjust)
        {
            // GD.Print(actionType[stepNumber + adjust]);
            if (IsEvent) { GD.PushError("Event active! StepInteract invalid usage."); return; }

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
                case InteractType.MOVE:
                    AddMoveRoute();
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
            if (choiceActive) { outText = choiceText[ConstTerm.TEXT + stepNumber + alphabet[choiceOption]]; }
            else { outText = choiceText[ConstTerm.TEXT + stepNumber]; }
            textBox.QueueText(objectName.Text, outText);
            choiceActive = false;
            choiceOption = 0;

            EmitSignal(SignalName.onInteractPhase, ConstTerm.TEXT); // -> MapSystem - OnInteractPhase
        }

        public void AddText(string textOut)
        {
            textBox.ResetTextRatio();
            textBox.QueueText(objectName.Text, textOut);

            EmitSignal(SignalName.onInteractPhase, ConstTerm.TEXT);
        }

        public void AddChoiceText()
        {
            // GD.Print(stepNumber);
            string[] options = new string[choicesGiven[choiceStep]];
            for (int c = 0; c < options.Length; c++)
            {
                options[c] = choiceText[ConstTerm.CHOICE + stepNumber + alphabet[c]];
            }
            choiceBox.AddChoiceList(options);
            choiceBox.Show();
            choiceStep++;
            choiceActive = true;

            EmitSignal(SignalName.onInteractPhase, ConstTerm.CHOICE);
        }

        public void AddChoiceText(string textOut, string[] options)
        {
            choiceBox.AddChoiceList(options);
            choiceBox.Show();
            choiceActive = true;

            EmitSignal(SignalName.onInteractPhase, ConstTerm.CHOICE);
        }

        public void AddItemGive()
        {
            string giveText;
            ItemType giveType;
            int count = 1;

            if (choiceText.ContainsKey(ConstTerm.ITEM + stepNumber)) { giveText = ConstTerm.ITEM + stepNumber; giveType = ItemType.Item; }
            else if (choiceText.ContainsKey(ConstTerm.WEAPON + stepNumber)) { giveText = ConstTerm.WEAPON + stepNumber; giveType = ItemType.Weapon; }
            else if (choiceText.ContainsKey(ConstTerm.ARMOR + stepNumber)) { giveText = ConstTerm.ARMOR + stepNumber; giveType = ItemType.Armor; }
            else { giveText = ConstTerm.ACCESSORY + stepNumber; giveType = ItemType.Accessory;}

            string outText = "Receieved " + choiceText[giveText] + "!"; // EDIT: Placeholder
            collectBox.QueueText(outText);
            ItemMessage();
            
            choiceActive = false;
            choiceOption = 0;

            EmitSignal(SignalName.onItemReceive, choiceText[giveText], (int)giveType, count); // -> MapSystem - OnItemReceive

            // EmitSignal(SignalName.onInteractPhase, ConstTerm.TEXT);
        }

        public void AddMoveRoute()
        {
            EmitSignal(SignalName.onInteractPhase, ConstTerm.DO_NOTHING);
            SetCurrPosition(GetCurrPosition() + 1);
            // if (GetCurrPosition() >= movePositions.Length) { SetCurrPosition(0); } // Looping. Unneeded for structured events.
            // GD.Print("Move to point - " + GetCurrPosition());
            GetMoveAgent().MoveToTarget(movePositions[GetCurrPosition()]); // -> NPCMove
        }


        //=============================================================================
        // SECTION: Event Calls
        //=============================================================================

        public void GiveItem(string itemName, int itemType, int amount)
        {
            string outText = "Receieved " + itemName + "!"; // EDIT: Placeholder
            collectBox.QueueText(outText);
            // await ToSignal(collectBox, ) // EDIT: Work in awaiter for animation

            EmitSignal(SignalName.onItemReceive, itemName, itemType, amount); // -> MapSystem
        }

        public void ItemMessage()
        {
            collectBox.CompleteCollectText();
        }


        //=============================================================================
        // SECTION: External Access
        //=============================================================================

        public bool StepCheck()
        {
            stepNumber++;
            // GD.Print("Stepcheck = " + stepNumber);
            bool stepComplete;

            if (IsEvent) { stepComplete = stepNumber >= eventStepCount; }
            else { stepComplete = stepNumber >= actionType.Count; }

            if (stepComplete) { IsPlaying = false; }
            return stepComplete;
        }

        public void TurnOffEvent()
        {
            IsEvent = false;
            IsInteractable = false;
        }

        public void SetPlayer(CharacterBody2D getPlayer)
        {
            playerTarget = getPlayer;
        }

        public void SetChoiceOption(int index)
        {
            choiceOption = index;
            choiceBox.SetChoiceOption(choiceOption);
        }

        public void SetIsPlaying(bool value)
        {
            IsPlaying = value;
        }

        public void SetInteractPhase(string phase)
        {
            interactPhase = phase;
        }

        public void SetCurrPosition(int value)
        {
            currentMovePos = value;
        }

        public void ResetInteractPhase()
        {
            if (moveableBody.DoesMove()) { SetInteractPhase(ConstTerm.WAIT); }
            else { SetInteractPhase(ConstTerm.DO_NOTHING); }
        }

        public void ResetEvent() // EDIT: Mainly testing purposes
        {
            stepNumber = 0;
            SetCurrPosition(0);
            GetMoveAgent().GetCollider().Disabled = false;
        }       

        public void ResetDirection()
        {
            moveableBody?.RevertDirection();
        }

        //=============================================================================
        // SECTION: Get Access
        //=============================================================================

        public NPCMove GetMoveAgent()
        {
            return moveableBody;
        }

        public PackedScene GetBattleGroup()
        {
            return battleGroup;
        }

        public Node2D[] GetMovePositions()
        {
            return movePositions;
        }

        public TextBox GetTextBox()
        {
            return textBox;
        }

        public ChoiceBox GetChoiceBox()
        {
            return choiceBox;
        }

        public int GetStep()
        {
            return stepNumber;
        }

        public int GetCurrPosition()
        {
            return currentMovePos;
        }        

        public string GetInteractPhase()
        {
            return interactPhase;
        }

        // public bool IsEventDirectStart()
        // {
        //     return eventDirectStart;
        // }

        //=============================================================================
        // SECTION: Save System
        //=============================================================================

        // public void OnSaveGame(SavedGame saveData)
        // {
        //     InteractData newData = new()
        //     {
        //         DataIsEvent = IsEvent,
        //         DataIsInteractable = IsInteractable,
        //         NoRepeatStep = noRepeatStep
        //     };
        //     if (!saveData.InteractData.TryGetValue(saveData.SystemData.SavedSceneName, out Dictionary<string, InteractData> data)) {
        //         data = [];
        //         saveData.InteractData.Add(saveData.SystemData.SavedSceneName, data); }

        //     // data[Name] = newData;
        //     saveData.InteractData[saveData.SystemData.SavedSceneName][Name] = newData;
        // }

        // public void OnLoadGame(Dictionary<MapID, Dictionary<string, InteractData>> loadData, int currentID)
        // {
        //     if (!loadData.ContainsKey((MapID)currentID)) { return; }

        //     InteractData saveData = loadData[(MapID)currentID][Name];
        //     if (saveData == null) { GD.Print("Interact Data - NULL"); return; }

        //     IsEvent = saveData.DataIsEvent;
        //     IsInteractable = saveData.DataIsInteractable;
        //     noRepeatStep = saveData.NoRepeatStep;
        // }

        public void OnSaveFile(ConfigFile saveData)
        {
            int map = (int)saveData.GetValue(ConstTerm.SYSTEM + ConstTerm.DATA, ConstTerm.SAVED + ConstTerm.MAP + ConstTerm.ID);
            string sectionID = ConstTerm.MAP + map + ConstTerm.INTERACT + Name + ConstTerm.DATA;

            saveData.SetValue(sectionID, ConstTerm.IS + ConstTerm.EVENT, IsEvent);
            saveData.SetValue(sectionID, ConstTerm.IS + ConstTerm.INTERACT, IsInteractable);
            saveData.SetValue(sectionID, ConstTerm.REPEAT_STEP, noRepeatStep);
            saveData.SetValue(sectionID, ConstTerm.CURRENT + ConstTerm.MOVE + ConstTerm.POSITION, currentMovePos);
            if (currentMovePos > 0) {
                saveData.SetValue(sectionID, ConstTerm.POSITION, GlobalPosition);
                saveData.SetValue(sectionID, ConstTerm.DIRECTION, moveableBody.GetLookDirection());
            }
        }

        public void OnLoadFile(ConfigFile loadData)
        {
            int map = (int)loadData.GetValue(ConstTerm.SYSTEM + ConstTerm.DATA, ConstTerm.LOADING + ConstTerm.MAP + ConstTerm.ID);
            string sectionID = ConstTerm.MAP + map + ConstTerm.INTERACT + Name + ConstTerm.DATA;

            if (loadData.HasSection(sectionID)) {
                IsEvent = (bool)loadData.GetValue(sectionID, ConstTerm.IS + ConstTerm.EVENT);
                IsInteractable = (bool)loadData.GetValue(sectionID, ConstTerm.IS + ConstTerm.INTERACT);
                noRepeatStep = (int)loadData.GetValue(sectionID, ConstTerm.REPEAT_STEP);
                currentMovePos = (int)loadData.GetValue(sectionID, ConstTerm.CURRENT + ConstTerm.MOVE + ConstTerm.POSITION);
                if (currentMovePos > 0) {
                    GlobalPosition = (Vector2)loadData.GetValue(sectionID, ConstTerm.POSITION);
                    moveableBody.SetLookDirection((Vector2)loadData.GetValue(sectionID, ConstTerm.DIRECTION));
                }
            }
        }

        //=============================================================================
        // SECTION: Signal Calls
        //=============================================================================

        // private void OnEndNavigation()
        // {
        //     // if (isEvent) { 
        //     //     stepNumber++;
        //     //     EmitSignal(SignalName.onEventStart); 
        //     // }
        // }

        // private void OnInputEvent(Node viewport, InputEvent inputEvent, long shapeidx)
        // {
        //     // EDIT
        //     EmitSignal(SignalName.onClickInteract, this);
        // }

        // private void OnMouseExited()
        // {
        //     EmitSignal(SignalName.onInteractClear);
        // }


        //=============================================================================
        // SECTION: Utility Methods
        //=============================================================================

        // public static bool CheckApprox(Vector2 first, Vector2 second) // Pixel checking scale
        // {
        //     GD.Print(first, second);
        //     bool xCheck = first.X <= second.X + 5 && first.X >= second.X - 5;
        //     GD.Print(xCheck);
        //     bool yCheck = first.Y <= second.Y + 5 && first.Y >= second.Y - 5;
        //     GD.Print(yCheck);

        //     return xCheck && yCheck;
        // }
    }
}