using Godot;
using System;

namespace ZAM.Stats
{
    public partial class CharData : Node
    {
        [Export] Battler charBattler;
        [Export] CharacterID charId;
    }
}