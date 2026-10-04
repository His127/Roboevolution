using Godot;

[GlobalClass]
public partial class TreeVariable : Resource {
    [Export]
    public string VariableName {get; set;} = "";
    
    [Export]
    public Texture2D VariableNodeInside {get; set;}

    public BigNumber VariableValue {get; set;} = new();
}