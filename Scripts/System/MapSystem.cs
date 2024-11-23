using Godot;
using System;

using ZAM.Control;
using ZAM.Interactions;
using ZAM.Abilities;
using ZAM.Stats;

using ZAM.Inventory;
using ZAM.MenuUI;

using ZAM.Managers;

namespace ZAM.System
{
    public partial class MapSystem : Node
    {
        [ExportGroup("Nodes")]
        [Export] private PackedScene battleScene;
        [Export] private PartyManager playerParty;
        [Export] private MenuController menuInput = null;

        [ExportGroup("Resources")]
        [Export] private PackedScene labelSettings = null;

        private CharacterController playerInput = null;
        private BattleSystem battleNode = null;
        private RayCast2D interactRay = null;
        private Interactable interactTarget;

        private CanvasLayer uiLayer = null;
        private TextBox textBox = null;
        private ChoiceBox choiceBox = null;

        private InfoMenu menuScreen = null;

        private int choiceCommand = 0;
        private int memberSelected = 0;
        private bool hasLoaded = false;

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

            // GD.Print("Map ready - save data");
            SaveLoader.Instance.GatherBattlers(); // Should only happen once, when game loads.

            hasLoaded = true;
        }

        public override void _EnterTree()
        {
            // if (hasLoaded) {
            //     GD.Print("Map enter tree - load data");
            //     SaveLoader.Instance.LoadBattlerData(SaveLoader.Instance.gameSession);
            //     GD.Print(SaveLoader.Instance.gameSession.CharData[playerParty.GetPlayerParty()[0].GetCharID()].CurrentHP);
            //     GD.Print(playerParty.GetPlayerParty()[0].GetHealth().GetHP());
            // }
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

            Node interactLayer = uiLayer.GetNode(ConstTerm.INTERACT_TEXT);
            textBox ??= interactLayer.GetNode<TextBox>(ConstTerm.TEXTBOX + ConstTerm.CONTAINER);
            choiceBox ??= interactLayer.GetNode<ChoiceBox>(ConstTerm.CHOICEBOX + ConstTerm.CONTAINER);

            menuInput ??= uiLayer.GetNode<MenuController>(ConstTerm.MENU_CONTROLLER);
            menuScreen ??= menuInput.GetNode<InfoMenu>(ConstTerm.MENU + ConstTerm.CONTAINER);

            // interactRay ??= playerInput.GetNode<RayCast2D>(ConstTerm.RAYCAST2D);
        }

        private void SubSignals()
        {
            playerInput.onInteractCheck += OnInteractCheck;
            playerInput.onSelectChange += OnSelectChange;
            playerInput.onTextProgress += OnTextProgress;
            playerInput.onChoiceSelect += OnChoiceSelect;
            playerInput.onMenuOpen += OnMenuOpen;

            menuInput.onMenuClose += OnMenuClose;
            menuInput.onMenuPhase += OnMenuPhase;
            menuInput.onTargetChange += OnTargetChange;
            menuInput.onMemberSelect += OnMemberSelect;
        }

        // private void UnSubSignals() // Inactive: Mapsystem doesn't run _Ready again after _ExitTree
        // {
        //     playerInput.onInteractCheck -= OnInteractCheck;
        //     playerInput.onSelectChange -= OnSelectChange;
        //     playerInput.onTextProgress -= OnTextProgress;
        //     playerInput.onChoiceSelect -= OnChoiceSelect;
        //     playerInput.onMenuOpen -= OnMenuOpen;

        //     menuInput.onMenuClose -= OnMenuClose;
        // }

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
            interactRay.ForceRaycastUpdate();
            if (interactRay.IsColliding() && !textBox.Visible)
            {
                playerInput.SetIdleAnim();
                if (interactRay.GetCollider() is Interactable)
                {
                    SetInteractTarget();
                    interactTarget.TargetInteraction(direction);
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
            interactTarget.onInteractPhase += OnInteractPhase;
            interactTarget.onItemReceive += OnItemReceive;
        }

        private void RemoveInteractTarget()
        {
            interactTarget.SetInteractPhase(ConstTerm.WAIT);
            interactTarget.ResetDirection();

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
            menuScreen.GetSkillPanel().Visible = menuInput.GetMenuPhase() == ConstTerm.SKILL + ConstTerm.SELECT;
            menuScreen.GetItemPanel().Visible = menuInput.GetMenuPhase() == ConstTerm.ITEM + ConstTerm.SELECT;
            menuScreen.GetMemberBar().Visible = menuInput.GetMenuPhase() == ConstTerm.MEMBER + ConstTerm.SELECT;
        }

        // private void TargetChange(string selectBar)
        // {
        //     int index = menuInput.GetCommand();
        //     ColorRect tempBar = menuScreen.GetSelectBar(selectBar);

        //     tempBar.Position = new Vector2(0, tempBar.Size.Y * index); // EDIT - Temporary!
        // }

        private void SetupSkillList(Battler player)
        {
            foreach (Node child in menuScreen.GetSkillPanel().GetNode(ConstTerm.SKILL + ConstTerm.LIST).GetChildren())
            { child.QueueFree(); }

            foreach (Ability skill in player.GetSkillList().GetSkills())
            {
                Label newSkill = (Label)ResourceLoader.Load<PackedScene>(labelSettings.ResourcePath).Instantiate();
                newSkill.Text = skill.AbilityName;
                newSkill.CustomMinimumSize = new Vector2(menuInput.GetSkillItemWidth(), 0);
                if (!player.CheckCanUse(skill)) {
                    newSkill.Modulate = new Color(ConstTerm.GREY);
                }

                menuScreen.GetSkillPanel().GetNode(ConstTerm.SKILL + ConstTerm.LIST).AddChild(newSkill);
            }
        }

        private void SetupItemList()
        {
            if (!CheckHasItems()) { return; };

            foreach (Node child in menuScreen.GetItemPanel().GetNode(ConstTerm.ITEM + ConstTerm.LIST).GetChildren())
            { child.QueueFree(); }

            foreach (Item item in ItemBag.Instance.GetItemBag())
            {
                Label newItem = (Label)ResourceLoader.Load<PackedScene>(labelSettings.ResourcePath).Instantiate();
                newItem.Text = item.ItemName;
                newItem.CustomMinimumSize = new Vector2(menuInput.GetSkillItemWidth(), 0);

                menuScreen.GetItemPanel().GetNode(ConstTerm.ITEM + ConstTerm.LIST).AddChild(newItem);
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
                menuScreen.GetCommandList().GetChild<Label>(itemCommand).Modulate = new Color(ConstTerm.GREY);
                return false;
            }
            else
            {
                menuScreen.GetCommandList().GetChild<Label>(itemCommand).Modulate = new Color(ConstTerm.WHITE);
                return true;
            }
        }


        //=============================================================================
        // SECTION: Access Methods
        //=============================================================================

        public async void LoadBattle(PackedScene randomGroup)
        {
            // GD.Print("Battle loading - save data");
            SaveLoader.Instance.GatherBattlers();
            playerParty.ChangePlayerActive(false);
            Fader.Instance.Transition();
            await ToSignal(Fader.Instance, ConstTerm.TRANSITION_FINISHED);

            battleNode = (BattleSystem)battleScene.Instantiate();
            battleNode.SetEnemyGroup(randomGroup);
            battleNode.StoreMapScene(this);
            battleNode.SetBattleControlActive(true);

            GetTree().Root.AddChild(battleNode);
            GetTree().Root.RemoveChild(this);

            // GD.Print("Battle loading - load data");
            SaveLoader.Instance.LoadBattlerData(SaveLoader.Instance.gameSession);

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

        private void OnInteractCheck(RayCast2D ray, Vector2 direction)
        {
            interactRay = ray;
            InteractCheck(direction);
        }

        private void OnInteractPhase(string newPhase)
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

        private void OnTargetChange()
        {
            ColorRect tempBar = menuScreen.GetSelectBar(menuInput.GetMenuPhase());

            float index = menuInput.GetCommand();
            float column = index % menuInput.GetNumColumn();
            float yPos = (float)(tempBar.Size.Y * Math.Ceiling(index / 2 - column));
            

            tempBar.Position = new Vector2(column * tempBar.Size.X, yPos); // EDIT - Temporary!

            // switch (menuInput.GetMenuPhase())
            // {
            //     case ConstTerm.COMMAND:
            //         TargetChange(ConstTerm.COMMAND);
            //         break;
            //     case ConstTerm.MEMBER + ConstTerm.SELECT:
            //         TargetChange(ConstTerm.MEMBER);
            //         break;
            //     case ConstTerm.SKILL + ConstTerm.SELECT:
            //         TargetChange(ConstTerm.SKILL);
            //         break;
            //     default:
            //         break;
            // }
        }

        private void OnMemberSelect()
        {
            memberSelected = menuInput.GetMemberCommand();
            if (menuInput.GetMenuPhase() == ConstTerm.SKILL + ConstTerm.SELECT) { SetupSkillList(playerParty.GetPlayerParty()[memberSelected]); }
            else if (menuInput.GetMenuPhase() == ConstTerm.STATUS_SCREEN) { } // EDIT: Create/Assign status screen

            menuInput.SetNumColumn();
        }
    }
}