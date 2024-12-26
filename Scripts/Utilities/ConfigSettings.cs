using Godot;
using System;

public partial class ConfigSettings : Node
{
    private enum Difficulties
    {
        Easy = 0, Normal = 1, Hard = 2, Nightmare = 3
    }

    private int currentDiff = (int)Difficulties.Normal;

    public int GetDifficulty()
    {
        return currentDiff;
    }

    public void SetDifficulty(int diff)
    {
        currentDiff = diff;
    }
}
