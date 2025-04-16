using Godot;
using Godot.Collections;
// using System.Collections.Generic;

namespace ZAM.MenuUI
{
    public partial class ConfigInfo : PanelContainer
    {
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

        private Container activeCommandList = null;
        private Container activeValueList = null;

        //=============================================================================
        // SECTION: Basic Methods
        //=============================================================================

        public override void _Ready()
        {
            Visible = false;
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
        }

        //=============================================================================
        // SECTION: Selection Handling
        //=============================================================================

        private void HideLists()
        {
            audioList.Visible = false;
            graphicsList.Visible = false;
            keybindsList.Visible = false;
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
                    ShowConfig(keybindsList);
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
            Array<Container> commandLists = [audioCommandList, graphicsCommandList, keybindsCommandList];
            return commandLists;
        }

        //=============================================================================
        // SECTION: Config Handling
        //=============================================================================

        public void ChangeVolumeText(int option, float value)
        {
            float oldValue = audioValueList.GetChild<Label>(option).Text.ToFloat();
            float newVolume;
            
            if (option == 0) { newVolume = Mathf.Clamp(oldValue + value, 0, BGMPlayer.Instance.GetMaxMaster() * 100);  }
            else { newVolume = Mathf.Clamp(oldValue + value, 0, BGMPlayer.Instance.GetMaxVolume()); }

            audioValueList.GetChild<Label>(option).Text = newVolume.ToString();
        }

        public void ChangeBorderlessText(int option, bool value)
        {
            graphicsValueList.GetChild<Label>(option).Text = value.ToString();
        }

        public void ChangeResolutionText(int option, (int x, int y) index)
        {
            graphicsValueList.GetChild<Label>(option).Text = index.x.ToString() + "x" + index.y.ToString();
        }

        public void ChangeKeybindText(int option)
        {
            InputEventKey tempKey = (InputEventKey)InputMap.ActionGetEvents(keybindsCommandList.GetChild<Label>(option).Text)[0];
            keybindsValueList.GetChild<Label>(option).Text = tempKey.PhysicalKeycode.ToString();
        }

        public void AwaitKeybindText(int option)
        {
            keybindsValueList.GetChild<Label>(option).Text = "  ";
        }

        //=============================================================================
        // SECTION: External Access
        //=============================================================================

        public void HideConfig()
        {
            Visible = false;
        }

        public void ShowConfig(Container option)
        {
            option.Visible = true;
            Visible = true;
        }

        public void SetupConfigValues((int, int) currentResolution)
        {
            audioValueList.GetChild<Label>(0).Text = BGMPlayer.Instance.GetVolume(ConstTerm.MASTER).ToString();
            audioValueList.GetChild<Label>(1).Text = BGMPlayer.Instance.GetVolume(ConstTerm.BGM).ToString();
            audioValueList.GetChild<Label>(2).Text = BGMPlayer.Instance.GetVolume(ConstTerm.SOUND).ToString();

            graphicsValueList.GetChild<Label>(0).Text = GetWindow().Mode == Window.ModeEnum.Fullscreen ? true.ToString() : false.ToString();
            graphicsValueList.GetChild<Label>(1).Text = currentResolution.Item1.ToString() + "x" + currentResolution.Item2.ToString();

            for (int c = 0; c < keybindsCommandList.GetChildCount(); c++)
            {
                InputEventKey tempKey = (InputEventKey)InputMap.ActionGetEvents(keybindsCommandList.GetChild<Label>(c).Text)[0];
                keybindsValueList.GetChild<Label>(c).Text = tempKey.PhysicalKeycode.ToString();
            }
        }
    }
}