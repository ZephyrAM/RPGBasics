using Godot;
using Godot.Collections;

using ZAM.MenuUI;

namespace ZAM.Controller
{
    public partial class StartController : BaseController
    {
        [Export] private PackedScene newGame = null;
        [Export] private AudioStream bgm = null;

        [Export] private VBoxContainer commandList = null;
        [Export] private ConfigController configInput = null;

        private Node2D newGameScene = null;
        private bool savesExist = false;


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

        protected override void IfNull()
        {
            newGame ??= ResourceLoader.Load<PackedScene>(ConstTerm.NEWGAME_SCENE);
            commandList ??= GetNode<CanvasLayer>(ConstTerm.CANVAS_LAYER).GetNode<PanelContainer>(ConstTerm.PANEL + ConstTerm.CONTAINER).GetNode<VBoxContainer>(ConstTerm.VBOX_CONTAINER);
            // commandScroll = commandList.GetParent<ScrollContainer>();

            // BGMPlayer.Instance.CallDeferred(BGMPlayer.MethodName.FadeInBGM, bgm);
            // StartBGM();
        }

        protected override void SetupListDict()
        {
            listDict.Add(ConstTerm.COMMAND, commandList);
        }


        private void StartBGM()
        {
            BGMPlayer.Instance.FadeInBGM(bgm);
        }

        private void SubSignals()
        {
            if (!signalsDone)
            {
                SubLists(commandList);
                configInput.onCloseConfigOptions += OnCloseConfigOptions;

                signalsDone = true;
            }
        }

        protected override void UnSubSignals()
        {
            UnSubLists(commandList);
            configInput.onCloseConfigOptions -= OnCloseConfigOptions;
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
            // if (dir.GetFiles().Contains(ConstTerm.CFG_FILE)) { fileCount--; }

            if (fileCount > 0) { savesExist = true; }
            else
            {
                commandList.GetNode<Label>(ConstTerm.CONTINUE).GetNode<ButtonUI>(ConstTerm.BUTTON).SetQuasiDisabled(true);
                // commandList.GetNode<Label>(ConstTerm.CONTINUE).Modulate = new Color(ConstTerm.GREY);
                // commandList.GetNode<Label>(ConstTerm.CONTINUE).GetNode<ButtonUI>(ConstTerm.BUTTON).Disabled = true; 
            }
        }

        //=============================================================================
        // SECTION: Phase Handling - Input
        //=============================================================================

        protected override bool PhaseCheck(InputEvent @event)
        {
            bool valid = base.PhaseCheck(@event);
            if (!valid) { return false; }

            switch (inputPhase)
            {
                case ConstTerm.COMMAND:
                    CommandPhase(@event);
                    break;
                default:
                    break;
            }

            if (@event is InputEventMouseButton)
            {
                if (@event.IsActionPressed(ConstTerm.CANCEL + ConstTerm.CLICK))
                {
                    CancelCycle();
                }
            }

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
            UIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Ignore, out mouseFocus);
            SelectOption();
        }

        //=============================================================================
        // SECTION: Selection Handling
        //=============================================================================

        protected override void CancelCycle()
        {
            if (inputPhase == ConstTerm.COMMAND) { return; }
            base.CancelCycle();
        }

        public void SelectOption() // EDIT: Separate, signal, implement
        {
            if (activeList.GetChild<Label>(currentCommand).GetNode<ButtonUI>(ConstTerm.BUTTON).GetIsDisabled()) { return; }

            // switch (currentCommand)
            // {
            //     case 0:
            //         await StartNewGame();
            //         break;
            //     case 1:
            //         LoadSaveGame();
            //         break;
            //     case 2:
            //         OpenConfigOptions();
            //         break;
            //     case 3:
            //         GetTree().Quit();
            //         break;
            //     default:
            //         break;
            // }
        }

        //=============================================================================
        // SECTION: Command Handling
        //=============================================================================

        // private async Task StartNewGame()
        // {
        //     newGameScene = (Node2D)newGame.Instantiate();
        //     controlActive = false;

        //     await BGMPlayer.Instance.FadeBGMTransition(bgm, newGameScene.GetNode<MapSystem>(ConstTerm.MAPSYSTEM).GetBGM());

        //     GetTree().Root.AddChild(newGameScene);
        //     LoadStarterGear();
        //     Fader.Instance.FadeIn();
        //     QueueFree();
        //     await SaveLoader.Instance.LoadAllData(false); // Saving initial map ID - all other loads should skip, as sections don't exist yet.
        //     SaveLoader.Instance.LoadConfig();

        //     // newGameScene.GetNode<MapSystem>(ConstTerm.MAPSYSTEM).GetPartyManager().GetPlayer().ChangeActive(true);

        //     return;
        // }

        // private void LoadStarterGear() // EDIT: Temporary!
        // {
        //     ItemBag.Instance.AddToBag("Sword", ItemType.Weapon, 1);
        //     ItemBag.Instance.AddToBag("Helmet", ItemType.Armor, 1);
        //     ItemBag.Instance.AddToBag("Breastplate", ItemType.Armor, 1);
        //     ItemBag.Instance.AddToBag("Ring", ItemType.Accessory, 1);
        //     ItemBag.Instance.AddToBag("Ring", ItemType.Accessory, 1);
        // }

        // private void LoadSaveGame()
        // {
        //     Transitions moveToScene = new();
        //     moveToScene.LoadSavedScene(GetTree(), this);

        //     controlActive = false;
        // }

        // private void OpenConfigOptions()
        // {
        //     inputPhase = ConstTerm.WAIT;
        //     controlActive = false;
        //     configInput.OpenConfigOptions();
        //     // configOptions.Visible = true;

        //     // previousPhase.Add(inputPhase);
        //     // inputPhase = ConstTerm.OPTIONS;

        //     // activeList = configOptionsList;
        //     // SetNewCommand();
        // }

        //=============================================================================
        // SECTION: Signal Methods
        //=============================================================================

        private void OnCloseConfigOptions()
        {
            Startup();
            activeControl = UIFunctions.FocusOn(activeList, currentCommand);
            UIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Stop, out mouseFocus);
        }

        protected override void OnMouseEntered(Container currList, Node currLabel)
        {
            if (currList != activeList) { return; }

            activeControl = UIFunctions.FocusOff(currList, currentCommand);
            currentCommand = currLabel.GetIndex();

            activeControl = UIFunctions.FocusOn(currList, currentCommand);
            mouseFocus = currLabel.GetNode<ButtonUI>(ConstTerm.BUTTON);
        }

        protected override void OnMouseClick()
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