using Godot;
using Godot.Collections;

namespace ZAM.MenuUI
{
    public partial class ConfigInfo : PanelContainer
    {
        [Export] private PanelContainer keybindsPanel = null;

        [ExportGroup("ConfigLists")]
        [Export] private VBoxContainer audioList = null;
        [Export] private VBoxContainer graphicsList = null;
        [Export] private VBoxContainer keybindsList = null;

        private VBoxContainer audioCommandList = null;
        private VBoxContainer audioValueList = null;
        private VBoxContainer graphicsCommandList = null;
        private VBoxContainer graphicsValueList = null;
        private VBoxContainer keybindsCommandList = null;
        private VBoxContainer keybindsValueList = null;
        private VBoxContainer keybindsSecondValueList = null;
        private VBoxContainer gamepadBindsValueList = null;

        private Container activeCommandList = null;
        private Container activeValueList = null;

        private string prevBindLabel = "";

        //=============================================================================
        // SECTION: Basic Methods
        //=============================================================================

        public override void _Ready()
        {
            HideConfig();
            IfNull();
        }

        private void IfNull()
        {
            audioCommandList = audioList.GetNode<GridContainer>(ConstTerm.GRID + ConstTerm.CONTAINER).GetChild<VBoxContainer>(0);
            audioValueList = audioList.GetNode<GridContainer>(ConstTerm.GRID + ConstTerm.CONTAINER).GetChild<VBoxContainer>(1);

            graphicsCommandList = graphicsList.GetNode<GridContainer>(ConstTerm.GRID + ConstTerm.CONTAINER).GetChild<VBoxContainer>(0);
            graphicsValueList = graphicsList.GetNode<GridContainer>(ConstTerm.GRID + ConstTerm.CONTAINER).GetChild<VBoxContainer>(1);

            keybindsCommandList = keybindsList.GetNode<GridContainer>(ConstTerm.GRID + ConstTerm.CONTAINER).GetChild<VBoxContainer>(0);
            keybindsValueList = keybindsList.GetNode<GridContainer>(ConstTerm.GRID + ConstTerm.CONTAINER).GetChild<VBoxContainer>(1);
            keybindsSecondValueList = keybindsList.GetNode<GridContainer>(ConstTerm.GRID + ConstTerm.CONTAINER).GetChild<VBoxContainer>(2);
            gamepadBindsValueList = keybindsList.GetNode<GridContainer>(ConstTerm.GRID + ConstTerm.CONTAINER).GetChild<VBoxContainer>(3);
        }

        //=============================================================================
        // SECTION: Selection Handling
        //=============================================================================

        private void HideLists()
        {
            audioList.Visible = false;
            graphicsList.Visible = false;
            // keybindsList.Visible = false;
        }

        public void SetConfigList(string option)
        {
            HideLists();

            switch (option)
            {
                case ConstTerm.AUDIO:
                    activeCommandList = audioCommandList;
                    // SetupConfigValues();
                    ShowConfig(audioList);
                    break;
                case ConstTerm.GRAPHICS:
                    activeCommandList = graphicsCommandList;
                    ShowConfig(graphicsList);
                    break;
                case ConstTerm.KEYBINDS:
                    activeCommandList = keybindsCommandList;
                    ShowKeyConfig();
                    break;
                default:
                    break;
            }
        }

        public Container GetConfigList()
        {
            return activeCommandList;
        }

        public Array<Container> GetAllLists()
        {
            Array<Container> commandLists = [audioCommandList, graphicsCommandList, keybindsValueList, keybindsSecondValueList, gamepadBindsValueList];
            return commandLists;
        }

        public Array<Container> GetBindLists()
        {
            Array<Container> bindLists = [keybindsValueList, keybindsSecondValueList, gamepadBindsValueList];
            return bindLists;
        }

        //=============================================================================
        // SECTION: Config Handling
        //=============================================================================

        public void ChangeVolumeText(int option, float value)
        {
            float oldValue = audioValueList.GetChild<Label>(option).Text.ToFloat();
            float newVolume;

            if (option == 0) { newVolume = Mathf.Clamp(oldValue + value, 0, BGMPlayer.Instance.GetMaxMaster() * 100); }
            else { newVolume = Mathf.Clamp(oldValue + value, 0, BGMPlayer.Instance.GetMaxVolume()); }

            audioValueList.GetChild<Label>(option).Text = newVolume.ToString();
        }

        public void ChangeBorderlessText(int option, bool value)
        {
            graphicsValueList.GetChild<Label>(option).Text = value.ToString();
        }

        public void ChangeResolutionText(int option, Vector2 index)
        {
            graphicsValueList.GetChild<Label>(option).Text = index.X.ToString() + "x" + index.Y.ToString();
        }

        public void AwaitKeybindText(int option, Container list)
        {
            prevBindLabel = list.GetChild<Label>(option).Text;
            list.GetChild<Label>(option).Text = "_";
        }

        public bool ChangeKeybindText(InputEvent @event, int option, Container list)
        {
            if (@event == null)
            {
                list.GetChild<Label>(option).Text = prevBindLabel;
                return true;
            }


            int listIndex = GetBindLists().IndexOf(list);
            string tempLabel = keybindsCommandList.GetChild<Label>(option).Text;

            // If event used on different action, play error sound, wait for further input.
            for (int l = 0; l < list.GetChildCount(); l++)
            {
                bool result = InputMap.EventIsAction(@event, activeCommandList.GetChild<Label>(l).Text) && l != option;
                if (result)
                {
                    ButtonUI tempButton = list.GetChild(0).GetNode<ButtonUI>(ConstTerm.BUTTON);
                    tempButton.SetQuasiDisabled(true);
                    tempButton.OnButtonPressed();
                    tempButton.SetQuasiDisabled(false);
                    goto InvalidKey;
                }
            }

            switch (listIndex)
            {
                case 0:
                case 1:
                    if (@event is InputEventKey || @event is InputEventMouseButton)
                    {
                        if (prevBindLabel == ConstTerm.NONE)
                        {
                            if (@event is InputEventKey isKey)
                            {
                                InputEventKey kbKey = new() { PhysicalKeycode = isKey.PhysicalKeycode };
                                InputMap.ActionAddEvent(tempLabel, kbKey);
                                list.GetChild<Label>(option).Text = kbKey.PhysicalKeycode.ToString();
                                goto ValidKey;
                            }
                            else if (@event is InputEventMouseButton isButton)
                            {
                                InputEventMouseButton mKey = new() { ButtonIndex = isButton.ButtonIndex };
                                InputMap.ActionAddEvent(tempLabel, mKey);
                                list.GetChild<Label>(option).Text = mKey.ButtonIndex.ToString();
                                goto ValidKey;
                            }
                        }
                        else
                        {
                            foreach (InputEvent key in InputMap.ActionGetEvents(tempLabel)) // Search events for selected Action to replace correct one
                            {
                                if (key is InputEventKey isKey)
                                {
                                    if (isKey.PhysicalKeycode.ToString() == prevBindLabel)
                                    {
                                        InputEventKey kbKey = @event as InputEventKey;
                                        InputEventKey actualKey = isKey;
                                        actualKey.PhysicalKeycode = kbKey.PhysicalKeycode;
                                        list.GetChild<Label>(option).Text = kbKey.PhysicalKeycode.ToString();
                                        goto ValidKey;
                                    }
                                }
                                else if (key is InputEventMouseButton isButton)
                                {
                                    if (isButton.ButtonIndex.ToString() == prevBindLabel)
                                    {
                                        InputEventMouseButton mKey = @event as InputEventMouseButton;
                                        InputEventMouseButton actualButton = isButton;
                                        actualButton.ButtonIndex = mKey.ButtonIndex;
                                        list.GetChild<Label>(option).Text = actualButton.ButtonIndex.ToString();
                                        goto ValidKey;
                                    }
                                }
                            }
                        }
                    }
                    else { goto InvalidKey; }
                    break;
                case 2:
                    if (@event is InputEventJoypadButton)
                    {
                        if (prevBindLabel == ConstTerm.NONE)
                        {
                            InputEventJoypadButton eventButton = @event as InputEventJoypadButton;
                            InputEventJoypadButton gpKey = new() { ButtonIndex = eventButton.ButtonIndex };
                            InputMap.ActionAddEvent(tempLabel, gpKey);
                            list.GetChild<Label>(option).Text = gpKey.ButtonIndex.ToString();
                            goto ValidKey;
                        }
                        else
                        {
                            foreach (InputEvent button in InputMap.ActionGetEvents(tempLabel))
                            {
                                if (button is InputEventJoypadButton isButton)
                                {
                                    if (isButton.ButtonIndex.ToString() == prevBindLabel)
                                    {
                                        InputEventJoypadButton gpKey = @event as InputEventJoypadButton;
                                        InputEventJoypadButton actualButton;
                                        actualButton = isButton;
                                        actualButton.ButtonIndex = gpKey.ButtonIndex;
                                        list.GetChild<Label>(option).Text = actualButton.ButtonIndex.ToString();
                                        goto ValidKey;
                                    }
                                }
                            }
                        }
                    }
                    else { goto InvalidKey; }
                    break;
                default:
                    GD.PushError($"Invalid keybind list number {listIndex}.");
                    break;
            }

        InvalidKey:
            // list.GetChild<Label>(option).Text = prevBindLabel;
            return false;

        ValidKey:
            return true;
        }

        //=============================================================================
        // SECTION: External Access
        //=============================================================================

        public void HideConfig()
        {
            Visible = false;
            keybindsPanel.Visible = false;
        }

        public void ShowConfig(Container option)
        {
            option.Visible = true;
            Visible = true;
        }

        public void ShowKeyConfig()
        {
            keybindsPanel.Visible = true;
        }

        public void SetupConfigValues(Vector2 currentResolution)
        {
            audioValueList.GetChild<Label>(0).Text = BGMPlayer.Instance.GetVolume(ConstTerm.MASTER).ToString();
            audioValueList.GetChild<Label>(1).Text = BGMPlayer.Instance.GetVolume(ConstTerm.BGM).ToString();
            audioValueList.GetChild<Label>(2).Text = BGMPlayer.Instance.GetVolume(ConstTerm.SOUND).ToString();

            graphicsValueList.GetChild<Label>(0).Text = GetWindow().Mode == Window.ModeEnum.Fullscreen ? true.ToString() : false.ToString();
            graphicsValueList.GetChild<Label>(1).Text = currentResolution.X.ToString() + "x" + currentResolution.Y.ToString();

            for (int c = 0; c < keybindsCommandList.GetChildCount(); c++)
            {
                Array<InputEventKey> tempKey = [];
                foreach (InputEvent key in InputMap.ActionGetEvents(keybindsCommandList.GetChild<Label>(c).Text))
                {
                    if (key is InputEventKey newKey) { tempKey.Add(newKey); }
                }
                string tempString = tempKey[0].PhysicalKeycode.ToString();
                keybindsValueList.GetChild<Label>(c).Text = tempString;
                if (tempKey.Count > 1) {
                    tempString = tempKey[1].PhysicalKeycode.ToString();
                } else {
                    tempString = ConstTerm.NONE;
                }
                keybindsSecondValueList.GetChild<Label>(c).Text = tempString;

                Array<InputEventJoypadButton> tempButton = [];
                foreach (InputEvent button in InputMap.ActionGetEvents(keybindsCommandList.GetChild<Label>(c).Text))
                {
                    if (button is InputEventJoypadButton newButton) { tempButton.Add(newButton); }
                }
                gamepadBindsValueList.GetChild<Label>(c).Text = tempButton[0].ButtonIndex.ToString();
            }
        }
    }
}