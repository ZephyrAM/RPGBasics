using Godot;

[GlobalClass]
public partial class Modifier : Resource
{
    [Export] public StatID Stat { get; private set; }
    [Export] public float Value { get; private set; }

    public void StoreModifier(ConfigFile saveData, string textID, string type)
    {
        saveData.SetValue(textID, type + ConstTerm.MODIFIER + ConstTerm.NAME, (int)Stat);
        saveData.SetValue(textID, type + ConstTerm.MODIFIER + ConstTerm.VALUE, Value);
    }

    public void SetModifier(ConfigFile loadData, string textID, string type)
    {
        Stat = (StatID)(int)loadData.GetValue(textID, type + ConstTerm.MODIFIER + ConstTerm.NAME);
        Value = (float)loadData.GetValue(textID, type + ConstTerm.MODIFIER + ConstTerm.VALUE);
    }
}
