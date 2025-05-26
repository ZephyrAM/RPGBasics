using Godot;
using Godot.Collections;
using System;
// using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ZAM.Controller;
using ZAM.Inventory;

using ZAM.MenuUI;

namespace ZAM.Core
{
    public partial class StartScreen : Node, IUIFunctions
    {
        [Export] private PackedScene newGame = null;
        [Export] private AudioStream bgm = null;

        [Export] private VBoxContainer commandList = null;
        [Export] private ConfigController configInput = null;

        // private ScrollContainer commandScroll = null;

        private Node2D newGameScene = null;
        private ButtonUI mouseFocus = null;
        private Container activeList = null;
        private ButtonUI activeControl = null;

        private string inputPhase = ConstTerm.WAIT;
        private Array<string> previousPhase = [];
        private int numColumn = 1;
        private int currentCommand = 0;
        private Array<int> previousCommand = [];

        private bool signalsDone = false;
        private bool savesExist = false;
        private bool controlActive = true;
        private string activeInput = ConstTerm.KEY_GAMEPAD;


        //=============================================================================
        // SECTION: Base Methods
        //=============================================================================

        public override void _Ready()
        {
            TranslationServer.SetLocale("EN");
            SaveLoader.Instance.LoadConfig();

            IfNull();
            CheckSaves();
            // selectBar.GetNode<AnimationPlayer>(ConstTerm.ANIM_PLAYER).Play(ConstTerm.CURSOR_BLINK);
            StartBGM();
            Startup();
            // commandScroll.ScrollVertical = 0;
            // SetResolutionInfo();
        }

        private void IfNull()
        {
            newGame ??= ResourceLoader.Load<PackedScene>(ConstTerm.NEWGAME_SCENE);
            commandList ??= GetNode<CanvasLayer>(ConstTerm.CANVAS_LAYER).GetNode<PanelContainer>(ConstTerm.PANEL + ConstTerm.CONTAINER).GetNode<VBoxContainer>(ConstTerm.VBOX_CONTAINER);
            // commandScroll = commandList.GetParent<ScrollContainer>();

            // BGMPlayer.Instance.CallDeferred(BGMPlayer.MethodName.FadeInBGM, bgm);
            // StartBGM();
        }

        private void StartBGM()
        {
            BGMPlayer.Instance.FadeInBGM(bgm);
        }

        private void SubSignals()
        {
            if (!signalsDone) {
                SubLists(commandList);
                configInput.onCloseConfigOptions += OnCloseConfigOptions;

                signalsDone = true;
            }
        }

        private void SubLists(Container targetList)
        {
            for (int c = 0; c < targetList.GetChildCount(); c++)
            {
                Node tempLabel = targetList.GetChild(c);
                targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).MouseEntered += () => OnMouseEntered(targetList, tempLabel);
                targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).Pressed += OnMouseClick;
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            SubSignals();
        }

        public override void _Input(InputEvent @event)
        {
            if (!signalsDone) { return; }
            PhaseCheck(@event);
        }

        //=============================================================================
        // SECTION: Setup
        //=============================================================================

        private void Startup()
        {
            controlActive = true;
            activeList = commandList;
            CommandSelect(0, ConstTerm.VERT);
            inputPhase = ConstTerm.COMMAND;

            // configPanel.SetupConfigValues();
        }

        private void CheckSaves()
        {
            // string[] dirTest = DirAccess.GetFilesAt(SaveLoader.Instance.GetSavePath()); // EDIT: Separate file names array needed?
            var dir = DirAccess.Open(SaveLoader.Instance.GetSavePath());

            int fileCount = dir.GetFiles().Length;
            if (dir.GetFiles().Contains(ConstTerm.CFG_FILE)) { fileCount--; }

            if (fileCount > 0) { savesExist = true; }
            else {
                commandList.GetNode<Label>(ConstTerm.CONTINUE).GetNode<ButtonUI>(ConstTerm.BUTTON).SetQuasiDisabled(true);
                // commandList.GetNode<Label>(ConstTerm.CONTINUE).Modulate = new Color(ConstTerm.GREY);
                // commandList.GetNode<Label>(ConstTerm.CONTINUE).GetNode<ButtonUI>(ConstTerm.BUTTON).Disabled = true; 
            }
        }

        //=============================================================================
        // SECTION: Phase Handling - Input
        //=============================================================================

        private void PhaseCheck(InputEvent @event)
        {
            if (@event is InputEventMouse && activeInput == ConstTerm.KEY_GAMEPAD) { activeInput = ConstTerm.MOUSE;
                Input.MouseMode = Input.MouseModeEnum.Visible; if (mouseFocus != null) { mouseFocus.MouseFilter = Control.MouseFilterEnum.Stop; } }
            else if (@event is not InputEventMouse && activeInput == ConstTerm.MOUSE) { activeInput = ConstTerm.KEY_GAMEPAD;
                Input.MouseMode = Input.MouseModeEnum.Hidden; if (mouseFocus != null) { mouseFocus.MouseFilter = Control.MouseFilterEnum.Ignore; } }

            if (!controlActive) { return; }
            switch (inputPhase)
            {
                case ConstTerm.COMMAND:
                    CommandPhase(@event);
                    break;
                default:
                    break;
            }

            if (@event is InputEventMouseButton) { 
                if (@event.IsActionPressed(ConstTerm.CANCEL + ConstTerm.CLICK)) {
                    CancelCycle(); } }
        }

        private bool AcceptInput()
        {
            activeControl = IUIFunctions.FocusOff(activeList, currentCommand);
            if (activeControl.OnButtonPressed()) { IUIFunctions.InvalidOption(activeList, currentCommand, ref activeControl, out mouseFocus); return false; }
            // IUIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Ignore, out mouseFocus);
            return true;
        }

        private void CommandPhase(InputEvent @event)
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT)) {
                CommandAccept();
            }
            else { PhaseControls(@event); }
        }

        private void CommandAccept()
        {
            if (!AcceptInput()) { return; }
            IUIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Ignore, out mouseFocus);
            SelectOption();
        }

        private void PhaseControls(InputEvent @event)
        {
            if (@event.IsActionPressed(ConstTerm.CANCEL)) {
                CancelCycle();
            }
            else if (@event.IsActionPressed(ConstTerm.UP)) {
                CommandSelect(-1, ConstTerm.VERT);
            }
            else if (@event.IsActionPressed(ConstTerm.DOWN)) {
                CommandSelect(1, ConstTerm.VERT);
            }
        }

        //=============================================================================
        // SECTION: Selection Handling
        //=============================================================================

        private void CommandSelect(int change, string direction)
        {
            change = IUIFunctions.CheckColumn(change, direction, numColumn);
            IUIFunctions.ChangeTarget(change, ref currentCommand, IUIFunctions.GetCommandCount(activeList));

            activeControl = IUIFunctions.FocusOn(activeList, currentCommand);
        }

        public void SetNewCommand()
        {
            previousCommand.Add(currentCommand);
            currentCommand = 0;

            activeControl = IUIFunctions.FocusOn(activeList, currentCommand);
        }

        private void CancelCycle()
        {
            if (inputPhase == ConstTerm.COMMAND) { return; }
            // else { SaveLoader.Instance.SaveConfig(); }
            
            activeControl = IUIFunctions.FocusOff(activeList, currentCommand);
            IUIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Ignore, out mouseFocus);

            string oldPhase = IUIFunctions.CancelSelect(out currentCommand, previousCommand, previousPhase);
            inputPhase = oldPhase;

            activeControl = IUIFunctions.FocusOn(activeList, currentCommand);
            IUIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Stop, out mouseFocus);
        }

        public async void SelectOption()
        {
            if (activeList.GetChild<Label>(currentCommand).GetNode<ButtonUI>(ConstTerm.BUTTON).GetIsDisabled()) { return; }

            switch (currentCommand)
            {
                case 0:
                    await StartNewGame();
                    break;
                case 1:
                    LoadSaveGame();
                    break;
                case 2:
                    OpenConfigOptions();
                    break;
                case 3:
                    GetTree().Quit();
                    break;
                default:
                    break;
            }
        }

        //=============================================================================
        // SECTION: Command Handling
        //=============================================================================

        private async Task StartNewGame()
        {
            newGameScene = (Node2D)newGame.Instantiate();
            controlActive = false;

            await BGMPlayer.Instance.FadeBGMTransition(bgm, newGameScene.GetNode<MapSystem>(ConstTerm.MAPSYSTEM).GetBGM());

            GetTree().Root.AddChild(newGameScene);
            LoadStarterGear();
            Fader.Instance.FadeIn();
            QueueFree();
            await SaveLoader.Instance.LoadAllData(false); // Saving initial map ID - all other loads should skip, as sections don't exist yet.
            SaveLoader.Instance.LoadConfig();

            // newGameScene.GetNode<MapSystem>(ConstTerm.MAPSYSTEM).GetPartyManager().GetPlayer().ChangeActive(true);

            return;
        }

        private void LoadStarterGear() // EDIT: Temporary!
        {
            ItemBag.Instance.AddToBag("Sword", ItemType.Weapon, 1);
            ItemBag.Instance.AddToBag("Helmet", ItemType.Armor, 1);
            ItemBag.Instance.AddToBag("Breastplate", ItemType.Armor, 1);
            ItemBag.Instance.AddToBag("Ring", ItemType.Accessory, 1);
            ItemBag.Instance.AddToBag("Ring", ItemType.Accessory, 1);
        }

        private void LoadSaveGame()
        {
            Transitions moveToScene = new();
            moveToScene.LoadSavedScene(GetTree(), this);

            controlActive = false;
        }

        private void OpenConfigOptions()
        {
            inputPhase = ConstTerm.WAIT;
            controlActive = false;
            configInput.OpenConfigOptions();
            // configOptions.Visible = true;

            // previousPhase.Add(inputPhase);
            // inputPhase = ConstTerm.OPTIONS;

            // activeList = configOptionsList;
            // SetNewCommand();
        }

        //=============================================================================
        // SECTION: Signal Methods
        //=============================================================================

        private void OnCloseConfigOptions()
        {
            Startup();
            activeControl = IUIFunctions.FocusOn(activeList, currentCommand);
            IUIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Stop, out mouseFocus);
        }

        private void OnMouseEntered(Container currList, Node currLabel)
        {
            if (currList != activeList) { return; }

            activeControl = IUIFunctions.FocusOff(currList, currentCommand);
            currentCommand = currLabel.GetIndex();

            activeControl = IUIFunctions.FocusOn(currList, currentCommand);
            mouseFocus = currLabel.GetNode<ButtonUI>(ConstTerm.BUTTON);
        }

        private void OnMouseClick()
        {
            switch (inputPhase)
            {
                case ConstTerm.COMMAND:
                    CommandAccept();
                    break;
                default:
                    break;
            }
        }
    }
}