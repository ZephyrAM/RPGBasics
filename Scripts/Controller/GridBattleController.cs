using Godot;
using System;

namespace ZAM.Controller
{
    public partial class GridBattleController : BattleController
    {
        //=============================================================================
        // SECTION: Signal Methods
        //=============================================================================

        protected override void OnMouseClick()
        {
            // activeControl = UIFunctions.FocusOff(activeList, currentCommand);

            // switch (GetInputPhase())
            // {
            //     case ConstTerm.COMMAND:
            //         CommandAccept();
            //         break;
            //     case ConstTerm.SKILL + ConstTerm.SELECT:
            //         SkillSelectAccept();
            //         break;
            //     case ConstTerm.ITEM + ConstTerm.SELECT:
            //         ItemSelectAccept();
            //         break;
            //     case ConstTerm.ATTACK:
            //     case ConstTerm.DEFEND:
            //     case ConstTerm.SKILL + ConstTerm.USE:
            //     case ConstTerm.ITEM + ConstTerm.USE:
            //     default:
            //         break;
            // }
        }
    }
}