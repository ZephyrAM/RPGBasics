using Godot;
using System;

namespace ZAM.Interactions
{
    public partial class Event : CharacterBody2D
    {
        [Export] private Node[] targetPositions;
    }
}