using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;

using ZAM.Controller;
using ZAM.Interactions;
using ZAM.Abilities;
using ZAM.Stats;

using ZAM.Inventory;
using ZAM.MenuUI;

using ZAM.Managers;
using ZAM.MapEvents;

namespace ZAM.Core
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
        // [Export] private PackedScene labelSettings = null;
        [Export] private PackedScene menuUseButton = null;
        [Export] private Script eventScript = null;

        private AnimationPlayer bgmPlayer = null;
        private BattleSystem battleNode = null;
        private Node2D eventNode = null;
        private MapEventScript mapEvents = null;

        private CharacterController playerInput = null;
        private Array<RayCast2D> interactArray = [];
        private RayCast2D mouseRay = null;
        private Interactable interactTarget;
        private Callable collideSignal;
        private bool enemyCatch = false;

        private Array<Transitions> travelList = [];
        private Dictionary<Interactable, Callable> chaseList = [];

        private TextBox textBox = null;
        private ChoiceBox choiceBox = null;

        private InfoMenu menuScreen = null;
        private PauseController pauseScreen = null;

        private Ability activeAbility;
        private Item activeItem;
        
        // private Array<Item> itemList = [];
        private Array<Equipment> equipList = [];
        private int currentGearSlot = 0;

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
            GetWindow().MoveToCenter();
            IfNull();
            SubSignals();

            menuInput.SetPlayer(playerInput);
            menuScreen.SetupCommandList(menuInput.GetCommandOptions());
            SetupItemList();
            UpdateMenuInfo();
            SetupUseList();

            // // GD.Print("Map ready - save data");
            // hasLoaded = true;
        }

        public override void _EnterTree()
        {
            enemyCatch = false;
            base._EnterTree();
        }


        protected override void Dispose(bool disposing)
        {
            UnSubSignals();
            base.Dispose(disposing);
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
            pauseScreen = uiLayer.GetNode<PauseController>(ConstTerm.PAUSE + ConstTerm.MENU);

            Node interactLayer = uiLayer.GetNode(ConstTerm.INTERACT_TEXT);
            textBox = interactLayer.GetNode<TextBox>(ConstTerm.TEXTBOX + ConstTerm.CONTAINER);
            choiceBox = interactLayer.GetNode<ChoiceBox>(ConstTerm.CHOICEBOX + ConstTerm.CONTAINER);

            interactArray = playerInput.GetInteractArray();
            
            eventNode = GetNode<Node2D>(ConstTerm.EVENTS);
            mapEvents = SafeScriptAssign(eventNode, eventScript) as MapEventScript;

            if (travelList.Count <= 0) { // Populate transition Array
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
            collideSignal = Callable.From((CharacterBody2D collider) => OnCollisionCheck(collider));
            playerInput.Connect(CharacterController.SignalName.onCollisionCheck, collideSignal);
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
            menuInput.onAbilitySelect += OnAbilitySelect;
            menuInput.onItemSelect += OnItemSelect;

            menuInput.onUseMember += OnUseMember;
            menuInput.onEquipSlot += OnEquipSlot;
            menuInput.onGearEquip += OnGearEquip;
            menuInput.onGearCompare += OnGearCompare;

            pauseScreen.onSaveMenu += OnSaveMenu;
            pauseScreen.onLoadMenu += OnLoadMenu;

            mapEvents.onEventComplete += OnEventComplete;

            for (int a = 0; a < battleAreas.Length; a++)
            {
                battleAreas[a].onBattleTrigger += OnBattleTrigger;
            }

            // chaseList = [];
            // for (int i = 0; i < mapEvents.GetChildCount(); i++)
            // {
            //     Interactable tempActor = (Interactable)mapEvents.GetChild(i);
            //     if (tempActor.ShouldChasePlayer)
            //     {
            //         chaseList[tempActor] = Callable.From((Interactable chaser) => OnCatchPlayer(chaser));
            //         // tempActor.GetMoveAgent().onCatchPlayer += async (toFree) => await OnCatchPlayer(toFree);
            //         tempActor.GetMoveAgent().Connect(NPCMove.SignalName.onCatchPlayer, chaseList[tempActor]);
            //     }
            // }
        }

        private void UnSubSignals()
        {
            // playerInput.Disconnect(CharacterController.SignalName.onCollisionCheck, collideSignal);
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
            menuInput.onAbilitySelect -= OnAbilitySelect;
            menuInput.onItemSelect -= OnItemSelect;

            menuInput.onUseMember -= OnUseMember;
            menuInput.onEquipSlot -= OnEquipSlot;
            menuInput.onGearEquip -= OnGearEquip;
            menuInput.onGearCompare -= OnGearCompare;

            pauseScreen.onSaveMenu -= OnSaveMenu;
            pauseScreen.onLoadMenu -= OnLoadMenu;

            mapEvents.onEventComplete -= OnEventComplete;

            for (int a = 0; a < battleAreas.Length; a++) {
                battleAreas[a].onBattleTrigger -= OnBattleTrigger; }

            // for (int i = 0; i < chaseList.Count; i++) {
            //     chaseList[i].GetMoveAgent().onCatchPlayer -= async (toFree) => await OnCatchPlayer(toFree); }
            // foreach (Interactable chaser in chaseList.Keys)
            // {
            //     if (chaser == null) { continue; }
            //     chaser.GetMoveAgent().Disconnect(NPCMove.SignalName.onCatchPlayer, chaseList[chaser]);
            //     chaseList.Remove(chaser);
            // }
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
                    if (interactArray[r].GetCollider() is Interactable checkTarget)
                    {
                        // Interactable checkTarget = (Interactable)interactArray[r].GetCollider();
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
                if (interactTarget.IsEvent) 
                {
                    textBox.HideTextBox();
                    playerInput.SetInputPhase(ConstTerm.DO_NOTHING);
                    mapEvents.Call(interactTarget.Name, interactTarget);
                }
                else { interactTarget.StepInteract(); }
            }
        }

        private void SelectChoice()
        {
            // playerInput.SetInputPhase(ConstTerm.INTERACT);
            choiceBox.HideChoiceBox();
            interactTarget.UpdateChoice();
            interactTarget.StepInteract();
            choiceCommand = 0;
        }

        private void SetInteractTarget(int index)
        {
            // GD.Print("Set interact target");
            playerInput.SetInteractToggle(true);
            interactTarget = (Interactable)interactArray[index].GetCollider();
            
            if (interactTarget.IsEvent && interactTarget.IsInteractable) {
                interactTarget.onEventStart += OnEventStart; }
            interactTarget.onInteractPhase += OnInteractPhase;
            interactTarget.onItemReceive += OnItemReceive;
        }

        private void RemoveInteractTarget()
        {
            interactTarget.ResetInteractPhase();
            interactTarget.ResetDirection();
            // interactTarget.ResetEvent();

            if (interactTarget.IsEvent && interactTarget.IsInteractable) {
                interactTarget.onEventStart -= OnEventStart;
                interactTarget.GetMoveAgent().onEndEventStep -= mapEvents.OnEndEventStep; 
            }
            interactTarget.onInteractPhase -= OnInteractPhase;
            interactTarget.onItemReceive -= OnItemReceive;

            interactTarget = null;
            playerInput.SetInteractToggle(false);
            playerInput.SetInputPhase(ConstTerm.MOVE);
        }

        public void StopAllChase()
        {
            foreach (Interactable chaser in chaseList.Keys)
            {
                chaser.GetMoveAgent().DisableChaseArea();
                chaser.SetInteractPhase(ConstTerm.DO_NOTHING);
            }
            // for (int i = 0; i < chaseList.Count; i++)
            // {
            //     chaseList[i].GetMoveAgent().DisableChaseArea();
            //     chaseList[i].SetInteractPhase(ConstTerm.DO_NOTHING);
            // }
        }


        //=============================================================================
        // SECTION: Menu Handling
        //=============================================================================

        private void UIVisibility()
        {
            CheckHasItems();
            menuScreen.GetSkillPanel().Visible = menuInput.GetInputPhase() == ConstTerm.SKILL + ConstTerm.SELECT || menuInput.GetInputPhase() == ConstTerm.SKILL + ConstTerm.USE;
            menuScreen.GetItemPanel().Visible = menuInput.GetInputPhase() == ConstTerm.ITEM + ConstTerm.SELECT || menuInput.GetInputPhase() == ConstTerm.ITEM + ConstTerm.USE;
            menuScreen.GetUsePanel().Visible = menuInput.GetInputPhase() == ConstTerm.SKILL + ConstTerm.USE || menuInput.GetInputPhase() == ConstTerm.ITEM + ConstTerm.USE;
            menuScreen.GetEquipPanel().Visible = menuInput.GetInputPhase() == ConstTerm.EQUIP + ConstTerm.SELECT || menuInput.GetInputPhase() == ConstTerm.EQUIP + ConstTerm.USE;
        }

        private void UpdateMenuInfo()
        {
            menuScreen.SetupParty(playerParty.GetPartySize());

            for (int i = 0; i < menuScreen.GetInfoSize(); i++)
            {
                MemberInfo tempInfo = menuScreen.GetMemberInfo(i);
                // tempInfo.SetCharPortrait(playerParty.GetPlayerParty()[i].GetPortrait());
                tempInfo.SetCharName(playerParty.GetPlayerParty()[i].GetBattlerName());
                tempInfo.SetCharTitle(playerParty.GetPlayerParty()[i].GetBattlerTitle());
                tempInfo.SetHPCurrent(playerParty.GetPlayerParty()[i].GetHealth().GetHP());
                tempInfo.SetHPMax(playerParty.GetPlayerParty()[i].GetHealth().GetMaxHP());
                tempInfo.SetMPCurrent(playerParty.GetPlayerParty()[i].GetHealth().GetMP());
                tempInfo.SetMPMax(playerParty.GetPlayerParty()[i].GetHealth().GetMaxMP());
                tempInfo.SetCurrentLevel(playerParty.GetPlayerParty()[i].GetExperience().GetCurrentLevel());
                tempInfo.SetNextLevel(playerParty.GetPlayerParty()[i].GetExperience().GetExpToLevel());

                menuScreen.UpdateMemberInfo(i, tempInfo);
            }

            SetupItemList();
        }

        private void UpdateEquipInfo()
        {
            Battler currBattler = playerParty.GetPlayerParty()[menuInput.GetMemberCommand()];

            menuScreen.GetEquipPanel().SetEquipValues(currBattler.GetEquipList().GetCharEquipment());
            menuScreen.GetEquipPanel().SetStatValues(currBattler.GetStats().GetModifiedStatSheet());

            // menuInput.SubLists(menuScreen.GetEquipPanel().GetEquipList());
        }

        private void UpdateUseList(Battler user)
        {
            // menuInput.SubLists(menuScreen.GetUsePanel().GetNode<Container>(ConstTerm.USE + ConstTerm.LIST));
            
            for (int c = 0; c < menuScreen.GetUsePanel().GetNode(ConstTerm.USE + ConstTerm.LIST).GetChildCount(); c++) {
                if (!user.CheckCanTarget(playerParty.GetPlayerParty()[c], activeAbility, activeItem)) {
                    menuScreen.GetUsePanel().GetNode(ConstTerm.USE + ConstTerm.LIST).GetChild(c).GetChild<ButtonUI>(0).SetQuasiDisabled(true);
                } else {
                    menuScreen.GetUsePanel().GetNode(ConstTerm.USE + ConstTerm.LIST).GetChild(c).GetChild<ButtonUI>(0).SetQuasiDisabled(false);
                }
            }
        }

        private void SetupCharInfo()
        {
            Battler currentChar = playerParty.GetPlayerParty()[menuInput.GetMemberCommand()];
            menuScreen.GetEquipPanel().SetCharInfo(currentChar.GetPortrait(), currentChar.GetNameLabel().Text);
        }

        // private void SetupEquipList()
        // {
        //     foreach(Node child in menuScreen.GetEquipPanel().GetNode(ConstTerm.TEXT + ConstTerm.LIST).GetChildren())
        //     { child.QueueFree(); }

        //     foreach (Equipment gear in ItemBag.Instance.GetEquipBag())
        //     {
        //         if (gear.GearSlot[0] != menuScreen.GetEquipPanel().GetSlotID(menuInput.GetCommand())) { continue; } // EDIT: Update to check multiple slot types.

        //         Label newGear = (Label)ResourceLoader.Load<PackedScene>(menuScreen.GetButtonLabel().ResourcePath).Instantiate();
        //         newGear.Text = gear.ItemName;
        //         newGear.CustomMinimumSize = new Vector2(menuInput.GetSkillItemWidth(), 0);

        //         menuScreen.GetEquipPanel().GetNode(ConstTerm.TEXT + ConstTerm.LIST).AddChild(newGear);
        //     }
        // }

        // private void TargetChange(string selectBar)
        // {
        //     int index = menuInput.GetCommand();
        //     ColorRect tempBar = menuScreen.GetSelectBar(selectBar);

        //     tempBar.Position = new Vector2(0, tempBar.Size.Y * index); // EDIT - Temporary!
        // }

        private void SetupSkillList(Battler player)
        {
            // menuInput.UnSubLists(menuScreen.GetSkillPanel().GetNode<GridContainer>(ConstTerm.TEXT + ConstTerm.LIST));

            foreach (Node child in menuScreen.GetSkillPanel().GetNode(ConstTerm.TEXT + ConstTerm.LIST).GetChildren())
            { child.Free(); }

            foreach (Ability skill in player.GetSkillList().GetSkills())
            {
                Label newSkill = (Label)ResourceLoader.Load<PackedScene>(menuScreen.GetButtonLabel().ResourcePath).Instantiate();
                newSkill.Text = skill.AbilityName;
                newSkill.CustomMinimumSize = new Vector2(menuInput.GetSkillItemWidth(), 0);
                if (!player.CheckCanUse(skill, false))
                { // false = notInBattle
                    newSkill.GetNode<ButtonUI>(ConstTerm.BUTTON).SetQuasiDisabled(true);
                }

                menuScreen.GetSkillPanel().GetNode(ConstTerm.TEXT + ConstTerm.LIST).AddChild(newSkill);
            }

            menuInput.SubLists(menuScreen.GetSkillPanel().GetNode<Container>(ConstTerm.TEXT + ConstTerm.LIST));
        }

        private void SetupItemList()
        {
            if (!CheckHasItems()) { return; };

            ClearItemWindow();
            BuildItemList();
            BuildEquipList();
        }

        private void ClearItemWindow()
        {
            // menuInput.UnSubLists(menuScreen.GetItemPanel().GetNode<GridContainer>(ConstTerm.TEXT + ConstTerm.LIST));

            foreach (Node child in menuScreen.GetItemPanel().GetNode(ConstTerm.TEXT + ConstTerm.LIST).GetChildren())
            { child.Free(); }
        }

        private void BuildItemList()
        {
            // itemList = []; // EDIT: Useful for anything?

            foreach (Item item in ItemBag.Instance.GetItemBag().Keys)
            {
                // if (!item.UseableOutOfBattle) { continue; }
                // itemList.Add(item);

                Label newItem = (Label)ResourceLoader.Load<PackedScene>(menuScreen.GetEquipLabel().ResourcePath).Instantiate();
                newItem.Text = item.ItemName;
                newItem.SetMeta(ConstTerm.UNIQUE + ConstTerm.ID, item.UniqueID);

                newItem.CustomMinimumSize = new Vector2(menuInput.GetSkillItemWidth(), 0);
                if (ItemBag.Instance.GetItemBag()[item] > 1)
                {
                    newItem.GetNode<Label>(ConstTerm.COUNT).Text = "x" + ItemBag.Instance.GetItemBag()[item];
                    newItem.GetNode<Label>(ConstTerm.COUNT).Visible = true;
                }
                if (!item.UseableOutOfBattle)
                {
                    newItem.GetNode<ButtonUI>(ConstTerm.BUTTON).SetQuasiDisabled(true);
                }

                menuScreen.GetItemPanel().GetNode(ConstTerm.TEXT + ConstTerm.LIST).AddChild(newItem);
            }
        }

        private void RefreshEquipList() // EDIT: Find newly equip/changed gear, rather than cycling full list.
        {
            for (int e = 0; e < ItemBag.Instance.GetEquipBag().Count; e++) {
                menuScreen.GetItemPanel().GetNode(ConstTerm.TEXT + ConstTerm.LIST).GetChild(e).GetNode<Label>(ConstTerm.EQUIPPED).Visible = ItemBag.Instance.GetEquipBag()[e].IsEquipped;
            }
        }

        private void BuildEquipList()
        {
            foreach (Equipment equip in ItemBag.Instance.GetEquipBag())
            {
                // itemList.Add(equip);

                Label newEquip = (Label)ResourceLoader.Load<PackedScene>(menuScreen.GetEquipLabel().ResourcePath).Instantiate();
                newEquip.Text = equip.ItemName;
                newEquip.SetMeta(ConstTerm.UNIQUE + ConstTerm.ID, equip.UniqueID);

                newEquip.CustomMinimumSize = new Vector2(menuInput.GetSkillItemWidth(), 0);
                if (!equip.UseableOutOfBattle)
                {
                    newEquip.GetNode<ButtonUI>(ConstTerm.BUTTON).SetQuasiDisabled(true);
                }
                newEquip.GetNode<Label>(ConstTerm.EQUIPPED).Visible = equip.IsEquipped;

                menuScreen.GetItemPanel().GetNode(ConstTerm.TEXT + ConstTerm.LIST).AddChild(newEquip);
            }

            menuInput.SubLists(menuScreen.GetItemPanel().GetNode<Container>(ConstTerm.TEXT + ConstTerm.LIST));
        }

        private void SetupUseList()
        {
            // menuInput.UnSubLists(menuScreen.GetUsePanel().GetNode<VBoxContainer>(ConstTerm.USE + ConstTerm.LIST));

            foreach (Node child in menuScreen.GetUsePanel().GetNode(ConstTerm.USE + ConstTerm.LIST).GetChildren())
            { child.Free(); }

            foreach (Battler member in playerParty.GetPlayerParty())
            {
                PanelContainer newMember = (PanelContainer)ResourceLoader.Load<PackedScene>(menuUseButton.ResourcePath).Instantiate();
                newMember.GetNode<HBoxContainer>(ConstTerm.STATUS + ConstTerm.LIST).GetNode<Label>(ConstTerm.NAME).Text = member.GetBattlerName();
                newMember.GetNode<HBoxContainer>(ConstTerm.STATUS + ConstTerm.LIST).GetNode<HealthDisplay>(ConstTerm.HEALTH_DISPLAY).SetBattler(member);
                newMember.GetNode<HBoxContainer>(ConstTerm.STATUS + ConstTerm.LIST).GetNode<HealthDisplay>(ConstTerm.RESOURCE_DISPLAY).SetBattler(member);
                // newMember.CustomMinimumSize = new Vector2(menuInput.GetSkillItemWidth(), 0);

                menuScreen.GetUsePanel().GetNode(ConstTerm.USE + ConstTerm.LIST).AddChild(newMember);
            }
            // menuInput.SubLists(menuScreen.GetUsePanel().GetNode<Container>(ConstTerm.USE + ConstTerm.LIST));
        }

        private void SetupGearList(int slot)
        {
            if (slot < 0) { // CancelCycle
                // menuInput.UnSubLists(menuScreen.GetEquipPanel().GetGearList());
                menuScreen.GetEquipPanel().ClearChangeValues();
                menuScreen.GetEquipPanel().GetGearList().Visible = false;
                RefreshEquipList();
                return;
            }

            foreach (Node child in menuScreen.GetEquipPanel().GetGearList().GetChildren())
            { child.Free(); }
            equipList = [];
            menuScreen.GetEquipPanel().GetGearList().Visible = true;

            currentGearSlot = slot + 1;

            Battler currentBattler = playerParty.GetPlayerParty()[menuInput.GetMemberCommand()];
            Array<Equipment> tempEquip = ItemBag.Instance.GetSlotContents(menuScreen.GetEquipPanel().GetSlotID(slot), currentBattler.GetCharClass());

            Label removeEquip = (Label)ResourceLoader.Load<PackedScene>(menuScreen.GetEquipLabel().ResourcePath).Instantiate();
            removeEquip.Text = "-Remove-";
            removeEquip.CustomMinimumSize = new Vector2(menuInput.GetSkillItemWidth(), 0);

            menuScreen.GetEquipPanel().GetGearList().AddChild(removeEquip);
            equipList.Add(null);

            foreach (Equipment equip in tempEquip)
            {
                equipList.Add(equip);

                Label newEquip = (Label)ResourceLoader.Load<PackedScene>(menuScreen.GetEquipLabel().ResourcePath).Instantiate();
                newEquip.Text = equip.ItemName;
                newEquip.CustomMinimumSize = new Vector2(menuInput.GetSkillItemWidth(), 0);
                newEquip.GetNode<Label>(ConstTerm.EQUIPPED).Visible = equip.IsEquipped;
                if (equip.IsEquipped) { newEquip.GetNode<ButtonUI>(ConstTerm.BUTTON).SetQuasiDisabled(true); }

                if (equip == currentBattler.GetEquipList().GetCharEquipment()[(GearSlotID)currentGearSlot])
                { newEquip.GetNode<ButtonUI>(ConstTerm.BUTTON).GetParent().Set(CanvasItem.PropertyName.Modulate, new Color(ConstTerm.GREEN)); }

                menuScreen.GetEquipPanel().GetGearList().AddChild(newEquip);
            }

            ShowCompareValues(0);

            menuInput.SubLists(menuScreen.GetEquipPanel().GetGearList());
        }

        private void ShowCompareValues(int currentSlot)
        {
            Battler currentBattler = playerParty.GetPlayerParty()[memberSelected];
            Equipment oldEquip = currentBattler.GetEquipList().GetCharEquipment()[(GearSlotID)currentGearSlot];
            Equipment newEquip = equipList[currentSlot];

            Array<float> changes = [];
            float newAdd, oldAdd;

            for (int s = 0; s < Enum.GetValues(typeof(StatID)).Length - 1; s++) {
                newAdd = 0;
                oldAdd = 0;
                if (newEquip != null) { newAdd = newEquip.GetStatModifiers()[s + 1]; }
                if (oldEquip != null) { oldAdd = oldEquip.GetStatModifiers()[s + 1]; }

                changes.Add(newAdd - oldAdd);
            }

            menuScreen.GetEquipPanel().ShowChangeValues(changes);
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

        private void CheckRemoveItemFromList()
        {
            ItemBag.Instance.RemoveItemFromBag(activeItem);
            if (!ItemBag.Instance.GetItemBag().TryGetValue(activeItem, out var count)) { 
                menuScreen.GetItemPanel().GetNode(ConstTerm.TEXT + ConstTerm.LIST).GetChild(menuInput.GetPrevCommand()).QueueFree();
            }else {
                menuScreen.GetItemPanel().GetNode(ConstTerm.TEXT + ConstTerm.LIST).GetChild(menuInput.GetPrevCommand()).GetNode<Label>(ConstTerm.COUNT).Text = 
                "x" + ItemBag.Instance.GetItemBag()[activeItem];
            }
        }


        //=============================================================================
        // SECTION: Access Methods
        //=============================================================================

        public async Task LoadBattle(PackedScene randomGroup)
        {
            // EDIT: this(MapSystem) node not found. 'Disposed'
            // GD.Print("Battle loading - save data");
            SaveLoader.Instance.GatherBattlers(false);
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
            await SaveLoader.Instance.LoadBattlerData(false);
            battleNode.InitialHealthBars();

            Fader.Instance.FadeIn();
            await ToSignal(Fader.Instance.GetAnimPlayer(), ConstTerm.ANIM_FINISHED);

            battleNode.SetBattleControlActive(true);

            return;
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

        public Array<Transitions> GetTransitions()
        {
            return travelList;
        }

        //=============================================================================
        // SECTION: Battler Handling
        //=============================================================================

        private void UseHeal(Battler user, Battler target)
        {
            if (activeAbility != null) {
                if (activeAbility.TargetArea == ConstTerm.GROUP) {
                    for (int c = 0; c < playerParty.GetPlayerParty().Count; c++) {
                        if (!playerParty.GetPlayerParty()[c].GetHealth().IsHPFull()) {
                            playerParty.GetPlayerParty()[c].GetHealth().ChangeHP(activeAbility.NumericValue);
                        }
                    }
                    user.GetHealth().ChangeMP(-activeAbility.CostValue);
                } else {
                    if (!target.GetHealth().IsHPFull()) {
                        target.GetHealth().ChangeHP(activeAbility.NumericValue);
                        user.GetHealth().ChangeMP(-activeAbility.CostValue);
                    }
                }
            }
            
            if (activeItem != null) {
                if (activeItem.TargetArea == ConstTerm.GROUP) {
                    for (int c = 0; c < playerParty.GetPlayerParty().Count; c++) {
                        if (!playerParty.GetPlayerParty()[c].GetHealth().IsHPFull()) {
                            playerParty.GetPlayerParty()[c].GetHealth().ChangeHP(activeAbility.NumericValue);
                        }
                    }
                    if (activeItem.IsConsumable) { ItemBag.Instance.RemoveItemFromBag(activeItem); }
                } else {
                    if (!target.GetHealth().IsHPFull()) {
                        target.GetHealth().ChangeHP(activeItem.NumericValue);
                        if (activeItem.IsConsumable) { 
                            CheckRemoveItemFromList();
                        }
                    }
                }
            }

            UpdateUseList(user);
        }

        private async void CatchPlayer(Interactable chaser)
        {
            if (enemyCatch) { return; }
            enemyCatch = true;
            chaser.SetInteractPhase(ConstTerm.DO_NOTHING); // EDIT: Does nothing?

            while (menuScreen.Visible) { await Task.Delay(60); } // If player is in the menu, wait before starting a battle.
            await LoadBattle(chaser.GetBattleGroup());

            // if (chaser.ShouldChasePlayer)
            // {
            //     // chaser.GetMoveAgent().onCatchPlayer -= async (chaser) => await OnCatchPlayer(chaser);
            //     chaser.GetMoveAgent().Disconnect(NPCMove.SignalName.onCatchPlayer, chaseList[chaser]);
            //     chaseList.Remove(chaser);
            // }

            chaser.QueueFree();
        }


        //=============================================================================
        // SECTION: Signal Methods
        //=============================================================================

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
            if (!interactTarget.IsRepeatable) { interactTarget.TurnOffEvent(); }
            RemoveInteractTarget();
        }

        private void OnCollisionCheck(CharacterBody2D collider)
        {
            if (enemyCatch) { return; }

            if (collider is Interactable)
            {
                Interactable enemyBattle = collider as Interactable;
                if (enemyBattle.IsAutoBattle) { CatchPlayer(enemyBattle); }
            }
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

        private void OnItemReceive(string newItem, int newType, int count)
        {
            ItemBag.Instance.AddToBag(newItem, (ItemType)newType, count);
        }

        private void OnMenuPhase()
        {
            UIVisibility();
            // if (menuInput.GetInputPhase() == ConstTerm.SKILL + ConstTerm.USE) { UpdateUseList(); }
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

        private void OnSaveMenu()
        {
            GD.Print("Saving game!");
            SaveLoader.Instance.SaveGame();
            pauseScreen.ClosePauseMenu();
        }

        private void OnLoadMenu()
        {
            playerInput.ChangeActive(false);

            GD.Print("Loading game!");
            Transitions moveToScene = new();
            moveToScene.LoadSavedScene(GetTree(), GetParent());

            if (pauseScreen.IsControlActive()) { pauseScreen.ClosePauseMenu(); }
        }

        private void OnPauseMenu()
        {
            pauseScreen.OpenPauseMenu(true);
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
            else if (menuInput.GetInputPhase() == ConstTerm.EQUIP + ConstTerm.SELECT)
            {
                SetupCharInfo();
                UpdateEquipInfo();

                menuScreen.GetEquipPanel().SetMaxHPMP(playerParty.GetPlayerParty()[memberSelected].GetHealth().GetMaxHP(),
                playerParty.GetPlayerParty()[memberSelected].GetHealth().GetMaxMP());
            }
            // else if (menuInput.GetInputPhase() == ConstTerm.STATUS_SCREEN) { } // EDIT: Create/Assign status screen

            // menuInput.SetNumColumn();
        }

        private void OnAbilitySelect(int index)
        {
            activeAbility = playerParty.GetPlayerParty()[menuInput.GetMemberCommand()].GetSkillList().GetSkills()[index];
            activeItem = null;

            UpdateUseList(playerParty.GetPlayerParty()[0]);
        }

        private void OnItemSelect(int index)
        {
            ulong id = (ulong)menuScreen.GetItemPanel().GetNode(ConstTerm.TEXT + ConstTerm.LIST).GetChild(index).GetMeta(ConstTerm.UNIQUE + ConstTerm.ID);
            activeItem = ItemBag.Instance.GetItemFromBag(id);
            activeAbility = null;

            UpdateUseList(playerParty.GetPlayerParty()[0]);
        }

        private void OnUseMember(int user, int target)
        {
            Battler memberUser = playerParty.GetPlayerParty()[user];
            Battler memberTarget = playerParty.GetPlayerParty()[target];

            string damageType = "";
            if (activeAbility != null) { damageType = activeAbility.DamageType; }
            if (activeItem != null) { damageType = activeItem.DamageType;}
            UpdateUseList(memberTarget);

            switch (damageType)
            {
                case "Healing":
                    UseHeal(memberUser, memberTarget);
                    break;
                default:
                    GD.PushError("ActiveAbility/Item not properly assigned!");
                    break;
            }
        }

        private void OnEquipSlot(int slot)
        {
            SetupGearList(slot);
        }

        private void OnGearEquip(int gearSelect)
        {
            Battler currentBattler = playerParty.GetPlayerParty()[memberSelected];

            if (gearSelect == 0) { currentBattler.GetEquipList().RemoveGear(currentGearSlot); }
            else { currentBattler.GetEquipList().EquipGear(currentGearSlot, equipList[gearSelect]); }

            menuScreen.GetEquipPanel().SetEquipSlot(equipList[gearSelect], currentGearSlot);
            menuScreen.GetEquipPanel().SetStatValues(currentBattler.GetStats().GetModifiedStatSheet());
        }

        private void OnGearCompare()
        {
            ShowCompareValues(menuInput.GetCommand());
        }

        private async void OnBattleTrigger(PackedScene battleGroup)
        {
            await LoadBattle(battleGroup);
        }

        //=============================================================================
        // SECTION: Save System
        //=============================================================================

        // public void OnSaveGame(SavedGame saveData)
        // {
        //     if (saveData.SystemData == null) {
        //         SystemData newData = new() {
        //             SavedSceneName = mapId
        //         };

        //         saveData.SystemData = newData;
        //     } else {
        //         saveData.SystemData.SavedSceneName = mapId;
        //     }
        // }

        // public void OnLoadGame(SystemData loadData)
        // {
        //     SystemData saveData = loadData;
        //     if (saveData == null) { GD.Print("System Data - NULL"); return; }

        //     saveData.LoadCurrentMapID = mapId;

        //     // Transitions newTransition = new();
        //     // newTransition.MapSceneSwitch(ConstTerm.MAP_SCENE + saveData.SceneName + ConstTerm.TSCN, (Node2D)GetParent());
        // }

        public void OnSaveFile(ConfigFile saveData)
        {
            saveData.SetValue(ConstTerm.SYSTEM + ConstTerm.DATA, ConstTerm.SAVED + ConstTerm.MAP + ConstTerm.ID, (int)mapId);
        }

        public void OnLoadFile(ConfigFile loadData)
        {
            // Saves opening mapID for map specific info to load from. New map info loads from Transition method.
            loadData.SetValue(ConstTerm.SYSTEM + ConstTerm.DATA, ConstTerm.LOADING + ConstTerm.MAP + ConstTerm.ID, (int)mapId);
            // if (loadData == null) { return; }

            // if (loadData.HasSection(ConstTerm.SYSTEM + ConstTerm.DATA)) {
            //     if (loadData.HasSectionKey(ConstTerm.SYSTEM + ConstTerm.DATA, ConstTerm.))
            // }
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
