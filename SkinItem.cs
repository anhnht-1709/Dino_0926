using Godot;

[GlobalClass]
public partial class SkinItem : Resource
{
    [Export] public string SkinID { get; set; }
    [Export] public string SkinName { get; set; }
    [Export] public int Price { get; set; }
    [Export] public Texture2D Icon { get; set; }
    [Export] public bool IsUnlocked { get; set; } = false;
}