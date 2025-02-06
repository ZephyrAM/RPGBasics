using Godot;

namespace ZAM.System
{
    public partial class StartScreen : Node
    {
        [Export] private PackedScene newGame = null;

        [Export] private VBoxContainer optionList = null;
        [Export] private ColorRect selectBar = null;

        private Node2D newGameScene = null;

        private int optionCommand = 0;
        private bool savesExist = false;
        private bool controlActive = true;

        public override void _Ready()
        {
            IfNull();
            CheckSaves();
            selectBar.GetNode<AnimationPlayer>(ConstTerm.ANIM_PLAYER).Play(ConstTerm.CURSOR_BLINK);
        }

        private void IfNull()
        {
            newGame ??= ResourceLoader.Load<PackedScene>(ConstTerm.NEWGAME_SCENE);
            optionList ??= GetNode<CanvasLayer>(ConstTerm.CANVAS_LAYER).GetNode<PanelContainer>(ConstTerm.PANEL + ConstTerm.CONTAINER).GetNode<VBoxContainer>(ConstTerm.VBOX_CONTAINER);
            selectBar ??= GetNode<CanvasLayer>(ConstTerm.CANVAS_LAYER).GetNode<PanelContainer>(ConstTerm.PANEL + ConstTerm.CONTAINER).GetNode<PanelContainer>(ConstTerm.SELECT + ConstTerm.CONTAINER).GetNode<GridContainer>(ConstTerm.SELECT + ConstTerm.LIST).GetNode<ColorRect>(ConstTerm.COLOR_RECT);
        }

        public override void _Input(InputEvent @event)
        {
            if (!controlActive) { return; }
            if (@event.IsActionPressed(ConstTerm.ACCEPT)) {
                SelectOption();
            }
            else if (@event.IsActionPressed(ConstTerm.UP)) {
                CommandSelect(-1, optionList);
            }
            else if (@event.IsActionPressed(ConstTerm.DOWN)) {
                CommandSelect(1, optionList);
            }
        }

        //=============================================================================
        // SECTION: Selection Handling
        //=============================================================================

        private void CommandSelect(int change, Container targetList)
        {
            // if (direction == ConstTerm.VERT) { change *= numColumn; }
            optionCommand = ChangeTarget(change, optionCommand, optionList.GetChildCount());//choiceBox.CountChoices());
            MoveCursor(optionCommand);
        }

        private int ChangeTarget(int change, int target, int listSize)
        {
            // if (direction == ConstTerm.HORIZ) { change += change;}
            if (target + change > listSize - 1) { return 0; }
            else if (target + change < 0) { return listSize - 1; }
            else { return target += change; }
        }

        public void MoveCursor(int index)
        {
            selectBar.Position = new Vector2(0, selectBar.Size.Y * index);
        }

        public async void SelectOption()
        {
            if (optionCommand == 0)
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
            string[] dirTest = DirAccess.GetFilesAt(SaveLoader.Instance.GetSavePath());
            var dir = DirAccess.Open(SaveLoader.Instance.GetSavePath());
            if (dir.GetFiles().Length > 0) { savesExist = true; }
            else
            {
                optionList.GetNode<Button>(ConstTerm.CONTINUE).Disabled = true;
            }
        }
    }
}