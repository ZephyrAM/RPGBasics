using Godot;
using System;
using System.IO;

public partial class Json : Node
{
    public void ReadJson(string pathName)
    {
        string readJson = File.ReadAllText(pathName);
        Json json = new Json();
    }
}
