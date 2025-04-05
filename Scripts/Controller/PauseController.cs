using Godot;

using ZAM.MenuUI;

namespace ZAM.Controller
{
    public partial class PauseController : BaseController, IUIFunctions
    {
        [Export] private VBoxContainer commandList = null;
        [Export] private ConfigController configInput = null;
        [Export] private Container pauseScreen = null;

        // private ButtonUI mouseFocus = null;
        // private Container activeList = null;
        // private ButtonUI activeControl = null;

        // private string inputPhase = ConstTerm.WAIT;
        // private List<string> previousPhase = [];
        // private int numColumn = 1;
        // private int currentCommand = 0;
        // private List<int> previousCommand = [];

        // private bool signalsDone = false;
        // private bool controlActive = true;
        // private string activeInput = ConstTerm.KEY_GAMEPAD;

        // Delegate Events \\
        [Signal]
        public delegate void onSaveMenuEventHandler();
        [Signal]
        public delegate void onLoadMenuEventHandler();

        //=============================================================================
        // SECTION: Base Methods
        //=============================================================================
        // public override void _Ready()
        // {
        //     SetControlActive(false);
        // }

        private void SubSignals()
        {
            if (!signalsDone) {
                SubLists(commandList);
                configInput.onCloseConfigOptions += OnCloseConfigOptions;

                signalsDone = true;
            }
        }

        protected override void SetupListDict()
        {
            listDict.Add(ConstTerm.COMMAND, commandList);
        }

        public override void _PhysicsProcess(double delta)
        {
            SubSignals();
        }

        public override void _Input(InputEvent @event)
        {
            if (!signalsDone) { return; }
            if (!PhaseCheck(@event)) { return; };
            
            if (@event.IsActionPressed(ConstTerm.PAUSE)) { ClosePauseMenu(); }
        }

        private void Startup()
        {
            SetControlActive(true);
            activeList = commandList;
            CommandSelect(0, ConstTerm.VERT);
            SetInputPhase(ConstTerm.COMMAND);
        }

        //=============================================================================
        // SECTION: Phase Handling - Input
        //=============================================================================

        protected override bool PhaseCheck(InputEvent @event)
        {
            bool valid = base.PhaseCheck(@event);
            if (!valid) { return false; }

            switch (GetInputPhase())
            {
                case ConstTerm.COMMAND:
                    CommandPhase(@event);
                    break;
                default:
                    break;
            }

            // if (@event.IsActionPressed(ConstTerm.PAUSE)) { ClosePauseMenu(); }

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

        public void SelectOption()
        {
            if (activeList.GetChild<Label>(currentCommand).GetNode<ButtonUI>(ConstTerm.BUTTON).GetIsDisabled()) { return; }

            switch (currentCommand)
            {
                case 0:
                    OpenConfigOptions();
                    break;
                case 1:
                    GetTree().Quit();
                    break;
                case 2:
                    EmitSignal(SignalName.onSaveMenu);
                    break;
                case 3:
                    EmitSignal(SignalName.onLoadMenu);
                    break;
                default:
                    break;
            }
        }

        //=============================================================================
        // SECTION: Command Handling
        //=============================================================================

        private void OpenConfigOptions()
        {
            SetInputPhase(ConstTerm.WAIT);
            controlActive = false;
            configInput.OpenConfigOptions();
            // configOptions.Visible = true;

            // previousPhase.Add(inputPhase);
            // inputPhase = ConstTerm.OPTIONS;

            // activeList = configOptionsList;
            // SetNewCommand();
        }

        protected override void CancelCycle()
        {
            if (GetInputPhase() == ConstTerm.COMMAND) { ClosePauseMenu(); return; }
            base.CancelCycle();
        }

        //=============================================================================
        // SECTION: Signal Methods
        //=============================================================================

        private async void OnCloseConfigOptions()
        {
            await ToSignal(GetTree(), ConstTerm.PROCESS_FRAME);
            Startup();

            activeControl = IUIFunctions.FocusOn(activeList, currentCommand);
            IUIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Stop, out mouseFocus);
        }

        protected override void OnMouseClick()
        {
            switch (GetInputPhase())
            {
                case ConstTerm.COMMAND:
                    CommandAccept();
                    break;
                default:
                    break;
            }
        }

        //=============================================================================
        // SECTION: External Access
        //=============================================================================

        public void OpenPauseMenu()
        {
            pauseScreen.Visible = true;
            GetTree().Paused = true;

            Startup();
        }

        public async void ClosePauseMenu()
        {
            await ToSignal(GetTree(), ConstTerm.PROCESS_FRAME);
            pauseScreen.Visible = false;
            GetTree().Paused = false;
            SetControlActive(false);
        }
    }
}