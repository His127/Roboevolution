using System;
using Godot;

[GlobalClass]
public partial class JunkDrop : Resource {
    [Export]
    public Texture2D Texture {get; set;}
    [Export(PropertyHint.Range, "0,100,1")]
    public float DropChance {get; set;}
    [Export]
    public string Value {get; set;}

    public override int GetHashCode() {
        return HashCode.Combine(Texture, DropChance);
    }
}