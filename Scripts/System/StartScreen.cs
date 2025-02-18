using Godot;
using ZAM.Control;

namespace ZAM.System
{
    public partial class StartScreen : Node, IUIFunctions
    {
        [Export] private PackedScene newGame = null;

        [Export] private VBoxContainer optionList = null;
        // [Export] private ColorRect selectBar = null;

        private Node2D newGameScene = null;
        private Button activeControl = null;
        private Button mouseFocus = null;
        private Container activeList = null;

        private int numColumn = 1;
        private int currentCommand = 0;

        private bool signalsDone = false;
        private bool savesExist = false;
        private bool controlActive = true;
        private string activeInput = ConstTerm.KEY_GAMEPAD;

        public override void _Ready()
        {
            IfNull();
            CheckSaves();
            // selectBar.GetNode<AnimationPlayer>(ConstTerm.ANIM_PLAYER).Play(ConstTerm.CURSOR_BLINK);
        }

        private void IfNull()
        {
            newGame ??= ResourceLoader.Load<PackedScene>(ConstTerm.NEWGAME_SCENE);
            optionList ??= GetNode<CanvasLayer>(ConstTerm.CANVAS_LAYER).GetNode<PanelContainer>(ConstTerm.PANEL + ConstTerm.CONTAINER).GetNode<VBoxContainer>(ConstTerm.VBOX_CONTAINER);
            // selectBar ??= GetNode<CanvasLayer>(ConstTerm.CANVAS_LAYER).GetNode<PanelContainer>(ConstTerm.PANEL + ConstTerm.CONTAINER).GetNode<PanelContainer>(ConstTerm.SELECT + ConstTerm.CONTAINER).GetNode<GridContainer>(ConstTerm.SELECT + ConstTerm.LIST).GetNode<ColorRect>(ConstTerm.COLOR_RECT);
        }

        public override void _PhysicsProcess(double delta)
        {
            SubLists(optionList);
        }

        private void SubLists(Container targetList)
        {
            if (signalsDone) { return; }
            for (int c = 0; c < targetList.GetChildCount(); c++)
            {
                Node tempLabel = targetList.GetChild(c);
                targetList.GetChild(c).GetNode<Button>(ConstTerm.BUTTON).MouseEntered += () => OnMouseEntered(targetList, tempLabel);
                targetList.GetChild(c).GetNode<Button>(ConstTerm.BUTTON).Pressed += OnMouseClick;
            }

            Startup();
            signalsDone = true;
        }

        private void Startup()
        {
            activeList = optionList;
            CommandSelect(0, activeList, ConstTerm.VERT);
        }

        public override void _Input(InputEvent @event)
        {
            if (!signalsDone) { return; }
            PhaseCheck(@event);
        }

        //=============================================================================
        // SECTION: Phase Handling - Input
        //=============================================================================

        private void PhaseCheck(InputEvent @event)
        {
            if (@event is InputEventMouse && activeInput == ConstTerm.KEY_GAMEPAD) {
                activeInput = ConstTerm.MOUSE;
                Input.MouseMode = Input.MouseModeEnum.Visible; if (mouseFocus != null) { mouseFocus.MouseFilter = Godot.Control.MouseFilterEnum.Stop; } }
            else if (@event is not InputEventMouse && activeInput == ConstTerm.MOUSE) {
                activeInput = ConstTerm.KEY_GAMEPAD;
                Input.MouseMode = Input.MouseModeEnum.Hidden; if (mouseFocus != null) { mouseFocus.MouseFilter = Godot.Control.MouseFilterEnum.Ignore; } }

            if (!controlActive) { return; }
            if (@event.IsActionPressed(ConstTerm.ACCEPT)) {
                AcceptInput();
            }
            else if (@event.IsActionPressed(ConstTerm.UP)) {
                CommandSelect(-1, optionList, ConstTerm.VERT);
            }
            else if (@event.IsActionPressed(ConstTerm.DOWN)) {
                CommandSelect(1, optionList, ConstTerm.VERT);
            }
        }

        private void AcceptInput()
        {
            activeControl = IUIFunctions.FocusOff(activeList, currentCommand);
            // IUIFunctions.ToggleMouseFilter(activeList, Godot.Control.MouseFilterEnum.Ignore, out mouseFocus);
            SelectOption();
        }

        //=============================================================================
        // SECTION: Selection Handling
        //=============================================================================

        private void CommandSelect(int change, Container targetList, string direction)
        {
            change = IUIFunctions.CheckColumn(change, direction, numColumn);
            IUIFunctions.ChangeTarget(change, ref currentCommand, IUIFunctions.GetCommandCount(targetList));

            activeControl = IUIFunctions.FocusOn(targetList, currentCommand);
        }

        // private int ChangeTarget(int change, int target, int listSize)
        // {
        //     // if (direction == ConstTerm.HORIZ) { change += change;}
        //     if (target + change > listSize - 1) { return 0; }
        //     else if (target + change < 0) { return listSize - 1; }
        //     else { return target += change; }
        // }

        // public void MoveCursor(int index)
        // {
        //     selectBar.Position = new Vector2(0, selectBar.Size.Y * index);
        // }

        public async void SelectOption()
        {
            if (currentCommand == 0)
            {
                controlActive = false;

                Fader.Instance.FadeOut();
                await ToSignal(Fader.Instance.GetAnimPlayer(), ConstTerm.ANIM_FINISHED);
                // await ToSignal(Fader.Instance, ConstTerm.TRANSITION_FINISHED);

                newGameScene = (Node2D)newGame.Instantiate();
                GetTree().Root.AddChild(newGameScene);

                Fader.Instance.FadeIn();

                QueueFree();
            }
        }

        private void CheckSaves()
        {
            // string[] dirTest = DirAccess.GetFilesAt(SaveLoader.Instance.GetSavePath()); // EDIT: Separate file names array needed?
            var dir = DirAccess.Open(SaveLoader.Instance.GetSavePath());
            if (dir.GetFiles().Length > 0) { savesExist = true; }
            else { 
                optionList.GetNode<Label>(ConstTerm.CONTINUE).Modulate = new Color(ConstTerm.GREY);
                optionList.GetNode<Label>(ConstTerm.CONTINUE).GetNode<Button>(ConstTerm.BUTTON).Disabled = true; 
            }
        }

        //=============================================================================
        // SECTION: Signal Methods
        //=============================================================================

        private void OnMouseEntered(Container currList, Node currLabel)
        {
            if (currList != activeList) { return; }

            activeControl = IUIFunctions.FocusOff(currList, currentCommand);
            currentCommand = currLabel.GetIndex();

            activeControl = IUIFunctions.FocusOn(currList, currentCommand);
            mouseFocus = currLabel.GetNode<Button>(ConstTerm.BUTTON);
        }

        private void OnMouseClick()
        {
            AcceptInput();
        }
    }
}