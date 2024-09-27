using Godot;

public partial class SaveData : Resource
{
    public void SaveAllData(Variant data)
    {
        ResourceSaver.Save((Resource)data, "user://saveTest.tres");
    }
}
