using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class UpgradeEffect :Resource {

    public string Variable {get; set;} = "";

    public BigNumber Increase {get; set;} = new();
}