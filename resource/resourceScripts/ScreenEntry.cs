using Godot;


[GlobalClass]
public partial class ScreenEntry : Resource{
    
    [Export]
    public string Key {get; set;} = "";
    
    [Export] 
    public bool IsOverlay {get; set;} = true;
    
    // [Export(PropertyHint.Range, "0,1,0.01")]
    // public float Opacity {get; set;} = 0.4f;

    [Export]
    public bool CanReturnToScreen {get; set;} = true;

    [Export] 
    public PackedScene Screen {get; set;}
}
