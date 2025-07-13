using Godot;
using Godot.Collections;
using System;
// using System.Collections.Generic;

using ZAM.MenuUI;

namespace ZAM.Controller
{
    public partial class ConfigController : BaseController
    {
        [Export] private Label listName = null;
        [Export] private ConfigInfo configPanel = null;
        [Export] private Container configOptions = null;
        [Export] private VBoxContainer configOptionsList = null;
        [Export] private ScrollContainer scrollContainer = null;
        [Export] private ScrollContainer keyScrollContainer = null;

        // private Container activeList = null;
        // private ButtonUI activeControl = null;
        // private string activeInput = ConstTerm.KEY_GAMEPAD;
        // private ButtonUI mouseFocus = null;

        // private string configPhase = ConstTerm.WAIT;
        // private List<string> previousPhase = [];
        // private int numColumn = 1;
        // private int currentCommand = 0;
        // private List<int> previousCommand = [];

        // private bool isActive = false;
        // private bool signalsDone = false;
        private bool borderlessToggle = false;

        private Array<Vector2> resolutionList = [new Vector2(640, 480), new Vector2(800, 600), new Vector2(1280, 720), new Vector2(1360, 768),
        new Vector2(1600, 900), new Vector2(1920, 1080), new Vector2(2560, 1080), new Vector2(3440, 1440)];
        // private (int, int) currentResolution;
        private int resolutionIndex = 4;
        // private int resolutionIndex = 0;

        private Array<string> changedKeys = [];

        // Delegate Events \\
        [Signal]
        public delegate void onCloseConfigOptionsEventHandler();

        //=============================================================================
        // SECTION: Base Methods
        //=============================================================================

        // public override void _Ready()
        // {
        //     SetControlActive(false);
        // }

        // private void StartupResolution()
        // {
        //     int resX = Mathf.CeilToInt(GetWindow().Size.X / 10f) * 10;
        //     int resY = Mathf.CeilToInt(GetWindow().Size.Y / 10f) * 10;
        //     currentResolution = (resX, resY);
        // }

        // private void SetResolutionInfo()
        // {
        //     StartupResolution();
        //     configPanel.SetupConfigValues(currentResolution);

        //     Predicate<(int, int)> resPred = r => { return r == currentResolution; };
        //     resolutionIndex = resolutionList.FindIndex(resPred);
        //     // resolutionIndex = resolutionIndex;
        // }

        private void SubSignals()
        {
            if (!signalsDone)
            {
                SubLists(configOptionsList);
                for (int c = 0; c < configPanel.GetAllLists().Count; c++)
                {
                    SubLists(configPanel.GetAllLists()[c]);
                }

                SetBorderless();
                configPanel.SetupConfigValues(resolutionList[resolutionIndex]);
                signalsDone = true;
            }
        }

        protected override void UnSubSignals()
        {
            UnSubLists(configOptionsList);
            for (int c = 0; c < configPanel.GetAllLists().Count; c++) {
                    UnSubLists(configPanel.GetAllLists()[c]);
                }
        }

        protected override void SetupListDict()
        {
            listDict.Add(ConstTerm.OPTIONS, configOptionsList);

            Array<Container> tempLists = configPanel.GetAllLists();
            listDict.Add(ConstTerm.AUDIO, tempLists[0]);
            listDict.Add(ConstTerm.GRAPHICS, tempLists[1]);
            listDict.Add(ConstTerm.KEYBINDS, tempLists[2]);
        }

        // private void SubLists(Container targetList)
        // {
        //     for (int c = 0; c < targetList.GetChildCount(); c++) {
        //         Node tempLabel = targetList.GetChild(c);
        //         targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).MouseEntered += () => OnMouseEntered(targetList, tempLabel);
        //         targetList.GetChild(c).GetNode<ButtonUI>(ConstTerm.BUTTON).Pressed += OnMouseClick;
        //     }
        // }


        public override void _PhysicsProcess(double delta)
        {
            SubSignals();
        }

        public override void _Input(InputEvent @event)
        {
            if (!IsControlActive()) { return; }
            if (!PhaseCheck(@event)) { return; }
        }

        //=============================================================================
        // SECTION: Phase Handling - Input
        //=============================================================================

        protected override bool PhaseCheck(InputEvent @event)
        {
            // if (@event is InputEventMouse && activeInput == ConstTerm.KEY_GAMEPAD) { activeInput = ConstTerm.MOUSE; 
            //     Input.MouseMode = Input.MouseModeEnum.Visible; if (mouseFocus != null) { mouseFocus.MouseFilter = Control.MouseFilterEnum.Stop; } }
            // else if (@event is not InputEventMouse && activeInput == ConstTerm.MOUSE) { activeInput = ConstTerm.KEY_GAMEPAD; 
            //     Input.MouseMode = Input.MouseModeEnum.Hidden; if (mouseFocus != null) { mouseFocus.MouseFilter = Control.MouseFilterEnum.Ignore; } }

            // if (@event is InputEventMouse) { EmitSignal(SignalName.onMouseHandling, false); }
            // else { EmitSignal(SignalName.onMouseHandling, true); }
            bool valid = base.PhaseCheck(@event);
            if (!valid) { return false; }

            switch (GetInputPhase())
            {
                case ConstTerm.OPTIONS:
                    OptionsPhase(@event);
                    break;
                case ConstTerm.AUDIO:
                    AudioPhase(@event);
                    break;
                case ConstTerm.GRAPHICS:
                    GraphicsPhase(@event);
                    break;
                case ConstTerm.KEYBINDS:
                    KeybindsPhase(@event);
                    break;
                case ConstTerm.REBIND:
                    RebindPhase(@event);
                    break;
                default:
                    break;
            }

            return true;

            // if (@event is InputEventMouseButton) { 
            //     if (@event.IsActionPressed(ConstTerm.CANCEL + ConstTerm.CLICK)) {
            //         CancelCycle(); } }
        }

        // private bool AcceptInput()
        // {
        //     activeControl = UIFunctions.FocusOff(activeList, currentCommand);
        //     if (activeControl.OnButtonPressed()) { UIFunctions.InvalidOption(activeList, currentCommand, ref activeControl, out mouseFocus); return false; }
        //     // UIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Ignore, out mouseFocus);
        //     return true;
        // }

        private void OptionsPhase(InputEvent @event)
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT)) {
                OptionsAccept();
            }
            else { PhaseControls(@event); }
        }

        private void OptionsAccept()
        {
            if (!AcceptInput()) { return; }
            // UIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Ignore, out mouseFocus);
            SelectConfigOption();
        }

        private void AudioPhase(InputEvent @event)
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT))
            {
                // AudioAccept(); // Does nothing currently.
            }
            else if (@event.IsActionPressed(ConstTerm.LEFT))
            {
                ChangeAudioVolume(-10);
            }
            else if (@event.IsActionPressed(ConstTerm.RIGHT))
            {
                ChangeAudioVolume(10);
            }
            else { PhaseControls(@event); }
        }

        private void AudioAccept()
        {
            if (!AcceptInput()) { return; }
            CommandSelect(0, ConstTerm.VERT);
            // UIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Ignore, out mouseFocus);
        }

        private void GraphicsPhase(InputEvent @event)
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT))
            {
                GraphicsAccept();
            }
            else if (@event.IsActionPressed(ConstTerm.LEFT))
            {
                ShiftGraphicsOption(-1);
            }
            else if (@event.IsActionPressed(ConstTerm.RIGHT))
            {
                ShiftGraphicsOption(1);
            }
            else { PhaseControls(@event); }
        }

        private void GraphicsAccept()
        {
            // if (!AcceptInput()) { return; } // No phase/window change. Not needed.
            SelectGraphicsOption();
        }

        private void KeybindsPhase(InputEvent @event)
        {
            if (@event.IsActionPressed(ConstTerm.ACCEPT))
            {
                KeybindsAccept();
            }
            else if (@event.IsActionPressed(ConstTerm.LEFT))
            {
                ChangeKeybindList(-1);
            }
            else if (@event.IsActionPressed(ConstTerm.RIGHT))
            {
                ChangeKeybindList(1);
            }
            else { PhaseControls(@event); }
        }

        private void KeybindsAccept()
        {
            if (!AcceptInput()) { return; }
            StartChangeKeybind(activeControl.GetParent<Label>().Text);
            previousCommand.Add(currentCommand);
        }

        private void RebindPhase(InputEvent @event)
        {
            if (!@event.IsPressed()) { return; }
            FinishChangeKeybind(@event);
        }

        // private void PhaseControls(InputEvent @event)
        // {
        //     if (@event.IsActionPressed(ConstTerm.CANCEL))
        //     {
        //         CancelCycle();
        //     }
        //     else if (@event.IsActionPressed(ConstTerm.UP))
        //     {
        //         CommandSelect(-1, ConstTerm.VERT);
        //     }
        //     else if (@event.IsActionPressed(ConstTerm.DOWN))
        //     {
        //         CommandSelect(1, ConstTerm.VERT);
        //     }
        // }

        //=============================================================================
        // SECTION: Selection Handling
        //=============================================================================

        // private void CommandSelect(int change, string direction)
        // {
        //     change = UIFunctions.CheckColumn(change, direction, numColumn);
        //     UIFunctions.ChangeTarget(change, ref currentCommand, UIFunctions.GetCommandCount(activeList));

        //     activeControl = UIFunctions.FocusOn(activeList, currentCommand);

        //     if (currentCommand == 0) { scrollContainer.ScrollVertical = 0; }
        // }

        // public void SetNewCommand()
        // {
        //     previousCommand.Add(currentCommand);
        //     currentCommand = 0;

        //     activeControl = UIFunctions.FocusOn(activeList, currentCommand);
        // }

        protected override void CancelCycle()
        {
            if (GetInputPhase() == ConstTerm.OPTIONS) { ConfigClose(); return; }
            else { configPanel.HideConfig(); }

            base.CancelCycle();

            // activeControl = UIFunctions.FocusOff(activeList, currentCommand);
            // UIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Ignore, out mouseFocus);

            // string oldPhase = UIFunctions.CancelSelect(out currentCommand, previousCommand, previousPhase);
            // SetInputPhase(oldPhase);

            // // activeList = configOptionsList; // EDIT: Only works with the two layer deep setup
            // activeControl = UIFunctions.FocusOn(activeList, currentCommand);
            // UIFunctions.ToggleMouseFilter(activeList, Control.MouseFilterEnum.Stop, out mouseFocus);
        }

        private void SelectGraphicsOption()
        {
            if (activeList.GetChild<Label>(currentCommand).GetNode<ButtonUI>(ConstTerm.BUTTON).GetIsDisabled()) { return; }

            switch (currentCommand)
            {
                case 0:
                    ChangeBorderless();
                    break;
                case 1:
                    UIFunctions.ChangeTarget(1, ref resolutionIndex, resolutionList.Count);
                    ChangeResolution();
                    break;
                default:
                    break;
            }
        }

        private void ShiftGraphicsOption(int change)
        {
            if (activeList.GetChild<Label>(currentCommand).GetNode<ButtonUI>(ConstTerm.BUTTON).GetIsDisabled()) { return; }

            switch (currentCommand)
            {
                case 0:
                    ChangeBorderless();
                    break;
                case 1:
                    UIFunctions.ChangeTarget(change, ref resolutionIndex, resolutionList.Count);
                    ChangeResolution();
                    break;
                case 2:
                    break;
                default:
                    break;
            }
        }

        private void SelectConfigOption()
        {
            string nextPhase = ConstTerm.AUDIO;

            switch (currentCommand)
            {
                case 0:
                    nextPhase = ConstTerm.AUDIO;
                    break;
                case 1:
                    nextPhase = ConstTerm.GRAPHICS;
                    break;
                case 2:
                    nextPhase = ConstTerm.KEYBINDS;
                    break;
            }

            SetNextConfig(nextPhase);
        }

        private void ChangeKeybindList(int change)
        {
            int index = configPanel.GetBindLists().IndexOf(activeList);
            index += change;

            if (index >= configPanel.GetBindLists().Count) { index = 0; }
            else if (index < 0) { index = configPanel.GetBindLists().Count - 1; }

            ChangeActiveList(configPanel.GetBindLists()[index]);
        }

        private void CancelKeybind()
        {
            int currList = configPanel.GetBindLists().IndexOf(activeList);
            string oldPhase = UIFunctions.CancelSelect(out currentCommand, previousCommand, previousPhase);
            SetInputPhase(oldPhase);

            activeList = configPanel.GetBindLists()[currList];
            activeControl = UIFunctions.FocusOn(activeList, currentCommand);

            foreach (Container list in configPanel.GetBindLists())
            {
                UIFunctions.ToggleMouseFilter(list, Control.MouseFilterEnum.Stop, out mouseFocus);
            }
        }

        //=============================================================================
        // SECTION: Command Handling
        //=============================================================================

        private void ChangeAudioVolume(int change)
        {
            configPanel.ChangeVolumeText(currentCommand, change);
            BGMPlayer.Instance.AdjustVolume(activeList.GetChild<Label>(currentCommand).Text, change);
            BGMPlayer.Instance.SetSoundVolume(activeControl.GetAudioPlayer());
            activeControl.OnButtonPressed();
        }

        private void SetBorderless()
        {
            GetWindow().Borderless = borderlessToggle;
            configPanel.ChangeBorderlessText(0, borderlessToggle); // EDIT: Set commands to numbers somewhere?
            if (borderlessToggle) { GetWindow().Mode = Window.ModeEnum.Fullscreen; } // EDIT: Needs to adjust fullscreen resolution to active resolution.
            else { GetWindow().Mode = Window.ModeEnum.Windowed; }
            UpdateResolution();
        }

        private void UpdateResolution()
        {
            // SetResolutionInfo();
            ChangeResolution();
        }

        private void ChangeBorderless()
        {
            borderlessToggle = !borderlessToggle;
            SetBorderless();
        }

        private void ChangeResolution()
        {
            configPanel.ChangeResolutionText(1, resolutionList[resolutionIndex]); // EDIT: Set commands to numbers somewhere?
            GetWindow().Size = new Vector2I((int)resolutionList[resolutionIndex].X, (int)resolutionList[resolutionIndex].Y);
            GetWindow().MoveToCenter();
        }

        private void StartChangeKeybind(string key)
        {
            // GD.Print(InputMap.ActionGetEvents(key));
            // InputEventKey tempKey = (InputEventKey)InputMap.ActionGetEvents(key)[0];
            // GD.Print(tempKey.PhysicalKeycode);

            configPanel.AwaitKeybindText(currentCommand, activeList);
            // previousPhase.Add(configPhase);
            SetInputPhase(ConstTerm.REBIND);

            foreach (Container list in configPanel.GetBindLists())
            {
                UIFunctions.ToggleMouseFilter(list, Control.MouseFilterEnum.Ignore, out mouseFocus);
            }
        }

        private void FinishChangeKeybind(InputEvent @event)
        {
            // EDIT: Make mouse clicks invalid instead of cancel
            if (@event.IsActionPressed(ConstTerm.ESCAPE) || @event.IsActionPressed(ConstTerm.ACCEPT + ConstTerm.CLICK) || @event.IsActionPressed(ConstTerm.CANCEL + ConstTerm.CLICK))
            {
                configPanel.ChangeKeybindText(null, currentCommand, activeList);
                CancelKeybind();
                return;
            }
            
            string tempLabel = configPanel.GetConfigList().GetChild<Label>(currentCommand).Text + "_" + configPanel.GetBindLists().IndexOf(activeList);
            bool keyCheck = configPanel.ChangeKeybindText(@event, currentCommand, activeList);
            if (!keyCheck) { return; }

            if (!changedKeys.Contains(tempLabel)) { changedKeys.Add(tempLabel); }

            // string oldPhase = previousPhase[^1];
            // previousPhase.RemoveAt(previousPhase.Count - 1);
            // SetInputPhase(ConstTerm.KEYBINDS);
            // CommandSelect(0, ConstTerm.VERT);
            CancelKeybind();
        }

        private void SetNextConfig(string option)
        {
            // previousPhase.Add(GetInputPhase());
            Label tempLabel = (Label)activeList.GetChild(currentCommand);
            listName.Text = tempLabel.Text;
            SetInputPhase(option);

            configPanel.SetConfigList(GetInputPhase());
            // activeList = configPanel.GetConfigList();
            SetNewCommand();

            scrollContainer.ScrollVertical = 0;
            keyScrollContainer.ScrollVertical = 0;
        }

        //=============================================================================
        // SECTION: External Access
        //=============================================================================

        public void OpenConfigOptions()
        {
            currentCommand = 0;
            configOptions.Visible = true;

            previousPhase.Add(GetInputPhase());
            SetInputPhase(ConstTerm.OPTIONS);

            activeList = configOptionsList;

            ButtonUI initialFocus = activeList.GetChild(currentCommand).GetNode<ButtonUI>(ConstTerm.BUTTON);
            initialFocus?.GrabFocus();

            SetControlActive(true);
        }

        public void ConfigClose()
        {
            SaveLoader.Instance.SaveConfig();
            UIFunctions.ResetMouseInput(configOptionsList, out mouseFocus);

            currentCommand = 0;
            previousCommand = [];
            previousPhase = [];

            SetInputPhase(ConstTerm.WAIT);
            configOptions.Visible = false;
            SetControlActive(false);

            EmitSignal(SignalName.onCloseConfigOptions);
        }

        // public void SetControlActive(bool active)
        // {
        //     isActive = active;
        // }

        //=============================================================================
        // SECTION: Signal Methods
        //=============================================================================

        // private void OnMouseEntered(Container currList, Node currLabel)
        // {
        //     if (currList != activeList) { return; }

        //     activeControl = UIFunctions.FocusOff(currList, currentCommand);
        //     currentCommand = currLabel.GetIndex();

        //     activeControl = UIFunctions.FocusOn(currList, currentCommand);
        //     mouseFocus = currLabel.GetNode<ButtonUI>(ConstTerm.BUTTON);
        // }

        protected override void OnMouseClick()
        {
            switch (GetInputPhase())
            {
                case ConstTerm.OPTIONS:
                    OptionsAccept();
                    break;
                case ConstTerm.AUDIO:
                    AudioAccept();
                    break;
                case ConstTerm.GRAPHICS:
                    GraphicsAccept();
                    break;
                case ConstTerm.KEYBINDS:
                    KeybindsAccept();
                    break;
                default:
                    break;
            }
        }

        //=============================================================================
        // SECTION: Save System
        //=============================================================================

        public void OnSaveConfig(ConfigFile config)
        {
            config ??= new ConfigFile();

            config.SetValue(ConstTerm.GRAPHICS, ConstTerm.BORDERLESS, borderlessToggle);
            config.SetValue(ConstTerm.GRAPHICS, ConstTerm.RESOLUTION, resolutionIndex);

            for (int k = 0; k < changedKeys.Count; k++) {
                string[] temp = changedKeys[k].Split('_');
                string action = temp[0];
                int id = temp[1].ToInt();

                config.SetValue(ConstTerm.KEYBINDS, changedKeys[k], InputMap.ActionGetEvents(action)[id]);
            }
            changedKeys = [];

            SaveLoader.Instance.SaveConfigFile(config);
        }

        public void OnLoadConfig(ConfigFile config)
        {
            if (config == null) { return; }
            if (config.HasSection(ConstTerm.GRAPHICS)) {
                borderlessToggle = (bool)config.GetValue(ConstTerm.GRAPHICS, ConstTerm.BORDERLESS);
                resolutionIndex = (int)config.GetValue(ConstTerm.GRAPHICS, ConstTerm.RESOLUTION);
            }

            if (config.HasSection(ConstTerm.KEYBINDS)) {
                for (int k = 0; k < config.GetSectionKeys(ConstTerm.KEYBINDS).Length; k++) {
                    string[] temp = config.GetSectionKeys(ConstTerm.KEYBINDS)[k].Split('_');
                    string action = temp[0];
                    int id = temp[1].ToInt();

                    InputEventKey tempKey = (InputEventKey)InputMap.ActionGetEvents(action)[id];
                    InputEventKey tempCode = (InputEventKey)config.GetValue(ConstTerm.KEYBINDS, config.GetSectionKeys(ConstTerm.KEYBINDS)[k]);
                    tempKey.PhysicalKeycode = tempCode.PhysicalKeycode;
                }
            }
        }
    }
}