using Godot;
using System;
using System.Collections.Generic;

using ZAM.Controller;
using ZAM.Interactions;
using ZAM.Abilities;
using ZAM.Stats;

using ZAM.Inventory;
using ZAM.MenuUI;

using ZAM.Managers;
using ZAM.MapEvents;

namespace ZAM.System
{
    public partial class MapSystem : Node
    {
        [Export] private MapID mapId;

        [ExportGroup("Nodes")]
        [Export] private PartyManager playerParty;
        [Export] private AudioStream bgm = null;
        [Export] private MenuController menuInput = null;
        [Export] private CanvasLayer uiLayer = null;
        [Export] private Node2D transit = null;

        [ExportGroup("Battle Info")]
        [Export] private PackedScene battleScene;
        [Export] private EncounterArea[] battleAreas;

        [ExportGroup("Resources")]
        [Export] private PackedScene labelSettings = null;
        [Export] private Script eventScript = null;

        private AnimationPlayer bgmPlayer = null;
        private BattleSystem battleNode = null;
        private Node2D eventNode = null;
        private MapEventScript mapEvents = null;

        private CharacterController playerInput = null;
        private List<RayCast2D> interactArray = [];
        private RayCast2D mouseRay = null;
        private Interactable interactTarget;

        private List<Transitions> travelList = [];

        private TextBox textBox = null;
        private ChoiceBox choiceBox = null;

        private InfoMenu menuScreen = null;
        private PauseController pauseScreen = null;

        private int choiceCommand = 0;
        private int memberSelected = 0;
        private int rayTarget = 0;

        private bool barVisible = false;
        // private bool hasLoaded = false;

        //=============================================================================
        // SECTION: Base Methods
        //=============================================================================

        public override void _Ready()
        {
            IfNull();
            SubSignals();

            menuInput.SetPlayer(playerInput);
            menuScreen.SetupParty(playerParty.GetPartySize());
            menuScreen.SetupCommandList(menuInput.GetCommandOptions());
            SetupItemList();
            UpdateMenuInfo();

            // // GD.Print("Map ready - save data");
            SaveLoader.Instance.GatherBattlers(); // Should only happen once, when game loads.
            // hasLoaded = true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            UnSubSignals();
        }

        // public override void _EnterTree()
        // {
        //     // if (hasLoaded) {
        //     //     GD.Print("Map enter tree - load data");
        //     //     SaveLoader.Instance.LoadBattlerData(SaveLoader.Instance.gameSession);
        //     //     GD.Print(SaveLoader.Instance.gameSession.CharData[playerParty.GetPlayerParty()[0].GetCharID()].CurrentHP);
        //     //     GD.Print(playerParty.GetPlayerParty()[0].GetHealth().GetHP());
        //     // }
        // }

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
            menuInput ??= uiLayer.GetNode<MenuController>(ConstTerm.MENU + ConstTerm.CONTROLLER);
            
            menuScreen = menuInput.GetNode<InfoMenu>(ConstTerm.MENU + ConstTerm.CONTAINER);
            pauseScreen = uiLayer.GetNode<Container>(ConstTerm.PAUSE + ConstTerm.CONTAINER).GetNode<PauseController>(ConstTerm.PAUSE + ConstTerm.CONTROLLER);

            Node interactLayer = uiLayer.GetNode(ConstTerm.INTERACT_TEXT);
            textBox = interactLayer.GetNode<TextBox>(ConstTerm.TEXTBOX + ConstTerm.CONTAINER);
            choiceBox = interactLayer.GetNode<ChoiceBox>(ConstTerm.CHOICEBOX + ConstTerm.CONTAINER);

            interactArray = playerInput.GetInteractArray();
            
            eventNode = GetNode<Node2D>(ConstTerm.EVENTS);
            mapEvents = SafeScriptAssign(eventNode, eventScript) as MapEventScript;

            if (travelList.Count <= 0) { // Populate transition list
                travelList = [];
                for (int n = 0; n < transit.GetChildCount(); n++)
                { 
                    Transitions nextTravel = transit.GetChild(n) as Transitions;
                    travelList.Add(nextTravel); 
                }
            }
            // GD.Print(mapEvents);

            // StartBGM();
        }

        // private void StartBGM()
        // {
        //     bgmPlayer = bgm.GetNode<AnimationPlayer>(ConstTerm.ANIM_PLAYER);
        //     // bgm.Play();
        //     bgmPlayer.Play(ConstTerm.AUDIO_FADE);
        // }

        private void SubSignals()
        {
            playerInput.onSaveGame += OnSaveGame;
            playerInput.onLoadGame += OnLoadGame;

            playerInput.onInteractCheck += OnInteractCheck;
            playerInput.onSelectChange += OnSelectChange;
            playerInput.onTextProgress += OnTextProgress;
            playerInput.onChoiceSelect += OnChoiceSelect;
            playerInput.onMenuOpen += OnMenuOpen;
            playerInput.onPauseMenu += OnPauseMenu;

            menuInput.onMenuClose += OnMenuClose;
            menuInput.onMenuPhase += OnMenuPhase;
            // menuInput.onTargetChange += OnTargetChange;
            menuInput.onMemberSelect += OnMemberSelect;

            mapEvents.onEventComplete += OnEventComplete;

            for (int a = 0; a < battleAreas.Length; a++) {
                battleAreas[a].onBattleTrigger += OnBattleTrigger; }
        }

        private void UnSubSignals() // Inactive: Mapsystem doesn't run _Ready again after _ExitTree
        {
            playerInput.onSaveGame -= OnSaveGame;
            playerInput.onLoadGame -= OnLoadGame;

            playerInput.onInteractCheck -= OnInteractCheck;
            playerInput.onSelectChange -= OnSelectChange;
            playerInput.onTextProgress -= OnTextProgress;
            playerInput.onChoiceSelect -= OnChoiceSelect;
            playerInput.onMenuOpen -= OnMenuOpen;
            playerInput.onPauseMenu -= OnPauseMenu;

            menuInput.onMenuClose -= OnMenuClose;
            menuInput.onMenuPhase -= OnMenuPhase;
            // menuInput.onTargetChange += OnTargetChange;
            menuInput.onMemberSelect -= OnMemberSelect;

            mapEvents.onEventComplete -= OnEventComplete;

            for (int a = 0; a < battleAreas.Length; a++) {
                battleAreas[a].onBattleTrigger -= OnBattleTrigger; }
        }

        // public override void _EnterTree()
        // {
        //     GD.Print("Map Tree Enter");
        // }

        // public override void _ExitTree()
        // {
        //     battleNode.onBuildPlayerTeam -= BuildParty;
        // }

        // private void SetPlayerToEvents()
        // {
        //     Node2D eventNode = GetNode<Node2D>(ConstTerm.EVENTS);
        //     // Interactable[] eventList = eventNode.GetChildren();
        // }


        //=============================================================================
        // SECTION: Interaction Handling
        //=============================================================================

        private void SpeedyText()
        {
            if (playerInput.GetInputPhase() == ConstTerm.TEXT) { 
                interactTarget.GetTextBox().FasterText(playerInput.TextSpeedCheck()); }
        }

        private void InteractCheck(Vector2 direction)
        {
            // interactRay.ForceRaycastUpdate();
            // if (interactRay.IsColliding() && !textBox.Visible)
            // {
            //     if (interactRay.GetCollider() is Interactable)
            //     {
            //         playerInput.SetIdleAnim();

            //         SetInteractTarget();
            //         interactTarget.TargetInteraction(direction);
            //     }
            // }
            // GD.Print("Interact check");
            for (int r = 0; r < interactArray.Count; r++)
            {
                interactArray[r].ForceRaycastUpdate();
                if (interactArray[r].IsColliding() && !textBox.Visible)
                {
                    if (interactArray[r].GetCollider() is Interactable)
                    {
                        Interactable checkTarget = (Interactable)interactArray[r].GetCollider();
                        if (!checkTarget.CheckValidInteract()) { return; }

                        playerInput.SetIdleAnim();

                        SetInteractTarget(r);
                        interactTarget.TargetInteraction(direction); // -> Interactable
                        // GD.Print(interactTarget);
                        // GD.Print(playerInput.GetInputPhase());
                        return;
                    }
                }
            }
        }

        private void UpdateInteraction()
        {
            bool stepComplete = interactTarget.StepCheck();
            if (stepComplete)
            {
                textBox.HideTextBox();
                OnEventComplete();
            }
            else
            {
                if (interactTarget.IsEvent()) 
                {
                    textBox.HideTextBox();
                    playerInput.SetInputPhase(ConstTerm.DO_NOTHING);
                    mapEvents.Call(interactTarget.Name, interactTarget);
                }
                else { interactTarget.StepInteract(0); }
            }
        }

        private void SelectChoice()
        {
            // playerInput.SetInputPhase(ConstTerm.INTERACT);
            choiceBox.HideChoiceBox();
            interactTarget.StepInteract(1);
            choiceCommand = 0;
        }

        private void SetInteractTarget(int index)
        {
            // GD.Print("Set interact target");
            playerInput.SetInteractToggle(true);
            interactTarget = (Interactable)interactArray[index].GetCollider();
            
            if (interactTarget.IsEventAndInteractStart()) {
                interactTarget.onEventStart += OnEventStart; }
            interactTarget.onInteractPhase += OnInteractPhase;
            interactTarget.onItemReceive += OnItemReceive;
        }

        private void RemoveInteractTarget()
        {
            interactTarget.ResetInteractPhase();
            interactTarget.ResetDirection();
            interactTarget.ResetEvent();

            if (interactTarget.IsEventAndInteractStart()) {
                interactTarget.onEventStart -= OnEventStart;
                interactTarget.GetMoveAgent().onEndEventStep -= mapEvents.OnEndEventStep; 
            }
            interactTarget.onInteractPhase -= OnInteractPhase;
            interactTarget.onItemReceive -= OnItemReceive;

            interactTarget = null;
            playerInput.SetInteractToggle(false);
            playerInput.SetInputPhase(ConstTerm.MOVE);
        }


        //=============================================================================
        // SECTION: Menu Handling
        //=============================================================================


        private void UpdateMenuInfo()
        {
            for (int i = 0; i < menuScreen.GetInfoSize(); i++)
            {
                MemberInfo tempInfo = menuScreen.GetMemberInfo(i);
                // tempInfo.SetCharPortrait(playerParty.GetPlayerParty()[i].GetPortrait());
                tempInfo.SetCharName(playerParty.GetPlayerParty()[i].GetBattlerName());
                tempInfo.SetCharTitle(playerParty.GetPlayerParty()[i].GetBattlerTitle());
                tempInfo.SetHPCurrent(playerParty.GetPlayerParty()[i].GetHealth().GetHP());
                tempInfo.SetHPMax(playerParty.GetPlayerParty()[i].GetHealth().GetMaxHP());
                // tempInfo.SetMPCurrent(playerParty.GetPlayerParty()[i].Get???().GetMP());
                // tempInfo.SetMPMax(playerParty.GetPlayerParty()[i].Get???().GetMaxMP());
                tempInfo.SetCurrentLevel(playerParty.GetPlayerParty()[i].GetExperience().GetCurrentLevel());
                tempInfo.SetNextLevel(playerParty.GetPlayerParty()[i].GetExperience().GetExpToLevel());

                menuScreen.UpdateMemberInfo(i, tempInfo);
            }

            SetupItemList();
        }

        private void UIVisibility()
        {
            CheckHasItems();
            menuScreen.GetSkillPanel().Visible = menuInput.GetInputPhase() == ConstTerm.SKILL + ConstTerm.SELECT;
            menuScreen.GetItemPanel().Visible = menuInput.GetInputPhase() == ConstTerm.ITEM + ConstTerm.SELECT;
            // menuScreen.GetMemberBar().Visible = menuInput.GetInputPhase() == ConstTerm.MEMBER + ConstTerm.SELECT;
        }

        // private void TargetChange(string selectBar)
        // {
        //     int index = menuInput.GetCommand();
        //     ColorRect tempBar = menuScreen.GetSelectBar(selectBar);

        //     tempBar.Position = new Vector2(0, tempBar.Size.Y * index); // EDIT - Temporary!
        // }

        private void SetupSkillList(Battler player)
        {
            foreach (Node child in menuScreen.GetSkillPanel().GetNode(ConstTerm.TEXT + ConstTerm.LIST).GetChildren())
            { menuScreen.GetSkillPanel().GetNode(ConstTerm.TEXT + ConstTerm.LIST).RemoveChild(child); child.QueueFree(); }

            // for (int c = 0; c < menuScreen.GetSkillPanel().GetNode(ConstTerm.SKILL + ConstTerm.LIST).GetChildCount(); c++)
            // {
            //     Node tempChild = menuScreen.GetSkillPanel().GetNode(ConstTerm.SKILL + ConstTerm.LIST).GetChild(c);
            //     menuScreen.GetSkillPanel().GetNode(ConstTerm.SKILL + ConstTerm.LIST).RemoveChild(tempChild);
            //     tempChild.QueueFree();
            // }
            // GD.Print(menuScreen.GetSkillPanel().GetNode(ConstTerm.SKILL + ConstTerm.LIST).GetChildCount());

            // for (int a = 0; a < player.GetSkillCount(); a++)
            // {
            //     Label newSkill = (Label)ResourceLoader.Load<PackedScene>(labelSettings.ResourcePath).Instantiate();
            //     newSkill.Text = player.GetSkillName(a);
            //     newSkill.CustomMinimumSize = new Vector2(menuInput.GetSkillItemWidth(), 0);
            // }

            foreach (Ability skill in player.GetSkillList().GetSkills())
            {
                Label newSkill = (Label)ResourceLoader.Load<PackedScene>(labelSettings.ResourcePath).Instantiate();
                newSkill.Text = skill.AbilityName;
                newSkill.CustomMinimumSize = new Vector2(menuInput.GetSkillItemWidth(), 0);
                if (!player.CheckCanUse(skill)) {
                    newSkill.GetNode<ButtonUI>(ConstTerm.BUTTON).SetQuasiDisabled(true);
                }

                menuScreen.GetSkillPanel().GetNode(ConstTerm.TEXT + ConstTerm.LIST).AddChild(newSkill);
            }
        }

        private void SetupItemList()
        {
            if (!CheckHasItems()) { return; };

            foreach (Node child in menuScreen.GetItemPanel().GetNode(ConstTerm.TEXT + ConstTerm.LIST).GetChildren())
            { child.QueueFree(); }

            foreach (Item item in ItemBag.Instance.GetItemBag())
            {
                Label newItem = (Label)ResourceLoader.Load<PackedScene>(labelSettings.ResourcePath).Instantiate();
                newItem.Text = item.ItemName;
                newItem.CustomMinimumSize = new Vector2(menuInput.GetSkillItemWidth(), 0);

                menuScreen.GetItemPanel().GetNode(ConstTerm.TEXT + ConstTerm.LIST).AddChild(newItem);
            }
        }

        private bool CheckHasItems()
        {
            int itemCommand = 0;
            for (int i = 0; i < menuInput.GetCommandOptions().Length; i++)
            {
                if (menuInput.GetCommandOptions()[i] == ConstTerm.ITEM) { itemCommand = i; break; }
            }

            if (ItemBag.Instance.BagIsEmpty())
            {
                menuScreen.GetCommandList().GetChild<Label>(itemCommand).GetNode<ButtonUI>(ConstTerm.BUTTON).SetQuasiDisabled(true);
                // IUIFunctions.DisableOption(menuScreen.GetCommandList().GetChild<Label>(itemCommand));
                // menuScreen.GetCommandList().GetChild<Label>(itemCommand).Modulate = new Color(ConstTerm.GREY);
                return false;
            }
            else
            {
                menuScreen.GetCommandList().GetChild<Label>(itemCommand).GetNode<ButtonUI>(ConstTerm.BUTTON).SetQuasiDisabled(false);
                // IUIFunctions.EnableOption(menuScreen.GetCommandList().GetChild<Label>(itemCommand));
                // menuScreen.GetCommandList().GetChild<Label>(itemCommand).Modulate = new Color(ConstTerm.WHITE);
                return true;
            }
        }


        //=============================================================================
        // SECTION: Access Methods
        //=============================================================================

        public async void LoadBattle(PackedScene randomGroup)
        {
            // EDIT: this(MapSystem) node not found. 'Disposed'
            // GD.Print("Battle loading - save data");
            SaveLoader.Instance.GatherBattlers();
            // GD.Print("Change player active control");
            playerParty.ChangePlayerActive(false);
            // GD.Print("Instantiate battle scene");
            battleNode = (BattleSystem)battleScene.Instantiate();
            // GD.Print("Set battle group");
            battleNode.SetEnemyGroup(randomGroup);
            // GD.Print("Storing Map scene - ");
            // GD.Print(Name);
            battleNode.StoreMapScene(GetParent<Node2D>());

            // GD.Print("Fade Out");
            await BGMPlayer.Instance.FadeBGMTransition(bgm, battleNode.GetBGM());
            // AudioStream newBgm = battleNode.GetBGM();
            // Fader.Instance.FadeOut();

            // BGMPlayer.Instance.TransitionBGM(bgm, newBgm);
            // await ToSignal(Fader.Instance.GetAnimPlayer(), ConstTerm.ANIM_FINISHED);

            GetTree().Root.AddChild(battleNode);
            GetTree().Root.RemoveChild(GetParent());

            // GD.Print("Battle loading - load data");
            SaveLoader.Instance.LoadBattlerData(SaveLoader.Instance.gameSession);

            Fader.Instance.FadeIn();
            await ToSignal(Fader.Instance.GetAnimPlayer(), ConstTerm.ANIM_FINISHED);

            battleNode.SetBattleControlActive(true);

            // QueueFree();
        }

        public AudioStream GetBGM()
        {
            return bgm;
        }

        public PartyManager GetPartyManager()
        {
            return playerParty;
        }

        public void SetParty(PartyManager party)
        {
            playerParty = party;
            // for (int c = 0; c < party.GetChildCount(); c++)
            // {
            //     playerParty.AddChild(party.GetChild(c));
            // }
        }

        public List<Transitions> GetTransitions()
        {
            return travelList;
        }


        //=============================================================================
        // SECTION: Signal Methods
        //=============================================================================

        private void OnSaveGame()
        {
            SaveLoader.Instance.SaveGame();
        }

        private void OnLoadGame()
        {
            SaveLoader.Instance.LoadGame();
        }

        private void OnEventStart()
        {
            playerInput.SetInputPhase(ConstTerm.DO_NOTHING);
            mapEvents.Call(interactTarget.Name, interactTarget); // -> MapEventScript: Map# - Event# function
            // GD.Print(mapEvents.GetScriptMethodList()[0]["name"].ToString());
            // GD.Print(interactTarget.Name);

            // for (int e = 0; e < mapEvents.GetScriptMethodList().Count; e++)
            // {
            //     if (mapEvents.GetScriptMethodList()[e]["name"].ToString() == interactTarget.Name) {
            //         mapEvents.Call(mapEvents.GetScriptMethodList()[e]["name"].ToString());
            //     }
            // }
        }

        private void OnEventComplete()
        {
            // GD.Print("Event complete");
            interactTarget.TurnOffEvent();
            RemoveInteractTarget();
        }

        private void OnInteractCheck(Vector2 direction)
        {
            // interactRay = ray;
            InteractCheck(direction);
        }

        private void OnInteractPhase(string newPhase)
        {
            playerInput.SetInputPhase(newPhase); // -> CharacterControler
            // GD.Print("MapSystem " + playerInput.GetInputPhase());
        }

        private void OnSelectChange()
        {
            int index = playerInput.GetChoice();
            // interactTarget.GetChoiceBox().MoveCursor(index);
            interactTarget.SetChoiceOption(index);
        }

        private void OnTextProgress()
        {
            if (interactTarget.GetTextBox().IsTextComplete()) {
                UpdateInteraction();
                // else { } // EDIT: Add Event processing
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

        private void OnMenuPhase()
        {
            UIVisibility();
        }

        private void OnMenuOpen()
        {
            UpdateMenuInfo();
            menuScreen.Visible = true;
            playerInput.ChangeActive(false);
            playerInput.SetInputPhase(ConstTerm.WAIT);
            menuInput.MenuOpen();
        }

        private void OnMenuClose()
        {
            menuScreen.Visible = false;
            playerInput.ChangeActive(true);
            playerInput.SetInputPhase(ConstTerm.MOVE);
        }

        private void OnPauseMenu()
        {
            pauseScreen.OpenPauseMenu();
        }

        // private void OnTargetChange()
        // {
        //     ColorRect tempBar = menuScreen.GetSelectBar(menuInput.GetInputPhase());

        //     float index = menuInput.GetCommand();
        //     float numColumns = menuInput.GetNumColumn();
        //     float column = index % numColumns;
        //     float yPos = (float)(tempBar.Size.Y * Math.Ceiling(index / numColumns - column));

        //     tempBar.Position = new Vector2(column * tempBar.Size.X, yPos); // EDIT - Temporary!

        //     // switch (menuInput.GetInputPhase())
        //     // {
        //     //     case ConstTerm.COMMAND:
        //     //         TargetChange(ConstTerm.COMMAND);
        //     //         break;
        //     //     case ConstTerm.MEMBER + ConstTerm.SELECT:
        //     //         TargetChange(ConstTerm.MEMBER);
        //     //         break;
        //     //     case ConstTerm.SKILL + ConstTerm.SELECT:
        //     //         TargetChange(ConstTerm.SKILL);
        //     //         break;
        //     //     default:
        //     //         break;
        //     // }
        // }

        private void OnMemberSelect()
        {
            memberSelected = menuInput.GetMemberCommand();
            if (menuInput.GetInputPhase() == ConstTerm.SKILL + ConstTerm.SELECT) { SetupSkillList(playerParty.GetPlayerParty()[memberSelected]); }
            else if (menuInput.GetInputPhase() == ConstTerm.STATUS_SCREEN) { } // EDIT: Create/Assign status screen

            menuInput.SetNumColumn();
        }

        private void OnCatchPlayer(PackedScene battleGroup)
        {
            LoadBattle(battleGroup);
        }

        private void OnBattleTrigger(PackedScene battleGroup)
        {
            LoadBattle(battleGroup);
        }

        //=============================================================================
        // SECTION: Utility Methods
        //=============================================================================

        private static Node SafeScriptAssign(Node target, Script scriptAssign) // May shift to shared Utilities script
        {
            // This whole section... \\
            ulong charId = target.GetInstanceId();
            target.SetScript(ResourceLoader.Load(scriptAssign.ResourcePath));
            target = (Node)InstanceFromId(charId);

            target._Ready();
            target.SetProcess(true);
            target.SetPhysicsProcess(true);
            target.SetProcessInput(true);
            return target;
            // ... is a blasted mess. \\
        }
    }
}
