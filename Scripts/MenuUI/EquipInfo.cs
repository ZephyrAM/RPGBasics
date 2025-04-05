using Godot;
using Godot.Collections;

using ZAM.Inventory;

namespace ZAM.MenuUI
{
    public partial class EquipInfo : Panel
    {
        [ExportGroup("MemberInfo")]
        [Export] private TextureRect charPortrait = null;
        [Export] private Label charName = null;
        [Export] private Label hpValue = null;
        [Export] private Label mpValue = null;
        [Export] private HSplitContainer equipLists = null;
        [Export] private HSplitContainer statLists = null;
        [Export] private VBoxContainer changeList = null;
        [Export] private Container gearPanel = null;

        private VBoxContainer equipLabels = null;
        private VBoxContainer statLabels = null;
        private VBoxContainer equipValues = null;
        private VBoxContainer statValues = null;
        private GridContainer gearList = null;

        private Array<GearSlotID> slotList = [];

        //=============================================================================
        // SECTION: Basic Methods
        //=============================================================================

        public override void _Ready()
        {
            IfNull();
            SetupChangeValues();
        }

        private void IfNull()
        {
            equipLabels = equipLists.GetChild<VBoxContainer>(0);
            equipValues = equipLists.GetChild<VBoxContainer>(1);

            statLabels = statLists.GetChild<VBoxContainer>(0);
            statValues = statLists.GetChild<VBoxContainer>(1);

            gearList = gearPanel.GetChild<GridContainer>(0);
        }

        public void SetupChangeValues()
        {
            for (int c = 0; c < changeList.GetChildCount(); c++) {
                changeList.GetChild<Label>(c).Text = "";
            }
        }

        //=============================================================================
        // SECTION: Access Methods
        //=============================================================================

        public void SetCharInfo(Texture2D portrait, string name)
        {
            charPortrait.Texture = portrait;
            charName.Text = name;
        }

        public void SetEquipValues(Array<EquipSlot> charEquipment)
        {
            slotList = [];
            for (int v = 0; v < equipValues.GetChildCount(); v++) {
                if (charEquipment[v + 1].Equip == null) {
                    equipValues.GetChild<Label>(v).Text = ConstTerm.EMPTY;
                } else {
                    equipValues.GetChild<Label>(v).Text = charEquipment[v + 1].Equip.ItemName; 
                }

                slotList.Add(charEquipment[v + 1].Slot);
            }
        }

        public void SetStatValues(Dictionary<StatID, float> statSheet)
        {
            for (int v = 0; v < statValues.GetChildCount(); v++) {
                statValues.GetChild<Label>(v).Text = statSheet[(StatID)v + 1].ToString();
            }
        }

        public void SetMaxHPMP(float hp, float mp)
        {
            hpValue.Text = hp.ToString();
            mpValue.Text = mp.ToString();
        }

        public void SetEquipSlot(Equipment gear, int slot)
        {
            slot--;
            if (gear == null) { equipValues.GetChild<Label>(slot).Text = ConstTerm.EMPTY; }
            else { equipValues.GetChild<Label>(slot).Text = gear.ItemName; }
        }

        public void ShowChangeValues(Array<float> values)
        {
            string textValue;
            for (int c = 0; c < values.Count; c++) {
                textValue = "";

                if (values[c] > 0) { 
                    changeList.GetChild<Label>(c).SelfModulate = new Color(ConstTerm.COLOR_INCREASE); 
                    textValue = "+" + values[c].ToString();
                }
                else if (values[c] < 0) { 
                    changeList.GetChild<Label>(c).SelfModulate = new Color(ConstTerm.COLOR_DECREASE);
                    textValue = values[c].ToString();
                }
                else { 
                    changeList.GetChild<Label>(c).SelfModulate = new Color(ConstTerm.WHITE); 
                }

                changeList.GetChild<Label>(c).Text = textValue;
            }
        }

        public void ClearChangeValues()
        {
            for (int s = 0; s < changeList.GetChildCount(); s++) {
                changeList.GetChild<Label>(s).Text = "";
            }
        }

        // public void GetChangeValues(Equipment oldGear, Equipment newGear)
        // {
        //     Dictionary<StatID, float> changeStat = [];

        //     foreach (Modifier change in oldGear.AddModifier) {
        //         changeStat[change.Stat] = change.Value;
        //     }
        //     foreach (Modifier change in newGear.AddModifier) {
        //         changeStat[change.Stat] += change.Value;
        //     }

        //     for (int s = 0; s < statValues.GetChildCount(); s++) {
        //     }
        // }

        public Container GetEquipList()
        {
            return equipValues;
        }

        public Container GetGearList()
        {
            return gearList;
        }

        public GearSlotID GetSlotID(int slot) // EDIT: Is this necessary?
        {
            return slotList[slot];
        }
    }
}